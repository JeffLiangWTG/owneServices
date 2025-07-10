using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	sealed class Phase5MessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestGetProvider()
		{
			AssertType<Phase5MessagingMenuProvider>("Provider Type", Phase5MessagingMenuProvider.GetProvider(header));
		}

		const string QueryOnGuaranteeMenuItemText = "Query on Guarantee";
		const string GuaranteeAccessCodesMenuItemText = "Guarantee Access Codes";

		public void TestQueryOnGuaranteeIsUnavailable_WhenArrival()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var guarantee1 = header.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var provider = new Phase5MessagingMenuProvider(header);

			Assert("Query on Guarantee menu item is unavailable when Arrival", !provider.CreateMenuItems().Select(x => x.Text).Contains("Query on Guarantee"));
		}

		public void TestGuaranteeAccessCodesIsUnavailable_WhenArrival()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var guarantee1 = header.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var provider = new Phase5MessagingMenuProvider(header);

			Assert("Guarantee Access Codes menu item is unavailable when Arrival", !provider.CreateMenuItems().Select(x => x.Text).Contains("Guarantee Access Codes"));
		}

		public void TestCreateMenuItems()
		{
			Assert("Query on Guarantee menu item is present", provider.CreateMenuItems().Select(x => x.Text).Contains(QueryOnGuaranteeMenuItemText));
			Assert("Guarantee Access Codes menu item is present", provider.CreateMenuItems().Select(x => x.Text).Contains(GuaranteeAccessCodesMenuItemText));
		}

		public void TestRefreshMenu()
		{
			provider.RefreshMenu();
			Assert("Query on Guarantee menu item is visible and present", menuItems.Where(x => x.Visible).Select(x => x.Text).Contains(QueryOnGuaranteeMenuItemText));
			Assert("Guarantee Access Codes menu item is visible and present", menuItems.Where(x => x.Visible).Select(x => x.Text).Contains(GuaranteeAccessCodesMenuItemText));
		}

		public void TestSendToCustoms_CredentialCheck_NoValidCredential()
		{
			CredentialCheck_NoValidCredential("Send to Customs");
		}

		public void TestSendToCustoms_CredentialCheck_ExpiredCredential()
		{
			CredentialCheck_ExpiredCredential("Send to Customs");
		}

		[RequiresSTA]
		public void TestSendToCustoms_CredentialCheck_NoCredential()
		{
			CredentialCheck_NoCredential("Send to Customs");
		}

		public void TestQueryOnGuarantee_PreSaveInvoked()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(QueryOnGuaranteeMenuItemText);

				AssertPreSaveInvoked(queryOnGuaranteeMenuItem);
			}
		}

		public void TestQueryOnGuarantee_CredentialCheck_NoValidCredential()
		{
			CredentialCheck_NoValidCredential(QueryOnGuaranteeMenuItemText);
		}

		[RequiresSTA]
		public void TestQueryOnGuarantee_CredentialCheck_ExpiredCredential()
		{
			CredentialCheck_ExpiredCredential(QueryOnGuaranteeMenuItemText);
		}

		public void TestQueryOnGuarantee_CredentialCheck_NoCredential()
		{
			CredentialCheck_NoCredential(QueryOnGuaranteeMenuItemText);
		}

		[RequiresSTA]
		public void TestQueryOnGuarantee_JobHasNoGuarantee()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(QueryOnGuaranteeMenuItemText);

				const string expectedErrorMessage = "Please provide Guarantee details in the Guarantees Grid to send Query message.";

				AssertJobHasNoGuaranteeErrorMessage(queryOnGuaranteeMenuItem, expectedErrorMessage);
			}
		}

		public void TestQueryOnGuarantee_OpenedForm()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(QueryOnGuaranteeMenuItemText);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					queryOnGuaranteeMenuItem.PerformClick();
					AssertType<QueryOnGuaranteeForm>("Last dialog form type = QueryOnGuaranteeForm", ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		[RequiresSTA]
		public void TestGuaranteeAccessCodes_PreSaveInvoked()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(GuaranteeAccessCodesMenuItemText);

				AssertPreSaveInvoked(queryOnGuaranteeMenuItem);
			}
		}

		public void TestGuaranteeAccessCodes_CredentialCheck_NoValidCredential()
		{
			CredentialCheck_NoValidCredential(GuaranteeAccessCodesMenuItemText);
		}

		public void TestGuaranteeAccessCodes_CredentialCheck_ExpiredCredential()
		{
			CredentialCheck_ExpiredCredential(GuaranteeAccessCodesMenuItemText);
		}

		[RequiresSTA]
		public void TestGuaranteeAccessCodes_CredentialCheck_NoCredential()
		{
			CredentialCheck_NoCredential(GuaranteeAccessCodesMenuItemText);
		}

		[RequiresSTA]
		public void TestGuaranteeAccessCodes_CredentialCheck_JobHasNoGuarantee()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(GuaranteeAccessCodesMenuItemText);

				const string expectedErrorMessage = "Please provide valid Guarantee details in the Guarantees Grid to send Guarantee Access code message.";

				AssertJobHasNoGuaranteeErrorMessage(queryOnGuaranteeMenuItem, expectedErrorMessage);
			}
		}

		public void TestGuaranteeAccessCodes_OpenedForm()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(GuaranteeAccessCodesMenuItemText);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					queryOnGuaranteeMenuItem.PerformClick();
					AssertType<GuaranteeAccessCodesUserSelectionForm>("Last dialog form type = GuaranteeAccessCodesUserSelectionForm", ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		void AssertPreSaveInvoked(ZMenuItem queryOnGuaranteeMenuItem)
		{
			const string messageSaveConfirmation = "The Job has not yet been saved. Do you want to save and proceed?";

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition not saved", true, header.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				queryOnGuaranteeMenuItem.PerformClick();
				AssertEquals("Message for deny save", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save denied", true, header.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				queryOnGuaranteeMenuItem.PerformClick();
				AssertEquals("Message for confirm save", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save confirmed", false, header.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				queryOnGuaranteeMenuItem.PerformClick();
				AssertNotEquals("No message when no changes", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		void CredentialCheck_NoValidCredential(string menuItemCaption)
		{
			using (var nctsMovementForm = GetForm(false))
			{
				var company = header.Company;
				var companyCredential = GlbCompanyWrapper.Get(company).GlbExternalPassword;
				companyCredential.CurrentDecryptedCertificatePassphrase = "HELLO";
				Factory.Save();
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(menuItemCaption);

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					queryOnGuaranteeMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		void CredentialCheck_ExpiredCredential(string menuItemCaption)
		{
			using (var nctsMovementForm = GetForm(false))
			{
				var company = header.Company;
				var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
				companyCredential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
				Factory.Save();
				AssertEquals("Pre-condition", MasterFiles.Business.PasswordStatusList.Codes.Valid, companyCredential.GP_PasswordStatus);
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(menuItemCaption);

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					queryOnGuaranteeMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		void CredentialCheck_NoCredential(string menuItemCaption)
		{
			using (var nctsMovementForm = GetForm(false))
			{
				var company = header.Company;
				var queryOnGuaranteeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(menuItemCaption);

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					queryOnGuaranteeMenuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		void AssertJobHasNoGuaranteeErrorMessage(ZMenuItem queryOnGuaranteeMenuItem, string expectedErrorMessage)
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.Guarantees.RemoveAndDeleteAll();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				queryOnGuaranteeMenuItem.PerformClick();

				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[RequiresSTA]
		public void TestSendDocuments_HeaderHasNoMRN()
		{
			using (var nctsMovementForm = GetForm(true))
			{
				var documentSendingForm = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Upload Supporting Documents");

				CombineAssertions("No MRN", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					documentSendingForm.PerformClick();

					AssertEquals("The Supporting Documents message cannot be sent before the declaration has an MRN.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSendDocuments_OpenedForm()
		{
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, MasterFiles.Business.GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";
			using (var nctsMovementForm = GetForm(true))
			{
				var documentSendingForm = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Upload Supporting Documents");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					documentSendingForm.PerformClick();
					AssertType<DocumentsSendingForm>("Last dialog form type = DocumentSendingForm", ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		EU.NCTS.GUI.Phase5DepartureMovementForm GetForm(bool setupCredential = false)
		{
			var form = new EU.NCTS.GUI.Phase5DepartureMovementForm(header);
			if (setupCredential)
			{
				InterchangeProcessorTestHelper.CreateValidCredential(header.Company);
			}
			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var guarantee1 = header.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			provider = new Phase5MessagingMenuProvider(header);
			menuItems = provider.CreateMenuItems();
		}
		NctsHeader header;
		Phase5MessagingMenuProvider provider;
		IEnumerable<ZMenuItem> menuItems;
	}
}
