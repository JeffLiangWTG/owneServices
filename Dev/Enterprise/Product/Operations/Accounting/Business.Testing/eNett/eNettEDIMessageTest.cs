using System.Linq;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(eNettEDIMessage))]
	public class eNettEDIMessageTest : EDIMessageTest
	{
		public void TestENettMessageSubTypeListWithEHubMessagingRegistry()
		{
			var eNettApplicationCodeObj = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes.Cast<ApplicationCodeObj>().FirstOrDefault(x => x.ApplicationCode == ApplicationCodeList.Codes.eNett);
			var expectedAPICodes = eNettApplicationCodeObj.MessageTypes;

			var eNettMessageSubTypeList = new eNettMessageSubTypeList();
			var codes = eNettMessageSubTypeList.GetAllCodes();

			AssertEquals("The number of Codes in eNettMessageSubTypeList should be equal with eHubMessagingRegistry", expectedAPICodes.Count, codes.Length);
			AssertContainsExactElementsInAnyOrder("All Codes in eNettMessageSubTypeList should be set in eHubMessagingRegistry", codes, expectedAPICodes.Cast<MessageTypeObj>().Select(x => x.MessageSubType.ToString()));
		}
	}
}
