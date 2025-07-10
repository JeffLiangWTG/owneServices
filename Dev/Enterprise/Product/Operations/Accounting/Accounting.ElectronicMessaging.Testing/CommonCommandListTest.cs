using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Testing
{
	public class CommonCommandListTest : TestCase
	{
		public void TestCommandListCodesConfiguration()
		{
			var assembly = Assembly.Load("Enterprise.Accounting.ElectronicMessaging");
			var commandListTypes = assembly.GetTypes();

			var configuredCodes = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes
				.Cast<ApplicationCodeObj>()
				.FirstOrDefault(x => x.ApplicationCode == ApplicationCodeList.Codes.GlobalElectronicInvoice)
				?.MessageTypes;

			var codeValueList = new HashSet<string>();
			foreach (var type in commandListTypes)
			{
				var codeFields = type.GetFields().Where(f => f.GetCustomAttribute<EInvoiceMessageSubTypeAttribute>() != null);

				var nestedCodesClass = type.GetNestedType("Codes");
				if (nestedCodesClass?.GetCustomAttribute<EInvoiceMessageSubTypeAttribute>() != null)
				{
					codeFields = codeFields.Concat(nestedCodesClass.GetFields());
				}

				foreach (var field in codeFields)
				{
					var codeValue = field.GetValue(null)?.ToString();
					codeValueList.Add(codeValue);
				}
			}

			AssertContainsExactElementsInAnyOrder("All Codes should be set in eHubMessagingRegistry", codeValueList, configuredCodes.Cast<MessageTypeObj>().Select(x => x.MessageSubType.ToString()));
		}
	}
}
