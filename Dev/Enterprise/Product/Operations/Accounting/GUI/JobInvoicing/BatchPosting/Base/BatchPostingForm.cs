using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class BatchPostingForm : ZChildForm
	{
		public BatchPostingForm()
		{
			Init();
		}

		public BatchPostingForm(BatchPostingBusinessObject businessEntity)
			: base(businessEntity)
		{
			Init();
		}

		void Init()
		{
			this.ProgressTextBox.ReadOnly = true;

			if (BusinessEntity != null)
			{
				BusinessEntity.OnAfterPosted += new EventHandler(BusinessEntity_OnAfterPosted);
				BusinessEntity.OnProgress += new EventHandler(DataPostingBusinessObject_OnProgress);
				BusinessEntity.OnShowInfoMessage += new BatchPostingBusinessObject.PostingEvent(BusinessEntity_OnShowInfoMessage);
				this.ObjectsPostedCalcEdit.GetExtension<LabelCaptionRenderer>().Caption = BusinessEntity.ObjectsPostedLabelText;
				this.ProgressBar.Maximum = BusinessEntity.NumberOfObjectsToPost;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void BusinessEntity_OnShowInfoMessage(string message)
		{
			ProgressTextBox.AppendText(message);
			ProgressTextBox.ScrollToCaret();
		}

		void BusinessEntity_OnAfterPosted(object sender, EventArgs e)
		{
			ProgressTextBox.AppendText("\r\n");
			SetAllButtonsEnabled(false);
		}

		void DataPostingBusinessObject_OnProgress(object sender, EventArgs e)
		{
			ProgressBar.Value++;
		}

		void UnhookEvents()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.OnAfterPosted -= new EventHandler(BusinessEntity_OnAfterPosted);
				BusinessEntity.OnProgress -= new EventHandler(DataPostingBusinessObject_OnProgress);
				BusinessEntity.OnShowInfoMessage -= new BatchPostingBusinessObject.PostingEvent(BusinessEntity_OnShowInfoMessage);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			UnhookEvents();
			this.Close();
		}

		void CancelPostingButton_Click(object sender, EventArgs e)
		{
			UnhookEvents();
			BusinessEntity.StopPostingOnNextIteration();
		}

		#region Implementation

		void SetAllButtonsEnabled(bool value)
		{
			this.CloseButton.Enabled = !value;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return BusinessEntity.FormCaption; }
		}

		protected override void Dispose(bool disposing)
		{
			UnhookEvents();

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			SetAllButtonsEnabled(true);
			if (BusinessEntity != null)
			{
				BusinessEntity.StartPosting();
			}
		}

		public new BatchPostingBusinessObject BusinessEntity
		{
			get { return (BatchPostingBusinessObject)base.BusinessEntity; }
		}

		#endregion
	}
}

