using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Codes;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceRiskPresentationHelper
	{
		/// <summary>
		/// Init a GUI label which will display whether current job has potential risk.
		/// Please see "<see cref="https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/4042/ComplianceWise-Plug-in"/>" for more details.
		/// </summary>
		/// <param name="parentForm">Parent from which hosts current label.</param>
		public static void AddComplianceRiskWarningMessageBannerIfNeeded(ZForm parentForm)
		{
			if (parentForm.BusinessEntity is IComplianceItemRiskStatusProvider riskStatusProvider
				&& riskStatusProvider.IsEnabledComplianceWise
				&& ComplianceRiskHelper.IsComplianceWarningMessageEnabled)
			{
				var complianceRiskMessageBanner = new ZLabel();
				complianceRiskMessageBanner.AutoSize = true;
				complianceRiskMessageBanner.BackColor = System.Drawing.Color.Transparent;
				complianceRiskMessageBanner.FontType = (ZArchitecture.Core.OFontTypes.Normal | ZArchitecture.Core.OFontTypes.Bold);
				complianceRiskMessageBanner.ForeColor = System.Drawing.Color.Red;
				complianceRiskMessageBanner.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
				complianceRiskMessageBanner.IsFontBold = true;
				complianceRiskMessageBanner.Name = "CompliancePotentialRiskMessageBanner";
				complianceRiskMessageBanner.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 14, true);
				complianceRiskMessageBanner.TabIndex = 0;
				complianceRiskMessageBanner.CaptionResourceString = Res.GetData("d493820e-1940-45bc-b04e-11354f8ad7e7", "Job Compliance status is not Clear. View the Compliance Risk tab.");
				complianceRiskMessageBanner.Text = Res.GetString("d493820e-1940-45bc-b04e-11354f8ad7e7", "Job Compliance status is not Clear. View the Compliance Risk tab.");
				complianceRiskMessageBanner.VisibleChanged += ComplianceRiskMessageBanner_VisibleChanged;

				parentForm.MainStatusBar.Controls.Add(complianceRiskMessageBanner);

				complianceRiskMessageBanner.Anchor = System.Windows.Forms.AnchorStyles.Right;
				complianceRiskMessageBanner.Visible = false;

				parentForm.Shown += UpdateComplianceRiskWarningMessageBannerIfNeeded;
			}
		}

		static void UpdateComplianceRiskWarningMessageBannerIfNeeded(object sender, System.EventArgs e)
		{
			if (sender is ZForm parentForm)
			{
				parentForm.Shown -= UpdateComplianceRiskWarningMessageBannerIfNeeded;
				var complianceRiskMessageBanner = parentForm.MainStatusBar.FindSingleOrDefault<ZLabel>(c => c.Name == "CompliancePotentialRiskMessageBanner");
				if (complianceRiskMessageBanner != null)
				{
					var complianceRiskStatus = (ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)parentForm.BusinessEntity));
					complianceRiskMessageBanner.Visible = parentForm.BusinessEntity.IsInDatabase
														  && IsRiskStatusNotClear(complianceRiskStatus.JobRisk);
				}
			}
		}

		static void ComplianceRiskMessageBanner_VisibleChanged(object sender, System.EventArgs e)
		{
			if (sender is ZLabel complianceRiskMessageBanner && complianceRiskMessageBanner.Parent is ZStatusBar parent && complianceRiskMessageBanner.Visible && complianceRiskMessageBanner.Width > 0)
			{
				var bannerPosition = parent.Width - complianceRiskMessageBanner.Width;
				complianceRiskMessageBanner.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(bannerPosition - 30, 7, isInStandardDpi: false);
			}
		}

		internal static bool IsRiskStatusNotClear(ZString status) => status.ToString() is PotentialRisk or Blocked or Held;
	}
}
