using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class GuidedDecisionMakingForm : ZChildForm
	{
		public GuidedDecisionMakingForm(GuidedDecisionMakingBasic gDMBasic, IGuidedDecisionMakingTarget guidedDecisionMakingTarget)
			: base(gDMBasic)
		{
			GDMBasicLayoutPanel.UpdateLayout(gDMBasicLayout);
			gdmTarget = guidedDecisionMakingTarget;
			GDMBasic.HasChangesChanged += OnGDMBasicHasChangesChanged;
			InitializeGuidedDecisionMakingTabsManagement();
			RefreshTabContent();
			SelectAllWaiversLinkLabel.Text = GDMBasic.SelectAllWaiversCaption;

			NavigationPanel.AllowOverlap(GuidedDecisionMakingTabControl);
		}

		protected virtual IPanelLayoutProvider gDMBasicLayout => new GDMBasicLayout();

		protected virtual Type ExpectedDataSourceType => typeof(GuidedDecisionMakingBasic);

		GuidedDecisionMakingBasic GDMBasic => BusinessEntity as GuidedDecisionMakingBasic;

		protected IGuidedDecisionMakingTarget gdmTarget;

		protected virtual SummaryControl GetSummaryControl() => new SummaryControl();

		public override string FormVerb => string.Empty;

		void GuidedDecisionMakingTabControl_DrawItem(object sender, DrawItemEventArgs e)
		{
#if !WINZOR
			var tab = (ZTabPage)GuidedDecisionMakingTabControl.TabPages[e.Index];
			var gdmTab = gdmTabsManagement.FindGuidedDecisionMakingTab(tab);
			if (gdmTab != null)
			{
				var status = gdmTab.Status;
				using var brBack = new SolidBrush(gdmTab.CaptionBackground);
				var caption = gdmTab.FullCaption;

				e.Graphics.FillRectangle(brBack, e.Bounds);
				var textSize = e.Graphics.MeasureString(caption, e.Font);

				var textX = e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2;
				var textY = e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2 + 1;
				e.Graphics.DrawString(caption, e.Font, Brushes.Black, textX, textY);

				var icon = gdmTab.Icon;
				if (icon != null)
				{
					var iconX = textX + textSize.Width + 1;
					var iconY = e.Bounds.Top + (e.Bounds.Height - icon.Height) / 2 + 1;
					e.Graphics.DrawIcon(icon, (int)iconX, iconY);
				}

				Rectangle rect = e.Bounds;
				rect.Offset(0, 1);
				rect.Inflate(0, -1);
				e.Graphics.DrawRectangle(Pens.DarkGray, rect);
				e.DrawFocusRectangle();

				var imageIndex = tab.ImageIndex;
				if (imageIndex >= 0 && GuidedDecisionMakingTabControl.ImageList?.Images is ImageList.ImageCollection images && imageIndex < images.Count)
				{
					var image = images[imageIndex];
					var imageX = textX - image.Width - 1;
					var imageY = e.Bounds.Top + (e.Bounds.Height - image.Height) / 2 + 1;
					e.Graphics.DrawImage(image, imageX, imageY);
				}
			}
#endif
		}

		protected GuidedDecisionMakingTabsManagement gdmTabsManagement;

		void InitializeGuidedDecisionMakingTabsManagement()
		{
			gdmTabsManagement = new GuidedDecisionMakingTabsManagement(GuidedDecisionMakingTabControl);

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Caption = Res.GetString("3BA68CF8-4D31-48D0-B797-30A23A9A59D0", "1. Basic"),
				Tab = BasicTabPage,
				CanNavigateToNext = () =>
				{
					if (GDMBasic.HasErrors)
					{
						Globals.Message.Show((string)ResString.GetMultilingualString("2E198D5A-CFB0-40E0-B3FA-3C1E1AF2F86A", "You must fix errors before switching to next tab."), ResString.GetMultilingualString("3CA19053-FC26-4D63-A46E-E65E8237FE15", "Errors"), MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						return true;
					}
				}
			});

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Caption = Res.GetString("D14054B8-0FD8-42FB-9A8F-ABC4492BB96A", "2. VAT"),
				Tab = VATTabPage,
				IsApplicable = () => GDMBasic.IsVATApplicable,
				GetUnsatisfiedGroupDescriptions = () => GetVatUnsatisfiedGroupDescriptions(),
				CanNavigateToNext = () =>
				{
					var result = true;
					var message = GetVatUnsatisfiedGroupDescriptions();
					if (!message.IsEmpty)
					{
						message = $"{message}\r\n{doYouWantToContinue}";
						var queryUserResult = Globals.Message.Show(message, conditionIncompleteCaptureCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (queryUserResult == DialogResult.No)
						{
							result = false;
						}
					}
					return result;
				}
			});

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Caption = Res.GetString("D07A05CD-74C6-45B5-9B32-DD6412949C0B", "3. Additional Codes"),
				Tab = AdditionalCodesTabPage,
				IsApplicable = () => GDMBasic.AdditionalCodes.Any(),
				GetUnsatisfiedGroupDescriptions = () => GetAdditionalCodesUnsatisfiedGroupDescriptions(),
				CanNavigateToNext = () =>
				{
					ZBool result = true;

					var descriptions = GetAdditionalCodesUnsatisfiedGroupDescriptions();

					if (descriptions != ZString.Empty)
					{
						var message = $"{additionalCodeIncompleteCaptureBaseMessage}\r\n{descriptions}\r\n{doYouWantToContinue}";
						var queryUserResult = Globals.Message.Show(message, additionalCodeIncompleteCaptureCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (queryUserResult == DialogResult.No)
						{
							result = false;
						}
					}
					return result;
				}
			});

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Caption = Res.GetString("2FB1BAE0-38DE-4393-972C-E6175B842D1F", "4. Meursing"),
				Tab = MeursingTabPage,
				IsApplicable = () => GDMBasic.IsMeursingApplicable,
				GetUnsatisfiedGroupDescriptions = () => GetMeursingUnsatisfiedGroupDescriptions(),
				CanNavigateToNext = () =>
				{
					ZBool result = true;
					var message = GetMeursingUnsatisfiedGroupDescriptions();
					if (!message.IsEmpty)
					{
						var queryUserResult = Globals.Message.Show(message, meursingIncompleteCaptureCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (queryUserResult == DialogResult.No)
						{
							result = false;
						}
					}
					return result;
				}
			});

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Caption = Res.GetString("17889DBB-51BD-4A21-B179-B4A4E8FC7842", "5. Conditions"),
				Tab = ConditionsTabPage,
				IsApplicable = () => GDMBasic.DocumentConditions.Count > 0,
				GetUnsatisfiedGroupDescriptions = () => GetConditionsUnsatisfiedGroupDescriptions(),
				CanNavigateToNext = () =>
				{
					ZBool result = true;

					var message = GetConditionsUnsatisfiedGroupDescriptions();
					if (!message.IsEmpty)
					{
						message = $"{message}\r\n{doYouWantToContinue}";
						var queryUserResult = Globals.Message.Show(message, conditionIncompleteCaptureCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (queryUserResult == DialogResult.No)
						{
							result = false;
						}
					}
					return result;
				}
			});

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Caption = Res.GetString("0EC5DE96-F807-441A-8BB9-32A4FC7D1892", "6. Summary"),
				Tab = SummaryTabPage
			});

			this.SelectAllWaiversLinkLabel.Visible = ShouldShowSelectAllWaiversButton();

			InvalidateTabControl();
		}

		ZString GetAdditionalCodesUnsatisfiedGroupDescriptions()
		{
			var descriptions = ZString.Empty;

			foreach (var groupedAdditionalCodes in GDMBasic.GroupedAdditionalCodes)
			{
				if (groupedAdditionalCodes.All(x => !x.IsTicked))
				{
					descriptions += $"- {groupedAdditionalCodes.First().ApplicableToDescription}\r\n";
				}
			}

			return descriptions;
		}
		ZString GetMeursingUnsatisfiedGroupDescriptions()
			=> GDMBasic.IsMeursingApplicable && GDMBasic.MeursingResult.IsEmpty ? meursingIncompleteCaptureMessage : ZString.Empty;

		ZBool ShouldShowSelectAllWaiversButton()
		{
			var conditionGroups = GDMBasic.DocumentConditions;
			return conditionGroups.Cast<GuidedDecisionMakingCondition>().SelectMany(x => x.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>()).Any(x => GDMBasic.Configuration.IsAWaiver(x.Code));
		}

		ZString GetConditionsUnsatisfiedGroupDescriptions()
		{
			var message = ZString.Empty;
			if (GDMBasic.DocumentConditions.Count > 0)
			{
				var referenceNotProvidedMessage = ZString.Empty;
				var noConditionsChosenMessage = ZString.Empty;
				foreach (var condition in GDMBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>())
				{
					var details = condition.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>();
					if (details.All(x => !x.IsSatisfied))
					{
						if (details.All(x => !x.IsTicked) && !condition.IsInformationCondition)
						{
							noConditionsChosenMessage += $"- {condition.ConditionType}, {condition.ConditionTypeDescription}\r\n";
						}
						else if (details.Any(x => x.Type != Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber && x.Reference.IsEmpty))
						{
							referenceNotProvidedMessage += $"- {condition.ConditionType}, {condition.ConditionTypeDescription}\r\n";
						}
					}
				}
				if (!referenceNotProvidedMessage.IsEmpty)
				{
					message += $"{conditionIncompleteForNoReferenceProvidedMessage}\r\n{referenceNotProvidedMessage}";
				}
				if (!noConditionsChosenMessage.IsEmpty)
				{
					message += $"{conditionIncompleteForNoConditionsChosenMessage}\r\n{noConditionsChosenMessage}";
				}
			}
			return message;
		}

		ZString GetVatUnsatisfiedGroupDescriptions()
		{
			var vats = GDMBasic.VATApplicabilities;
			if (vats.Count > 0 && vats.Cast<GuidedDecisionMakingVAT>().All(v => !v.IsTicked))
			{
				return $"{vatIncompleteCaptureMessage}";
			}
			return ZString.Empty;
		}

		void GuidedDecisionMakingTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			var tab = (ZTabPage)e.TabPage;
			var gdmTab = gdmTabsManagement.FindGuidedDecisionMakingTab(tab);
			if (gdmTab != null && gdmTab.Status == GuidedDecisionMakingTabStatus.NotApplicable)
			{
				e.Cancel = true;
			}
		}

		void GuidedDecisionMakingTabControl_Selected(object sender, TabControlEventArgs e)
		{
			RefreshTabContent();
			InvalidateTabControl();
		}

		void RefreshTabContent()
		{
			if (GuidedDecisionMakingTabControl.SelectedTab == AdditionalCodesTabPage)
			{
				AdditionalCodesContainerPanel.PopulateAdditionalCodeControls(GDMBasic);
			}
			else if (GuidedDecisionMakingTabControl.SelectedTab == ConditionsTabPage)
			{
				DocumentConditionsContainerPanel.PopulateDocumentConditionControls(GDMBasic);
			}
			else if (GuidedDecisionMakingTabControl.SelectedTab == VATTabPage)
			{
				VATContainerPanel.PopulateVATControl(GDMBasic);
			}
			else if (GuidedDecisionMakingTabControl.SelectedTab == SummaryTabPage)
			{
				SummaryControl.PopulateSummaryControls(GDMBasic);
			}
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			gdmTabsManagement.NavigateToNext();
		}

		void PreviousButton_Click(object sender, EventArgs e)
		{
			gdmTabsManagement.NavigateToPrevious();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			GuidedDecisionMakingTabControl.Dispose();
			this.Dispose();
		}

		void DoneButton_Click(object sender, EventArgs e)
		{
			GDMBasic.TransferToTarget(gdmTarget);
			this.GuidedDecisionMakingTabControl.Dispose();
			this.Dispose();
		}

		void OnGDMBasicHasChangesChanged(object sender, EventArgs e)
		{
			InvalidateTabControl();
		}

		void InvalidateTabControl()
		{
			gdmTabsManagement.InvalidateTabControl();

			RefreshButtons();
		}

		void RefreshButtons()
		{
			PreviousButton.Visible = gdmTabsManagement.HasPreviousTabs;
			PreviousButton.Text = gdmTabsManagement.PreviousButtonText;
			NextButton.Visible = gdmTabsManagement.HasNextTabs;
			NextButton.Text = gdmTabsManagement.NextButtonText;
			DoneButton.Visible = !gdmTabsManagement.HasNextTabs;
		}

		void SelectAllWaiversLinkLabelClicked(object sender, EventArgs e)
		{
			SelectWaivers();
		}

		protected void SelectWaivers()
		{
			var conditionGroups = GDMBasic.DocumentConditions;

			foreach (GuidedDecisionMakingCondition conditionGroup in conditionGroups)
			{
				var conditionDetails = conditionGroup.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList();

				var waiverDetails = conditionDetails.Where(x => GDMBasic.Configuration.IsAWaiver(x.Code)).ToList();
				var nonWaiverDetails = conditionDetails.Where(x => !GDMBasic.Configuration.IsAWaiver(x.Code)).ToList();

				if (waiverDetails.Count == 1 && nonWaiverDetails.Any())
				{
					waiverDetails.First().IsTicked = true;
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				if (GDMBasic != null)
				{
					GDMBasic.HasChangesChanged -= OnGDMBasicHasChangesChanged;
				}
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		readonly ZString additionalCodeIncompleteCaptureCaption = ResString.GetMultilingualString("A82983F1-D0EE-42EC-88F5-570B5DCAA066", "Additional code capture is incomplete.");

		readonly ZString additionalCodeIncompleteCaptureBaseMessage = ResString.GetMultilingualString("056E4D45-4520-41B2-885C-DB27BA0D77CF", "You didn't choose any additional code for");

		readonly ZString meursingIncompleteCaptureCaption = ResString.GetMultilingualString("7C3FCB04-9932-4CBD-8869-1C6BDAFA02D1", "Meursing capture is incomplete.");

		readonly ZString vatIncompleteCaptureMessage = ResString.GetMultilingualString("2BE67B6F-8D48-4138-862E-4892C0648168", "You didn't choose any VAT code.");

		readonly ZString meursingIncompleteCaptureMessage = ResString.GetMultilingualString("9BF6B447-AB19-4084-9D5C-ACD5E6002A65", "A supplementary code matching pattern '7NNN' should exist for Meursing duty calculation purposes, do you want to continue?");

		readonly ZString conditionIncompleteCaptureCaption = ResString.GetMultilingualString("63934CD6-5985-41D8-9C14-B7338050A5C2", "Condition capture is incomplete.");

		readonly ZString conditionIncompleteForNoReferenceProvidedMessage = ResString.GetMultilingualString("3A9D52A8-1864-4C5B-A99E-A8A222FE60BF", "You didn't fill in document reference for");

		readonly ZString conditionIncompleteForNoConditionsChosenMessage = ResString.GetMultilingualString("FE6BB5C4-BD07-43D4-8C26-67D306DD0D09", "You didn't choose any conditions for");

		readonly ZString doYouWantToContinue = ResString.GetMultilingualString("31C26611-C146-4C2A-9D54-9E1B1088B15A", "Do you want to continue?");
	}
}
