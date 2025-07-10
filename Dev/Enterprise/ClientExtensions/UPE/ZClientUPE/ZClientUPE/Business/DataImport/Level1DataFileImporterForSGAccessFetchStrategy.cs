using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Business.SGAccess;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using OrganizationAddressesForRecord = Enterprise.Client.UPE.Business.DataImport.Level1DataFileImporterForSGAccess.OrganizationAddressesForRecord;
using OrgSupplierPart = Enterprise.Customs.SG.V4.Business.OrgSupplierPart;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1DataFileImporterForSGAccessFetchStrategy
	{
		public Level1DataFileImporterForSGAccessFetchStrategy(Level1DataFileImporterForSGAccess level1DataFileImporterForSGAccess)
		{
			this.level1DataFileImporterForSGAccess = level1DataFileImporterForSGAccess;
		}
		readonly Level1DataFileImporterForSGAccess level1DataFileImporterForSGAccess;

		BusinessObjectFactory Factory => level1DataFileImporterForSGAccess.FactoryProvider.Current;

		public void ExecuteFetchStrategy(Level1Record[] level1RecordList, ProgressNotification progressNotification, Dictionary<_500000Line, TariffView> lineTariffMap, Dictionary<Level1Record, OrganizationAddressesForRecord> organizationAddressesDictionary)
		{
			var accountNumbers = new HashSet<ZString>();
			var recordsToLoad = level1RecordList.Where(record => !record.IsEmpty && !Level1DataFileImporterForSGAccess.RecordShouldNotBeLoadedCore(record)).ToArray();
			var linesRelatedToPartNumberDictionary = new Dictionary<ZString, ISet<_50000LineWithParentRecord>>();

			foreach (Level1Record record in recordsToLoad)
			{
				AddHouseBillFetchHints(record);
				AddOrgCusCodeFetchHints(record, accountNumbers);

				foreach (_500000Line line500000 in record._500000Lines)
				{
					var partNumber = line500000.PartNumber;
					linesRelatedToPartNumberDictionary.AddOrAppend(partNumber, new _50000LineWithParentRecord(record, line500000));
				}
			}

			AddOrgHeaderFetchHints(accountNumbers);
			CreateOrganizationAddressDataForBill(recordsToLoad, progressNotification, organizationAddressesDictionary);

			var loadedParts = LoadParts(linesRelatedToPartNumberDictionary);

			var pivotTypes = new ZString[] { level1DataFileImporterForSGAccess.level1DataImport.IsExport ? ClassificationTypeList.Codes.HTE : ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTB };

			var linesRelatedToTariffCodeDictionary = new Dictionary<ZString, ISet<_500000Line>>();
			AddTariffCodesForLoadedParts(linesRelatedToTariffCodeDictionary, linesRelatedToPartNumberDictionary, organizationAddressesDictionary, loadedParts, pivotTypes);
			AddTariffCodesForRemainingLines(linesRelatedToTariffCodeDictionary, linesRelatedToPartNumberDictionary, pivotTypes);
			LoadTariffs(linesRelatedToTariffCodeDictionary, lineTariffMap);
		}

		void AddHouseBillFetchHints(Level1Record record)
		{
			var houseBillNumber = record.IsGCCLead() ? record._401000.LeadTrackingNumberForGCCShipment : record._200000.TrackingNumber;
			if (!level1DataFileImporterForSGAccess.level1DataImport.DisableDecisionProvider && !houseBillNumber.IsEmpty)
			{
				Factory.AddFetchHint(AsycudaBillSchema.Instance, SGDecisionSupporter.GetHouseBillQuery(houseBillNumber, ShipmentReferenceNumberRecyclePeriod));
			}
		}

		void AddOrgCusCodeFetchHints(Level1Record record, HashSet<ZString> accountNumbers)
		{
			AddOrgAccountNumberAndFetchHint(record._300000?.AccountNumber);
			AddOrgAccountNumberAndFetchHint(record._400000?.AccountNumber);
			AddOrgAccountNumberAndFetchHint(record._401000?.AccountNumber);

			void AddOrgAccountNumberAndFetchHint(string accountNumber)
			{
				if (!string.IsNullOrEmpty(accountNumber) && !accountNumbers.Contains(accountNumber))
				{
					accountNumbers.Add(accountNumber);
					Factory.AddFetchHint(OrgCusCodeSchema.Instance, UPEOrganisationMatching.GetOrgCusCodeFilter(accountNumber));
				}
			}
		}

		void AddOrgHeaderFetchHints(ISet<ZString> accountNumbers)
		{
			foreach (var accountNumber in accountNumbers)
			{
				var cusCodes = Factory.Load<OrgCusCode>(UPEOrganisationMatching.GetOrgCusCodeFilter(accountNumber));
				if (cusCodes.Length == 1)
				{
					Factory.AddFetchHint(OrgHeaderSchema.PK, cusCodes[0].OK_OH);
				}
			}
		}

		void CreateOrganizationAddressDataForBill(Level1Record[] recordsToProcess, ProgressNotification progressNotification, IDictionary<Level1Record, OrganizationAddressesForRecord> organizationAddressesDictionary)
		{
			var currentRecord = 0;
			var totalNoOfRecords = recordsToProcess.Length;
			foreach (Level1Record record in recordsToProcess)
			{
				var organizationAddresses = level1DataFileImporterForSGAccess.CreateOrganizationAddressDataForBill(record);
				organizationAddressesDictionary.Add(record, organizationAddresses);

				currentRecord++;
				progressNotification.PercentageComplete = (currentRecord * 100) / totalNoOfRecords;
				level1DataFileImporterForSGAccess.UpdateProgress(progressNotification);
			}
		}

		IList<OrgSupplierPart> LoadParts(IDictionary<ZString, ISet<_50000LineWithParentRecord>> linesRelatedToPartNumberDictionary)
		{
			var loadedParts = new List<OrgSupplierPart>();
			foreach (var batch in linesRelatedToPartNumberDictionary.Keys.Where(key => !key.IsEmpty).Batch(MaximumParametersPerFetchHint))
			{
				loadedParts.AddRange(Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, batch)));
			}
			loadedParts.ForEach(part => Factory.AddFetchHint(CusClassPartPivotSchema.CI_OP, part.PK));
			return loadedParts;
		}

		void AddTariffCodesForLoadedParts(
			IDictionary<ZString, ISet<_500000Line>> linesRelatedToTariffCodeDictionary,
			IDictionary<ZString, ISet<_50000LineWithParentRecord>> linesRelatedToPartNumberDictionary,
			IDictionary<Level1Record, OrganizationAddressesForRecord> organizationAddressesDictionary,
			IList<OrgSupplierPart> loadedParts,
			ZString[] pivotTypes)
		{
			foreach (var part in loadedParts)
			{
				var partNumber = part.OP_PartNum;
				if (linesRelatedToPartNumberDictionary.TryGetValue(partNumber, out var lines))
				{
					linesRelatedToPartNumberDictionary.Remove(partNumber);
					foreach (var lineWithRecord in lines)
					{
						var code = lineWithRecord.Line.CommodityCode;
						if (organizationAddressesDictionary.TryGetValue(lineWithRecord.Record, out var organizationAddresses))
						{
							code = GetTariffCode(Factory, partNumber, pivotTypes, code, organizationAddresses.Consignee, organizationAddresses.Consignor, lineWithRecord.Line.Description);
						}
						linesRelatedToTariffCodeDictionary.AddOrAppend(code, lineWithRecord.Line);
					}
				}
			}
		}

		void AddTariffCodesForRemainingLines(
			IDictionary<ZString, ISet<_500000Line>> linesRelatedToTariffCodeDictionary,
			IDictionary<ZString, ISet<_50000LineWithParentRecord>> linesRelatedToPartNumberDictionary,
			ZString[] pivotTypes)
		{
			foreach (IEnumerable<_50000LineWithParentRecord> set in linesRelatedToPartNumberDictionary.Values)
			{
				foreach (var lineWithRecord in set.Where(lineWithRecord => !lineWithRecord.Line.CommodityCode.IsEmpty))
				{
					var line = lineWithRecord.Line;
					var tariffCode = GetTariffCode(Factory, ZString.Empty, pivotTypes, line.CommodityCode, null, null, lineWithRecord.Line.Description);
					if (!tariffCode.IsEmpty)
					{
						linesRelatedToTariffCodeDictionary.AddOrAppend(tariffCode, line);
					}
				}
			}
		}

		void LoadTariffs(IDictionary<ZString, ISet<_500000Line>> linesRelatedToTariffCodeDictionary, Dictionary<_500000Line, TariffView> lineTariffMap)
		{
			for (var tariffCodeLength = 8; tariffCodeLength >= tariffCodeMinimumLength; tariffCodeLength--)
			{
				foreach (var batch in linesRelatedToTariffCodeDictionary.Keys.Where(key => key.Length >= tariffCodeLength).Batch(MaximumParametersPerFetchHint))
				{
					LoadTariffBatch(batch, tariffCodeLength, linesRelatedToTariffCodeDictionary, lineTariffMap);
				}

				linesRelatedToTariffCodeDictionary = linesRelatedToTariffCodeDictionary.GroupBy(pair => pair.Key.Left(tariffCodeLength - 1)).ToDictionary(group => group.Key, group => (ISet<_500000Line>)group.SelectMany(x => x.Value).ToHashSet());
			}

			foreach (var tariff in lineTariffMap.Values)
			{
				Factory.AddFetchHint(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, tariff.PK);
				Factory.AddFetchHint(TariffUOMViewSchema.ZZ8_ZZ1_ParentTariffOrNationalCode, tariff.PK);
				Factory.AddFetchHint(TariffRelationshipViewSchema.Instance, new TariffView.Loader(Factory).GetTariffRelationshipViewQuery(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.Commodity, System.Array.Empty<ZString>(), tariff.ZZ1_TariffCode, Constants.TariffTypes.HarmonizedSystem));
			}

			foreach (var tariff in lineTariffMap.Values)
			{
				Factory.AddFetchHint(TariffViewSchema.Instance, new TariffView.Loader(Factory).GetEffectiveTariffFilter(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.Commodity, ZString.Empty, level1DataFileImporterForSGAccess.EffectiveDateForDutyRate, tariff.ZZ1_TariffCode, Constants.TariffTypes.HarmonizedSystem));
			}
		}

		void LoadTariffBatch(IEnumerable<ZString> batchOfTariffCodes, int tariffCodeLength, IDictionary<ZString, ISet<_500000Line>> linesRelatedToTariffCodeDictionary, Dictionary<_500000Line, TariffView> lineTariffs)
		{
			var query = TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, batchOfTariffCodes.ToArray(), level1DataFileImporterForSGAccess.EffectiveDateForDutyRate, SQLComparisonOperator.StartsWith);
			var tariffs = Factory.Load<TariffView>(query);
			var matchingTariffs = tariffs.GroupBy(tariff => tariff.ZZ1_TariffCode.Left(tariffCodeLength)).Select(group => LoadBestMatch(group));
			foreach (TariffView matchingTariff in matchingTariffs)
			{
				var matchingTariffCode = matchingTariff.ZZ1_TariffCode;
				var matchingKeys = batchOfTariffCodes.Where(tariffCode => matchingTariffCode.StartsWith(tariffCode, System.StringComparison.OrdinalIgnoreCase));
				foreach (var key in matchingKeys)
				{
					if (linesRelatedToTariffCodeDictionary.TryGetValue(key, out var lines))
					{
						foreach (var line in lines)
						{
							lineTariffs.Add(line, matchingTariff);
						}
						linesRelatedToTariffCodeDictionary.Remove(key);
					}
				}
			}
		}

		ZInt MaximumParametersPerFetchHint
		{
			get
			{
				if (maximumParametersPerFetchHint == null)
				{
					maximumParametersPerFetchHint = new CachedProperty<ZInt>(Factory, () => ObjectFactory.Get<IEntityFrameworkSettings>().MaximumParametersPerFetchHint);
				}
				return maximumParametersPerFetchHint.Value;
			}
		}
		CachedProperty<ZInt> maximumParametersPerFetchHint;

		int ShipmentReferenceNumberRecyclePeriod
		{
			get
			{
				if (!shipmentReferenceNumberRecyclePeriod.HasValue)
				{
					shipmentReferenceNumberRecyclePeriod = UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod;
				}
				return shipmentReferenceNumberRecyclePeriod.Value;
			}
		}
		int? shipmentReferenceNumberRecyclePeriod;

		BusinessObject LoadBestMatch(IEnumerable<TariffView> tariffs)
		{
			var maxTariffs = tariffs.CollectMaxBy(tariff => tariff.ZZ1_TariffCode);

			BusinessObject result = null;
			var restrictiveDateTo = ZDateTime.Empty;
			foreach (var possibleMatch in maxTariffs.OrderByDescending(x => x.ZZ1_StartDate))
			{
				if (restrictiveDateTo.IsEmpty || restrictiveDateTo > possibleMatch.ZZ1_EndDate)
				{
					result = possibleMatch;
					restrictiveDateTo = possibleMatch.ZZ1_EndDate;
				}
			}
			return result;
		}

		ZString GetTariffCode(BusinessObjectFactory factory, ZString partNumber, ZString[] pivotTypes, ZString commodityCode, OrgHeader importer, OrgHeader supplier, ZString description)
		{
			return factory.GetCachedValue(string.Join("|", partNumber, commodityCode, importer?.OH_Code, supplier?.OH_Code), () =>
			{
				var harmonisedCode = ZString.Empty;
				if (!partNumber.IsEmpty)
				{
					var loadResult = Customs.Business.JobComInvoiceLinePartSynchronisationManager.LoadResults(factory, typeof(OrgSupplierPart), partNumber, importer, supplier, false, false, false, false);
					var pivots = ((OrgSupplierPart)loadResult.BestMatchingProduct)?.PivotsForBinding;
					if (pivots != null)
					{
						var importerPK = importer?.PK ?? ZGuid.Empty;
						var supplierPK = supplier?.PK ?? ZGuid.Empty;
						foreach (var pivotType in pivotTypes)
						{
							var matchedPivots = pivots.GetMatchesIgnoringAttributes(pivotType, importerPK, supplierPK);
							if (matchedPivots.Length > 0)
							{
								harmonisedCode = matchedPivots.Select(x => x.TariffNumber).Max();
								break;
							}
						}
					}
				}

				var useCommodityCode = harmonisedCode.IsEmpty;
				if (useCommodityCode)
				{
					if (commodityCode != description)
					{
						commodityCode = commodityCode.KeepAlphanumericCharacters();
						if (commodityCode.IsNumbersOnlyOrEmpty && commodityCode.Length >= tariffCodeMinimumLength)
						{
							harmonisedCode = commodityCode.Left(8);
						}
					}
				}

				return harmonisedCode;
			});
		}
		const int tariffCodeMinimumLength = 6;

		class _50000LineWithParentRecord
		{
			public Level1Record Record { get; }
			public _500000Line Line { get; }

			public _50000LineWithParentRecord(Level1Record record, _500000Line line)
			{
				Record = record;
				Line = line;
			}
		}
	}
}
