using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	class RegistryChangeLoggerTest : TestCase
	{
		public void TestGetLogReference()
		{
			var originalElements = new List<IRegistryWithLogs>();
			var newElements = new List<IRegistryWithLogs>();

			var identifierForChanged = ZGuid.NewZGuid();
			var identifier2ForDeleted = ZGuid.NewZGuid();
			var identifier3ForAdded = ZGuid.NewZGuid();
			var identifierForNoAnyChange = ZGuid.NewZGuid();

			originalElements.Add(new RegistryConfigurationSetupLogDummy() { Identifier = identifierForChanged, Number = "111" });
			newElements.Add(new RegistryConfigurationSetupLogDummy() { Identifier = identifierForChanged, Number = "1112" });

			originalElements.Add(new RegistryConfigurationSetupLogDummy() { Identifier = identifier2ForDeleted, Number = "222" });

			newElements.Add(new RegistryConfigurationSetupLogDummy() { Identifier = identifier3ForAdded, Number = "333" });

			originalElements.Add(new RegistryConfigurationSetupLogDummy() { Identifier = identifierForNoAnyChange, Number = "444" });
			newElements.Add(new RegistryConfigurationSetupLogDummy() { Identifier = identifierForNoAnyChange, Number = "444" });

			var logReference = RegistryChangeLogger.GetLogReference(originalElements, newElements);

			AssertEquals(@"Edit
Add
Delete
", logReference);
		}
	}

	class RegistryConfigurationSetupLogDummy : IRegistryWithLogs
	{
		public ZGuid Identifier { get; set; }

		public string Number { get; set; }

		public bool IsSameItem(IRegistryWithLogs item) => Identifier == (item as RegistryConfigurationSetupLogDummy).Identifier;

		public bool IsEqual(IRegistryWithLogs newElement)
		{
			if (Number != (newElement as RegistryConfigurationSetupLogDummy).Number)
			{
				return false;
			}
			return true;
		}

		public string GetLogText(RegistryChangeLogger.EventType eventType)
		{
			return eventType.ToString();
		}
	}
}