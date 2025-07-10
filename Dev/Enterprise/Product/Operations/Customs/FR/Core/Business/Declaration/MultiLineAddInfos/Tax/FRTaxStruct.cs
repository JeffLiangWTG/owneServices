using System.Globalization;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class FRTaxStruct : TaxStruct
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void AddTax(EU.Business.Declaration.CusEntryHeaderCharges charge)
		{
			G4_Type = charge.C1_ChargeType.Substring(0, 3);
			G4_Amount_InDeclarationCurrency = charge.C1_ChargeAmount.ToString("0.00", CultureInfo.CurrentCulture);
		}
	}
}
