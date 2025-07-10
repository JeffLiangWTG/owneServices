using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class ImmediateDeliveryValidation : CusCodeDataValidation
	{
		public ImmediateDeliveryValidation(ImmediateDelivery parent)
			: base(parent)
		{
		}

		new ImmediateDelivery Parent => (ImmediateDelivery)base.Parent;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			var invoiceLine = Parent.Parent as JobComInvoiceLine;
			if (invoiceLine != null && invoiceLine.IsImport)
			{
				foreach (var immadiateDelivery in invoiceLine.ImmediateDeliveries)
				{
					if (immadiateDelivery.PK != Parent.PK && immadiateDelivery.CY_Data == Parent.CY_Data)
					{
						Parent.CY_DataInfo.AddMessageError(Res.GetString("CE9518F2-D034-4909-A015-7CC36B5A2442", "You cannot enter an 'Immediate Delivery Number' that already exists."));
						break;
					}
				}
			}
		}
	}
}
