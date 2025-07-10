using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public sealed partial class AllJobTypesForm : ZChildForm
	{
		public AllJobTypesForm(JobTypePicker jobTypePicker)
			: base(jobTypePicker)
		{
			this.JobTypePicker = jobTypePicker;
		}

#if DEBUG
		AllJobTypesForm()
			: base()
		{
		}
#endif

		public event EventHandler JobTypeSelectionChanged;
		public event EventHandler JobTypeSelectionCanceled;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		JobTypePicker JobTypePicker;

		void btnCancel_Click(object sender, EventArgs e)
		{
			Cursor prevCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (JobTypeSelectionCanceled != null)
				{
					JobTypeSelectionCanceled(sender, e);
				}
				this.Close();
			}
			finally
			{
				Cursor.Current = prevCursor;
			}
		}

		void btnSelect_Click(object sender, EventArgs e)
		{
			Cursor prevCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (JobTypeCheckedListBox.CheckedItems.Count == 0)
				{
					Globals.Message.ShowError(Res.GetString("be3b2cab-397f-4a6f-a26e-bc7c3405b9e8", "No Job Type is selected. Please tick-on the left side box in the list to select a job type"));
					return;
				}
				if (JobTypeSelectionChanged != null)
				{
					JobTypeSelectionChanged(sender, e);
				}
				this.Close();
			}
			finally
			{
				Cursor.Current = prevCursor;
			}
		}

		void btnSelectAll_Click(object sender, EventArgs e)
		{
			Cursor prevCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				bool deSelectAll = !JobTypePicker.JobTypeList.All(x => !x.Value);
				JobTypePicker.JobTypeList.ToList().ForEach(x => x.Value = !deSelectAll);
			}
			finally
			{
				Cursor.Current = prevCursor;
			}
		}

		void JobTypeCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			bool deSelectAll = !JobTypePicker.JobTypeList.All(x => !x.Value);
			btnSelectAll.Text = deSelectAll ? Res.GetString("6a4f8492-c76c-429c-9f60-33692805b18c", "Deselect All") : Res.GetString("802895a0-2c28-4083-8386-69dde3e69b09", "Select All");
		}

#if DEBUG
		public void SelectJobTypes_ForTestOnly()
		{
			this.Show();
			btnSelect.PerformClick();
		}
		public void UndoJobTypeSelection_ForTestOnly()
		{
			btnCancel.PerformClick();
		}

#endif
	}
}
