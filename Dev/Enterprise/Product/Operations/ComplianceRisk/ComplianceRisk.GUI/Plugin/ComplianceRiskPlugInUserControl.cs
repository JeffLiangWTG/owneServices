using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class ComplianceRiskPlugInUserControl : ZUserControl
	{
		readonly ComplianceRiskPlugInBusinessObject ComplianceRiskPluginBizO;
		readonly int originalSplitterDistance;
		readonly int originalSubSplitterDistance;

		internal readonly int collapsedPanelMinHeight = 40;
		internal readonly int defaultPanelMinHeight = 90;
		internal readonly int defaultCommodityPanelMinHeight = 230;

		public ComplianceRiskPlugInUserControl(ComplianceRiskPlugInBusinessObject complianceRiskPluginBizO)
		{
			ComplianceRiskPluginBizO = complianceRiskPluginBizO;
			BindPlugInBusinessObject(ComplianceRiskPluginBizO, dataMember: "");
			InitializeComponent();
			AddUserControls();
			SetRiskProvidersVisible();

			originalSplitterDistance = SplitContainer.SplitterDistance;
			originalSubSplitterDistance = SubSplitContainer.SplitterDistance;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is null)
			{
				base.SetDataBinding(dataSource: null, dataMember: "");
			}
		}

		internal void SetRiskLabelBackColor()
		{
			OverallRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskPluginBizO.ComplianceRiskStatus.COR_OverallRisk);
			PartyRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskPluginBizO.ComplianceRiskStatus.COR_PartyRisk);
			LocationRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskPluginBizO.ComplianceRiskStatus.COR_LocationRisk);
			CommodityRiskLabel.BackColor = ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskPluginBizO.ComplianceRiskStatus.COR_CommodityRisk);
		}

		void AddUserControls()
		{
			PartyTableLayoutPanel.Controls.Add(new PartyRiskUserControl(ComplianceRiskPluginBizO) { Dock = DockStyle.Fill }, 0, 1);
			LocationTableLayoutPanel.Controls.Add(new LocationRiskUserControl(ComplianceRiskPluginBizO) { Dock = DockStyle.Fill }, 0, 1);

			CommodityTableLayoutPanel.Controls.Add(new CommodityAssessmentPanelUserControl(ComplianceRiskPluginBizO) { Dock = DockStyle.Fill }, 0, 1);

			ComplianceRiskPluginBizO.ComplianceRiskSpinnerIndicatorVisibility = (bool visible) =>
			{
				if (!IsDisposing && !IsDisposed)
				{
					CommoditySpinnerIndicator.Visible = visible;
				}
			};

			if (ComplianceRiskPluginBizO.ComplianceItemRiskStatusProvider.ComplianceRiskSupport.IsSupportInitialization())
			{
				AssessmentInitializeButton.Click += async (sender, args) =>
				{
					var jobForm = FindForm() as ISupportSwitchTabPage;
					await ComplianceRiskStatusFormSynchronizer.InitializeComplianceAssessmentWhenMenuClick(ComplianceRiskPluginBizO, jobForm);
				};
			}
			else
			{
				AssessmentInitializeButton.Visible = false;
			}
		}

		void SetRiskProvidersVisible()
		{
			PartyGroupBox.Visible = ComplianceRiskPluginBizO.CompliancePartyRiskStatusProvider != null;
			LocationGroupBox.Visible = ComplianceRiskPluginBizO.ComplianceLocationRiskStatusProvider != null;
			CommodityGroupBox.Visible = ComplianceRiskPluginBizO.ComplianceCommodityRiskStatusProvider != null;
		}

		internal static ZUserControl CreateCommodityRiskUserControl(ComplianceRiskPlugInBusinessObject compliancePluginBizO)
		{
			var commodityRiskUserControl = ObjectFactory.Get<ICommodityRiskUserControl>("ICommodityRiskUserControl", compliancePluginBizO) as ZUserControl;
			commodityRiskUserControl.Dock = DockStyle.Fill;
			commodityRiskUserControl.Name = "CommodityRiskUserControl";

			ValidateSecurityCheckPoint(commodityRiskUserControl, compliancePluginBizO);

			return commodityRiskUserControl;
		}

		static void ValidateSecurityCheckPoint(ZUserControl commodityRiskUserControl, ComplianceRiskPlugInBusinessObject compliancePluginBizO)
		{
			var securityEditHarmonizedCode = ComplianceRiskSecurityRights.GetSecurityCheckPointEditHarmonizedCode(compliancePluginBizO.HostBusinessEntity);
			if (securityEditHarmonizedCode != null)
			{
				((ICommodityRiskUserControl)commodityRiskUserControl).ValidateSecurityEditHarmonizedCode = () =>
				{
					return ProcessSecurityCheckPoint(securityEditHarmonizedCode);
				};
			}

			var securityEditComplianceAssessment = ComplianceRiskSecurityRights.GetSecurityCheckPointEditComplianceAssessment(compliancePluginBizO.HostBusinessEntity);
			if (securityEditComplianceAssessment != null)
			{
				((ICommodityRiskUserControl)commodityRiskUserControl).ValidateSecurityEditComplianceAssessment = () =>
				{
					return ProcessSecurityCheckPoint(securityEditComplianceAssessment);
				};
			}

			static bool ProcessSecurityCheckPoint(SecurityCheckpoint securityCheckpoint)
			{
				if (!securityCheckpoint.IsAllowed)
				{
					securityCheckpoint.ShowError();
					return false;
				}
				return true;
			}
		}

		void BindPlugInBusinessObject(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
		}

		void RiskStatusValueChanged(object sender, EventArgs e)
		{
			SetRiskLabelBackColor();
		}

		void SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (SplitContainer.SplitterDistance - SubSplitContainer.SplitterDistance - SubSplitContainer.SplitterWidth < SubSplitContainer.Panel2MinSize)
			{
				var newSubSplitterDistance = (SplitContainer.SplitterDistance - SubSplitContainer.SplitterWidth) / 2;
				if (newSubSplitterDistance >= SubSplitContainer.Panel1MinSize)
				{
					SubSplitContainer.SplitterDistance = newSubSplitterDistance;
				}
			}
		}

		void HideCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateRiskFactorControlsVisibility();
		}

		internal void UpdateRiskFactorControlsVisibility()
		{
			SplitContainer.SplitterMoved -= SplitterMoved;

			// reset availability
			PartyHideCheckBox.Enabled = true;
			LocationHideCheckBox.Enabled = true;
			CommodityHideCheckBox.Enabled = true;

			// setup availability
			if (SplitContainer.Panel2Collapsed)
			{
				if (PartyHideCheckBox.Checked)
				{
					LocationHideCheckBox.Enabled = false;
				}
				if (LocationHideCheckBox.Checked)
				{
					PartyHideCheckBox.Enabled = false;
				}
			}
			else
			{
				if (PartyHideCheckBox.Checked && LocationHideCheckBox.Checked)
				{
					CommodityHideCheckBox.Enabled = false;
				}
				else if (PartyHideCheckBox.Checked && CommodityHideCheckBox.Checked)
				{
					LocationHideCheckBox.Enabled = false;
				}
				else if (LocationHideCheckBox.Checked && CommodityHideCheckBox.Checked)
				{
					PartyHideCheckBox.Enabled = false;
				}
			}

			// reset heights
			SplitContainer.IsSplitterFixed = false;
			SubSplitContainer.IsSplitterFixed = false;

			SetPanel1MinSizeSafe(SplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(190));
			SetPanel2MinSizeSafe(SplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(defaultCommodityPanelMinHeight));
			SetSplitterDistanceSafe(SplitContainer, originalSplitterDistance);

			SetPanel1MinSizeSafe(SubSplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(defaultPanelMinHeight));
			SetPanel2MinSizeSafe(SubSplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(defaultPanelMinHeight));
			SetSplitterDistanceSafe(SubSplitContainer, originalSubSplitterDistance);

			// setup heights
			if (CommodityHideCheckBox.Checked)
			{
				SetPanel2MinSizeSafe(SplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(collapsedPanelMinHeight));
				SetSplitterDistanceSafe(SplitContainer, SplitContainer.Height - SplitContainer.Panel2MinSize);
				SplitContainer.IsSplitterFixed = true;
			}

			if (PartyHideCheckBox.Checked && LocationHideCheckBox.Checked)
			{
				SetPanel1MinSizeSafe(SubSplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(collapsedPanelMinHeight));
				SetPanel2MinSizeSafe(SubSplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(collapsedPanelMinHeight));
				SetSplitterDistanceSafe(SubSplitContainer, Math.Min(SubSplitContainer.Height - SubSplitContainer.Panel2MinSize, SubSplitContainer.Panel1MinSize));
				SubSplitContainer.IsSplitterFixed = true;

				SetPanel1MinSizeSafe(SplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(92));
				SetSplitterDistanceSafe(SplitContainer, Math.Min(SplitContainer.Height - SplitContainer.Panel2MinSize, SplitContainer.Panel1MinSize));
				SplitContainer.IsSplitterFixed = true;
			}
			else
			{
				if (PartyHideCheckBox.Checked)
				{
					SetPanel1MinSizeSafe(SubSplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(collapsedPanelMinHeight));
					SetSplitterDistanceSafe(SubSplitContainer, Math.Min(SubSplitContainer.Height - SubSplitContainer.Panel2MinSize, SubSplitContainer.Panel1MinSize));
					SubSplitContainer.IsSplitterFixed = true;
				}

				if (LocationHideCheckBox.Checked)
				{
					SetPanel2MinSizeSafe(SubSplitContainer, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(collapsedPanelMinHeight));
					SetSplitterDistanceSafe(SubSplitContainer, SubSplitContainer.Height - SubSplitContainer.Panel2MinSize);
					SubSplitContainer.IsSplitterFixed = true;
				}
			}

			SplitContainer.SplitterMoved += SplitterMoved;
		}

		static void SetSplitterDistanceSafe(SplitContainer splitContainer, int splitterDistance)
		{
			// Ensure the splitterDistance is within the valid range before setting it
			if (splitterDistance > 0
				&& splitterDistance >= splitContainer.Panel1MinSize)
			{
				var maxDistance = splitContainer.Height - splitContainer.Panel2MinSize;

				if (splitterDistance <= maxDistance)
				{
					splitContainer.SplitterDistance = splitterDistance;
				}
				else if (maxDistance > 0)
				{
					splitContainer.SplitterDistance = maxDistance;
				}
			}
		}

		static void SetPanel1MinSizeSafe(SplitContainer splitContainer, int panel1MinSize)
		{
			if (panel1MinSize <= splitContainer.Height - splitContainer.Panel2MinSize)
			{
				splitContainer.Panel1MinSize = panel1MinSize;
			}
		}

		static void SetPanel2MinSizeSafe(SplitContainer splitContainer, int panel2MinSize)
		{
			if (panel2MinSize <= splitContainer.Height - splitContainer.Panel1MinSize)
			{
				splitContainer.Panel2MinSize = panel2MinSize;
			}
		}
	}
}
