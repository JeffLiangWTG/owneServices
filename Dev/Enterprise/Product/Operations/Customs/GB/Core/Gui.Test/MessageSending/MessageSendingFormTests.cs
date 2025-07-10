using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public class MessageSendingFormTests : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new MessageSendingForm(new JobDeclarationMessageSendingObjectParent(declaration));
		}

		[TestDate(2011, 12, 13)]
		public void TestSendToCDSMenuItem()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var invoice = declaration.Invoices.AddNew();
				var invLine1 = invoice.InvoiceLines.AddNew();
				var invLine2 = invoice.InvoiceLines.AddNew();
				var cei1 = declaration.CustomsEntryInstructions.AddNew();
				cei1.CEI_Style = "H1";
				var cei2 = declaration.CustomsEntryInstructions.AddNew();
				cei2.CEI_Style = "H2";
				invLine1.JI_CEI = cei1.PK;
				invLine2.JI_CEI = cei2.PK;
				var merger = new LineMerger(declaration);
				merger.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 2, declaration.CustomsEntryHeaders.Count);

				var menu = new CDSEDIMenuForTest
				{
					Declaration = declaration
				};
				menu.RefreshMenu();
				var sendToCDSMenuItem = menu.MenuItems.FindByText(CDSEDIMenu.SendToCDS);
				AssertNotNull(sendToCDSMenuItem);
				AssertEquals(true, sendToCDSMenuItem.Visible);
				declaration.JE_GoodsDescription = "ABC";
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendToCDSMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.HasChanges);

				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				menu.RefreshMenu();
				AssertEquals(true, sendToCDSMenuItem.Visible);
				sendToCDSMenuItem.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(MessageSendingFormForTest), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestWaitForResponseCheckBox()
		{
			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_PasswordType = PasswordTypesList.Codes.CDS;
			password.GP_UserID = ".";
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(1);
			password.GP_IssueDate = ZDateTime.UtcNow;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.ZG_Gateway = GatewayList.Codes.CDS;
			var invoice = declaration.Invoices.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "H1";
			invLine1.JI_CEI = cei1.PK;
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupCode = declaration.GetDefaultDataGroupingCode();
			var fullDecCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "10", "00", "056", "Test", "EXP", "EFD");
			invLine1.JI_Procedure = fullDecCpc.FullCodeCurrentPlusPreviousPlusConcession;

			Factory.Save();
			using var form = new JobDeclarationFormForTest(declaration);

			var sendToCDSMenuItem = form.TopLevelMenu.MenuItems.FindByText(CDSEDIMenu.SendToCDS);
			AssertNotNull("Pre-requisite: sendToCDSMenuItem", sendToCDSMenuItem);

			AssertWaitForResponseCheckBox(sendToCDSMenuItem, checkboxStateToTest: true);
			AssertWaitForResponseCheckBox(sendToCDSMenuItem, checkboxStateToTest: false);
		}

		void AssertWaitForResponseCheckBox(MenuItem sendToCDSMenuItem, bool checkboxStateToTest)
		{
			JobDeclarationMessageSendingObjectParent decWrapper = null;

			using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var sendForm = (MessageSendingForm)f;
				var checkbox = sendForm.FindSingleOrDefault<ZCheckBox>("WaitForResponseCheckBox");
				AssertNotNull("Pre-requisite: WaitForResponseCheckBox", checkbox);
				sendForm.Shown += (s, e) =>
				{
					AssertEquals("Pre-requisite: WaitForResponseCheckBox.Visible", expected: true, checkbox.Visible);
					checkbox.Checked = checkboxStateToTest;
					decWrapper = sendForm.DataSource as JobDeclarationMessageSendingObjectParent;
				};
			}))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.ShowDialogsInTest = true;
				sendToCDSMenuItem.PerformClick();
			}
			AssertNotNull("Pre-requisite: decWrapper should be set when the form is shown", decWrapper);

			AssertEquals("LockDeclarationUntilResponseReceived", expected: checkboxStateToTest, decWrapper.LockDeclarationUntilResponseReceived);
		}

		class MessageSendingFormForTest : MessageSendingForm
		{
			public MessageSendingFormForTest(JobDeclarationMessageSendingObjectParent declarationWrapper)
				: base(declarationWrapper)
			{
			}

			public ZGrid MessageSendingGrid => base.MessageSendingObjectsGrid;
		}

		class CDSEDIMenuForTest : CDSEDIMenu
		{
			protected override MessageSendingForm GetMessageSendingForm(JobDeclarationMessageSendingObjectParent decWrapper) => new MessageSendingFormForTest(decWrapper);
		}

		class JobDeclarationFormForTest : JobDeclarationForm
		{
			public JobDeclarationFormForTest(JobDeclaration declaration) : base(declaration) { }

			public new ZMenuItem TopLevelMenu => base.TopLevelMenu;

			protected override IEDIMenu GetNewTopLevelMenuCore() => new CDSEDIMenuForTest();
		}
	}
}
