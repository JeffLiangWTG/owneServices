using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.CustomerService.GUI.Testing
{
	[TestedType(typeof(IncidentApprovalForm))]
	sealed class IncidentApprovalFormTest : ZFormBasherTest
	{
		public void TestApproveButton()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_GS_NKReportingStaff = Env.CurrentUser.Initials;
			incident.IA_IncidentDetails = "Some Details Is Long Enough";
			incident.IA_IncidentSummary = "Some Summary";
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.CashBooks;
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;

			bool existingSecurity = CanApprove;
			try
			{
				CanApprove = true;
				using (IncidentApprovalForm testForm = new IncidentApprovalForm(incident))
				{
					AssertEquals("", incident.IA_Status);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ClickApproveButton(testForm);
					AssertEquals("APP", incident.IA_Status);
					AssertContains("Your eRequest has been sent to CargoWise and will be responded to as soon as possible.", UnitTestUserNotification.Instance.LastMessage.Text);
					ClickApproveButton(testForm);
					AssertContains("This request has already been sent to CargoWise - please contact CargoWise for more information.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
			finally
			{
				CanApprove = existingSecurity;
			}
		}

		[ExpectNoExceptions]
		public void TestApproveButton_ConcurrentSave()
		{
			IncidentApprovalForTest incident = Factory.New<IncidentApprovalForTest>();
			incident.IA_GS_NKReportingStaff = Env.CurrentUser.Initials;
			incident.IA_IncidentDetails = "Some Details Is Long Enough";
			incident.IA_IncidentSummary = "Some Summary";
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.CashBooks;
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			Factory.Save();

			bool existingSecurity = CanApprove;
			try
			{
				CanApprove = true;

				var loadFactory1 = new BusinessObjectFactory();
				loadFactory1.RefreshEnabled = false;
				var loadedIncident1 = loadFactory1.Load<IncidentApproval>(incident.PK);
				using (IncidentApprovalForm testForm = new IncidentApprovalForm(loadedIncident1))
				{
					var loadFactory2 = new BusinessObjectFactory();
					loadFactory2.RefreshEnabled = false;
					var loadedIncident2 = loadFactory2.Load<IncidentApproval>(incident.PK);
					loadedIncident2.IA_IncidentSummary = "Some Summary 2";
					loadFactory2.Save();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					loadedIncident1.IA_IncidentSummary = "Some Summary 1";
					ClickApproveButton(testForm);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
			finally
			{
				CanApprove = existingSecurity;
			}
		}

		class IncidentApprovalForTest : IncidentApproval
		{
			public IncidentApprovalForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public void TestConfirmMessageWhenCriticalityChangedToDefect()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_GS_NKReportingStaff = Env.CurrentUser.Initials;
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;
			incident.IA_IncidentDetails = "";
			incident.IA_IncidentSummary = "Some Summary";
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR5_Training;

			using (IncidentApprovalForm form = new IncidentApprovalForm(incident))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
				ClickApproveButton(form);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				incident.IA_IncidentDetails = "Some Details Is Long Enough";
				ClickApproveButton(form);
				string expectedString = @"This Criticality CR2 means ""ENTIRE MODULE NOT WORKING WITH NO MANUAL WORK AROUND"".";
				AssertEquals(expectedString, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
				ClickApproveButton(form);
				expectedString = @"This Criticality CR3 means ""SINGLE FUNCTION NOT WORKING WITH NO MANUAL WORK AROUND"".";
				AssertEquals(expectedString, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				ClickApproveButton(form);
				expectedString = @"This Criticality CR4 means ""SINGLE FUNCTION NOT WORKING WITH MANUAL WORK AROUND"".";
				AssertEquals(expectedString, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdateStatusIfFirstSendingCR7()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_GS_NKReportingStaff = Env.CurrentUser.Initials;
			incident.IA_IncidentDetails = "Some Details Is Long Enough";
			incident.IA_IncidentSummary = "Some Summary";
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			Factory.Save();

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.FormalQuotationRequested;
			Factory.Save();
			using (IncidentApprovalForm form = new IncidentApprovalForm(incident))
			{
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
				ClickApproveButton(form);
				AssertEquals(Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest, incident.IA_Criticality);
				AssertEquals("No change", IncidentApprovalLookups.StatusCodes.FormalQuotationRequested, incident.IA_Status);
			}
		}

		public void TestConfirmMessageWhenRequestQuote()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_GS_NKReportingStaff = Env.CurrentUser.Initials;
			incident.IA_IncidentDetails = "Some Details Is Long Enough";
			incident.IA_IncidentSummary = "Some Summary";
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;
			incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			Factory.Save();

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.DevelopmentEstimateProvided;
			Factory.Save();

			using (IncidentApprovalForm form = new IncidentApprovalForm(incident))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				ClickRequestQuoteButton(form);
				string expected = @"Because of the cost and effort of CargoWise providing a full formal and firm quotation and design, the request, once made, will be subject to a cancellation fee.
Please confirm that you would like to formally request a Quote and Order for this feature request.

Please note that formal requests will be subject to a cancellation fee:
- If you choose to decline a formal Order, or
- If no response is received within 30 days of the Order being issued

If the amount on the Order exceeds the High End estimate provided prior to this, the cancellation fee is waived.";
				AssertEquals(expected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHideInvalidMenuItems()
		{
			var incident = Factory.NewWithValidTestData<IncidentApproval>();
			Factory.Save();

			using (var form = new IncidentApprovalForm(incident))
			{
				form.Show();
				Application.DoEvents();
				foreach (MenuItem menuItem in form.Menu.MenuItems[0].MenuItems)
				{
					if (menuItem.Name == ZFormMenuStrategy.FileNewMenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSaveMenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSaveAndCloseMenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSeperator1MenuItemName
						|| menuItem.Name == ZFormMenuStrategy.FileSeperator2MenuItemName)
					{
						AssertEquals(false, menuItem.Enabled);
						AssertEquals(false, menuItem.Visible);
					}
				}
			}
		}

		public void TestPopulateModuleWhenSelectingAMenuSectionCriticality()
		{
			var incident = Factory.New<IncidentApproval>();
			incident.IA_Module = "";
			incident.IA_Criticality = "";
			incident.SetCurrentModule(ModuleIDs.RefAirline.ToString(), ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles);

			using (IncidentApprovalForm form = new IncidentApprovalForm(incident))
			{
				AssertEquals("Precondition", incident.IA_ActiveModuleId, ModuleIDs.RefAirline.ToString());

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				AssertEquals("", incident.IA_Module);

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles, incident.IA_Module);

				incident.IA_Module = "XXX";
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
				AssertEquals("Should not overwite existing module", "XXX", incident.IA_Module);
			}
		}

		public void TestErrorMessageWhenChangeCriticality()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			incident.IA_GS_NKReportingStaff = Env.CurrentUser.Initials;
			incident.IA_IncidentDetails = "Some Details Is Long Enough";
			incident.IA_IncidentSummary = "Some Summary";
			incident.IA_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing;

			AssertEquals("Pre-condition:", ZString.Empty, incident.IA_Criticality);

			using (IncidentApprovalForm form = new IncidentApprovalForm(incident))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
				AssertEquals("You cannot lodge a genuine CR1 request electronically. CR1 means \"Entire System down/System failure\".\r\nPlease re-classify your incident to the appropriate criticality.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZString.Empty, incident.IA_Criticality);

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
				AssertEquals(Constants.CustomerService.CriticalityCodes.CR2_ModuleDown, incident.IA_Criticality);

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
				AssertEquals(Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround, incident.IA_Criticality);

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				AssertEquals(Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident.IA_Criticality);

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
				AssertEquals(Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest, incident.IA_Criticality);

				incident.IA_Criticality = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
				AssertEquals(Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest, incident.IA_Criticality);
				AssertEquals("Should not auto save", false, incident.IsInDatabase);
				AssertEquals("Should not auto send request", ZString.Empty, incident.IA_Status);
			}
		}

		#region Implementation

		void ClickApproveButton(IncidentApprovalForm form)
		{
			var clickMethodInfo = typeof(IncidentApprovalForm).GetMethod("ApproveButton_Click", BindingFlags.Instance | BindingFlags.NonPublic);
			clickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
		}

		void ClickRequestQuoteButton(IncidentApprovalForm form)
		{
			var clickMethodInfo = typeof(IncidentApprovalForm).GetMethod("RequestQuoteButton_Click", BindingFlags.Instance | BindingFlags.NonPublic);
			clickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
		}

		bool CanApprove
		{
			get { return Env.Security.IncidentApprovalApprove.IsAllowed; }
			set { Env.Security.IncidentApprovalApprove.IsAllowed = value; }
		}

		protected override Form GetFormToBashCore()
		{
			IncidentApproval incident = Factory.New<IncidentApproval>();
			Factory.Save();
			return new IncidentApprovalForm(incident);
		}

		#endregion
	}
}
