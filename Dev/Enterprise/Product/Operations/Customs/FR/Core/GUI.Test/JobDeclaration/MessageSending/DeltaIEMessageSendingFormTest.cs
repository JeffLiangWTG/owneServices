using System.Windows.Forms;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(DeltaIEMessageSendingForm))]
	class DeltaIEMessageSendingFormTest : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new DeltaIEMessageSendingForm(new DeltaIEJobDeclarationMessageSendingObjectParent(declaration));
		}

		public override void TestBashingForm()
		{
			Assert(true);
		}

		public void TestSendDeltaMenuItem()
		{
			var testMenu = new DeltaIESendMenuForTest();

			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = "DI";

			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.sendDeltaIEMessageMenuItem.Visible);
			declaration.JE_DeclarationReference = "B00001001";
			testMenu.sendDeltaIEMessageMenuItem.PerformClick();
			AssertEquals("Declaration B00001001 has no entry – Please generate entries before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.Visible);
			testMenu.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class DeltaIESendFormForTest : DeltaIEMessageSendingForm
		{
			public DeltaIESendFormForTest(DeltaIEJobDeclarationMessageSendingObjectParent declarationWrapper)
				: base(declarationWrapper)
			{
			}

			public ZGrid MessageSendingGrid => base.MessageSendingObjectsGrid;
		}

		class DeltaIESendMenuForTest : EDIMenu
		{
			public new MenuItem sendDeltaIEMessageMenuItem => base.sendDeltaIEMessageMenuItem;
			protected override DeltaIEMessageSendingForm GetDeltaIEMessageSendingForm(DeltaIEJobDeclarationMessageSendingObjectParent decWrapper) => new DeltaIESendFormForTest(decWrapper);
		}
	}
}
