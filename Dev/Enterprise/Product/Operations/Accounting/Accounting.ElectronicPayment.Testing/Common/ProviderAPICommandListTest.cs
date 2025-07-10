using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class ProviderAPICommandListTest : TestCaseWithFactory
	{
		public void TestAPICommandList()
		{
			var expectedAPICodes = new[]
			{
				"GRT",
				"GAQ",
				"PAY",
				"SBN"
			};

			var providerApiCommandList = new GEPProviderAPICommandList();
			var codes = providerApiCommandList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(expectedAPICodes, codes);

			var expectedAPIDescriptions = new List<string>()
			{
				"Get Rates",
				"Get A Quote",
				"Create A Deal",
				"Search Beneficiary"
			};

			foreach (var code in codes)
			{
				var description = providerApiCommandList.GetDescriptionFromCode(code);
				expectedAPIDescriptions.Contains(description);
			}
		}

		public void TestGEPProviderAPICommandListWithEHubMessagingRegistry()
		{
			var gEPApplicationCodeObj = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes.Cast<ApplicationCodeObj>().FirstOrDefault(x => x.ApplicationCode == ApplicationCodeList.Codes.GlobalElectronicPayment);
			var expectedAPICodes = gEPApplicationCodeObj.MessageTypes;

			var providerApiCommandList = new GEPProviderAPICommandList();
			var codes = providerApiCommandList.GetAllCodes();

			AssertEquals("The number of Codes in GEPProviderAPICommandList should be equal with eHubMessagingRegistry", expectedAPICodes.Count, codes.Length);
			AssertContainsExactElementsInAnyOrder("All Codes in GEPProviderAPICommandList should be set in eHubMessagingRegistry", codes, expectedAPICodes.Cast<MessageTypeObj>().Select(x => x.MessageSubType));
		}
	}
}
