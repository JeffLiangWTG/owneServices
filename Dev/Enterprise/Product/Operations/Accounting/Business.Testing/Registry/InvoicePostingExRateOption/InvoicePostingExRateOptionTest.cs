using Enterprise.Accounting.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoicePostingExRateOption))]
	class InvoicePostingExRateOptionTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestRunPreSaveValidation()
		{
			BizObj.InvoiceCurrencyType = "";
			BizObj.ExRateOption = "";

			BizObj.ClearAllNotifications();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.InvoiceCurrencyTypeInfo);
			AssertHasErrors(BizObj.ExRateOptionInfo);
		}

		public void TestValidateInvoiceCurrencyType()
		{
			BizObj.InvoiceCurrencyType = "XXX";
			AssertHasError(BizObj.InvoiceCurrencyTypeInfo, "Enter a valid Invoice Currency Type.");

			BizObj.InvoiceCurrencyType = "";
			AssertHasError(BizObj.InvoiceCurrencyTypeInfo, "Please enter an Invoice Currency Type.");

			BizObj.InvoiceCurrencyType = Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
			AssertNoErrors(BizObj.InvoiceCurrencyTypeInfo);
		}

		public void TestValidateExRateOption()
		{
			BizObj.ExRateOption = "XXX";
			AssertHasError(BizObj.ExRateOptionInfo, "Enter a valid Exchange Rate Option.");

			BizObj.ExRateOption = "";
			AssertHasError(BizObj.ExRateOptionInfo, "Please enter an Exchange Rate Option.");

			BizObj.ExRateOption = AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code;
			AssertNoErrors(BizObj.ExRateOptionInfo);
		}

		public void TestOffSetField()
		{
			BizObj.ExRateOption = AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code;
			BizObj.OffSet = 2;
			AssertNoErrors("positive offset value is allowed", BizObj.OffSetInfo);
			AssertEquals(2, BizObj.OffSet);
			Assert("Offset field is editable when option is not DEF", !BizObj.OffSetInfo.ReadOnly);

			BizObj.OffSet = -3;
			AssertNoErrors("negative offset value is allowed", BizObj.OffSetInfo);
			AssertEquals(-3, BizObj.OffSet);

			BizObj.ExRateOption = AccountingConstants.InvoicePostingExchangeRateOption.Default.Code;
			AssertNoErrors(BizObj.OffSetInfo);
			AssertEquals("offset value is reset when option is set to DEF", 0, BizObj.OffSet);
			Assert("Offset field is read only when option is DEF", BizObj.OffSetInfo.ReadOnly);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, 2);

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals(((InvoicePostingExRateOption)originalBusinessObject).InvoiceCurrencyType, ((InvoicePostingExRateOption)newBusinessObject).InvoiceCurrencyType);
			AssertEquals(((InvoicePostingExRateOption)originalBusinessObject).ExRateOption, ((InvoicePostingExRateOption)newBusinessObject).ExRateOption);
			AssertEquals(((InvoicePostingExRateOption)originalBusinessObject).OffSet, ((InvoicePostingExRateOption)newBusinessObject).OffSet);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected new InvoicePostingExRateOption BizObj => (InvoicePostingExRateOption)base.BizObj;

		#endregion
	}
}
