using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	[TestedType(typeof(ErrorLogForm))]
	class ErrorLogFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var errorLog = new BusinessObjectFactory().NewWithValidTestData<EdiHelpErrorLog>();
			errorLog.HE_IssueNumber = "01234567";
			using (var form = new ErrorLogForm(errorLog))
			{
				AssertEquals("Issue 01234567", form.FormCaption);
				errorLog.HE_ExceptionMessage = "Oh no something bad happened!";
				AssertEquals("Issue 01234567 - Oh no something bad happened!", form.FormCaption);
			}
		}

		public void TestFormCaptionOverride()
		{
			using (ZForm form = (ZForm)GetFormToBash())
			{
				Assert("FormCaption should be overridden", form.FormCaption.IndexOf("FormCaption") < 0);
			}
		}

		public void TestFormCaptionLength()
		{
			var errorLog = new BusinessObjectFactory().NewWithValidTestData<EdiHelpErrorLog>();
			errorLog.HE_IssueNumber = "7654321";
			using (var form = new ErrorLogForm(errorLog))
			{
				AssertEquals("Issue 7654321", form.FormCaption);
				errorLog.HE_ExceptionMessage = "I am a message that exceeds 80 characters and should be cut off appropriately please.";
				AssertEquals("Issue 7654321 - I am a message that exceeds 80 characters and should be cut off appropriately pl...", form.FormCaption);
			}
		}

		public void TestWorkItemButton()
		{
			EdiHelpErrorLog log = new BusinessObjectFactory().New<EdiHelpErrorLog>();
			log.HE_ExceptionMessage = "Something";
			using (ErrorLogForm form = new ErrorLogForm(log))
			{
				form.Show();
				form.WorkItemButton.PerformClick();
				AssertEquals(typeof(NewWorkItemForm), form.RelatedWorkItemGrid.LastShownZForm.GetType());
				AssertEquals("Something", ((NewWorkItemForm)form.RelatedWorkItemGrid.LastShownZForm).DataSource.WKI_Summary);
				form.RelatedWorkItemGrid.LastShownZForm.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestActionsMenu()
		{
			EdiHelpErrorLog log = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			log.HE_ExceptionMessage = "Something";
			EDIOrgHeader testHeader = log.Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database1 = testHeader.LicCompany.LicDatabases.AddNew();
			LicenceHeader licence = testHeader.LicCompany.GetHeader(database1);
			database1.FillWithValidTestData();
			database1.LD_HostedLocation = "SYD";
			database1.LicEnterprise.LE_EnterpriseCode = "ABC";
			HelpErrorLogOccurrence occurrence = log.Occurrences.AddNew();
			occurrence.HO_LD = licence.LA_LD;
			Factory.Save();
			using (ErrorLogForm form = new ErrorLogForm(log))
			{
				form.Show();
				Application.DoEvents();
				MenuItem item = GetActionsMenuItem(form, "View Licence");
				AssertNotNull(item);
				item.PerformClick();
				AssertNotNull(form.OccurrencesControl.LastController.LastShownForm);
				AssertEquals(typeof(EDIOrganisationForm), form.OccurrencesControl.LastController.LastShownForm.GetType());
				form.OccurrencesControl.LastController.LastShownForm.Dispose();
				form.Dispose();
			}
		}

		MenuItem GetActionsMenuItem(Form form, string text)
		{
			MenuItem actionsMenu = GetActionsMenu(form);
			actionsMenu.OnPopup(EventArgs.Empty);
			foreach (MenuItem actionItem in actionsMenu.MenuItems)
			{
				if (actionItem.Text == text && actionItem.Visible)
				{
					return actionItem;
				}
			}

			return null;
		}

		MenuItem GetActionsMenu(Form form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item.Text == "Actio&ns")
				{
					return item;
				}
			}

			return null;
		}

		//public void TestSplitAction()
		//{
		//    HelpErrorLog Log = new BusinessObjectFactory().New<HelpErrorLog>();
		//    Log.HE_ExceptionMessage = "Something";
		//    using (ErrorLogForm Form = new ErrorLogForm(Log))
		//    {
		//        Form.Show();
		//        Assert(Form.ActionsMenuItem.MenuItems.FindByText("Split Occurrences") != null);
		//        //Form.SplitAction_MenuItem.PerformClick();
		//        //AssertEquals(typeof(NewWorkItemForm), Form.las.RelatedWorkItemGrid.LastShownZForm.GetType());
		//        //AssertEquals("Issue " + Log.HE_IssueNumber + " - Something", ((NewWorkItemForm)Form.RelatedWorkItemGrid.LastShownZForm).DataSource.IM_Description);
		//        //Form.RelatedWorkItemGrid.LastShownZForm.Dispose();
		//    }
		//}
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ErrorLogForm(Factory.New<EdiHelpErrorLog>());
		}
		#endregion
	}
}
