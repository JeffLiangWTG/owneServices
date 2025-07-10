using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	/// <summary>
	/// Common interface between JobComINvoiceLineTax and CusAddInfo<Tax_CusAddInfoOnlyForPivot> (ie. under a pivot)
	/// </summary>
	public interface IEuTax
	{
		// These bits needed for a Tax/Docs calculator:
		void Delete();
		ZBool IsCopying { get; }
		ZString CountryCode { get; }
		MultiLineAddInfos.ICanBeImportOrExport ImportExportParent { get; }
		BusinessObjectFactory Factory { get; }

		// These bits needed to provide a common interface for both JobComInvoiceLineTax and CusAddInfo<Tax_CusAddINfoOnlyForPivot>
		ZDecimal G4_CalculatedPercentage { get; }
		ZString G4_MethodOfPayment { get; set; }
		ZString G4_RateDuty { get; set; }
		ZString G4_RateOverride { get; set; }
		ZString G4_RateSuspension { get; set; }
		ZString G4_Type { get; set; }
		ZString G4_Amount { get; set; }
		ZDecimal G4_BaseAmount { get; set; }
		ZDecimal G4_BaseQuantity { get; set; }
		ZString G4_BaseQuantityUQ { get; set; }
		ZPropertyInfo G4_MethodOfPaymentInfo { get; }
		ZPropertyInfo G4_RateDutyInfo { get; }
		ZPropertyInfo G4_RateOverrideInfo { get; }
		ZPropertyInfo G4_RateSuspensionInfo { get; }
		ZPropertyInfo G4_TypeInfo { get; }
		ZPropertyInfo G4_AmountInfo { get; }
		ZPropertyInfo G4_BaseAmountInfo { get; }
	}
}
