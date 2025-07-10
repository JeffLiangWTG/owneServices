using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(InvalidStagePopupForm))]
	public class InvalidStagePopupFormForTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			return new InvalidStagePopupForm(incidentManagementGroup);
		}

		public void TestOkButton()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentManagementGroup.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			Factory.Save();
			using (var form = new InvalidStagePopupForm(incidentManagementGroup))
			{
				form.Show();
				var stageGrid = (ZGrid)form.Controls.Find("stageGrid", true).FirstOrDefault();
				AssertEquals(5, stageGrid.VisibleRowCount);
				stageGrid.PerformMouseDownForTest(1, 1);
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				Factory.Save();
			}
			AssertEquals("incidentManagementGroup status should change to ESC", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, incidentManagementGroup.ING_Status);
		}

		public void TestBroadcastCheckboxShouldNotVisibleWhenNoPublishedMessage()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentManagementGroup.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var currentStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			currentStage.Enabled = false;
			var nextStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			nextStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			using (var form = new InvalidStagePopupForm(incidentManagementGroup))
			{
				form.Show();
				var broadcastCheckbox = (ZCheckBox)form.Controls.Find("broadcastCheckbox", true).FirstOrDefault();
				AssertEquals(false, broadcastCheckbox.Visible);
				var warnningLabel = (ZLabel)form.Controls.Find("warnningLabel", true).FirstOrDefault();
				AssertEquals(true, warnningLabel.Visible);
			}
		}

		public void TestBroadcastCheckboxShouldVisibleWhenHavePublishedMessage()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentManagementGroup.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = incidentManagementGroup.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var currentStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			currentStage.Enabled = false;
			var nextStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			nextStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			using (var form = new InvalidStagePopupForm(incidentManagementGroup))
			{
				form.Show();
				var broadcastCheckbox = (ZCheckBox)form.Controls.Find("broadcastCheckbox", true).FirstOrDefault();
				AssertEquals(true, broadcastCheckbox.Visible);
				var warnningLabel = (ZLabel)form.Controls.Find("warnningLabel", true).FirstOrDefault();
				AssertEquals(false, warnningLabel.Visible);
			}
		}

		public void TestCancelButton()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentManagementGroup.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			Factory.Save();
			using (var form = new InvalidStagePopupForm(incidentManagementGroup))
			{
				form.Show();
				var stageGrid = (ZGrid)form.Controls.Find("stageGrid", true).FirstOrDefault();
				AssertEquals(5, stageGrid.VisibleRowCount);
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
			AssertEquals("incidentManagementGroup status should not change", IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, incidentManagementGroup.ING_Status);
		}

		public void TestMessageLabelText()
		{
			var incidentManagementGroup = Factory.New<IncidentManagementGroup>();
			incidentManagementGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentManagementGroup.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;

			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var currentStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			currentStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Factory.Save();

			using (var form = new InvalidStagePopupForm(incidentManagementGroup))
			{
				form.Show();
				var stageGrid = (ZGrid)form.Controls.Find("stageGrid", true).FirstOrDefault();
				var messageLabel = (ZLabel)form.Controls.Find("messageLabel", true).FirstOrDefault();
				AssertEquals("The active stage (INV - Investigation) for this Incident Group is no longer available. Please select a valid stage for this Group Type (MIM - Major Incident).", messageLabel.Text);
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}
	}
}
