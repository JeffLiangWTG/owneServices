using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Module.Testing
{
	[TestedType(typeof(MailItemController))]
	public class MailItemControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject result = Factory.NewWithValidTestData(GetBusinessObjectType());
			((MailItem)result).MI_Direction = DirectionList.Codes.Receive;
			Factory.Save();
			return result;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MailItem;
		}

		public void TestModuleId()
		{
			AssertEquals(ModuleIDs.MailItem, new MailItemController().ModuleID);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestGetNewBusinessEntityInLocalFactory()
		{
			var controller = new MailItemControllerForTest();
			var mailItem = (MailItem)controller.GetNewBusinessEntityInLocalFactoryExposed();
			AssertEquals("QUE", mailItem.MI_Status);
			AssertEquals("RCV", mailItem.MI_Direction);
			AssertEquals(new ZDateTime(1987, 12, 11, 1, 2, 3), mailItem.MI_ReceivedDateTime);
			AssertEquals("Pre-req: pop3 address not empty", false, String.IsNullOrEmpty(Env.Registry.MailboxEmailAddress));
			AssertContains(Env.Registry.MailboxEmailAddress, mailItem.AllRecipients);
			AssertContains("X-CreatedByCargoWiseForTesting: true", mailItem.MI_Header);
		}

		class MailItemControllerForTest : MailItemController
		{
			internal IBusiness GetNewBusinessEntityInLocalFactoryExposed()
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}
	}
}
