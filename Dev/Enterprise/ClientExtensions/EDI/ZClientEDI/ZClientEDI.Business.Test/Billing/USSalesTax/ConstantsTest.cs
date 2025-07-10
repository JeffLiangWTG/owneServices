using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax.Test
{
	public class ConstantsTest : TestCase
	{
		public void TestIntegrationStatusList()
		{
			var list = AvalaraConstants.IntegrationStatus.List();
			var expectedListCodes = new[]
			{
				AvalaraConstants.IntegrationStatus.Codes.Off,
				AvalaraConstants.IntegrationStatus.Codes.Sandbox,
				AvalaraConstants.IntegrationStatus.Codes.Production,
			};
			AssertContainsExactElementsInExactOrder(expectedListCodes, list.GetAllCodes());

			AssertEquals("Off", list.GetDescriptionFromCode(AvalaraConstants.IntegrationStatus.Codes.Off));
			AssertEquals("Sandbox", list.GetDescriptionFromCode(AvalaraConstants.IntegrationStatus.Codes.Sandbox));
			AssertEquals("Production", list.GetDescriptionFromCode(AvalaraConstants.IntegrationStatus.Codes.Production));
		}
	}
}
