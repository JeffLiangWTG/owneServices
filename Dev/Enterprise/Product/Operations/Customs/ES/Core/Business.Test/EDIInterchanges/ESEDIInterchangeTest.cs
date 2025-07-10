using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.EDIInterchanges.Testing
{
	[TestedType(typeof(ESEDIInterchange))]
	public class ESEDIInterchangeTest : EDIInterchangeTest
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				var interchange = Factory.New<ESEDIInterchange>();
				AssertEquals("EI_ApplicationCode", "ESC", interchange.EI_ApplicationCode);
				AssertEquals("ShouldSendViaEHubCore", true, interchange.ShouldSendViaEHub);
			});
		}

		public void TestShouldSendViaeHub()
		{
			CombineAssertions(() =>
			{
				var interchange = Factory.New<ESEDIInterchange>();
				AssertEquals("EI_ApplicationCode", "ESC", interchange.EI_ApplicationCode);
				AssertEquals("ShouldSendViaEHub - eHub", true, interchange.ShouldSendViaEHub);

				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				AssertEquals("ShouldSendViaEHub - DirectxT", false, interchange.ShouldSendViaEHub);
			});
		}

		public void TestResendInterchange()
		{
			CombineAssertions(() =>
			{
				var interchange = Factory.NewWithValidTestData<ESEDIInterchange>();
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				Factory.Save();

				var result = InterchangeResender.GetInstance(interchange).Resend();
				AssertEquals("Interchange not resent", false, result);
				interchange.Reload();
				AssertEquals("Interchange status is empty, only resend when SNT", ZString.Empty, interchange.EI_Status);

				interchange.EI_Status = EDIInterchange.Status.Sent;
				result = InterchangeResender.GetInstance(interchange).Resend();
				AssertEquals("Interchange resent", true, result);
				interchange.Reload();
				AssertEquals("Interchange status is HQU, only resend when SNT", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			});
		}

		public void TestResendInterchange_DirectxT()
		{
			CombineAssertions(() =>
			{
				var interchange = Factory.NewWithValidTestData<ESEDIInterchange>();
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				Factory.Save();

				var result = InterchangeResender.GetInstance(interchange).Resend();
				AssertEquals("Interchange not resent", false, result);
				interchange.Reload();
				AssertEquals("Interchange status is empty, resetting not happening", ZString.Empty, interchange.EI_Status);

				interchange.EI_Status = EDIInterchange.Status.Sent;
				result = InterchangeResender.GetInstance(interchange).Resend();
				AssertEquals("Interchange resent", true, result);
				interchange.Reload();
				AssertEquals("Interchange status is QUE", EDIInterchange.Status.Queued, interchange.EI_Status);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() => Factory.New<ESEDIInterchange>();
	}
}
