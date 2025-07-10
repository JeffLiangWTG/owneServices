using System;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A form that displays a message and a progress bar.
	/// The progress bar needs to manually "poked" for it to update.
	/// The form also exposes a cancel button which fires an event that the consumer can hook into.
	/// 
	/// By default, both the progress bar and cancel button are shown.
	/// </summary>
	public partial class ProgressForm : ZChildForm, IProgressForm
	{
		static ProgressForm()
		{
			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(ProgressForm));
		}

		public ProgressForm(string initialMessage) : this()
		{
			initialMessageForFormCaption = initialMessage;
		}

		public ProgressForm()
		{
			InitializeComponent();
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			sw = Stopwatch.StartNew();
		}

		readonly Stopwatch sw;
		readonly string initialMessageForFormCaption;

		public override string FormCaption
		{
			get
			{
				if (!string.IsNullOrEmpty(initialMessageForFormCaption))
				{
					return initialMessageForFormCaption;
				}
				else
				{
					return base.FormCaption;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void UpdateForm()
		{
			#if !WINZOR
			using (ZFormModaliser.DisableAllFormsButOne(this))
			{
				Application.DoEvents();
			}
			#else
			Application.DoEvents();
			#endif
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#region ShouldAddFormActivityLog

		protected override bool ShouldAddFormActivityLog => false;

		#endregion

		#region Loading

		/// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Shown" /> event.</summary>
		/// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data. </param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			ResizeForm();
		}

		void ProgressForm_Load(object sender, EventArgs e)
		{
			EnterpriseLogo.Image = BrandingFactory.Instance.ProductIcon.ToBitmap();
			EnterpriseLogo.Size = EnterpriseLogo.Image.Size; //may not match Image size on other DPIs
		}

		void ResizeForm()
		{
			if (ShowProgressBar || ShowCancelButton)
			{
				BottomPanel.Visible = true;
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(this, Height - BottomPanel.Height, false);
				BottomPanel.Visible = false;
			}
		}

		#endregion

		#region Constants

		public const int ProgressBarOriginalLengthWithButton = 248;
		public const int ProgressBarLengthModifier = 3;

		#endregion

		#region ProgressBar

		protected int ProgressBarLengthWithoutButton
		{
			get { return Width - (ProgressBar.Left * ProgressBarLengthModifier); }
		}

		/// <summary>
		/// Controls whether the Progress Bar is displayed on the form.
		/// </summary>
		public bool ShowProgressBar
		{
			get { return fShowProgressBar; }
			set
			{
				fShowProgressBar = value;
				ProgressBar.Visible = value;
				UpdateForm();
			}
		}

		bool fShowProgressBar = true;

		#endregion

		#region Status Display

		/// <summary>
		/// The text to display on the popup.
		/// </summary>
		public string Status
		{
			get { return lazyStatus; }
			set
			{
				lazyStatus = value;
				LazyUpdateProgressFormAndStatus();
			}
		}
		string lazyStatus = "";

		/// <summary>
		/// The completion status, as a percentage.
		/// </summary>
		public int PercentComplete
		{
			get { return lazyPercentComplete; }
			set
			{
				if (value > ProgressBar.Maximum)
				{
					ErrorReporter.ReportOnce("ProgressForm.set_PercentComplete.value>Maximum", // not a column name
						string.Format("New PercentComplete value {0} is greater then ProgressBar.Maximum {1}\r\nStatus: \"{2}\"", value, ProgressBar.Maximum, Status));

					value = ProgressBar.Maximum;
				}
				lazyPercentComplete = value;
				LazyUpdateProgressFormAndStatus();
			}
		}
		int lazyPercentComplete;

		public void SetStatusAndPercentComplete(string status, int percentComplete)
		{
			ModifyStatusAndPercentComplete(ref status, ref percentComplete);
			Status = status;
			PercentComplete = percentComplete;
		}

		public double SleepBetweenRefreshMilliseconds { get; set; } = 250;

		protected TimeSpan SleepBetweenRefresh
		{
			get { return TimeSpan.FromMilliseconds(SleepBetweenRefreshMilliseconds); }
		}

		protected void LazyUpdateProgressFormAndStatus()
		{
			if (sw.Elapsed > SleepBetweenRefresh)
			{
				UpdateProgressFormAndStatus();
			}
		}

		protected void UpdateProgressFormAndStatus()
		{
			sw.Restart();
			SuspendLayout();
			try
			{
				ProgressLabel.Text = lazyStatus;
				ProgressBar.Value = lazyPercentComplete;
			}
			finally
			{
				ResumeLayout();
			}
			UpdateForm();
		}

		protected virtual void ModifyStatusAndPercentComplete(ref string status, ref int percentComplete)
		{
		}

		public bool HideOnModal
		{
			get
			{
				return hideOnModal;
			}
			set
			{
				if (hideOnModal != value)
				{
					if (value)
					{
						Application.EnterThreadModal += Application_EnterThreadModal;
						Application.LeaveThreadModal += Application_LeaveThreadModal;
					}
					else
					{
						Application.EnterThreadModal -= Application_EnterThreadModal;
						Application.LeaveThreadModal -= Application_LeaveThreadModal;
					}
					hideOnModal = value;
				}
			}
		}
		bool hideOnModal;

		void Application_EnterThreadModal(object sender, EventArgs e)
		{
			Visible = false;
		}

		void Application_LeaveThreadModal(object sender, EventArgs e)
		{
			Visible = true;
			Update();
		}

		#endregion

		#region Cancelling

		/// <summary>
		/// Consumer should hookup to this event to be notified if the user clicks the Cancel 
		/// button on the progress form. Consumer should use this to cancel the action being performed.
		/// </summary>
		public event EventHandler Cancelled;
		public bool DoNotCloseWhenClickCancel { get; set; }

		void CancelProgressButton_Click(object sender, EventArgs e)
		{
			if (Cancelled != null)
			{
				Cancelled(sender, e);
			}

			if (DoNotCloseWhenClickCancel)
			{
				this.CancelProgressButton.Enabled = false;
			}
			else
			{
				Close();
			}
		}

		/// <summary>
		/// Controls whether the Cancel button is displayed on the form.
		/// </summary>
		public bool ShowCancelButton
		{
			get { return fShowCancelButton; }
			set
			{
				fShowCancelButton = value;
				CancelProgressButton.Visible = value;
				CancelButton = value ? CancelProgressButton : null;

				ControlDpiScalingHelper.SetWidth(ref ProgressBar, value ? ControlDpiScalingHelper.ScaleToCurrentDpiX(ProgressBarOriginalLengthWithButton) : ProgressBarLengthWithoutButton, false);
				UpdateForm();
			}
		}

		public bool EnableCancelButton
		{
			get
			{
				return CancelProgressButton.Enabled;
			}
			set
			{
				CancelProgressButton.Enabled = value;
			}
		}

		bool fShowCancelButton = true;

		/// <summary>
		/// Sets the text to be displayed on the Cancel button.
		/// </summary>
		public string CancelProgressButtonText
		{
			get { return CancelProgressButton.Text; }
			set { CancelProgressButton.Text = value; }
		}

		#endregion

		public void ShowModalTo(Form parentForm)
		{
			ZFormModaliser.Show(this, parentForm);
			Update();
		}

		#region IDisposable Members

#if !WINZOR
		protected override void MaybeNukeAllEvents()
		{
			//due to ProcessStatusFormManager.cs form.FormClosed += Form_FormClosed; needing to run AFTER this form has been disposed. don't really get the timing but whatever
		}
#endif

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (HideOnModal)
				{
					HideOnModal = false;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}

	#region IProgressForm

	public interface IProgressForm : IDisposable
	{
		void SetStatusAndPercentComplete(string status, int percentComplete);
		void ShowModalTo(Form parentForm);
		void Hide();

		string Status { get; set; }
		int PercentComplete { get; set; }

		event EventHandler Cancelled;
	}

	#endregion
}
