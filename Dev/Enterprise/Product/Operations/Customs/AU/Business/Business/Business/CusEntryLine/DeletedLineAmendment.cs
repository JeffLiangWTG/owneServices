using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for DeletedLineAmendment.
	/// </summary>
	public class DeletedLineAmendment : Customs.Business.DeletedLineAmendment, ICusEntryLine
	{
		public DeletedLineAmendment(ICusEntryLine deletePendingEntryLine, CusEntryHeader currentEntryHeader)
			: base(null, currentEntryHeader)
		{
			this.deletePendingEntryLine = deletePendingEntryLine;
		}

		readonly ICusEntryLine deletePendingEntryLine;

		public const string RefundReasonForDeletedLines = "126A";

		#region ICusEntryLine Members

		CusEntryHeader ICusEntryLine.Header
		{
			get { return currentEntryHeader as CusEntryHeader; }
		}

		ZString ICusEntryLine.ActionCodeForMessage
		{
			get { return LineAction.Delete; }
		}

		ZShort Customs.Business.ICusEntryLine.CL_LineNumber
		{
			get { return deletePendingEntryLine.CL_LineNumber; }
		}

		ZString ICusEntryLine.RefundReasonCode
		{
			get { return deletePendingEntryLine.RefundReasonCode; }
		}

		ZString ICusEntryLine.NatureTypeForCMR
		{
			get { return ZString.Empty; }
		}

		Money ICusEntryLine.Price
		{
			get { return null; }
		}

		Money ICusEntryLine.TransportAndInsuranceForMessage
		{
			get { return null; }
		}

		bool ICusEntryLine.DoesTILVExist
		{
			get { return false; }
		}

		protected override Money CustomsValueCore
		{
			get { return null; }
		}

		ZDecimal Customs.Business.ICusEntryLine.CustomsQuantity
		{
			get { return 0M; }
		}

		ZString Customs.Business.ICusEntryLine.CustomsUnitQty
		{
			get { return ZString.Empty; }
		}

		ZDecimal ICusEntryLine.WRQ
		{
			get { return 0M; }
		}

		ZString ICusEntryLine.WRU
		{
			get { return ZString.Empty; }
		}

		public ZBool IsNature10 => ZBool.True;
		public ZBool IsNature20 => ZBool.False;
		public ZBool IsNature30 => ZBool.False;

		ZString ICusEntryLine.SupplierCode
		{
			get { return ZString.Empty; }
		}

		OrgHeader ICusEntryLine.Supplier => null;

		ZDecimal ICusEntryLine.SecondCustomsQuantity
		{
			get { return 0M; }
		}

		ZString ICusEntryLine.SecondCustomsUnitQty
		{
			get { return ZString.Empty; }
		}

		public bool SendZeroManualDuty
		{
			get { return false; }
		}

		Money ICusEntryLine.ManualDutyAmount
		{
			get { return null; }
		}

		Money ICusEntryLine.StandardDutyOverriden
		{
			get { return null; }
		}

		ZString ICusEntryLine.StatCodeForCMR
		{
			get { return ZString.Empty; }
		}

		bool ICusEntryLine.IsGeneralRate
		{
			get { return false; }
		}

		ZString ICusEntryLine.TariffNumber
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.GSTE
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.WETE
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.WETQ
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.WMC
		{
			get { return ZString.Empty; }
		}

		char[] ICusEntryLine.OrderedAMBs
		{
			get { return System.Array.Empty<char>(); }
		}

		ZString ICusEntryLine.ORG
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.POC
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.DCX
		{
			get { return ZString.Empty; }
		}

		ZDateTime ICusEntryLine.FOD
		{
			get { return ZDateTime.Empty; }
		}

		ZDecimal ICusEntryLine.ISS
		{
			get { return 0M; }
		}

		ZDecimal ICusEntryLine.LCP
		{
			get { return 0M; }
		}

		CMRCusEntryCPDecCollection ICusEntryLine.Questions
		{
			get { return null; }
		}

		AQISPackageCollection ICusEntryLine.OrderedAQISPackages
		{
			get { return null; }
		}

		AQISDocumentCollection ICusEntryLine.OrderedAQISDocuments
		{
			get { return null; }
		}

		AQISCommodityCodeCollection ICusEntryLine.OrderedAQISCommodityCodes
		{
			get { return null; }
		}

		AQISEntityIdCollection ICusEntryLine.OrderedAQISEntityIds
		{
			get { return null; }
		}

		AQISProducerCodeCollection ICusEntryLine.OrderedAQISProducerCodes
		{
			get { return null; }
		}

		AQISPermitIdCollection ICusEntryLine.OrderedAQISPermitIds
		{
			get { return null; }
		}

		AQISPremisesIdAndProcessingTypeCollection ICusEntryLine.OrderedAQISPremisesIdAndProcessingTypes
		{
			get { return null; }
		}

		Money ICusEntryLine.DumpingExportPrice
		{
			get { return null; }
		}

		Money ICusEntryLine.PriceAdjustment
		{
			get { return null; }
		}

		ZString ICusEntryLine.VAN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.SCN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.PST
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.PRT
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.WRN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.DSN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.LCTE
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.TR2
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.CL2
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.TreatmentCode
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.TRN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.ISC
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.TAN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.RNO
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.ICN
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.LCTI
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.LCTQ
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.MLPI
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.PUP
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.REL
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.SEC
		{
			get { return ZString.Empty; }
		}

		ZDecimal ICusEntryLine.DRE
		{
			get { return 0M; }
		}

		Money ICusEntryLine.OtherDutyFactor
		{
			get { return null; }
		}

		ZString ICusEntryLine.TCI_InstrumentType
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.TI2_InstrumentType
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.PRI_InstrumentType
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.DXT
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.InstrumentCode
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.InstrumentType
		{
			get { return ZString.Empty; }
		}

		ZString[] ICusEntryLine.OrderedELAs
		{
			get { return System.Array.Empty<ZString>(); }
		}

		Customs.Business.CusContainersInvoiceLinesCollection ICusEntryLine.ContainersPivot
		{
			get { return null; }
		}

		ZString ICusEntryLine.ValuationBasisForCMR
		{
			get { return ZString.Empty; }
		}

		ZInt ICusEntryLine.WRL
		{
			get { return new ZInt(); }
		}

		ZString[] ICusEntryLine.OrderedVIDs
		{
			get { return System.Array.Empty<ZString>(); }
		}

		bool ICusEntryLine.IsExWarehouse
		{
			get { return false; }
		}

		ZString ICusEntryLine.TCI_InstrumentNo
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.TI2_InstrumentNo
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.PRI_InstrumentNo
		{
			get { return ZString.Empty; }
		}

		ZString ICusEntryLine.ConsignorVendor => ZString.Empty;

		ZString ICusEntryLine.WAR => ZString.Empty;
		IEnumerable<ZString> ICusEntryLine.ImportPermitNumbers => Enumerable.Empty<ZString>();

		#endregion
	}
}
