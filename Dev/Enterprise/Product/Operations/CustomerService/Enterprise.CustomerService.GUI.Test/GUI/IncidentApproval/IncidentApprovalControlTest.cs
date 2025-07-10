using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.GUI.Testing
{
	sealed class IncidentApprovalControlTest : TestCaseWithFactory
	{
		public void TestELearningNoticeFormIsOpened()
		{
			var incident = Factory.NewWithValidTestData<IncidentApproval>();

			using (var form = new ZForm())
			using (var control = new IncidentApprovalControlForTest())
			{
				form.SetDataBinding(incident, "");
				form.Controls.Add(control);
				form.Show();

				control.GetCriticalityDropEditForTesting().Focus();
				control.SetCriticality(Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest);
				control.GetIncidentSummaryTextBoxTesting().Focus();
				AssertNull(ZFormModaliser.LastFormShownForTest);

				control.GetCriticalityDropEditForTesting().Focus();
				control.SetCriticality(Constants.CustomerService.CriticalityCodes.CR5_Training);
				AssertNull("Should not show form until drop edit loses focus", ZFormModaliser.LastFormShownForTest);

				control.GetIncidentSummaryTextBoxTesting().Focus();
				AssertEquals(typeof(ELearningNoticeForm), ZFormModaliser.LastFormShownForTest.GetType());

				ZFormModaliser.LastFormShownForTest = null;
				AssertEquals("Precondition:", Constants.CustomerService.CriticalityCodes.CR5_Training, incident.IA_Criticality);
				control.GetCriticalityDropEditForTesting().Focus();
				control.GetIncidentSummaryTextBoxTesting().Focus();
				AssertNull("Should not show form if was already originally CR5", ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestReopenIncident()
		{
			var incident = Factory.NewWithValidTestData<IncidentApproval>();
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			loadFactory.RefreshEnabled = false;
			var incidentInOtherFactory = loadFactory.Load<IncidentApproval>(incident.PK);
			incidentInOtherFactory.IA_Status = IncidentApprovalLookups.StatusCodes.Closed;
			loadFactory.Save();

			using (var form = new ZForm())
			using (var control = new IncidentApprovalControlForTest())
			{
				form.SetDataBinding(incident, "");
				form.Controls.Add(control);
				form.Show();

				control.ReopenIncidentForTest(IncidentApprovalLookups.StatusCodes.PendingFeatureResult);
				AssertEquals(IncidentApprovalLookups.StatusCodes.PendingFeatureResult, incident.IA_Status);

				control.ReopenIncidentForTest(IncidentApprovalLookups.StatusCodes.DevelopmentTeam);
				AssertEquals(IncidentApprovalLookups.StatusCodes.DevelopmentTeam, incident.IA_Status);

				control.ReopenIncidentForTest(IncidentApprovalLookups.StatusCodes.SupportTeam);
				AssertEquals(IncidentApprovalLookups.StatusCodes.SupportTeam, incident.IA_Status);
			}
		}

		class IncidentApprovalControlForTest : IncidentApprovalControl
		{
			public ZDropEdit GetCriticalityDropEditForTesting()
			{
				return CriticalityDropEdit;
			}

			public ZTextBox GetIncidentSummaryTextBoxTesting()
			{
				return IncidentSummaryTextBox;
			}

			public void ReopenIncidentForTest(ZString newStatus)
			{
				ReopenIncident(newStatus);
			}
		}
	}
}
