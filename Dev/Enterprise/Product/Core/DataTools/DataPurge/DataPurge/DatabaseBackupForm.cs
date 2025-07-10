using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataPurge
{
	public partial class DatabaseBackupForm : ZChildForm
	{
		public DatabaseBackupForm() : this(new DataPurger())
		{
		}

		DatabaseBackupForm(DataPurger purger)
			: base(purger)
		{
			InitializeComponent();
			this.Purger = purger;
			SetDataBinding(purger, "");
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#region Backup

		void BackupButton_Click(object sender, System.EventArgs e)
		{
			PerformBackup();
		}

		void PerformBackup()
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				Purger.BackupCatchingExceptions();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		#endregion

		readonly DataPurger Purger;
	}
}
