using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class ImportInvoiceLineChargeLookups : EU.Business.Declaration.InvoiceLineChargeLookups
	{
		public ImportInvoiceLineChargeLookups(InvoiceLineCharge parent) : base(parent)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue("IE.ImportInvoiceLineChargeLookups.ChargeTypeList", delegate
		{
			var list = new AISChargeCodeList();
			list.RemoveCode(AISChargeCodeList.Codes._1X);
			list.RemoveCode(AISChargeCodeList.Codes.BA);
			list.RemoveCode(AISChargeCodeList.Codes.AK);
			return list;
		});
	}
}
