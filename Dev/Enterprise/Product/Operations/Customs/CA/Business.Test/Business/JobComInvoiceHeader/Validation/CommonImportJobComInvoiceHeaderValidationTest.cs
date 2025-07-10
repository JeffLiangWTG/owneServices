using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.CA.Business.Testing
{
	abstract class CommonImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);
		}

		#region TestCheckJZ_RW_NKOriginState

		public void TestCheckJZ_RW_NKOriginState()
		{
			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_RW_NKOriginStateInfo);

			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceHeader.JZ_RW_NKOriginStateInfo, "12", USStatesList.Codes.Wyoming);
		}

		#endregion

		protected new JobComInvoiceHeader invoiceHeader
		{
			get { return (JobComInvoiceHeader)base.invoiceHeader; }
		}

		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			return invoiceHeader;
		}

		protected abstract ZString GetMessageType();

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = GetMessageType();
		}
	}
}
