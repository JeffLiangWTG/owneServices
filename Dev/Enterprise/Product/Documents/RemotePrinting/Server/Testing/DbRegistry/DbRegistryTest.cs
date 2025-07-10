using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class DbRegistryTest : TransactionedTestCase
	{
		const string SMTPDefaultReturnEmailAddress = "SMTPDefaultReturnEmailAddress";
		const string WebPrintNotificationGroup = "WebPrintNotificationGroup";
		const string WebServicePassword = "WebServicePassword";
		const string WebServiceUsername = "WebServiceUsername";

		public void TestDbRegistry()
		{
			SetValue(SMTPDefaultReturnEmailAddress, "my@email.com");
			AssertEquals(SMTPDefaultReturnEmailAddress, "my@email.com", RegistryData.SMTPDefaultReturnEmailAddress(TestConnection));

			var webPrintGroup = Guid.NewGuid();
			SetValue(WebPrintNotificationGroup, webPrintGroup);
			AssertEquals(WebPrintNotificationGroup, webPrintGroup, RegistryData.WebPrintNotificationGroup(TestConnection));

			SetValue(WebServiceUsername, "Barry");
			AssertEquals(WebServiceUsername, "Barry", RegistryData.WebServiceUsername(TestConnection));

			SetValue(WebServicePassword, "Bazza");
			AssertEquals(WebServicePassword, "Bazza", RegistryData.WebServicePassword(TestConnection));
		}

		public void TestWebPrintNotificationGroupRegistryItem()
		{
			WebPrintNotificationGroupRegistryItem regItem = new WebPrintNotificationGroupRegistryItem();
			AssertEquals("Default Value", NotificationDataRegistry.Instance.WebPrintNotificationGroup.Value, regItem.LoadValue(TestConnection));

			Guid testGuid1 = Guid.NewGuid();
			regItem.SaveValue(testGuid1, TestConnection);
			NotificationDataRegistry.Instance.WebPrintNotificationGroup.Inner.ClearCache();
			AssertEquals("Local Registry Value 1", testGuid1, regItem.LoadValue(TestConnection));
			AssertEquals("Enterprise Registry Value 1", testGuid1, NotificationDataRegistry.Instance.WebPrintNotificationGroup.Value);

			Guid testGuid2 = Guid.NewGuid();
			NotificationDataRegistry.Instance.WebPrintNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testGuid2);
			AssertEquals("Local Registry Value 2", testGuid2, regItem.LoadValue(TestConnection));
			AssertEquals("Enterprise Registry Value 2", testGuid2, NotificationDataRegistry.Instance.WebPrintNotificationGroup.Value);
		}

		public void TestSMTPDefaultReturnEmailAddressRegistryItem()
		{
			SMTPDefaultReturnEmailAddressRegistryItem regItem = new SMTPDefaultReturnEmailAddressRegistryItem();
			AssertEquals("Default Value", EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress, regItem.LoadValue(TestConnection));

			// Set MailboxEmailAddress Enterprise Registry Value (fallback default value)
			EnvProxy.Instance.Registry.MailboxEmailAddress = "testfallback@pop3.registry";
			AssertEquals("Local Registry Fallback Default", "testfallback@pop3.registry", regItem.LoadValue(TestConnection));

			// Set Local Registry Value
			regItem.SaveValue("testemail1@remoteprinting.server", TestConnection);
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals("Local Registry Value 1", "testemail1@remoteprinting.server", regItem.LoadValue(TestConnection));
			AssertEquals("Enterprise Registry Value 1", "testemail1@remoteprinting.server", EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress);

			// Set Enterprise Registry Value
			EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress = "testemail2@smtp.registry";
			AssertEquals("Local Registry Value 2", "testemail2@smtp.registry", regItem.LoadValue(TestConnection));
			AssertEquals("Enterprise Registry Value 2", "testemail2@smtp.registry", EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress);
		}

		public void TestWebPrintDocumentPackMaxSize()
		{
			DocumentsDataRegistry.Instance.WebPrintDocumentPackMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0, RegistryData.WebPrintDocumentPackMaxSize(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintDocumentPackMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, RegistryData.WebPrintDocumentPackMaxSize(TestConnection));
		}

		public void TestWebPrintSignalRIncomingMaxSize()
		{
			AssertEquals("Default value", 100, RegistryData.WebPrintSignalRIncomingMaxSize(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintSignalRIncomingMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0, RegistryData.WebPrintSignalRIncomingMaxSize(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintSignalRIncomingMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, RegistryData.WebPrintSignalRIncomingMaxSize(TestConnection));
		}

		public void TestWebPrintAllowDirectPrintPrintPushNotification()
		{
			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, RegistryData.WebPrintAllowDirectPrintPrintPushNotification(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, RegistryData.WebPrintAllowDirectPrintPrintPushNotification(TestConnection));
		}

		public void TestWebServiceAlternativeCredentials()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "xxx");
			list.AddPair("bbb", "yyy");
			list.AddPair("ccc", "zzz");

			WebDataRegistry.Instance.WebServiceAlternativeCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(list));

			var list2 = RegistryData.WebServiceAlternativeCredentials(TestConnection);

			AssertEquals(3, list2.Count);
			AssertEquals("xxx", list2.GetDescriptionFromCode("aaa"));
			AssertEquals("yyy", list2.GetDescriptionFromCode("bbb"));
			AssertEquals("zzz", list2.GetDescriptionFromCode("ccc"));
		}

		public void TestWebServiceCredentialsCacheTime()
		{
			AssertEquals("Default value", 15, RegistryData.WebServiceCredentialsCacheTime(TestConnection));

			WebDataRegistry.Instance.WebServiceCredentialsCacheTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			AssertEquals(5, RegistryData.WebServiceCredentialsCacheTime(TestConnection));

			WebDataRegistry.Instance.WebServiceCredentialsCacheTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0, RegistryData.WebServiceCredentialsCacheTime(TestConnection));
		}

		public void TestWebPrintForceToUseHTTPSForWebPrintRequests()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("IsHostedWithCargowise", true, EnvProxy.IsHostedWithCargowise);

			AssertEquals("Default value", true, RegistryData.WebPrintForceToUseHTTPSForWebPrintRequests(TestConnection));

			DocumentsDataRegistry.Instance.WebPrintForceToUseHTTPSForWebPrintRequests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should be false", false, RegistryData.WebPrintForceToUseHTTPSForWebPrintRequests(TestConnection));
		}

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}

		public void SetValue<T>(string name, T value)
		{
			if (!(value is string || value is Guid))
			{
				throw new ArgumentException("Only know strings and guids");
			}

			var regItem = new SimpleRegistryItem<T>(name, value is string ? "STR" : "GID");
			regItem.SaveValue(value, TestConnection);
		}

		class SimpleRegistryItem<T> : BaseDbRegistryItem<T>
		{
			public SimpleRegistryItem(string itemName, string typeCode)
			{
				ItemName = itemName;
				TypeCode = typeCode;
			}

			protected override T DefaultValue { get; }

			public override string ItemName { get; }
			protected override string TypeCode { get; }

			protected override byte[] GetBytesFromValue(T value)
			{
				return Encoding.Unicode.GetBytes(value.ToString());
			}

			protected override T GetValueFromBytes(byte[] binaryValue)
			{
				throw new NotSupportedException("Not needed for this test.");
			}
		}
	}
}
