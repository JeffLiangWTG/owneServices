using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class QuantityPerUnitInfoValidation : CusSupportingInfoValidation
	{
		public QuantityPerUnitInfoValidation(QuantityPerUnitInfo parent)
			: base(parent)
		{
		}

		public new QuantityPerUnitInfo Parent => (QuantityPerUnitInfo)base.Parent;

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();

			var invoiceLine = Parent.Parent;
			if (invoiceLine != null && invoiceLine.IsImportSiscomex && !Parent.CSI_UnitOfQuantity.IsEmpty)
			{
				var (descInEn, descInPt) = Parent.Factory.GetInvoiceUQDescriptions(invoiceLine.Lookups.InvoiceUQList, Parent.CSI_UnitOfQuantity);
				if (!descInEn.IsEmpty && !descInPt.IsEmpty && descInEn == descInPt)
				{
					Parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("87C5B891-E369-4082-9630-8FDFECE74171", "Unity Of Measure- UQ Portuguese description is the same as English Description"));
				}
			}
		}
	}
}
