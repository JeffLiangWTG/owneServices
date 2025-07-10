using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	class CommonImportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public CommonImportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		#region CheckJZ_RX_NKInvoice_Currency

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_RX_NKInvoice_CurrencyInfo);
		}

		#endregion

		#region CheckJZ_RW_NKOriginState

		protected override void CheckJZ_RW_NKOriginState()
		{
			base.CheckJZ_RW_NKOriginState();
			if (Parent.JZ_RN_NKDefaultOrigin == Core.Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_RW_NKOriginStateInfo, Parent.AddInfoLookups.StatesOfOrigin);
			}
		}

		#endregion
	}
}
