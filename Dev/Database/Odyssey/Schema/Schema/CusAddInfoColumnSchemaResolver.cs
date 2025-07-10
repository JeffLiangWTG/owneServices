using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Schema
{
	[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	public class CusAddInfoColumnSchemaResolver
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:Avoid excessive complexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public ITableSchema GetCusAddInfoSchemaSchema(string addInfoTypeCode)
		{
			ITableSchema addInfoTableSchema = null;

			switch (addInfoTypeCode)
			{
				case "AML":
					addInfoTableSchema = USAMSLineAddInfoSchema.Instance;
					break;
				case "AMS":
					addInfoTableSchema = USAMSAddInfoSchema.Instance;
					break;
				case "APH":
					addInfoTableSchema = USAPHISHeaderAddInfoSchema.Instance;
					break;
				case "API":
					addInfoTableSchema = USAPHISIdentityNumberRangeAddInfoSchema.Instance;
					break;
				case "APL":
					addInfoTableSchema = USAPHISLicenseAddInfoSchema.Instance;
					break;
				case "APN":
					addInfoTableSchema = USAPHISInspectionAddInfoSchema.Instance;
					break;
				case "APP":
					addInfoTableSchema = USAPHISProductAddInfoSchema.Instance;
					break;
				case "APR":
					addInfoTableSchema = USAPHISRoutingAddInfoSchema.Instance;
					break;
				case "APS":
					addInfoTableSchema = USAPHISSourceAddInfoSchema.Instance;
					break;
				case "ARL":
					addInfoTableSchema = AURecommendationLetterAddInfoSchema.Instance;
					break;
				case "ATF":
					addInfoTableSchema = USATFAddInfoSchema.Instance;
					break;
				case "CAC":
					addInfoTableSchema = CACargoControlNumberAddInfoSchema.Instance;
					break;
				case "CSM":
					addInfoTableSchema = TradeChainPartnerAddInfoSchema.Instance;
					break;
				case "CCF":
					addInfoTableSchema = CFIAPGAHeaderAddInfoSchema.Instance;
					break;
				case "CCN":
					addInfoTableSchema = CNSCPGAHeaderAddInfoSchema.Instance;
					break;
				case "CCP":
					addInfoTableSchema = ComponentAddInfoSchema.Instance;
					break;
				case "CDT":
					addInfoTableSchema = CADutyAndTaxAddInfoSchema.Instance;
					break;
				case "CEC":
					addInfoTableSchema = ECCCPGAHeaderAddInfoSchema.Instance;
					break;
				case "CFO":
					addInfoTableSchema = DFOPGAHeaderAddInfoSchema.Instance;
					break;
				case "CGA":
					addInfoTableSchema = GACPGAHeaderAddInfoSchema.Instance;
					break;
				case "CHC":
					addInfoTableSchema = HCPGAHeaderAddInfoSchema.Instance;
					break;
				case "CLR":
					addInfoTableSchema = AUTravelDocAddInfoSchema.Instance;
					break;
				case "CNR":
					addInfoTableSchema = NRCanPGAHeaderAddInfoSchema.Instance;
					break;
				case "CON":
					addInfoTableSchema = AUCLREGContactInfoProviderAddInfoSchema.Instance;
					break;
				case "CPC":
					addInfoTableSchema = SGCPCAddInfoSchema.Instance;
					break;
				case "CPH":
					addInfoTableSchema = PHACPGAHeaderAddInfoSchema.Instance;
					break;
				case "CPR":
					addInfoTableSchema = USCPSCRuleAddInfoSchema.Instance;
					break;
				case "CPS":
					addInfoTableSchema = USCPSCAddInfoSchema.Instance;
					break;
				case "CPT":
					addInfoTableSchema = USCPSCLabReportAddInfoSchema.Instance;
					break;
				case "CTC":
					addInfoTableSchema = TCPGAHeaderAddInfoSchema.Instance;
					break;
				case "DEA":
					addInfoTableSchema = USDEAHeaderAddInfoSchema.Instance;
					break;
				case "DEC":
					addInfoTableSchema = USDEAConstituentAddInfoSchema.Instance;
					break;
				case "DOG":
					addInfoTableSchema = USOGADispositionDataAddInfoSchema.Instance;
					break;
				case "ERE":
					addInfoTableSchema = EnRouteEventIncidentSchema.Instance;
					break;
				case "ERI":
					addInfoTableSchema = EnRouteEventIncidentSchema.Instance;
					break;
				case "FDA":
					addInfoTableSchema = USACEFDAAddInfoSchema.Instance;
					break;
				case "FDH":
					addInfoTableSchema = USFSISLineAddInfoSchema.Instance;
					break;
				case "FLS":
					addInfoTableSchema = FDALicenseAddInfoSchema.Instance;
					break;
				case "FSH":
					addInfoTableSchema = USFSISLineAddInfoSchema.Instance;
					break;
				case "FSL":
					addInfoTableSchema = USFSISLotAddInfoSchema.Instance;
					break;
				case "FWH":
					addInfoTableSchema = USFWSHeaderAddInfoSchema.Instance;
					break;
				case "FWL":
					addInfoTableSchema = USFWSLicenseAddInfoSchema.Instance;
					break;
				case "GBA":
					addInfoTableSchema = GBCusAddInfoSchema.Instance;
					break;
				case "GBM":
					addInfoTableSchema = MawbExportAddInfoSchema.Instance;
					break;
				case "GSH":
					addInfoTableSchema = CcsukCusAddInfoSchema.Instance;
					break;
				case "GTX":
					addInfoTableSchema = EUAddInfoTaxSchema.Instance;
					break;
				case "HFC":
					addInfoTableSchema = USHFCHeaderAddInfoSchema.Instance;
					break;
				case "HFD":
					addInfoTableSchema = USHFCDetailAddInfoSchema.Instance;
					break;
				case "IPK":
					addInfoTableSchema = ItemPackagingAddInfoSchema.Instance;
					break;
				case "ITN":
					addInfoTableSchema = USITNumberAddInfoSchema.Instance;
					break;
				case "LAC":
					addInfoTableSchema = USCountriesAddInfoSchema.Instance;
					break;
				case "LOT":
					addInfoTableSchema = USFDALotAddInfoSchema.Instance;
					break;
				case "LSN":
					addInfoTableSchema = USLicenseAddInfoSchema.Instance;
					break;
				case "NFH":
					addInfoTableSchema = USNMFSHarvestingDetailAddInfoSchema.Instance;
					break;
				case "NFL":
					addInfoTableSchema = USNMFSLineAddInfoSchema.Instance;
					break;
				case "NFV":
					addInfoTableSchema = USNMFSVesselsAddInfoSchema.Instance;
					break;
				case "NMD":
					addInfoTableSchema = NZAddInfoSchema.Instance;
					break;
				case "NMF":
					addInfoTableSchema = NZMAFFilesAddInfoSchema.Instance;
					break;
				case "NTA":
					addInfoTableSchema = USNHTSAAdditionalNumAddInfoSchema.Instance;
					break;
				case "NTC":
					addInfoTableSchema = USNHTSADocumentAddInfoSchema.Instance;
					break;
				case "NTD":
					addInfoTableSchema = USNHTSADetailsAddInfoSchema.Instance;
					break;
				case "NTH":
					addInfoTableSchema = USNHTSAAddInfoSchema.Instance;
					break;
				case "NTP":
					addInfoTableSchema = USNHTSAPermitAndLicenseAddInfoSchema.Instance;
					break;
				case "OMC":
					addInfoTableSchema = USOMCAddInfoSchema.Instance;
					break;
				case "OMD":
					addInfoTableSchema = USOMCAquacultureFacilityAddInfoSchema.Instance;
					break;
				case "PGA":
					addInfoTableSchema = USPGAAddInfoSchema.Instance;
					break;
				case "PSL":
					addInfoTableSchema = USPSTLineAddInfoSchema.Instance;
					break;
				case "PST":
					addInfoTableSchema = USPSTAddInfoSchema.Instance;
					break;
				case "REG":
					addInfoTableSchema = AUCLREGInfoProviderAddInfoSchema.Instance;
					break;
				case "RFP":
					addInfoTableSchema = RFPNumberAddInfoSchema.Instance;
					break;
				case "ROC":
					addInfoTableSchema = ResultsOfControlAddInfoSchema.Instance;
					break;
				case "ROL":
					addInfoTableSchema = AURollAddInfoSchema.Instance;
					break;
				case "RQD":
					addInfoTableSchema = CIQRequiredDocumentAddInfoSchema.Instance;
					break;
				case "SCI":
					addInfoTableSchema = USScientificDataAddInfoSchema.Instance;
					break;
				case "TBC":
					addInfoTableSchema = USTTBCigarAddInfoSchema.Instance;
					break;
				case "TBL":
					addInfoTableSchema = USTTBLineAddInfoSchema.Instance;
					break;
				case "TBP":
					addInfoTableSchema = USTTBCOLAAndCertificateAddInfoSchema.Instance;
					break;
				case "TCC":
					addInfoTableSchema = NZCommodityConstituentAddInfoSchema.Instance;
					break;
				case "TCD":
					addInfoTableSchema = NZCommodityAddInfoSchema.Instance;
					break;
				case "TCI":
					addInfoTableSchema = NZCommodityItineraryAddInfoSchema.Instance;
					break;
				case "TCP":
					addInfoTableSchema = NZCommodityProductAddInfoSchema.Instance;
					break;
				case "UCN":
					addInfoTableSchema = MaritimeUcnThatIsHeldSchema.Instance;
					break;
				case "UDC":
					addInfoTableSchema = USDeliveryOrderContainerAddInfoSchema.Instance;
					break;
				case "UDH":
					addInfoTableSchema = USDeliveryOrderHeaderAddInfoSchema.Instance;
					break;
				case "UDL":
					addInfoTableSchema = USDeliveryOrderLineAddInfoSchema.Instance;
					break;
				case "UDP":
					addInfoTableSchema = USDispositionDataAddInfoSchema.Instance;
					break;
				case "UDZ":
					addInfoTableSchema = USDeliveryOrderHazmatAddInfoSchema.Instance;
					break;
				case "UFH":
					addInfoTableSchema = USFSISForm9540HeaderAddInfoSchema.Instance;
					break;
				case "UFL":
					addInfoTableSchema = USFSISForm9540LineAddInfoSchema.Instance;
					break;
				case "ULE":
					addInfoTableSchema = USLinkedEntryAddInfoSchema.Instance;
					break;
				case "ULR":
					addInfoTableSchema = UnloadingRemarkAddInfoSchema.Instance;
					break;
				case "UOD":
					addInfoTableSchema = USOGADispositionDetailAddInfoSchema.Instance;
					break;
				case "UPQ":
					addInfoTableSchema = USPPQForm368DataAddInfoSchema.Instance;
					break;
				case "US7":
					addInfoTableSchema = US7501DocPrintingAddInfoSchema.Instance;
					break;
				case "USA":
					addInfoTableSchema = USFDAAddInfoSchema.Instance;
					break;
				case "USC":
					addInfoTableSchema = USFCCAddInfoSchema.Instance;
					break;
				case "USD":
					addInfoTableSchema = USDrawbackNAFTAAddInfoSchema.Instance;
					break;
				case "USE":
					addInfoTableSchema = USAIILineAddInfoSchema.Instance;
					break;
				case "USI":
					addInfoTableSchema = USITDocAddInfoSchema.Instance;
					break;
				case "USP":
					addInfoTableSchema = USPGAAddInfoSchema.Instance;
					break;
				case "UST":
					addInfoTableSchema = USDOTAddInfoSchema.Instance;
					break;
				case "USV":
					addInfoTableSchema = USDOTVINAddInfoSchema.Instance;
					break;
				case "VDE":
					addInfoTableSchema = USVehicleDetailsAddInfoSchema.Instance;
					break;
				case "VEH":
					addInfoTableSchema = USVehicleAddInfoSchema.Instance;
					break;
				case "VID":
					addInfoTableSchema = CNVINDataAddInfoSchema.Instance;
					break;
				case "VV1":
					addInfoTableSchema = VesselVoyageAddInfoSchema.Instance;
					break;
				case "BLL":
					addInfoTableSchema = BLLFunctionAddInfoSchema.Instance;
					break;
				case "UWD":
					addInfoTableSchema = USWarehouseDetailAddInfoSchema.Instance;
					break;
				case "WCA":
					addInfoTableSchema = CusAddInfoSchema.Instance;
					break;
				case "ALI":
					addInfoTableSchema = CusAddInfoSchema.Instance;
					break;
				case "WPK":
					addInfoTableSchema = USWHSPackAddInfoSchema.Instance;
					break;
				case "WPL":
					addInfoTableSchema = USWHSPackLineAddInfoSchema.Instance;
					break;
				case "DOF":
					addInfoTableSchema = DrawbackOtherFeeAddInfoSchema.Instance;
					break;
				case "DTI":
					addInfoTableSchema = DrawbackAdditionalImportTariffNumberAddInfoSchema.Instance;
					break;
				case "SID":
					addInfoTableSchema = SIDataAddInfoSchema.Instance;
					break;
				default:
					break;
			}

			return addInfoTableSchema;
		}
	}
}
