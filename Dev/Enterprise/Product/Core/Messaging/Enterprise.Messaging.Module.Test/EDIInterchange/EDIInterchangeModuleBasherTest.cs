using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIInterchangeModule))]
	sealed class EDIInterchangeModuleBasherTest : ZModuleBasherTest
	{
		public void TestAllowNewEdit()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("AllowNew", false, module.AllowNew);
				AssertEquals("AllowEdit", false, module.AllowEdit);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Messaging.EDIInterchange;
		}

		public void TestResetToQueued()
		{
			InterchangeForTest.EI_ReceiveTransmit = "TRX";
			InterchangeForTest.Factory.Save();

			using (TestEDIInterchangeModule module = new TestEDIInterchangeModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				Application.DoEvents();

				module.Grid.UnSelectAll();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals(1, module.GridCollection.Count);
				module.ClickReset();
				AssertEquals("FOO", InterchangeForTest.EI_Status);
				AssertEquals(EDIMessageModule.SelectAtLeastOneMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				module.Grid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				module.ClickReset();
				AssertEquals("Message status unchanged if 'No' selected", "FOO", InterchangeForTest.EI_Status);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.ClickReset();
				AssertEquals("Message status not changed", "FOO", InterchangeForTest.EI_Status);
			}

			AssertEquals("Pre-req - running as support", true, GlbStaff.CurrentUser.IsSupportUser);
			using (TestEDIInterchangeModule module = new TestEDIInterchangeModule())
			{
				var throwAwayJustToMakeActionsMenuAppear = module.EmbeddedControl;
				AssertNotNull("Menu for support has the reset option", module.ActionsMenuItem.MenuItems.FindByText(EDIMessageModule.ResetStatusToQueuedMenuName));
			}
			var originalLogin = GlbStaff.CurrentUser.GS_LoginName;
			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "Daniel";
				AssertEquals("Pre-req - not running as support", false, GlbStaff.CurrentUser.IsSupportUser);
				using (TestEDIInterchangeModule module = new TestEDIInterchangeModule())
				{
					var throwAwayJustToMakeActionsMenuAppear = module.EmbeddedControl;
					AssertNull("No item", module.ActionsMenuItem.MenuItems.FindByText(EDIMessageModule.ResetStatusToQueuedMenuName));
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalLogin;
			}
		}

		public void TestEdiClientColumnVisibility()
		{
			AssertEdiClientColumnVisibility(true);
		}

		void AssertEdiClientColumnVisibility(bool shouldShow)
		{
			using (TestEDIInterchangeModule module = new TestEDIInterchangeModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				Application.DoEvents();

				AssertEquals("Correct EDI client column visibility", shouldShow, !module.Grid.GetColumnStyle("CommunicationPartyConfig+Party+Name").IsUnavailable);
			}
		}

		EDIInterchange InterchangeForTest
		{
			get
			{
				if (interchange == null)
				{
					interchange = Factory.NewWithValidTestData<EDIInterchange>();
					interchange.EI_Status = "FOO";
					interchange.EI_ReceiveTransmit = "RCV";
					interchange.EI_From = "X";
					interchange.EI_From = "Y";
					interchange.EI_BodyText = "Z";
				}
				return interchange;
			}
		}
		EDIInterchange interchange;

		class TestEDIInterchangeModule : EDIInterchangeModule
		{
			public void ClickReset()
			{
				ResetToQueued_Click(null, null);
			}

			public new BusinessObjectCollection GridCollection
			{
				get { return (BusinessObjectCollection)base.GridCollection; }
			}

			public new ZDisplayGrid Grid
			{
				get { return base.Grid; }
			}

			public new BusinessObject FilterBusinessObject
			{
				get { return base.FilterBusinessObject; }
			}
		}
	}
}
