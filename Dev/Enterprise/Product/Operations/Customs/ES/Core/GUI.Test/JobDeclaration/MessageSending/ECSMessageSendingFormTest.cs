using System.Windows.Forms;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ECSMessageSendingForm))]
	public class ECSMessageSendingFormTest : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var exitDetail1 = exitHeader.CusExitDetails.AddNew();
			exitDetail1.CED_Status = "XXX";
			var exitDetail2 = exitHeader.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "XXX";
			return new ECSMessageSendingForm(new ECSExitHeaderMessageSendingObjectParent(new ECSMessageSendingObject(exitHeader, GlbStaff.CurrentUser)));
		}

		public void TestSendToCustomsMenuItemVisible()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var testMenu = new SendMenuForTest(exitHeader);

			UnitTestUserNotification.Instance.ClearMessages();
			AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
		}

		public void TestMessageSendingGridColumns()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var exitDetail1 = exitHeader.CusExitDetails.AddNew();
			exitDetail1.CED_Status = "XXX";
			var exitDetail2 = exitHeader.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "XXX";
			using (var testForm = new SendFormForTest(new ECSExitHeaderMessageSendingObjectParent(new ECSMessageSendingObject(exitHeader, GlbStaff.CurrentUser))))
			{
				testForm.Show();
				AssertEquals(4, testForm.MessageSendingGrid.ColumnStyles.Count);
			}
		}

		class SendFormForTest : ECSMessageSendingForm
		{
			public SendFormForTest(ECSExitHeaderMessageSendingObjectParent exitHeaderWrapper)
				: base(exitHeaderWrapper)
			{
			}

			public ZGrid MessageSendingGrid => base.MessageSendingObjectsGrid;
		}

		class SendMenuForTest : EcsMessageMenuProvider
		{
			public SendMenuForTest(EU.Business.CusExitControlHeader exitHeader) : base(exitHeader)
			{
			}
			public MenuItem sendToCustomsMenuItem => base.CreateMenuItems().FindByText("Arrive at Exit Location");
			protected override ECSMessageSendingForm GetMessageSendingForm(ECSExitHeaderMessageSendingObjectParent exitHeaderWrapper)
				=> new SendFormForTest(exitHeaderWrapper);
		}
	}
}
