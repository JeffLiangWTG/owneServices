using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientFaxPriceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCFP_RX_NKCurrencyCode()
		{
			clientFaxPrice.CFP_RX_NKCurrencyCode = "";
			Assert("CFP_RX_NKCurrencyCode is empty, expecting error", clientFaxPrice.CFP_RX_NKCurrencyCodeInfo.HasErrors());

			clientFaxPrice.CFP_RX_NKCurrencyCode = "ZZZ";
			Assert("CFP_RX_NKCurrencyCode does not exist, expecting error", clientFaxPrice.CFP_RX_NKCurrencyCodeInfo.HasErrors());

			clientFaxPrice.CFP_RX_NKCurrencyCode = "AUD";
			ClientFaxPrice clientFaxPrice2 = Factory.New<ClientFaxPrice>();
			clientFaxPrice2.CFP_RX_NKCurrencyCode = "AUD";
			Assert("CFP_RX_NKCurrencyCode is not unique, expecting error", clientFaxPrice2.CFP_RX_NKCurrencyCodeInfo.HasErrors());

			clientFaxPrice.Delete();
			Factory.Save();

			clientFaxPrice2.Validation.ValidateCFP_RX_NKCurrencyCode();
			Assert("CFP_RX_NKCurrencyCode is unique, not expecting error", !clientFaxPrice2.CFP_RX_NKCurrencyCodeInfo.HasNotifications());

			ClientFaxPrice clientFaxPrice3 = Factory.New<ClientFaxPrice>();
			clientFaxPrice3.CFP_RX_NKCurrencyCode = "AUD";
			Assert("CFP_RX_NKCurrencyCode is not unique, expecting error", clientFaxPrice3.CFP_RX_NKCurrencyCodeInfo.HasErrors());
		}

		public void TestValidateCFP_PageRate()
		{
			clientFaxPrice.CFP_PageRate = 0m;
			Assert("CFP_PageRate is 0, expecting error", clientFaxPrice.CFP_PageRateInfo.HasErrors());

			clientFaxPrice.CFP_PageRate = -0.1m;
			Assert("CFP_PageRate is negative, expecting error", clientFaxPrice.CFP_PageRateInfo.HasErrors());

			clientFaxPrice.CFP_PageRate = 0.1m;
			Assert("CFP_PageRate is positive, not expecting error", !clientFaxPrice.CFP_PageRateInfo.HasNotifications());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			clientFaxPrice = Factory.New<ClientFaxPrice>();
		}

		ClientFaxPrice clientFaxPrice;
		#endregion
	}
}