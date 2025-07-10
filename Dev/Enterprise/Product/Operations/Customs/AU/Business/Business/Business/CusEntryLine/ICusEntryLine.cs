using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusEntryLine : Customs.Business.ICusEntryLine
	{
		CusEntryHeader Header { get; }
		ZString TariffNumber { get; }
		ZDecimal SecondCustomsQuantity { get; }
		ZString SecondCustomsUnitQty { get; }
		ZString StatCodeForCMR { get; }
		ZString GSTE { get; }
		ZString WETE { get; }
		ZString WETQ { get; }
		ZString NatureTypeForCMR { get; }
		ZString WMC { get; }
		char[] OrderedAMBs { get; }
		ZString ORG { get; }
		ZString POC { get; }
		ZString DCX { get; }
		ZDateTime FOD { get; }
		ZBool IsNature10 { get; }
		ZBool IsNature20 { get; }
		ZBool IsNature30 { get; }
		ZDecimal WRQ { get; }
		ZString WRU { get; }
		ZDecimal ISS { get; }
		ZDecimal LCP { get; }
		bool SendZeroManualDuty { get; }
		Money ManualDutyAmount { get; }
		Money StandardDutyOverriden { get; }
		ZString SupplierCode { get; }
		OrgHeader Supplier { get; }
		CMRCusEntryCPDecCollection Questions { get; }
		Money Price { get; }
		Money TransportAndInsuranceForMessage { get; }
		bool DoesTILVExist { get; }
		Money DumpingExportPrice { get; }
		Money PriceAdjustment { get; }
		ZString VAN { get; }
		ZString SCN { get; }
		ZString PST { get; }
		ZString PRT { get; }
		ZString WRN { get; }
		ZString DSN { get; }
		ZString LCTE { get; }
		ZString TR2 { get; }
		ZString CL2 { get; }
		ZString TreatmentCode { get; }
		ZString TRN { get; }
		ZString ISC { get; }
		ZString TAN { get; }
		ZString RNO { get; }
		ZString ICN { get; }
		ZString LCTI { get; }
		ZString LCTQ { get; }
		ZString MLPI { get; }
		ZString PUP { get; }
		ZString REL { get; }
		ZString SEC { get; }
		ZDecimal DRE { get; }
		Money OtherDutyFactor { get; }
		ZString TCI_InstrumentType { get; }
		ZString TI2_InstrumentType { get; }
		ZString PRI_InstrumentType { get; }
		ZString DXT { get; }
		ZString InstrumentCode { get; }
		ZString InstrumentType { get; }
		ZString[] OrderedELAs { get; }
		CusContainersInvoiceLinesCollection ContainersPivot { get; }
		ZString ValuationBasisForCMR { get; }
		ZInt WRL { get; }
		ZString[] OrderedVIDs { get; }
		bool IsExWarehouse { get; }
		ZString TCI_InstrumentNo { get; }
		ZString TI2_InstrumentNo { get; }
		ZString PRI_InstrumentNo { get; }
		ZString ActionCodeForMessage { get; }
		ZString RefundReasonCode { get; }
		bool IsGeneralRate { get; }
		AQISPackageCollection OrderedAQISPackages { get; }
		AQISDocumentCollection OrderedAQISDocuments { get; }
		AQISPremisesIdAndProcessingTypeCollection OrderedAQISPremisesIdAndProcessingTypes { get; }
		AQISCommodityCodeCollection OrderedAQISCommodityCodes { get; }
		AQISEntityIdCollection OrderedAQISEntityIds { get; }
		AQISPermitIdCollection OrderedAQISPermitIds { get; }
		AQISProducerCodeCollection OrderedAQISProducerCodes { get; }
		ZString ConsignorVendor { get; }
		IEnumerable<ZString> ImportPermitNumbers { get; }
		ZString WAR { get; }
	}
}
