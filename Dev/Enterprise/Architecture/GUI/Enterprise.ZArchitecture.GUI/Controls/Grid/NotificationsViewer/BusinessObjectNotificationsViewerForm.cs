using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class BusinessObjectNotificationsViewerForm : ZChildForm
	{
		public Action<ZGuid> OnCurrentSelectedItemChangedAction;

		public BusinessObjectNotificationsViewerForm(BusinessObjectNotificationsViewerSupporter supporter)
			: base(supporter)
		{
			InitializeComponent();

			AddHumanReadableColumns(supporter);
			this.NotificationsGrid.AfterBind += NotificationsGrid_AfterBind;
			supporter.LoadingNotificationsProcessAction = (statusMessage, percent) =>
			{
				ProgressForm.SetStatusAndPercentComplete(statusMessage, percent);

				if (percent < 100)
				{
					ProgressForm.Show();
				}
				else
				{
					ProgressForm.Hide();
				}
			};
		}

		public override string FormCaption => Res.GetString("486b0a9a-1eae-41a0-979a-e5dce4b40460", "Notifications Viewer");

		public override string FormVerb => "";

		void NotificationsGrid_AfterBind(object sender, EventArgs e)
		{
			NotificationsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
		}

		void AddHumanReadableColumns(BusinessObjectNotificationsViewerSupporter supporter)
		{
			var humanReadableColumnIndex = 0;

			foreach (var humanReadableColumn in supporter.HumanReadableColumnCaptionsAndWidth)
			{
				var humanReadableColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				this.NotificationsGrid.ColumnStyles.Insert(humanReadableColumnIndex, humanReadableColumnStyleInfo);
				humanReadableColumnStyleInfo.CaptionResourceString = humanReadableColumn.Caption;
				humanReadableColumnStyleInfo.Width = humanReadableColumn.ColumnWidth;
				humanReadableColumnStyleInfo.ColumnName = $"HumanReadableColumn{++humanReadableColumnIndex}";
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var currentLine = NotificationsGrid.ListManager.GetCurrent();
			if (currentLine is BusinessObjectNotificationsViewerLine line && OnCurrentSelectedItemChangedAction != null)
			{
				OnCurrentSelectedItemChangedAction(line.PK);
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			if (DataSource is BusinessObjectNotificationsViewerSupporter supporter)
			{
				supporter.NotificationsCollection.Load();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#region ProgressForm

		ProgressForm ProgressForm
		{
			get
			{
				if (fProgressForm == null)
				{
					fProgressForm = new ProgressForm();
					fProgressForm.ShowCancelButton = false;
					fProgressForm.ShowProgressBar = true;
					fProgressForm.CaptionResourceString = Res.GetData("a6447219-4d9d-490d-8df3-903c40f49537", "Loading Notifications...");
					fProgressForm.ShowModalTo(this);
				}

				return fProgressForm;
			}
		}
		ProgressForm fProgressForm;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				NotificationsGrid.AfterBind -= NotificationsGrid_AfterBind;

				if (NotificationsGrid.ListManager != null)
				{
					NotificationsGrid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
				}

				if (fProgressForm != null)
				{
					fProgressForm.Close();
				}
			}

			base.Dispose(disposing);
		}
	}
}
