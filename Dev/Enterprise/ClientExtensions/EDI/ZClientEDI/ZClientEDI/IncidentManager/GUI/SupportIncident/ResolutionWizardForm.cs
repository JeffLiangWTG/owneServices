using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class ResolutionWizardForm : BaseIncidentPopupForm
	{
		public ResolutionWizardForm(SupportIncidentResolutionWizardAction action)
			: base(action)
		{
			spellChecker = SpellChecker.InitialiseSpellcheck(ResolutionMessageTextBox, "ResolutionWizardForm_ResolutionCommentTextBox");
			UpdateKnownNames();
			SetupCloseButton();
		}

		public ResolutionWizardForm()
		{
			spellChecker = SpellChecker.InitialiseSpellcheck(ResolutionMessageTextBox, "ResolutionWizardForm_ResolutionCommentTextBox");
		}

		protected const int InitialMinimumFormWidth = 820;

		protected const int InitialMaximumFormWidth = 1000;

		protected const int InitialMinimumFormHeight = 185;

		protected const int InitialMaximumFormHeight = 900;

		SupportIncidentResolutionWizardAction IncidentAction
		{
			get { return (SupportIncidentResolutionWizardAction)BusinessEntity; }
		}

		void SetupCloseButton()
		{
			SetupCloseButtonEnable(null, EventArgs.Empty);

			IncidentAction.CompletelySolvedOptionInfo.ValueChanged += SetupCloseButtonEnable;
			IncidentAction.ContentBeDevelopYesOptionInfo.ValueChanged += SetupCloseButtonEnable;
			IncidentAction.HighlyClientSpecificOptionInfo.ValueChanged += SetupCloseButtonEnable;
			IncidentAction.ComplexEdgeCaseOptionInfo.ValueChanged += SetupCloseButtonEnable;
			IncidentAction.OtherOptionInfo.ValueChanged += SetupCloseButtonEnable;
			IncidentAction.ResolutionMethodInfo.ValueChanged += SetupCloseButtonEnable;
		}

		void SetupCloseButtonEnable(object sender, EventArgs e)
		{
			bool solvedCondition = IncidentAction.PartlySolvedOption || IncidentAction.ContentCouldNotBeFoundOption;

			if (IncidentAction.CompletelySolvedOption ||
				(IncidentAction.ContentBeDevelopYesOption && solvedCondition) ||
				((IncidentAction.HighlyClientSpecificOption || IncidentAction.ComplexEdgeCaseOption || IncidentAction.OtherOption)
					&& IncidentAction.ContentBeDevelopNotWorthOption && solvedCondition) ||
				(!string.IsNullOrEmpty(IncidentAction.ResolutionMethod) && IncidentAction.ContentIsIrrelevantOption && solvedCondition))
			{
				this.CloseButton.Enabled = true;
			}
			else
			{
				this.CloseButton.Enabled = false;
			}
		}

		void UpdateKnownNames()
		{
			var knownNames = new List<string>();

			knownNames.Add(IncidentAction.Incident?.Contact?.Name);
			IncidentAction.Incident?.EConversation.ExistingConversation?.Staff.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			IncidentAction.Incident?.EConversation.ExistingConversation?.RelatedParties.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			IncidentAction.Incident?.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(task => knownNames.Add(task.StaffName));

			spellChecker.UpdateWordsToIgnore(knownNames.Where(name => !string.IsNullOrWhiteSpace(name)));
		}

		void ResolutionMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			int xmlCharsRemoved;
			var xmlEscapedText = ZXmlValidation.EscapeInvalidXmlCharacters(ResolutionMessageTextBox.Text, out xmlCharsRemoved);
			if (xmlCharsRemoved > 0)
			{
				var previousSelectionStart = ResolutionMessageTextBox.SelectionStart;
				ResolutionMessageTextBox.Text = xmlEscapedText;
				ResolutionMessageTextBox.SelectionStart = Math.Max(previousSelectionStart - xmlCharsRemoved, 0);
			}
		}

		protected override void CloseButtonClickCore(object sender, EventArgs e)
		{
			spellChecker.CheckSpelling();
			base.CloseButtonClickCore(sender, e);
		}

		public ZDialogResult ShowDialogAndDispose()
		{
			return (ZDialogResult)ZFormModaliser.ShowDialogAndDispose(this);
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
			this.CancelButtonX.AllowOverlap(this.MainFlowLayoutPanel);
			this.CancelButtonX.AllowOverlap(this.MainTableLayoutPanel);
			this.CloseButton.AllowOverlap(this.MainFlowLayoutPanel);
			this.CloseButton.AllowOverlap(this.MainTableLayoutPanel);
			this.PopUpEConversationButton.AllowOverlap(this.MainTableLayoutPanel);
			this.Shown += (sender, e) => AdjustFormHeight();
			this.BottomFlagLabel.LocationChanged += new EventHandler(this.BottomFlagLabel_LocationChanged);
			this.MessageLabel.Visible = false;
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			if (!IncidentAction.CompletelySolvedOption && !IncidentAction.PartlySolvedOption && !IncidentAction.ContentCouldNotBeFoundOption)
			{
				return;
			}

			if (IncidentAction.CompletelySolvedOption || IncidentAction.PartlySolvedOption || IncidentAction.ContentCouldNotBeFoundOption)
			{
				CustomerFacingResolutionMessageGroupBox.Visible = true;
			}

			if (IncidentAction.ContentBeDevelopYesOption)
			{
				ShouldContentBeDevelopedGroupBox.Visible = true;
				if (IncidentAction.PartlySolvedOption)
				{
					PleaseProvideLinkGroupBox.Visible = true;
				}
				if (IncidentAction.PartlySolvedOption || IncidentAction.ContentCouldNotBeFoundOption)
				{
					WhatChangesGroupBox.Visible = true;
					HowCanWeGroupBox.Visible = true;
				}
			}
			else if (IncidentAction.ContentBeDevelopNotWorthOption)
			{
				ShouldContentBeDevelopedGroupBox.Visible = true;
				if (IncidentAction.PartlySolvedOption)
				{
					PleaseProvideLinkGroupBox.Visible = true;
				}
			}
			else if (IncidentAction.ContentIsIrrelevantOption)
			{
				ShouldContentBeDevelopedGroupBox.Visible = true;
				if (IncidentAction.PartlySolvedOption || IncidentAction.ContentCouldNotBeFoundOption)
				{
					PleaseSelectReasonGroupBox.Visible = true;
				}
			}

			if (IncidentAction.HighlyClientSpecificOption || IncidentAction.ComplexEdgeCaseOption || IncidentAction.OtherOption)
			{
				PleaseProvideReasonGroupBox.Visible = true;
				if (IncidentAction.OtherOption)
				{
					OtherReasonTextBox.Visible = true;
				}
			}
		}

		void BottomFlagLabel_LocationChanged(object sender, EventArgs e)
		{
			AdjustFormHeight();
		}

		#region Please choose Options Group Box

		void PartlySolvedRadioButton_Click(object sender, EventArgs e)
		{
			if (this.PartlySolvedRadioButton.Checked)
			{
				ClearShouldContentBeDevelopedGroupBox();

				this.ShouldContentBeDevelopedGroupBox.Visible = true;
				this.PleaseProvideLinkGroupBox.Visible = false;
				this.PleaseProvideReasonGroupBox.Visible = false;
				this.WhatChangesGroupBox.Visible = false;
				this.HowCanWeGroupBox.Visible = false;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = false;

				RecordFlowOrder();
			}
		}

		void ContentNotBeFoundRadioButton_Click(object sender, EventArgs e)
		{
			if (this.ContentNotBeFoundRadioButton.Checked)
			{
				ClearShouldContentBeDevelopedGroupBox();

				this.ShouldContentBeDevelopedGroupBox.Visible = true;
				this.PleaseProvideLinkGroupBox.Visible = false;
				this.PleaseProvideReasonGroupBox.Visible = false;
				this.WhatChangesGroupBox.Visible = false;
				this.HowCanWeGroupBox.Visible = false;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = false;

				RecordFlowOrder();
			}
		}

		void CompletelySolvedRadioButton_Click(object sender, EventArgs e)
		{
			if (this.CompletelySolvedRadioButton.Checked)
			{
				ClearShouldContentBeDevelopedGroupBox();

				this.ShouldContentBeDevelopedGroupBox.Visible = false;
				this.PleaseProvideLinkGroupBox.Visible = false;
				this.PleaseProvideReasonGroupBox.Visible = false;
				this.WhatChangesGroupBox.Visible = false;
				this.HowCanWeGroupBox.Visible = false;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = true;

				RecordFlowOrder();
			}
		}

		#endregion

		#region Should Content Be Developed Group Box

		void ContentBeDevelopYesRadioButton_Click(object sender, EventArgs e)
		{
			ClearPleaseProvideReasonGroupBox();
			IncidentAction.ResolutionMethod = string.Empty;
			IncidentAction.RecommendToContentText = string.Empty;
			IncidentAction.ContentEasierText = string.Empty;

			if (this.PartlySolvedRadioButton.Checked && this.ContentBeDevelopYesRadioButton.Checked)
			{
				this.PleaseProvideLinkGroupBox.Visible = true;
				this.PleaseProvideReasonGroupBox.Visible = false;
				this.WhatChangesGroupBox.Visible = true;
				this.HowCanWeGroupBox.Visible = true;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = true;
			}

			if (this.ContentNotBeFoundRadioButton.Checked && this.ContentBeDevelopYesRadioButton.Checked)
			{
				this.PleaseProvideLinkGroupBox.Visible = false;
				this.PleaseProvideReasonGroupBox.Visible = false;
				this.WhatChangesGroupBox.Visible = true;
				this.HowCanWeGroupBox.Visible = true;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = true;
			}

			RecordFlowOrder();
		}

		void ContentBeDevelopedNotWorthRadioButton_Click(object sender, EventArgs e)
		{
			ClearPleaseProvideReasonGroupBox();
			IncidentAction.ResolutionMethod = string.Empty;
			IncidentAction.RecommendToContentText = string.Empty;
			IncidentAction.ContentEasierText = string.Empty;

			if (this.PartlySolvedRadioButton.Checked && this.ContentBeDevelopedNotWorthRadioButton.Checked)
			{
				this.PleaseProvideLinkGroupBox.Visible = true;
				this.PleaseProvideReasonGroupBox.Visible = true;
				this.WhatChangesGroupBox.Visible = false;
				this.HowCanWeGroupBox.Visible = false;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = true;
			}

			if (this.ContentNotBeFoundRadioButton.Checked && this.ContentBeDevelopedNotWorthRadioButton.Checked)
			{
				this.PleaseProvideLinkGroupBox.Visible = false;
				this.PleaseProvideReasonGroupBox.Visible = true;
				this.WhatChangesGroupBox.Visible = false;
				this.HowCanWeGroupBox.Visible = false;
				this.PleaseSelectReasonGroupBox.Visible = false;
				this.CustomerFacingResolutionMessageGroupBox.Visible = true;
			}

			RecordFlowOrder();
		}

		void ContentIrrelevantERequestRadioButton_Click(object sender, EventArgs e)
		{
			ClearPleaseProvideReasonGroupBox();
			IncidentAction.LinksToExistingContent = string.Empty;
			IncidentAction.ResolutionMethod = string.Empty;
			IncidentAction.RecommendToContentText = string.Empty;
			IncidentAction.ContentEasierText = string.Empty;

			this.PleaseProvideLinkGroupBox.Visible = false;
			this.PleaseProvideReasonGroupBox.Visible = false;
			this.WhatChangesGroupBox.Visible = false;
			this.HowCanWeGroupBox.Visible = false;
			this.PleaseSelectReasonGroupBox.Visible = true;
			this.CustomerFacingResolutionMessageGroupBox.Visible = true;

			RecordFlowOrder();
		}

		void ClearShouldContentBeDevelopedGroupBox()
		{
			ContentBeDevelopedNotWorthRadioButton.Checked = false;
			ContentIrrelevantERequestRadioButton.Checked = false;
			ContentBeDevelopYesRadioButton.Checked = false;
			ClearPleaseProvideReasonGroupBox();

			IncidentAction.LinksToExistingContent = string.Empty;
			IncidentAction.ResolutionMethod = string.Empty;
			IncidentAction.RecommendToContentText = string.Empty;
			IncidentAction.ContentEasierText = string.Empty;
		}

		#endregion

		#region Clear Please Provide a Reason Group Box

		void ClearPleaseProvideReasonGroupBox()
		{
			IncidentAction.ReasonText = string.Empty;
			this.HighlyClientSpecificRadioButton.Checked = false;
			this.ComplexEdgeCaseRadioButton.Checked = false;
			this.OtherRadioButton.Checked = false;
		}
		#endregion

		void RecordFlowOrder()
		{
			MainFlowLayoutPanel.SuspendLayout();
			this.MainFlowLayoutPanel.Controls.Clear();
			this.MainFlowLayoutPanel.Controls.Add(this.TopFlagLabel);
			this.MainFlowLayoutPanel.Controls.Add(this.ShouldContentBeDevelopedGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.PleaseProvideLinkGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.PleaseProvideReasonGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.WhatChangesGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.HowCanWeGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.PleaseSelectReasonGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.CustomerFacingResolutionMessageGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.BottomFlagLabel);
			this.CustomerFacingResolutionMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 112, true);
			MainFlowLayoutPanel.ResumeLayout();
		}

		void PopUpEConversationButton_Click(object sender, EventArgs e)
		{
			var form = new EConversationTextForm(IncidentAction.Incident);
			form.Owner = this;
			form.Show();
		}

		void MainFlowLayoutPanel_SizeChanged(object sender, EventArgs e)
		{
			TopFlagLabel.Width = MainFlowLayoutPanel.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
		}

		void AdjustFormHeight()
		{
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(InitialMaximumFormWidth, InitialMaximumFormHeight, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(InitialMinimumFormWidth, InitialMinimumFormHeight, true);
			var bottomFlagPosition = BottomFlagLabel.PointToScreen(System.Drawing.Point.Empty).Y;
			var closeButtonY = this.CloseButton.PointToScreen(System.Drawing.Point.Empty).Y;

			var distance = closeButtonY - bottomFlagPosition;
			var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(this.Height) - ControlDpiScalingHelper.UnscaleFromCurrentDpiY(distance) + 10;
			this.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(height);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(InitialMinimumFormWidth, height, true);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(InitialMaximumFormWidth, height + 25, true);
		}
	}
}
