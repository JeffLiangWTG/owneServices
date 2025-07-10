using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class JordanQRCodeDataProviderTest : TestCaseWithFactory
	{
		public void TestJordanGetQRCodeString()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Jordan))
			{
				var qrString = "AQACAnt9AwVmYWxzZQQGMzczLjg1BQgyNTAwNjEwOAYABwoyMDI1LTA0LTA4CAc0MDU3NjUxCTfYp9mE2LTYsdmD2Kkg2KfZhNi02LHZgtmK2Kkg2YTZhNiu2K/Zhdin2Kog2LDYp9iqINmFMNmFCmBNRVFDSUJOa0QwZmE0bHhuSEowWjlRK2J6SzNHZVdFckw5NWRFVG9IWjN0UGlxTi9BaUFFWVcvOW91Yjd6Q2tEV0tIbnVLRlJGakg5RnhXSHJZZ3E3dXlIUW52WlRnPT0=";
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var authorisationRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice);
				authorisationRecord.AHF_RecordType = "ZZZ";

				Factory.Save();

				var qrCodeDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Jordan) as IQRCodeDataProvider;
				AssertEquals("QR Code String should be empty.", "", qrCodeDataProvider.GetTransactionQRCodeString(invoice));

				authorisationRecord.AHF_VerificationUrl = qrString;

				AssertEquals("QR Code String should not be empty.", qrString, qrCodeDataProvider.GetTransactionQRCodeString(invoice));
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
