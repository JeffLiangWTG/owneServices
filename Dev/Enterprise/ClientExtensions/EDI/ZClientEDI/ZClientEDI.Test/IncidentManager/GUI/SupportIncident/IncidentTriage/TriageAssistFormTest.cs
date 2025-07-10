using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.TriageAssistTreeTriageWrapper;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(TriageAssistForm))]
	public class TriageAssistFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestShowForm()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
			}
		}

		public override void TestBashingForm()
		{
#if WINZOR
			Assert(true);
#else
			base.TestBashingForm();
#endif
		}

		public void TestCollapseExcludedTreeViewNode()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = Triage.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var pivot3 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot3.LinkParent(Incident);
			pivot3.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			pivot3.IMV_Status = "INV";

			var pivot4 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot4.IMO_IMT_Triage = Triage.PK;
			pivot4.IMO_IMD_DiagnosticCriteria = criteria2.PK;

			Factory.Save();
			AssertTreeViewNode(TriageNodeStatus.Investigating, true);

			pivot3.IMV_Status = "NEG";
			Factory.Save();
			AssertTreeViewNode(TriageNodeStatus.Excluded, false);
		}

		public void TestShowFocusedObjectsOnlyCheckBox()
		{
			Factory.Save();
			var incident = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj = new TriageAssistBusinessObject(incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();

			using (var form = new TriageAssistForm(obj))
			{
				form.Show();
				obj.ShowFocusedTriageNodesOnly = true;
				AssertEquals(true, obj.ShowFocusedObjectsOnly);
				AssertEquals(false, obj.ShowFocusedSuggestedCriteriaOnly);

				var checkbox = typeof(TriageAssistForm).GetField("ShowFocusedObjectsOnlyCheckBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form) as ZCheckBox;
				AssertEquals(true, checkbox.Checked);
				AssertEquals(CheckState.Indeterminate, checkbox.CheckState);

				obj.ShowFocusedSuggestedCriteriaOnly = true;
				AssertEquals(true, checkbox.Checked);
				AssertEquals(CheckState.Checked, checkbox.CheckState);
			}
		}

		public void TestUserControlLayout()
		{
			Factory.Save();
			var incident = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj = new TriageAssistBusinessObject(incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();

			var layout = EDIDataRegistry.Instance.TriageAssistFormUserControlLayout.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty);
			AssertEquals(0, layout.Count);

			using (var form = new TriageAssistForm(obj))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(true, obj.ShowSearchOptions);
				AssertEquals(TriageAssistBusinessObject.ComparisonConstants.ContainsAny, obj.SearchTermOperator);
				AssertEquals(TriageAssistBusinessObject.ComparisonConstants.Any, obj.CriteriaTypeFilter);

				obj.SearchTermOperator = TriageAssistBusinessObject.ComparisonConstants.ContainsAll;
				obj.ShowSearchOptions = false;
				obj.CriteriaTypeFilter = IncidentDiagnosticCriteriaTypes.Descriptions.DiagnosticFactor;

				form.Close();
			}

			layout = EDIDataRegistry.Instance.TriageAssistFormUserControlLayout.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty);
			var layoutAsString = string.Join("\r\n", layout.OfType<ICodeDescription>().Select(x => $"{x.Code}-{x.Description}").OrderBy(x => x));
			AssertEquals(@"CriteriaTypeFilter-Diagnostic Factor
SearchTermOperator-Contains All
ShowSearchOptions-N", layoutAsString);

			var incident2 = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj2 = new TriageAssistBusinessObject(incident2);
			obj2.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj2.TriageAssistTreeWrapperCollection.Load();
			using (var form = new TriageAssistForm(obj2))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(false, obj2.ShowSearchOptions);
				AssertEquals(TriageAssistBusinessObject.ComparisonConstants.ContainsAll, obj2.SearchTermOperator);
				AssertEquals(IncidentDiagnosticCriteriaTypes.Descriptions.DiagnosticFactor, obj2.CriteriaTypeFilter);
				form.Close();
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); // override to allow 1080p
		}

		void AssertTreeViewNode(TriageNodeStatus nodeStatusExpected, bool isExpandedExpected)
		{
			var incident = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj = new TriageAssistBusinessObject(incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			var triageWrapper = obj.TriageAssistTreeWrapperCollection.Single() as TriageAssistTreeTriageWrapper;
			AssertEquals(Triage.PK, triageWrapper.BizObj.PK);
			AssertEquals(nodeStatusExpected, triageWrapper.TriageStatus);

			using (var form = new TriageAssistForm(obj))
			{
				form.Show();
				var treeView = typeof(TriageAssistForm).GetField("TriageNodeTreeView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form) as ZTreeViewAdv;
				AssertEquals(isExpandedExpected, treeView.Root.Children.Single().IsExpanded);

				if (nodeStatusExpected == TriageNodeStatus.Excluded && !isExpandedExpected)
				{
					treeView.ExpandAll();
					AssertEquals(true, treeView.Root.Children.Single().IsExpanded);
					incident.Factory.Save();
					AssertEquals(false, treeView.Root.Children.Single().IsExpanded);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			var triageAssist = new TriageAssistBusinessObject(incident);
			return new TriageAssistForm(triageAssist);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return new[] { "SymptomInputTextBox", "DiagnosticGuideTextBox", "DiagnosticCriteriaSuggestedGrid" }.Contains(control.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Triage = Factory.NewWithValidTestData<IncidentTriage>();
			Triage.IMT_IsPublishedToAssist = true;
		}

		SupportIncident Incident;
		IncidentTriage Triage;
	}
}
