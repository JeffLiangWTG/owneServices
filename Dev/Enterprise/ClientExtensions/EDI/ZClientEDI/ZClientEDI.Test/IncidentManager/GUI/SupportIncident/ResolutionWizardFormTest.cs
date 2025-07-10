using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(ResolutionWizardForm))]
	public class ResolutionWizardFormTest : BaseIncidentPopupFormTest
	{
		public void TestIsResolutionWizardEnabled()
		{
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			var mockData = new List<string> { "ENT" };

			mockFeatureControlManager.Setup(m => m.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CR5ResolutionWizard, CancellationToken.None))
									 .Returns(Task.FromResult(mockFeatureData.Object)).Verifiable();

			mockFeatureData.Setup(m => m.TryDeserializeParameterAsJson(out It.Ref<IEnumerable<string>>.IsAny))
						   .Returns(true)
						   .Callback((out IEnumerable<string> result) =>
						   {
							   result = mockData;
						   });

			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			using (ObjectFactory.Substitute(mockFeatureData.Object))
			{
				var incident = Factory.New<SupportIncident>();
				incident.IM_Priority = "CR5";
				incident.IM_Product = "ENT";

				AssertEquals(true, incident.IsResolutionWizardEnabled);
			}
		}

		public void TestResolutionWizardForm_InitialForm()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR5";
			var action = new SupportIncidentResolutionWizardAction(incident);

			using (var form = new ResolutionWizardFormForTest(action))
			{
				form.Show();
				AssertEquals(true, form.PleaseChooseOptionsGroupBoxForTest.Visible);
				AssertEquals(false, form.CompletelySolvedRadioButtonForTest.Checked);
				AssertEquals(false, form.PartlySolvedRadioButtonForTest.Checked);
				AssertEquals(false, form.ContentNotBeFoundRadioButtonForTest.Checked);
			}
		}

		public void TestResolutionWizardForm_WhenClickPartlySolvedRadioButton()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR5";
			var action = new SupportIncidentResolutionWizardAction(incident);

			using (var form = new ResolutionWizardFormForTest(action))
			{
				form.Show();
				var tableLayoutPanel = form.Controls.Find("MainTableLayoutPanel", true)[0] as KTableLayoutPanel;
				var flowLayoutPanel = tableLayoutPanel.Controls.Find("MainFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var pleaseChooseOptionsGroupBox = tableLayoutPanel.Controls.Find("PleaseChooseOptionsGroupBox", true)[0] as ZGroupBox;
				var pleaseChooseOptionsFlowLayoutPanel = pleaseChooseOptionsGroupBox.Controls.Find("PleaseChooseOptionsFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var partlySolvedRadioButton = pleaseChooseOptionsFlowLayoutPanel.Controls.Find("PartlySolvedRadioButton", true)[0] as ZRadioButton;

				partlySolvedRadioButton.PerformClick();
				AssertEquals(true, form.PleaseChooseOptionsGroupBoxForTest.Visible);
				AssertEquals(true, form.ShouldContentBeDevelopedGroupBox.Visible);
				AssertEquals(true, action.PartlySolvedOption);
				var shouldContentBeDevelopedGroupBox = flowLayoutPanel.Controls.Find("ShouldContentBeDevelopedGroupBox", true)[0] as ZGroupBox;
				var shouldContentBeDevelopedFlowLayoutPanel = shouldContentBeDevelopedGroupBox.Controls.Find("ShouldContentBeDevelopedFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var contentBeDevelopedNotWorthRadioButton = shouldContentBeDevelopedFlowLayoutPanel.Controls.Find("ContentBeDevelopedNotWorthRadioButton", true)[0] as ZRadioButton;
				contentBeDevelopedNotWorthRadioButton.PerformClick();
				AssertEquals(true, form.PleaseProvideLinkGroupBox.Visible);
				AssertEquals(true, form.PleaseProvideReasonGroupBox.Visible);
				AssertEquals(true, action.ContentBeDevelopNotWorthOption);
				AssertEquals(true, form.CustomerFacingResolutionMessageGroupBox.Visible);
			}
		}

		public void TestResolutionWizardForm_WhenClickContentNotBeFoundRadioButton()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR5";
			var action = new SupportIncidentResolutionWizardAction(incident);

			using (var form = new ResolutionWizardFormForTest(action))
			{
				form.Show();
				var tableLayoutPanel = form.Controls.Find("MainTableLayoutPanel", true)[0] as KTableLayoutPanel;
				var flowLayoutPanel = tableLayoutPanel.Controls.Find("MainFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var pleaseChooseOptionsGroupBox = tableLayoutPanel.Controls.Find("PleaseChooseOptionsGroupBox", true)[0] as ZGroupBox;
				var pleaseChooseOptionsFlowLayoutPanel = pleaseChooseOptionsGroupBox.Controls.Find("PleaseChooseOptionsFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var contentNotBeFoundRadioButton = pleaseChooseOptionsFlowLayoutPanel.Controls.Find("contentNotBeFoundRadioButton", true)[0] as ZRadioButton;
				contentNotBeFoundRadioButton.PerformClick();
				AssertEquals(true, form.PleaseChooseOptionsGroupBoxForTest.Visible);
				AssertEquals(true, form.ShouldContentBeDevelopedGroupBox.Visible);
				AssertEquals(true, action.ContentCouldNotBeFoundOption);
				var shouldContentBeDevelopedGroupBox = flowLayoutPanel.Controls.Find("ShouldContentBeDevelopedGroupBox", true)[0] as ZGroupBox;
				var shouldContentBeDevelopedFlowLayoutPanel = shouldContentBeDevelopedGroupBox.Controls.Find("ShouldContentBeDevelopedFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var contentBeDevelopYesRadioButton = shouldContentBeDevelopedFlowLayoutPanel.Controls.Find("contentBeDevelopYesRadioButton", true)[0] as ZRadioButton;
				contentBeDevelopYesRadioButton.PerformClick();
				AssertEquals(true, form.WhatChangesGroupBox.Visible);
				AssertEquals(true, form.HowCanWeGroupBox.Visible);
				AssertEquals(true, action.ContentBeDevelopYesOption);
				AssertEquals(true, form.CustomerFacingResolutionMessageGroupBox.Visible);
			}
		}

		public void TestResolutionWizardForm_SaveCloseEnabledWhenIncidentActionIsPrepopulated()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR5";
			var action = new SupportIncidentResolutionWizardAction(incident);
			action.ContentIsIrrelevantOption = true;
			action.ContentCouldNotBeFoundOption = true;
			action.ResolutionMethod = "SRS";

			using (var form = new ResolutionWizardFormForTest(action))
			{
				AssertEquals("Close button should be enabled", true, form.CloseButton_Exposed.Enabled);
			}
		}

		public void TestEConversationTextForm_WhenClickPopUpEConversationButton()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR5";
			var action = new SupportIncidentResolutionWizardAction(incident);

			using (var form = new ResolutionWizardFormForTest(action))
			{
				form.Show();
				var tableLayoutPanel = form.Controls.Find("MainTableLayoutPanel", true)[0] as KTableLayoutPanel;
				var flowLayoutPanel = tableLayoutPanel.Controls.Find("MainFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var pleaseChooseOptionsGroupBox = tableLayoutPanel.Controls.Find("PleaseChooseOptionsGroupBox", true)[0] as ZGroupBox;
				var pleaseChooseOptionsFlowLayoutPanel = pleaseChooseOptionsGroupBox.Controls.Find("PleaseChooseOptionsFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var contentNotBeFoundRadioButton = pleaseChooseOptionsFlowLayoutPanel.Controls.Find("contentNotBeFoundRadioButton", true)[0] as ZRadioButton;
				contentNotBeFoundRadioButton.PerformClick();
				AssertEquals(true, form.PleaseChooseOptionsGroupBoxForTest.Visible);
				AssertEquals(true, form.ShouldContentBeDevelopedGroupBox.Visible);
				AssertEquals(true, action.ContentCouldNotBeFoundOption);
				var shouldContentBeDevelopedGroupBox = flowLayoutPanel.Controls.Find("ShouldContentBeDevelopedGroupBox", true)[0] as ZGroupBox;
				var shouldContentBeDevelopedFlowLayoutPanel = shouldContentBeDevelopedGroupBox.Controls.Find("ShouldContentBeDevelopedFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var contentBeDevelopYesRadioButton = shouldContentBeDevelopedFlowLayoutPanel.Controls.Find("contentBeDevelopYesRadioButton", true)[0] as ZRadioButton;
				contentBeDevelopYesRadioButton.PerformClick();
				AssertEquals(true, form.WhatChangesGroupBox.Visible);
				AssertEquals(true, form.HowCanWeGroupBox.Visible);
				AssertEquals(true, action.ContentBeDevelopYesOption);
				AssertEquals(true, form.CustomerFacingResolutionMessageGroupBox.Visible);

				var popupEConversationButton = form.Controls.Find("PopUpEConversationButton", true)[0] as ZButton;
				popupEConversationButton.PerformClick();
				var ownedForms = form.OwnedForms;
				AssertEquals(1, ownedForms.Length);

				AssertType(typeof(EConversationTextForm), ownedForms[0]);
				var eConversationTextForm = (EConversationTextForm)ownedForms[0];
				var supportIncident = (SupportIncident)eConversationTextForm.LastDataSourceForTest;
				AssertNotNull("SupportIncident-dataSource", supportIncident);

				form.Close();
				AssertEquals("The econversation pop up also close", 0, form.OwnedForms.Length);
			}
		}

		public void TestLayoutChangeShouldReCalcMinimumAndMaximumSize()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR5";
			var action = new SupportIncidentResolutionWizardAction(incident);
			using (var form = new ResolutionWizardFormForTest(action))
			{
				form.Show();
				var originalMinimumHeight = form.MinimumSize.Height;
				var originalMaximumHeight = form.MaximumSize.Height;
				var tableLayoutPanel = form.Controls.Find("MainTableLayoutPanel", true)[0] as KTableLayoutPanel;
				var flowLayoutPanel = tableLayoutPanel.Controls.Find("MainFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var pleaseChooseOptionsGroupBox = tableLayoutPanel.Controls.Find("PleaseChooseOptionsGroupBox", true)[0] as ZGroupBox;
				var pleaseChooseOptionsFlowLayoutPanel = pleaseChooseOptionsGroupBox.Controls.Find("PleaseChooseOptionsFlowLayoutPanel", true)[0] as KFlowLayoutPanel;
				var contentNotBeFoundRadioButton = pleaseChooseOptionsFlowLayoutPanel.Controls.Find("contentNotBeFoundRadioButton", true)[0] as ZRadioButton;
				contentNotBeFoundRadioButton.PerformClick();
				AssertNotEquals("MinimumHeight should recalculate when click contentNotBeFoundRadioButton change the layout", originalMinimumHeight, form.MinimumSize.Height);
				AssertNotEquals("MaximumHeight should recalculate when click contentNotBeFoundRadioButton change the layout", originalMaximumHeight, form.MaximumSize.Height);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentResolutionWizardAction(incident);
			return new ResolutionWizardForm(action);
		}

		class ResolutionWizardFormForTest : ResolutionWizardForm
		{
			public ResolutionWizardFormForTest(SupportIncidentResolutionWizardAction action) : base(action)
			{
			}

			public ZButton CloseButton_Exposed => CloseButton;
			public ZGroupBox PleaseChooseOptionsGroupBoxForTest => PleaseChooseOptionsGroupBox;
			public ZRadioButton CompletelySolvedRadioButtonForTest => CompletelySolvedRadioButton;
			public ZRadioButton PartlySolvedRadioButtonForTest => PartlySolvedRadioButton;
			public ZRadioButton ContentNotBeFoundRadioButtonForTest => ContentNotBeFoundRadioButton;
		}
	}
}
