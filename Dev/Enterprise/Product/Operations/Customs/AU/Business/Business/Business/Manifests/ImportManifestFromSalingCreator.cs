using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public sealed class ImportManifestFromSailingCreator
	{
		public ImportManifestFromSailingCreator(BusinessObjectFactory factory, CustomsJobVoyageWrapper voyageWrapper)
		{
			this.factory = factory;
			this.voyageWrapper = voyageWrapper;
		}

		public TemporaryManifestHolder CreateImportManifests()
		{
			TemporaryManifestHolder result = new TemporaryManifestHolder(factory);
			GenerateImportManifests();

			foreach (TemporaryManifest manifest in importManifestsPerCountry.Values)
			{
				if (manifest != null)
				{
					SetLookupsDefaults(manifest.ImportManifest);
					manifest.ImportManifest.ReadOnly = false;
					result.Manifests.Add(manifest);
				}
			}

			return result;
		}

		#region Generate Import Manifests

		void GenerateImportManifests()
		{
			VoyageDestination firstArrivalPort = FindFirstArrivalPort(voyageWrapper.Voyage);
			VoyageOrigin lastForeignPort = FindLastForeignPort(firstArrivalPort);

			foreach (JobSailing sailing in voyageWrapper.Voyage.Sailings)
			{
				ZString dischargeCountry = sailing.JX_JB_RL_NKPortOfDischarge.Left(2);
				ZString loadCountry = sailing.JX_JA_RL_NKPortOfLoading.Left(2);

				if (!dischargeCountry.IsEmpty && !loadCountry.IsEmpty && (dischargeCountry == Core.Constants.CountryCodes.Australia || loadCountry == Core.Constants.CountryCodes.Australia))
				{
					TemporaryManifest manifest = GetOrCreateManifest(sailing.JX_JV_NKVessel, sailing.JX_JV_VoyageFlight, sailing.JX_Calc_DischargeCountry);

					if (lastForeignPort != null && manifest.ImportManifest.BT_RL_NKPortOfLastForeignPort.IsEmpty)
					{
						manifest.ImportManifest.BT_RL_NKPortOfLastForeignPort = lastForeignPort.JA_RL_NKPortOfLoading;
						manifest.ImportManifest.BT_PortOfLastForeignPortATD = lastForeignPort.JA_A_DEP;
					}

					if (manifest != null)
					{
						GetOrCreateArrival(manifest.ImportManifest, sailing);

						ZDBOnlySubQuery transports = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
						transports.AddToFilter(JobConsolTransportSchema.JW_IsLinked, true);
						transports.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.AgencyShipment);
						transports.AddToFilter(JobConsolTransportSchema.JW_JX, sailing.PK);

						ZDBOnlyQuery shipments = new ZDBOnlyQuery(typeof(AgencyShipment));
						shipments.AddToFilter(JobShipmentSchema.JS_JX, sailing.PK);
						shipments.AddSubQuery(transports, JoinCondition.Or);

						ZQuery filter = new ZQuery();
						filter.AddToFilter(shipments);
						filter.AddToFilter(JobShipmentSchema.JS_IsShipping, true);

						BillOfLading[] bills = factory.Load<BillOfLading>(filter);

						foreach (BillOfLading bill in bills)
						{
							CreateLine(manifest.ImportManifest, bill, sailing);
						}
					}
				}
			}

			foreach (TemporaryManifest manifest in importManifestsPerCountry.Values)
			{
				foreach (CusSeaManArrivalPort arrival in manifest.ImportManifest.Arrivals)
				{
					if (firstArrivalPort != null && arrival.BA_RL_NKArrivalPort == firstArrivalPort.JB_RL_NKPortOfDischarge)
					{
						arrival.BA_IsFirstArrival = true;
					}
					else
					{
						arrival.BA_IsFirstArrival = false;
					}
				}
			}
		}

		static VoyageDestination FindFirstArrivalPort(JobVoyage voyage)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Australia);
			filter.AddToFilter(JobVoyDestinationSchema.JB_JV, voyage.PK);
			filter.OrderBy = JobVoyDestinationSchema.Constants.JB_E_ARV + OrderByClause.Ascending;

			VoyageDestination[] destinations = voyage.Factory.Load<VoyageDestination>(filter);

			if (destinations.Length == 0 || destinations[0].JB_E_ARV.IsEmpty)
			{
				return null;
			}
			else
			{
				return destinations[0];
			}
		}

		static VoyageOrigin FindLastForeignPort(VoyageDestination firstArrivalPort)
		{
			if (firstArrivalPort == null)
			{
				return null;
			}

			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.Australia);
			filter.AddToFilter(JobVoyOriginSchema.JA_JV, firstArrivalPort.JB_JV);
			filter.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.LessThan, firstArrivalPort.JB_E_ARV);
			filter.OrderBy = JobVoyOriginSchema.Constants.JA_A_DEP + OrderByClause.Ascending;

			VoyageOrigin[] origins = firstArrivalPort.Factory.Load<VoyageOrigin>(filter);

			// empty dates sort ahead of non-empty dates, so if the first is not empty then none of them are.
			if (origins.Length == 0 || origins[0].JA_E_DEP.IsEmpty)
			{
				return null;
			}

			return origins[origins.Length - 1];
		}

		bool BillOfLadingExist(CusSeaManTranHead importManifest, BillOfLading bill)
		{
			foreach (CusSeaManOBLHeader manifestOceanBill in importManifest.OceanBills)
			{
				if (manifestOceanBill.BO_OceanBill == bill.JS_HouseBill &&
					manifestOceanBill.BO_RL_NKLoadPort == bill.JS_NKLoadPort &&
					manifestOceanBill.BO_RL_NKDischargePort == bill.JS_NKDischargePort)
				{
					return true;
				}
			}

			foreach (CusSeaManArrivalPort arrival in importManifest.Arrivals)
			{
				foreach (CusSeaManOBLHeaderCargoLine cargoLine in arrival.CargoLines)
				{
					if (cargoLine.BO_OceanBill == bill.JS_HouseBill &&
						cargoLine.BO_RL_NKLoadPort == bill.JS_NKLoadPort &&
						cargoLine.BO_RL_NKDischargePort == bill.JS_NKDischargePort)
					{
						return true;
					}
				}
			}

			return false;
		}

		TemporaryManifest GetOrCreateManifest(string vessel, string voyage, string countryCode)
		{
			string key = vessel + '|' + voyage + '|' + countryCode;
			TemporaryManifest result;

			if (!importManifestsPerCountry.TryGetValue(key, out result))
			{
				ZQuery manifestQuery = new ZQuery();
				manifestQuery.AddToFilter(CusSeaManTranHeadSchema.BT_VesselName, vessel);
				manifestQuery.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, voyage);

				CusSeaManTranHead importManifest = factory.LoadTop1<CusSeaManTranHead>(manifestQuery);

				if (importManifest == null)
				{
					importManifest = factory.New<CusSeaManTranHead>();
					importManifest.BT_VesselName = vessel;
					importManifest.BT_VoyageNum = voyage;
				}

				result = new TemporaryManifest(factory, importManifest);
				importManifestsPerCountry.Add(key, result);
			}

			return result;
		}

		static bool IsImportOrTranshipment(BillOfLading bill)
		{
			return bill.IsImport()
				|| (!bill.IsExport() && bill.JS_RL_NKDestination.Left(2) != Core.Constants.CountryCodes.Australia);
		}

		void CreateLine(CusSeaManTranHead manifest, BillOfLading bill, JobSailing sailing)
		{
			var portOfLoading = sailing.JX_JA_RL_NKPortOfLoading;
			var portOfDischarge = sailing.JX_JB_RL_NKPortOfDischarge;

			if (portOfDischarge.Left(2) == Core.Constants.CountryCodes.Australia && !BillOfLadingExist(manifest, bill))
			{
				var currentArrival = GetOrCreateArrival(manifest, sailing);
				currentArrival.BA_DischargeIndicator = true;

				if (bill.JS_PackingMode == Core.Constants.ContainerModes.FCL)
				{
					CusSeaManOBLHeader oceanBill = null;
					AgencyShipmentContainerDependentCollection containers = bill.IsBillOfLadingStage ? bill.RealContainers : bill.BookedContainers;
					var isImportOrTranshipmentBill = IsImportOrTranshipment(bill);

					foreach (BillOfLadingContainer container in containers)
					{
						if ((container.JC_IsEmptyContainer && container.JC_IsShipperOwned)
							|| (!container.JC_IsEmptyContainer && container.JC_RH_NKContainerCommodityCode != "MT" && isImportOrTranshipmentBill))
						{
							if (oceanBill == null)
							{
								oceanBill = CreateOceanBill(manifest, bill, portOfLoading, portOfDischarge);
							}

							CreateContainerLine(container, oceanBill, bill);
						}
						else
						{
							CreateCargoLine(currentArrival, container, bill, portOfLoading, portOfDischarge);
						}
					}
				}
				else if (bill.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk || bill.JS_PackingMode == Core.Constants.ContainerModes.Bulk
					|| bill.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff || bill.JS_PackingMode == Core.Constants.ContainerModes.Liquid)
				{
					if (IsImportOrTranshipment(bill))
					{
						var oceanBill = CreateOceanBill(manifest, bill, portOfLoading, portOfDischarge);
						CreateNonContainerLine(oceanBill, bill);
					}
					else
					{
						CreateCargoLine(currentArrival, null, bill, portOfLoading, portOfDischarge);
					}
				}
			}
		}

		CusSeaManArrivalPort GetOrCreateArrival(CusSeaManTranHead manifest, JobSailing sailing)
		{
			ZString dischargePort = sailing.JX_JB_RL_NKPortOfDischarge;

			foreach (CusSeaManArrivalPort arrival in manifest.Arrivals)
			{
				if (arrival.BA_RL_NKArrivalPort == dischargePort)
				{
					return arrival;
				}
			}

			CusSeaManArrivalPort result = null;

			if (!dischargePort.IsEmpty && dischargePort.Left(2) == Core.Constants.CountryCodes.Australia)
			{
				result = manifest.Arrivals.AddNew();
				result.BA_RL_NKArrivalPort = dischargePort;
				result.BA_ArrivalPortETA = sailing.JX_JB_E_ARV;
				result.BA_BerthCode = sailing.JX_JB_ArrivalBerth;
				result.BA_OA_CTOAddress = sailing.JX_JB_ArrivalCTOAddress;
				result.BA_ArrivalPortATA = sailing.JX_JB_A_ARV;
			}

			return result;
		}

		CusSeaManOBLHeader CreateOceanBill(CusSeaManTranHead manifest, BillOfLading bill, ZString portOfLoading, ZString portOfDischarge)
		{
			CusSeaManOBLHeader oceanBill = manifest.OceanBills.AddNew();

			oceanBill.BO_OceanBill = bill.JS_HouseBill;
			oceanBill.BO_RL_NKOriginPort = bill.JS_RL_NKOrigin;
			oceanBill.BO_RL_NKDestinationPort = bill.JS_RL_NKDestination;
			oceanBill.BO_RL_NKLoadPort = portOfLoading;
			oceanBill.BO_RL_NKDischargePort = portOfDischarge;

			oceanBill.BO_OH_Consignee = bill.ConsigneePK;
			oceanBill.BO_ConsigneeName = bill.ConsigneeDocumentaryAddress.E2_CompanyNameTruncated;
			oceanBill.BO_RN_NKConsigneeCountryCode = bill.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode;
			oceanBill.BO_ConsigneeCity = bill.ConsigneeDocumentaryAddress.E2_City;
			oceanBill.BO_ConsigneeAddress1 = bill.ConsigneeDocumentaryAddress.E2_Address1;
			oceanBill.BO_ConsigneeAddress2 = bill.ConsigneeDocumentaryAddress.E2_Address2;
			oceanBill.BO_ConsigneePostCode = bill.ConsigneeDocumentaryAddress.E2_Postcode;

			oceanBill.BO_OH_Consignor = bill.ConsignorPK;
			oceanBill.BO_ConsignorName = bill.ConsignorDocumentaryAddress.E2_CompanyNameTruncated;
			oceanBill.BO_RN_NKConsignorCountryCode = bill.ConsignorDocumentaryAddress.E2_RN_NKCountryCode;
			oceanBill.BO_ConsignorCity = bill.ConsignorDocumentaryAddress.E2_City;
			oceanBill.BO_ConsignorAddress1 = bill.ConsignorDocumentaryAddress.E2_Address1;
			oceanBill.BO_ConsignorAddress2 = bill.ConsignorDocumentaryAddress.E2_Address2;
			oceanBill.BO_ConsignorPostCode = bill.ConsignorDocumentaryAddress.E2_Postcode;
			switch (bill.JS_INCO)
			{
				case Core.Constants.DomesticPaymentTerms.Collect:
					oceanBill.BO_PaymentMethod = CMRMethodsOfPayment.Codes.Collect;
					break;
				case Core.Constants.DomesticPaymentTerms.Prepaid:
					oceanBill.BO_PaymentMethod = CMRMethodsOfPayment.Codes.PrepaidOnly;
					break;
			}

			return oceanBill;
		}

		void CreateContainerLine(CommonContainer container, CusSeaManOBLHeader oceanBill, BillOfLading bill)
		{
			CusSeaManOBLDetail billDetail = oceanBill.Details.AddNew();
			SetLineCargoType(container, billDetail);
			billDetail.BD_ContainerNumber = container.JC_ContainerNum;
			billDetail.BD_RC_ContainerType = container.JC_RC;
			billDetail.BD_NoOfPacks = container.JC_Calc_TotalPackages;
			billDetail.BD_PackType = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(container.JC_Calc_TotalPackagesUnit);
			billDetail.BD_CargoVolume = container.JC_Calc_TotalVolume;
			billDetail.BD_CargoVolumeUM = SeaCargoUtilities.ConvertVolumeUnitToCMRVolumeUnit(container.JC_Calc_TotalVolumeUnit);
			billDetail.BD_GrossWeight = container.JC_Calc_NetWeight;
			billDetail.BD_GrossWeightUM = container.JC_GrossWeightUQ;
			billDetail.BD_SealNo = container.JC_SealNum;
			billDetail.BD_HazardousIndicator = ContainerHasHazardous(container);
			billDetail.BD_GoodsDescription = GetGoodsDescriptionFromBill(bill, billDetail.BD_GoodsDescriptionInfo.MaxLength);
			billDetail.BD_MarksAndNumbers = bill.JS_MarksAndNumbersShort;
		}

		void CreateNonContainerLine(CusSeaManOBLHeader oceanBill, BillOfLading bill)
		{
			CusSeaManOBLDetail billDetail = oceanBill.Details.AddNew();
			switch (bill.JS_PackingMode)
			{
				case Core.Constants.ContainerModes.BreakBulk:
				case Core.Constants.ContainerModes.RollOnRollOff:
					billDetail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
					break;
				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Liquid:
					billDetail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
					break;
				default:
					billDetail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
					break;
			}
			billDetail.BD_NoOfPacks = bill.JS_OuterPacks;
			billDetail.BD_PackType = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(bill.JS_F3_NKPackType);
			billDetail.BD_CargoVolume = bill.JS_ActualVolume;
			billDetail.BD_CargoVolumeUM = SeaCargoUtilities.ConvertVolumeUnitToCMRVolumeUnit(bill.JS_UnitOfVolume);
			billDetail.BD_GrossWeight = bill.JS_ActualWeight;
			billDetail.BD_GrossWeightUM = bill.JS_UnitOfWeight;
			billDetail.BD_HazardousIndicator = bill.HasHazardous;
			billDetail.BD_GoodsDescription = GetGoodsDescriptionFromBill(bill, billDetail.BD_GoodsDescriptionInfo.MaxLength);
			billDetail.BD_MarksAndNumbers = bill.JS_MarksAndNumbersShort;
		}

		ZString GetGoodsDescriptionFromBill(BillOfLading bill, int maxLength)
		{
			if (bill != null)
			{
				if (!bill.DetailedGoodsDescriptionNoteText.IsEmpty)
				{
					return bill.DetailedGoodsDescriptionNoteText.Left(maxLength);
				}

				return bill.JS_GoodsDescription;
			}

			return ZString.Empty;
		}

		ZBool ContainerHasHazardous(CommonContainer container)
		{
			if (container.HasHazardous)
			{
				return true;
			}
			else
			{
				foreach (PackLine packline in container.PackLines)
				{
					if (packline.HasHazardous)
					{
						return true;
					}
				}
			}
			return false;
		}

		CusSeaManOBLHeaderCargoLine CreateCargoLine(CusSeaManArrivalPort currentArrival, CommonContainer container, BillOfLading bill, ZString portOfLoading, ZString portOfDischarge)
		{
			CusSeaManOBLHeaderCargoLine cargoLine = currentArrival.CargoLines.AddNew();
			using (cargoLine.GetValidationSuspender())
			{
				cargoLine.BO_RL_NKOriginPort = bill.JS_RL_NKOrigin;
				cargoLine.BO_RL_NKDestinationPort = bill.JS_RL_NKDestination;
				cargoLine.BO_RL_NKLoadPort = portOfLoading;
				cargoLine.BO_RL_NKDischargePort = portOfDischarge;
				cargoLine.NumberOfPackages = (container != null) ? container.JC_Calc_TotalPackages : bill.JS_OuterPacks;
				cargoLine.PackageType = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(container != null ? container.JC_Calc_TotalPackagesUnit : bill.JS_F3_NKPackType);
				cargoLine.BO_OceanBill = bill.JS_HouseBill;
				switch (bill.JS_PackingMode)
				{
					case Core.Constants.ContainerModes.BreakBulk:
					case Core.Constants.ContainerModes.RollOnRollOff:
						cargoLine.CargoType = CMRCargoTypes.Codes.BreakBulk;
						break;
					case Core.Constants.ContainerModes.Bulk:
					case Core.Constants.ContainerModes.Liquid:
						cargoLine.CargoType = CMRCargoTypes.Codes.Bulk;
						cargoLine.NumberOfPackages = 0;
						cargoLine.PackageType = ZString.Empty;
						break;
					default:
						cargoLine.CargoType = CMRCargoTypes.Codes.FullContainerLoad;
						break;
				}
				if (container != null)
				{
					cargoLine.CargoIdentifier = container.JC_ContainerNum;
				}
				else
				{
					cargoLine.CargoIdentifier = bill.JS_HouseBill;
				}

				if (container != null && (container.JC_IsEmptyContainer || container.JC_RH_NKContainerCommodityCode == "MT"))
				{
					cargoLine.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Empty;
					cargoLine.NumberOfPackages = 0;
					cargoLine.PackageType = ZString.Empty;
				}
				else if (bill.IsDomestic())
				{
					cargoLine.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Cabotage;
				}
				else
				{
					cargoLine.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Export;
				}
			}

			return cargoLine;
		}

		void SetLineCargoType(CommonContainer container, CusSeaManOBLDetail detail)
		{
			CusSeaManOBLDetail existedDetail = null;
			ZString key = container.JC_ContainerNum + container.JC_RC.ToString();
			if (containersCollection.TryGetValue(key, out existedDetail))
			{
				detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
				existedDetail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			}
			else
			{
				detail.BD_LineCargoType = container.JC_ContainerMode;
				containersCollection.Add(key, detail);
			}
		}

		#endregion

		#region Set Lookups Defaults

		void SetLookupsDefaults(CusSeaManTranHead importManifest)
		{
			importManifest.Lookups.SelectedPort = "ALL";
		}

		#endregion

		readonly Dictionary<ZString, CusSeaManOBLDetail> containersCollection = new Dictionary<ZString, CusSeaManOBLDetail>();
		readonly Dictionary<string, TemporaryManifest> importManifestsPerCountry = new Dictionary<string, TemporaryManifest>();
		readonly BusinessObjectFactory factory;
		readonly CustomsJobVoyageWrapper voyageWrapper;
	}
}
