using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public class MessageSendingFormTest : MessageSendingObjectFormTest
	{
		public void TestGetBottomSectionUserControl()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_SubStyle = "A";
			var entryHeader1 = (Business.Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction1.PK;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_SubStyle = "A";
			var entryHeader2 = (Business.Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;
			Factory.Save();
			CombineAssertions(() =>
			{
				using (var testForm = new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<MessageSendingFormBottomSectionUserControl>("Use parent control when Export but no entry has ZG_UCC6Version > 0 or ZG_POUSVersion > 0", splitContainer.Panel2.Controls[0]);
				}

				entryHeader1.ZG_UCC6Version = 1;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<Ucc6OrPOUSBottomSectionUserControl>("Use ES control when Export and at least one entry has ZG_UCC6Version > 0", splitContainer.Panel2.Controls[0]);
				}

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<Ucc6OrPOUSBottomSectionUserControl>("Use ES control when Import and at least one entry has ZG_UCC6Version > 0", splitContainer.Panel2.Controls[0]);
				}

				entryHeader1.ZG_UCC6Version = 0;
				entryHeader1.ZG_POUSVersion = 1;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<Ucc6OrPOUSBottomSectionUserControl>("Use ES control when Import and at least one entry has ZG_POUSVersion > 0", splitContainer.Panel2.Controls[0]);
				}

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryHeader1.ZG_POUSVersion = 0;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<MessageSendingFormBottomSectionUserControl>("Use parent control when Export but no entry has ZG_UCC6Version > 0 or ZG_POUSVersion > 0", splitContainer.Panel2.Controls[0]);
				}

				entryHeader1.ZG_POUSVersion = 1;
				Factory.Save();
				using (var testForm = new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
				{
					testForm.Show();
					var splitContainer = testForm.FindSingleOrDefault<KSplitContainer>("WarningSplitContainer");
					AssertType<Ucc6OrPOUSBottomSectionUserControl>("Use ES control when Export and at least one entry has ZG_POUSVersion > 0", splitContainer.Panel2.Controls[0]);
				}
			});
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new MessageSendingForm(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser)));
		}

		public override void TestBashingForm()
		{
			//base.TestBashingForm();
			Assert(true);
		}

		public void TestSendToCustomsMenuItem()
		{
			var testMenu = new SendMenuForTest();

			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);
			testMenu.sendToCustomsMenuItem.PerformClick();
			AssertEquals("You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.Visible);
			testMenu.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}
		public void TestMessageSendingGridColumns()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			Factory.Save();
			using (var testForm = new SendFormForTest(new Business.MessageSending.JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser))))
			{
				testForm.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("Ref No.", testForm.MessageSendingGrid.Columns["CH_BGMReference"]);
					AssertNotNull("Message Type", testForm.MessageSendingGrid.Columns["MessageType"]);
					AssertNotNull("Message Sub Type", testForm.MessageSendingGrid.Columns["MessageSubType"]);
					AssertNotNull("Message Status", testForm.MessageSendingGrid.Columns["MessageStatus"]);
					AssertNotNull("MRN", testForm.MessageSendingGrid.Columns["MRN"]);

					var entryType = testForm.MessageSendingGrid.Columns["DeclarationType"];
					AssertNotNull("Entry Type", entryType);
					AssertEquals("Entry type", entryType.ToString());
					AssertEquals(8, testForm.MessageSendingGrid.ColumnStyles.Count);
				});
			}
		}

		class SendFormForTest : MessageSendingForm
		{
			public SendFormForTest(Business.MessageSending.JobDeclarationMessageSendingObjectParent declarationWrapper)
				: base(declarationWrapper)
			{
			}

			public ZGrid MessageSendingGrid => base.MessageSendingObjectsGrid;
		}

		class SendMenuForTest : EDIMenu
		{
			public new MenuItem sendToCustomsMenuItem => base.sendToCustomsMenuItem;
			protected override MessageSendingForm GetMessageSendingForm(Business.MessageSending.JobDeclarationMessageSendingObjectParent decWrapper)
				=> new SendFormForTest(decWrapper);
		}
	}
}
