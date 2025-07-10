using System.Collections.Generic;

namespace Enterprise.Customs.GB.GVMS
{
	public static class GvmsItemReferencePartitions
	{
		public static IEnumerable<string> CustomsReferenceCodes
		{
			get
			{
				yield return GVMSCustomsReference.Codes.AtaCarnet;
				yield return GVMSCustomsReference.Codes.CdsMovementReferenceNumberMrn;
				yield return GVMSCustomsReference.Codes.CdsExportDeclarationUniqueConsignmentReferenceDucr;
				yield return GVMSCustomsReference.Codes.IndirectExportDeclarationEad;
				yield return GVMSCustomsReference.Codes.ChiefImportEntryReferenceNumber;
				yield return GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;
				yield return GVMSCustomsReference.Codes.TirCarnet;
				yield return GVMSCustomsReference.Codes.ExemptGoods;
				yield return GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle;
				yield return GVMSCustomsReference.Codes.ManualTransitProcedure;
			}
		}

		public static IEnumerable<string> TransitReferenceCodes
		{
			get
			{
				yield return GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber;
			}
		}

		public static IEnumerable<string> EidrReferenceCodes
		{
			get
			{
				yield return GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
				yield return GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration;
				yield return GVMSCustomsReference.Codes.OralDeclaration;
				yield return GVMSCustomsReference.Codes.UkCarrier;
			}
		}
	}
}
