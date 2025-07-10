using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(ComplianceWorkflowInitiationRiskControl))]
	public class ComplianceWorkflowInitiationRiskControlTest : TestCaseWithFactory
	{
		public void TestWorkflowInitiationRiskHeaderInfo()
		{
			var complianceRiskStatus = CreateComplianceRiskStatusWithShipment();
			using (var form = new ZForm(complianceRiskStatus))
			{
				CombineAssertions("Compliance Header workflow initiation", () =>
				{
					var headerControl = AddComplianceWorkflowInitiationRiskControl(form, (ComplianceRiskTypes)(-1), complianceRiskStatus);
					var headerLabels = headerControl.FindAll<ZLabel>();
					Assert(headerLabels.Any(c => c.Text == "Risk Factors"));
					Assert(headerLabels.Any(c => c.Text == "Risk"));
					Assert(headerLabels.Any(c => c.Text == "Reason"));
				});
			}
		}

		public void TestWorkflowInitiationRiskPartiesInfo()
		{
			var complianceRiskStatus = CreateComplianceRiskStatusWithShipment();
			using (var form = new ZForm(complianceRiskStatus))
			{
				CombineAssertions("Compliance Parties workflow initiation is Potential Risk", () =>
				{
					complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;

					var potentialRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Parties, complianceRiskStatus);
					var potentialRiskLabels = potentialRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", potentialRiskLabels.Any(c => c.Text == "Parties"));
					Assert("Reason", potentialRiskLabels.Any(c => c.Text == "One or more parties need screening or require a review."));

					var riskLabel = potentialRiskLabels.FirstOrDefault(c => c.Text == "High Risk");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_PartyRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Parties workflow initiation is Clear", () =>
				{
					complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;

					var clearedRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Parties, complianceRiskStatus);
					var clearedRiskLabels = clearedRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", clearedRiskLabels.Any(c => c.Text == "Parties"));
					Assert("Reason", clearedRiskLabels.Any(c => string.IsNullOrEmpty(c.Text)));

					var riskLabel = clearedRiskLabels.FirstOrDefault(c => c.Text == "Clear");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_PartyRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Parties workflow initiation is High Risk", () =>
				{
					complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;

					var highRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Parties, complianceRiskStatus);
					var highRiskLabels = highRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", highRiskLabels.Any(c => c.Text == "Parties"));
					Assert("Reason", highRiskLabels.Any(c => c.Text == "One or more parties need screening or require a review."));

					var riskLabel = highRiskLabels.FirstOrDefault(c => c.Text == "High Risk");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_PartyRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Parties workflow initiation is Blocked", () =>
				{
					complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

					var blockedRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Parties, complianceRiskStatus);
					var blockedRiskLabels = blockedRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", blockedRiskLabels.Any(c => c.Text == "Parties"));
					Assert("Reason", blockedRiskLabels.Any(c => c.Text == "One or more parties have been matched to a denied party."));

					var riskLabel = blockedRiskLabels.FirstOrDefault(c => c.Text == "Blocked");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_PartyRisk), riskLabel.BackColor);
				});
			}
		}

		public void TestWorkflowInitiationRiskLocationsInfo()
		{
			var complianceRiskStatus = CreateComplianceRiskStatusWithShipment();
			using (var form = new ZForm(complianceRiskStatus))
			{
				CombineAssertions("Compliance Locations workflow initiation is Potential Risk", () =>
				{
					complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

					var potentialRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Locations, complianceRiskStatus);
					var potentialRiskLabels = potentialRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", potentialRiskLabels.Any(c => c.Text == "Locations"));
					Assert("Reason", potentialRiskLabels.Any(c => c.Text == "One or more locations have been embargoed."));

					var riskLabel = potentialRiskLabels.FirstOrDefault(c => c.Text == "Potential Risk");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_LocationRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Locations workflow initiation is Clear", () =>
				{
					complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

					var clearedRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Locations, complianceRiskStatus);
					var clearedRiskLabels = clearedRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", clearedRiskLabels.Any(c => c.Text == "Locations"));
					Assert("Reason", clearedRiskLabels.Any(c => string.IsNullOrEmpty(c.Text)));

					var riskLabel = clearedRiskLabels.FirstOrDefault(c => c.Text == "Clear");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_LocationRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Locations workflow initiation is Blocked", () =>
				{
					complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

					var blockedRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Locations, complianceRiskStatus);
					var blockkedRiskLabels = blockedRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", blockkedRiskLabels.Any(c => c.Text == "Locations"));
					Assert("Reason", blockkedRiskLabels.Any(c => c.Text == "One or more locations have been embargoed."));

					var riskLabel = blockkedRiskLabels.FirstOrDefault(c => c.Text == "Blocked");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_LocationRisk), riskLabel.BackColor);
				});
			}
		}

		public void TestWorkflowInitiationRiskCommoditiesInfo()
		{
			var complianceRiskStatus = CreateComplianceRiskStatusWithShipment();
			using (var form = new ZForm(complianceRiskStatus))
			{
				CombineAssertions("Compliance Commodities workflow initiation is Potential Risk", () =>
				{
					complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

					var potentialRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Commodities, complianceRiskStatus);
					var potentialRiskLabels = potentialRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", potentialRiskLabels.Any(c => c.Text == "Commodities"));
					Assert("Reason", potentialRiskLabels.Any(c => c.Text == "One or more HS codes have conditions."));

					var riskLabel = potentialRiskLabels.FirstOrDefault(c => c.Text == "Potential Risk");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_CommodityRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Commodities workflow initiation is Clear", () =>
				{
					complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

					var clearedRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Commodities, complianceRiskStatus);
					var clearedRiskLabels = clearedRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", clearedRiskLabels.Any(c => c.Text == "Commodities"));
					Assert("Reason", clearedRiskLabels.Any(c => c.Text == "All HS codes have been cleared."));

					var riskLabel = clearedRiskLabels.FirstOrDefault(c => c.Text == "Clear");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_CommodityRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Commodities workflow initiation is Incomplete", () =>
				{
					complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;

					var incompleteRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Commodities, complianceRiskStatus);
					var incompleteRiskLabels = incompleteRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", incompleteRiskLabels.Any(c => c.Text == "Commodities"));
					Assert("Reason", incompleteRiskLabels.Any(c => c.Text == "No HS codes are entered on the job."));

					var riskLabel = incompleteRiskLabels.FirstOrDefault(c => c.Text == "Incomplete");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_CommodityRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Commodities workflow initiation is High Risk", () =>
				{
					complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;

					var highRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Commodities, complianceRiskStatus);
					var highRiskLabels = highRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", highRiskLabels.Any(c => c.Text == "Commodities"));
					Assert("Reason", highRiskLabels.Any(c => c.Text == "One or more HS codes have conditions."));

					var riskLabel = highRiskLabels.FirstOrDefault(c => c.Text == "High Risk");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_CommodityRisk), riskLabel.BackColor);
				});

				CombineAssertions("Compliance Commodities workflow initiation is Possible Risk", () =>
				{
					complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PossibleRisk;

					var possibleRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Commodities, complianceRiskStatus);
					var possibleRiskLabels = possibleRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", possibleRiskLabels.Any(c => c.Text == "Commodities"));
					Assert("Reason", possibleRiskLabels.Any(c => c.Text == "One or more HS codes may have some risk."));

					var riskLabel = possibleRiskLabels.FirstOrDefault(c => c.Text == "Possible Risk");
					AssertNotNull("Risk", riskLabel);
					AssertEquals("Risk BackColor", ComplianceRiskColorHelper.GetColorForRiskStatus(complianceRiskStatus.COR_CommodityRisk), riskLabel.BackColor);
				});
			}
		}

		public void TestWorkflowInitiationRiskAssessmentInfo()
		{
			var complianceRiskStatus = CreateComplianceRiskStatusWithShipment();
			using (var form = new ZForm(complianceRiskStatus))
			{
				CombineAssertions("Compliance Assessment workflow initiation is High Risk", () =>
				{
					var potentialRiskControl = AddComplianceWorkflowInitiationRiskControl(form, ComplianceRiskTypes.Assessment, complianceRiskStatus);
					var potentialRiskLabels = potentialRiskControl.FindAll<ZLabel>();
					Assert("Risk Factors", potentialRiskLabels.Any(c => c.Text == "Compliance Assessment"));
					Assert("Risk", potentialRiskLabels.Any(c => c.Text == "High risk"));
					Assert("Risk BackColor", potentialRiskLabels.Any(c => c.BackColor == ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskStatusCodeList.Codes.PotentialRisk)));
					Assert("Reason", potentialRiskLabels.Any(c => c.Text == "A compliance assessment has not been performed."));
				});
			}
		}

		ComplianceWorkflowInitiationRiskControl AddComplianceWorkflowInitiationRiskControl(ZForm parentForm, ComplianceRiskTypes risktype, ComplianceRiskStatus complianceRiskStatus)
		{
			var workflowRiskControl = new ComplianceWorkflowInitiationRiskControl();
			parentForm.Controls.Add(workflowRiskControl);
			workflowRiskControl.SetRiskInitiationInfo(risktype, complianceRiskStatus);

			return workflowRiskControl;
		}

		ComplianceRiskStatus CreateComplianceRiskStatusWithShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = (shipment as BusinessObject).TablePrefix;

			Factory.Save();

			return complianceRiskStatus;
		}
	}
}
