using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ICustomsWareRegistry = Enterprise.Integration.Customs.CustomsWare.ICustomsWareRegistry;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(CustomsWareRegistry))]
	class RegistryTest : RegistryItemSetTestCaseWithFactory<CustomsWareRegistry>
	{
		public void TestSubmitOutOfLine()
		{
			TestGenericRegistryItem(ItemSet.SubmitOutOfLine, "SubmitOutOfLine", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService, "Submit Declaration out of line", "Submit Declaration out of line", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, true);
		}

		[ExpectNoExceptions]
		public void TestICustomsWareRegistry_SubmitOutOfLine()
		{
			NUnit.Framework.Assert.That(((ICustomsWareRegistry)ItemSet).SubmitOutOfLine, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestICustomsWareRegistry_SubmitOutOfLine_false()
		{
			using (ItemSet.SubmitOutOfLine.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				NUnit.Framework.Assert.That(((ICustomsWareRegistry)ItemSet).SubmitOutOfLine, Is.EqualTo(false));
			}
		}

		public void TestSendNotifications()
		{
			TestGenericRegistryItem(ItemSet.SendNotifications, "CustomsWareSendNotifications", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare, "Send Response Notifications", "If ticked, A notification email will be sent for each successfully processed response.", RegistryStorageFlags.Company, true);
		}

		public void TestSiteID()
		{
			TestGenericRegistryItem(ItemSet.CustomsWareSiteID, "CustomsWareSiteID", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare, "Site ID", "Site ID", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter);
		}

		public void TestURI()
		{
			TestStringRegistryItem(ItemSet.URI, "CustomsWareURI", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService, "URI", "URI of web service", RegistryStorageFlags.System, TextEditorType.TextBox, RegistryOptions.Default, "http://83.141.75.90:1120/CustomsForceWebService.asmx", CharacterCase.Normal);
		}

		public void TestUserName()
		{
			TestGenericRegistryItem(ItemSet.UserName, "CustomsWareUserName", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService, "User Name", "User Name", RegistryStorageFlags.Company);
		}

		public void TestPassword()
		{
			TestGenericRegistryItem(ItemSet.Password, "CustomsWarePassword", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService, "Password", "Password", RegistryStorageFlags.Company);
		}

		public void TestApplicationID()
		{
			TestStringRegistryItem(ItemSet.ApplicationID, "CustomsWareApplicationID", CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService, "Application ID", "Application ID", RegistryStorageFlags.System, TextEditorType.TextBox, RegistryOptions.Default, "CWAPIEXTERNAL", CharacterCase.Normal);
		}
	}
}
