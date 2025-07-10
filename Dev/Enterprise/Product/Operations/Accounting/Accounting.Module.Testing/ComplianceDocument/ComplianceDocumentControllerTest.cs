using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class ComplianceDocumentControllerTest : ZControllerBasherTest
	{
		protected abstract SecurityCheckpoint CheckpointForVoid { get; }

		protected abstract AccComplianceDocumentHeader NewComplianceDocumentHeader();

		public override void TestNewForm()
		{
			Assert(true);
		}

		public void TestShowViewFormWithLoginCompanyMatch()
		{
			TestShowForm_MustMatchCurrentLoginCompany((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditFormWithLoginCompanyMatch()
		{
			TestShowForm_MustMatchCurrentLoginCompany((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		void TestShowForm_MustMatchCurrentLoginCompany(Action<ComplianceDocumentController, BusinessObject> showFormDelegate)
		{
			var header = NewComplianceDocumentHeader();
			header.ADH_Ledger = LedgerTypes.AccountsReceivable;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.ADH_DocumentStatus = "SET";

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "GC2";
			otherCompany.GC_Name = "company description";
			otherCompany.GC_RN_NKCountryCode = "AU";

			Factory.Save();

			var controller = Controller as ComplianceDocumentController;
			AssertNotNull("Should be ComplianceDocumentController", controller);

			AssertShowFormResult(showFormDelegate, controller, header, true);

			header.ADH_GC_Company = otherCompany.PK;
			AssertShowFormResult(showFormDelegate, controller, header, false);
		}

		void AssertShowFormResult(Action<ComplianceDocumentController, BusinessObject> showFormDelegate, ComplianceDocumentController controller, BusinessObject sourceEntity, bool expectedAllowShowForm)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var expectedLastMessage = "This Compliance Document is posted into the ledgers of another company on this database. You must login to the following company to view this transaction: GC2 - company description.";
			var expectedLastCaption = "Access Denied: Incorrect login company";

			showFormDelegate(controller, sourceEntity);
			using (var lastShownForm = controller.LastShownForm)
			{
				if (expectedAllowShowForm)
				{
					CombineAssertions(() =>
					{
						AssertNotNull("LastShownForm", lastShownForm);
						AssertNotContains("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNotContains("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					});
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertNull("LastShownForm", lastShownForm);
						AssertEquals("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					});
				}
			}
		}

		public virtual void TestShowVoidForm()
		{
			var header = NewComplianceDocumentHeader();
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.ADH_DocumentStatus = "SET";

			Factory.Save();

			var voidComplianceDocumentsOldSevurity = CheckpointForVoid.IsAllowed;

			try
			{
				var controller = Controller as ComplianceDocumentController;
				AssertNotNull("Should be ComplianceDocumentController", controller);

				var mockIComplianceDocumentVoidingProvider = new Mock<IComplianceDocumentVoidingProvider>();
				mockIComplianceDocumentVoidingProvider.Setup(x => x.IsAllowedSpecialVoid(It.IsAny<AccComplianceDocumentHeader>())).Returns(true);

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentVoidingProvider>>().Setup(x => x.Get()).Returns(mockIComplianceDocumentVoidingProvider.Object);

				var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				using (ObjectFactory.Substitute(mockAccountingCountryFactory.Object))
				{
					using (var form = controller.ShowVoidForm(header))
					{
						AssertEquals("The form should be shown", form, Controller.LastShownForm);
						var complianceDocumentForm = form as ComplianceDocumentForm;
						AssertNotNull("Should be ComplianceDocumentForm", complianceDocumentForm);

						var specialVoidingPanel = complianceDocumentForm.ComplianceDocumentUserControl.GetField("specialVoidingPanel") as ZPanel;
						AssertNotNull("Should have specialVoidingPanel", specialVoidingPanel);
						if (header.ADH_Ledger == LedgerTypes.AccountsPayable)
						{
							AssertEquals(false, specialVoidingPanel.Visible);
						}
						else
						{
							AssertEquals(true, specialVoidingPanel.Visible);
						}

						Assert("VoidInsteadOfDelete", complianceDocumentForm.VoidInsteadOfDelete);
						Assert("Form Text should start with Void", complianceDocumentForm.Text.StartsWith("Void"));

						var btnForm = form as IPostingButtonsProvider;
						AssertNotNull("Should be IPostingButtonsProvider", btnForm);
						AssertEquals("CommandButtonPost.Text", "Void", btnForm.CommandButtonPost.Text);

						var menuForm = form as IFileMenuItemsProvider;
						AssertNotNull("Should be IFileMenuItemsProvider", menuForm);
						AssertNotNull("Should exists a Void menu item ", menuForm.FileMenuItem.MenuItems.FindByText("Void"));
					}

					CheckpointForVoid.IsAllowed = false;
					using (var form = controller.ShowVoidForm(header))
					{
						AssertNull("No form should be shown", form);
						AssertContains("Error Message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
					}

					CheckpointForVoid.IsAllowed = true;
					using (var form = controller.ShowVoidForm(header))
					{
						AssertNotNull("The form should be shown", form);
					}

					header.ADH_DocumentStatus = "VOD";
					AssertNotNullOrEmpty("Precodintion", header.CheckCanVoid());
					using (var form = controller.ShowVoidForm(header))
					{
						AssertNull("No form should be shown", form);
						AssertContains("Error Message", "This compliance document is already voided.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				CheckpointForVoid.IsAllowed = voidComplianceDocumentsOldSevurity;
			}
		}
	}
}
