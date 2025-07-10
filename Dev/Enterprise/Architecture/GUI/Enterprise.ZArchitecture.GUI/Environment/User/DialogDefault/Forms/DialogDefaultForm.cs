using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.DialogDefault
{
	internal partial class DialogDefaultForm : ZChildForm
	{
		public DialogDefaultSaveOptions Options { get; }

		readonly DialogDefaultContext Context;
		readonly bool ReadOnly;
		readonly DialogResult DefaultResult;

		readonly DialogDefaultAdditionalOptions additionalOptionsControl;
		bool HasAdditionalOptions { get { return additionalOptionsControl != null; } }

		public DialogDefaultForm(DialogDefaultContext context, KUserControl control, DialogResult defaultResult, bool isReadOnly = false)
			: this(context, control, defaultResult, GetAdditionalOptionsControl(context), isReadOnly) { }

		internal DialogDefaultForm(DialogDefaultContext context, KUserControl control, DialogResult savedDefaultResult, DialogDefaultAdditionalOptions additionalOptionsControl, bool isReadOnly = false)
			: this(new DialogDefaultSaveOptions { KeepShowingDialog = false, Level = DialogDefaultLevel.Codes.User })
		{
			if (context.GlobalOnly)
			{
				Options.Level = DialogDefaultLevel.Codes.Global;
			}

			ReadOnly = isReadOnly;
			DefaultResult = savedDefaultResult == DialogResult.None ? (DialogResult)context.DefaultResult : savedDefaultResult;
			Context = context;

			InitializeComponent();
			this.additionalOptionsControl = additionalOptionsControl;

			ConstructDisplay(control);

			if (!context.ShowCheckboxOnly)
			{
				saveDefaultCheckBox.CheckedChanged += ToggleDefaultsControlVisible;
			}

			if (context.CheckBoxCaption != null)
			{
				saveDefaultCheckBox.CaptionResourceString = context.CheckBoxCaption;
			}

			if (ReadOnly)
			{
				whyReadOnlyLabel.LinkClicked += ShowWhyDialogIsReadOnly;
			}
		}

		internal DialogDefaultForm(DialogDefaultSaveOptions saveOptions)
			: base(saveOptions)
		{
			Options = Argument.NotNull(saveOptions, nameof(saveOptions));
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var optionsBefore = Options;

			base.SetDataBinding(dataSource, dataMember);

			if (optionsBefore != null && Options == null)
			{
				ErrorReporter.ReportOnce("I have lost the binding!");
			}
		}

		static DialogDefaultAdditionalOptions GetAdditionalOptionsControl(DialogDefaultContext context)
		{
			if (context.ShowCheckboxOnly)
			{
				return null;
			}
			else if (CanCreateGlobalDefaultsCheckpoint.IsAllowed)
			{
				return new DialogDefaultFullOptions(context);
			}
			else
			{
				return new DialogDefaultUserOnlyOptions(context);
			}
		}

		#region ShouldAddFormActivityLog

		protected override bool ShouldAddFormActivityLog => false;

		#endregion

		#region Form building

		int DialogButtonHorizontalSpacing { get { return 2 * ControlsMarginX; } }
		Size DialogButtonSize { get { return ControlDpiScalingHelper.NewScaledSize(100, 25); } }

		void ConstructDisplay(KUserControl control)
		{
			SetControlReadOnly(control, ReadOnly);
			Controls.Add(control);

			int lowestControlEdge;
			if (Context.Icon == ZMessageBoxIcon.None)
			{
				Controls.Remove(iconPictureBox);
				control.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);

				lowestControlEdge = control.Bottom;
			}
			else
			{
				iconPictureBox.Image = GetIconImage(Context.Icon).ToBitmap();
				control.Location = ControlDpiScalingHelper.NewScaledPoint(iconPictureBox.Right + ControlsMarginX, iconPictureBox.Location.Y, false);

				lowestControlEdge = Math.Max(iconPictureBox.Bottom, control.Bottom) + ControlsMarginY;
			}

			var hasReadOnlyLabel = ReadOnly;
			if (hasReadOnlyLabel)
			{
				whyReadOnlyLabel.Location = ControlDpiScalingHelper.NewScaledPoint(whyReadOnlyLabel.Location.X, lowestControlEdge, false);
				if (Context.Buttons != null)
				{
					lowestControlEdge = whyReadOnlyLabel.Bottom + ControlsMarginY;
				}
			}
			else
			{
				Controls.Remove(whyReadOnlyLabel);
			}

			int generatedComponentsWidth;
			int generatedComponentsHeight;
			int saveNewCheckboxYLocation;

			var buttonResults = GetDialogResultsFor(Context.Buttons);
			var buttons = CreateDialogResultButtons(buttonResults, lowestControlEdge);
			if (buttons.Any())
			{
				saveNewCheckboxYLocation = lowestControlEdge + (DialogButtonSize.Height - saveDefaultCheckBox.Height) / 2;

				var rightMostButton = buttons.First();
				generatedComponentsWidth = rightMostButton.Right + DialogButtonHorizontalSpacing;
				generatedComponentsHeight = DialogButtonSize.Height;
			}
			else
			{
				generatedComponentsWidth = hasReadOnlyLabel ?
					saveDefaultCheckBox.Right + whyReadOnlyLabel.Width + 2 * ControlsMarginX :
					saveDefaultCheckBox.Right + ControlsMarginX;

				generatedComponentsHeight = saveDefaultCheckBox.Height + ControlsMarginY;
				saveNewCheckboxYLocation = lowestControlEdge;
			}

			saveDefaultCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(saveDefaultCheckBox.Location.X, saveNewCheckboxYLocation, false);
			MainStatusBar.Visible = false;

			var largestComponentWidth = Math.Max(control.Right, generatedComponentsWidth);
			if (HasAdditionalOptions)
			{
				var additionalOptionsComponentWidth = additionalOptionsControl.MinimumSize.Width;
				largestComponentWidth = Math.Max(largestComponentWidth, additionalOptionsComponentWidth);
			}

			var width = largestComponentWidth + ControlsMarginX;
			var height = lowestControlEdge + generatedComponentsHeight + ControlsMarginY;

			if (HasAdditionalOptions)
			{
				height += additionalOptionsControl.MinimumSize.Height;
				additionalOptionsControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, lowestControlEdge + generatedComponentsHeight, false);
				ControlDpiScalingHelper.SetWidth(additionalOptionsControl, width, false);
			}

			ClientSize = ControlDpiScalingHelper.NewScaledSize(width, height, false);
			Text = (ReadOnly) ? Res.GetString("59FEC7D9-2088-467C-B4E0-70E22ADF7132", "Read only: {0}", Context.Caption) : Context.Caption;

			HideAdditionalOptions();

			//We want to be the same color as our child control
			BackColor = control.BackColor;
		}

		void SetControlReadOnly(KUserControl control, bool readOnly)
		{
			var defaultMembers = control as IDialogDefaultControlMembers;
			if (defaultMembers != null)
			{
				defaultMembers.SetReadOnly(readOnly, (ZDialogResult)DefaultResult);
			}
			else
			{
				control.Enabled = !readOnly;
			}
		}

		/// <summary> Gets the system icon for a given MessageBoxIcon </summary>
		/// <exception cref="ArgumentException">When the given MessageBoxIcon is not catered to by the function</exception>
		Icon GetIconImage(ZMessageBoxIcon icon)
		{
			//Many of the icons in MessageBoxIcons are aliases of eachother, so at time of writing this does cover them all
			switch (icon)
			{
				case ZMessageBoxIcon.Error:
					return SystemIcons.Error;
				case ZMessageBoxIcon.Information:
					return SystemIcons.Information;
				case ZMessageBoxIcon.Question:
					return SystemIcons.Question;
				case ZMessageBoxIcon.Warning:
					return SystemIcons.Warning;
				case ZMessageBoxIcon.None:
					return null;
				default:
					throw new ArgumentException("Invalid messagebox icon");
			}
		}

		#region dialog buttons

		DialogResult[] GetDialogResultsFor(ZMessageBoxButtons? buttons)
		{
			switch (buttons)
			{
				case null:
					return Array.Empty<DialogResult>();
				case ZMessageBoxButtons.AbortRetryIgnore:
					return new[] { DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore };
				case ZMessageBoxButtons.OK:
					return new[] { DialogResult.OK };
				case ZMessageBoxButtons.OKCancel:
					return new[] { DialogResult.OK, DialogResult.Cancel };
				case ZMessageBoxButtons.RetryCancel:
					return new[] { DialogResult.Retry, DialogResult.Cancel };
				case ZMessageBoxButtons.YesNo:
					return new[] { DialogResult.Yes, DialogResult.No };
				case ZMessageBoxButtons.YesNoCancel:
					return new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
				default:
					throw new ArgumentException("Unknown DialogResult type");
			}
		}

		List<ZButton> CreateDialogResultButtons(DialogResult[] buttons, int dialogButtonY)
		{
			var zbuttons = new List<ZButton>(buttons.Length);

			var leftBound = ClientSize.Width - (DialogButtonSize.Width + DialogButtonHorizontalSpacing) * buttons.Length;
			foreach (var result in buttons)
			{
				var button = new ZButton
				{
					Location = ControlDpiScalingHelper.NewScaledPoint(leftBound, dialogButtonY, false),
					Anchor = AnchorStyles.Top | AnchorStyles.Right,
					Size = DialogButtonSize,
					UseVisualStyleBackColor = true,
					Text = DialogResultCaptions.GetCaptionForDialogResult(result),
					DialogResult = result
				};

				if (result == DefaultResult)
				{
					AcceptButton = button;
				}
				else
				{
					button.Enabled = !ReadOnly;
				}

				Controls.Add(button);
				leftBound = button.Right + DialogButtonHorizontalSpacing;

				zbuttons.Add(button);
			}

			return zbuttons;
		}

		#endregion

		#endregion

		#region show/hide defaults

		void ToggleDefaultsControlVisible(object o, EventArgs e)
		{
			if (saveDefaultCheckBox.Checked)
			{
				ShowAdditionalOptions();
			}
			else
			{
				HideAdditionalOptions();
			}
		}

		internal void ShowAdditionalOptions()
		{
			if (HasAdditionalOptions)
			{
				Controls.Add(additionalOptionsControl);
				ControlDpiScalingHelper.SetHeight(this, Height + additionalOptionsControl.Height, false);
			}
		}

		internal void HideAdditionalOptions()
		{
			if (HasAdditionalOptions)
			{
				ControlDpiScalingHelper.SetHeight(this, Height - additionalOptionsControl.Height, false);
				Controls.Remove(additionalOptionsControl);
			}
		}

		#endregion

		#region events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			var defaultButton = AcceptButton as ZButton;
			if (defaultButton != null)
			{
				defaultButton.Focus();
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			// we should not allow to close the form if cancelling is not allowed
			var isUserTryingToCancelOrCloseForm = DialogResult == DialogResult.Cancel;
			var shouldProhibitClosingForm = isUserTryingToCancelOrCloseForm && Context.ForceOverriddenDefaults && ReadOnly && DefaultResult != DialogResult.Cancel;
			eventArgs.Cancel = shouldProhibitClosingForm;
			base.OnFormClosing(eventArgs);

			if (!eventArgs.Cancel)
			{
				//Only save if user clicked appropriate button
				//If the program crashed and someone was halfway through making some defaults it could end badly
				if (Options != null)
				{
					Options.SaveNewDefaults &= eventArgs.CloseReason == CloseReason.None;
				}
			}
		}

		#endregion

		#region scaling constants

		readonly int ControlsMarginX = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
		readonly int ControlsMarginY = ControlDpiScalingHelper.ScaleToCurrentDpiY(5);

		#endregion

		#region Misc

		void ShowWhyDialogIsReadOnly(object o, EventArgs a)
		{
			var caption = Res.GetString("2855A46F-6EA2-4A44-BF56-749CCF087F1A", "Why is this dialog read only?");
			var message = Res.GetString("82C33CF6-0DBF-4B09-81EE-69AC78CFD67E",
@"This dialog is read only because a saved default exists that has 'Override Lower Levels' on. Since you do not have permission to modify global defaults you may only accept this dialog as is. You may save these defaults as your own if you wish to not see the dialog any more.

In order to modify this dialog, please either remove the overriding default, or allow the permission '{0}'.", CanCreateGlobalDefaultsCheckpoint.DisplayTextPathToSecurityRight);

			Globals.Message.ShowInformation(message, caption);
		}

		internal static ISecurityCheckpoint CanCreateGlobalDefaultsCheckpoint
		{
			get { return EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults"); }
		}

		#endregion

		#region Exposed For Testing

#if DEBUG
		internal DialogResult[] GetDialogResultsForExposed(MessageBoxButtons btns)
		{
			return GetDialogResultsFor((ZMessageBoxButtons)btns);
		}

		public ZCheckBox SaveDefaultCheckBoxExposed
		{
			get { return saveDefaultCheckBox; }
		}

		public Control AdditionalOptionsControlExposed
		{
			get { return additionalOptionsControl; }
		}

		public ZLinkLabel WhyReadOnlyLabelExposed
		{
			get { return whyReadOnlyLabel; }
		}

#endif

		#endregion
	}
}
