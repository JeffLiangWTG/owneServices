using System.Windows.Forms;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.CustomerService.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(ReopenPeriodKeyForm))]
	internal sealed class ReopenPeriodKeyFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.Factory.Save();
			ReopenPeriodKeyBusinessObject bo = new ReopenPeriodKeyBusinessObject("", period);
			return new ReopenPeriodKeyForm(bo);
		}

		#endregion

		public void TestRequestButton()
		{
			using (ReopenPeriodKeyForm form = (ReopenPeriodKeyForm)GetFormToBash())
			{
				AccPeriodManagement period = ((ReopenPeriodKeyBusinessObject)form.BusinessEntity).Period;
				form.Show();
				form.RequestButton.PerformClick();
				ZForm lastForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				AssertEquals("Customer Service Form Shown", "Enterprise.CustomerService.GUI.IncidentApprovalForm", lastForm.GetType().ToString());
				IIncidentApproval incident = (IIncidentApproval)lastForm.BusinessEntity;
				AssertEquals("IA_IncidentSummary", "Request for Reopen GL Period Key", incident.IA_IncidentSummary);
				AssertEquals("IA_IncidentDetails", string.Format("Please issue a Reopen GL Period Key with the following details:\r\n1. User's Staff Code: {0}\r\n2. Period to reopen: {1}", GlbStaff.CurrentUser.GS_Code, period.AM_Period.ToString()), incident.IA_IncidentDetails);
				AssertEquals("IA_Criticality", Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, incident.IA_Criticality);
				AssertEquals("IA_Module", "RGP", incident.IA_Module);
			}
		}
	}
}
