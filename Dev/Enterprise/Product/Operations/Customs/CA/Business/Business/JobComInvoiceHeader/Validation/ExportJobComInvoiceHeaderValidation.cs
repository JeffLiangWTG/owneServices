using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class ExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo);
		}

		#region CheckJZ_RW_NKOriginState

		protected override void CheckJZ_RW_NKOriginState()
		{
			base.CheckJZ_RW_NKOriginState();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_RW_NKOriginStateInfo, Parent.AddInfoLookups.StatesOfOrigin);
		}

		#endregion
	}
}
