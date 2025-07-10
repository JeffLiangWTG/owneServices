using System;
using System.Linq;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageSending.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	class EcsMessageMenuProviderTest : EU.GUI.PlugIn.Testing.EcsMessageMenuProviderTest
	{
		protected override Type ExpectEcsMessageMenuProviderType => typeof(EcsMessageMenuProvider);

		public override void TestCreateArrivalMessages()
		{
			var arrMenuItem = menuItems.FindByText("Arrive at Exit Location");

			arrMenuItem.PerformClick();
			AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);

			ECSMessageSenderTest.AssertECSMessageCreated<ECSArrivalFREDIMessage>(exitDetail1.Messages[0], "<schemaID>MessageIE507</schemaID>", "ARR");
			ECSMessageSenderTest.AssertECSMessageCreated<ECSArrivalFREDIMessage>(exitDetail2.Messages[0], "<schemaID>MessageIE507</schemaID>", "ARR");

			exitHeader.CusExitDetails.RemoveAndDeleteAll();

			arrMenuItem.PerformClick();
			AssertEquals("There is no movements to be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override void TestCreateDepartureMessages()
		{
			var depMenuItem = menuItems.FindByText("Depart from Exit Location");

			depMenuItem.PerformClick();
			AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);

			ECSMessageSenderTest.AssertECSMessageCreated<ECSDepartureFREDIMessage>(exitDetail1.Messages[0], "<schemaID>MessageIE618</schemaID>", "DEP");
			ECSMessageSenderTest.AssertECSMessageCreated<ECSDepartureFREDIMessage>(exitDetail2.Messages[0], "<schemaID>MessageIE618</schemaID>", "DEP");

			exitHeader.CusExitDetails.RemoveAndDeleteAll();

			depMenuItem.PerformClick();
			AssertEquals("There is no movements to be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		CusExitControlHeader exitHeader;
		CusExitDetail exitDetail1;
		CusExitDetail exitDetail2;
		EcsMessageMenuProvider menuProvider;
		ZMenuItem[] menuItems;

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitDetail1 = exitHeader.CusExitDetails.AddNew();
			exitDetail1.CED_Status = "XXX";
			exitDetail2 = exitHeader.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "XXX";

			menuProvider = new EcsMessageMenuProvider(exitHeader);
			menuItems = menuProvider.CreateMenuItems().ToArray();
		}
	}
}
