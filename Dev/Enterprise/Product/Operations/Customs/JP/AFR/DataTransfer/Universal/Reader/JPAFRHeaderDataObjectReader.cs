using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRHeaderDataObjectReader : ShipmentDataObjectReader<JPAFRHeader>
	{
		public JPAFRHeaderDataObjectReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.AFRHeader; }
		}

		protected override JPAFRHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			JPAFRHeader result = null;
			var masterBill = dataObject.WayBillNumber.GetValueOrDefault();
			if (!masterBill.IsEmpty)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (!billType.IsEmpty && billType == WayBillTypeList.Codes.Master)
				{
					var query = new ZQuery(JPAFRHeaderSchema.JPH_MasterBillNumber, masterBill);
					query.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
					var mAWBRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
					if (mAWBRecyclePeriod > 0)
					{
						query.AddToFilter(JPAFRHeaderSchema.JPH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
					}
					query.OrderBy = JPAFRHeaderSchema.Constants.JPH_SystemCreateTimeUtc + " desc";
					result = factory.LoadTop1<JPAFRHeader>(query);
				}
			}

			return result;
		}

		protected override IMatchingBusinessEntityFinder<JPAFRHeader> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for AFR. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		protected override void PopulateBusinessObject(JPAFRHeader header)
		{
			var headerRow = GetColumnIndexer(header);
			var headerPK = headerRow.GetValue(JPAFRHeaderSchema.PK);
			SetValue(headerRow, JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			if (CheckUpdateDataIsAllowed(header))
			{
				FillBranch(headerRow);
				FillCarrierDetails(header);
				SetValue(headerRow, JPAFRHeaderSchema.JPH_VesselName, dataObject.VesselName);
				SetValue(headerRow, JPAFRHeaderSchema.JPH_Voyage, dataObject.VoyageFlightNo);
				SetValue(headerRow, JPAFRHeaderSchema.JPH_RL_NKLoading, dataObject.PortOfLoading);
				SetValue(headerRow, JPAFRHeaderSchema.JPH_LoadingPortSuffix, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingSuffix, logger));
				SetValue(headerRow, JPAFRHeaderSchema.JPH_RL_NKDischarge, dataObject.PortOfDischarge);
				FillDates(headerRow);
				SetValue(headerRow, JPAFRHeaderSchema.JPH_MasterBillNumber, dataObject.WayBillNumber);
				SetValue(headerRow, JPAFRHeaderSchema.JPH_RelaxedAppId, dataObject.AddInfoCollection.GetZBoolValue(AddInfoConstants.Header.IsDepartureFromRelaxedArea, logger));
				FillNotes(header);
				FillBills(header);
			}
		}

		void FillCarrierDetails(JPAFRHeader header)
		{
			var headerRow = GetColumnIndexer(header);
			var carrierCode = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCode, logger);
			if (!carrierCode.HasValue)
			{
				var carrierAddressData = dataObject.OrganizationAddressCollection.FindBestCarrierMatch();
				if (carrierAddressData != null)
				{
					carrierCode = carrierAddressData.RegistrationNumberCollection.GetValue(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Japan);
					var carrierAddressBO = new OrganisationDataObjectReader(carrierAddressData, logger, factory).GetMatched();
					var headerPK = headerRow.GetValue(JPAFRHeaderSchema.PK);
					var carrierJobDocAddressQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, headerPK);
					carrierJobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.Carrier);
					carrierJobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
					carrierJobDocAddressQuery.FetchOnlyFromLocalCache = !header.IsInDatabase;
					var carrierJobDocAddressBO = factory.LoadTop1<JobDocAddress>(carrierJobDocAddressQuery);
					if (carrierAddressBO == null)
					{
						if (carrierJobDocAddressBO != null)
						{
							var carrierJobDocAddressRow = GetColumnIndexer(carrierJobDocAddressBO);
							SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_AddressOverride, ZBool.False);
							SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
						}
					}
					else
					{
						carrierJobDocAddressBO = carrierJobDocAddressBO ?? factory.New<JobDocAddress>();
						var carrierJobDocAddressRow = GetColumnIndexer(carrierJobDocAddressBO);
						if (!carrierJobDocAddressBO.IsInDatabase)
						{
							SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_ParentID, headerPK);
							SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_ParentTableCode, JobDocAddressSchema.Constants.Prefix);
							SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.Carrier);
							SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_AddressSequence, ZByte.Zero);
						}
						SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_AddressOverride, ZBool.False);
						SetValue(carrierJobDocAddressRow, JobDocAddressSchema.E2_OA_Address, GetColumnIndexer(carrierAddressBO).GetValue(OrgAddressSchema.PK));
					}
				}
			}
			SetValue(headerRow, JPAFRHeaderSchema.JPH_CarrierCode, carrierCode);
		}

		void FillDates(IColumnIndexer headerRow)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(headerRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(JPAFRHeaderSchema.JPH_ETD, new[] { DateType.Departure, DateType.LoadingDate }),
					new DateTypeSchemaColumnMap(JPAFRHeaderSchema.JPH_ETA, new[] { DateType.Arrival, DateType.DischargeDate }));
			}
		}

		bool CheckUpdateDataIsAllowed(JPAFRHeader header)
		{
			var result = true;
			if (header.IsInDatabase && IsMessagingActive(header))
			{
				result = false;
				logger.LogBoth(LogType.Warning, Res.GetString("E74D5C93-1AF1-4171-A62E-CA0AA7F87CF8", "{0} data will not be updated as there is an active messaging.", header.HumanReadableName));
			}
			return result;
		}

		bool IsMessagingActive(JPAFRHeader header)
		{
			return false; // TODO: Add messaging check
		}

		JPManifestDataObjectReaderHelper Helper
		{
			get { return helper ?? (helper = new JPManifestDataObjectReaderHelper(factory)); }
		}
		JPManifestDataObjectReaderHelper helper;

		void FillBills(JPAFRHeader header)
		{
			if (dataObject.SubShipmentCollection != null)
			{
				Helper.MarkUnprocessedExistingBillsFor(header);
				foreach (var shipmentDataObject in dataObject.SubShipmentCollection)
				{
					var bill = new JPAFRBillsDataObjectReader(shipmentDataObject, logger, Helper, header).ReadIntoBusinessObject();
					Helper.MarkProcessed(bill);
				}
				Helper.DeleteUnprocessedBillsFor(header, logger);
			}
		}

		void FillBranch(IColumnIndexer headerRow)
		{
			ZGuid branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(headerRow, JPAFRHeaderSchema.JPH_GB_Branch, branchPK);
			}
		}

		void FillNotes(JPAFRHeader header)
		{
			if (dataObject.NoteCollection != null)
			{
				// TODO : Handle Linked to Consol
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, header).ReadIntoCollection();
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JPAFRHeader header)
		{
			var builder = new ZStringBuilder();
			if (dataObject.WayBillType.GetCodeAsUpperCase() != WayBillTypeList.Codes.Master)
			{
				builder.Append(Res.GetString("352EE0E7-F1CA-4E6F-8BBF-31DFA45E13EA", "{1} must be '{0}'.", WayBillTypeList.Codes.Master, "WayBillType.Code"));
			}
			if (dataObject.WayBillNumber.GetValueOrDefault().IsEmpty)
			{
				builder.Append(Res.GetString("57B2F4BB-6E48-4AFF-B99F-F31F457D26ED", "{0} must not be empty.", "WayBillNumber"));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}
	}
}
