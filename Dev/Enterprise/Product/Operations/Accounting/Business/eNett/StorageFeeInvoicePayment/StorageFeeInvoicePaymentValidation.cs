using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.eNett_Integration
{
	public class StorageFeeInvoicePaymentValidation : AutoStorageFeeInvoicePaymentValidation
	{
		public StorageFeeInvoicePaymentValidation(AutoStorageFeeInvoicePayment parent)
			: base(parent) { }

		protected override void CheckPortCode()
		{
			base.CheckPortCode();
			MandatoryValidation.CheckEntered(Parent.PortCodeInfo);
		}

		protected override void CheckPickupDate()
		{
			base.CheckPickupDate();
			MandatoryValidation.CheckEntered(Parent.PickupDateInfo);
		}

		#region Implementation

		public new StorageFeeInvoicePayment Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (StorageFeeInvoicePayment)base.Parent; }
		}

		#endregion
	}
}
