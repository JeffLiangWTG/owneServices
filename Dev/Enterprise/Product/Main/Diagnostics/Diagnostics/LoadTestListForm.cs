#if DEBUG

using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Testing
{
	public partial class LoadTestListForm : ZChildForm
	{
		public LoadTestListForm()
		{
		}

		public LoadTestListForm(TestListLoader loader)
			: base(loader)
		{
		}

		public new TestListLoader BusinessEntity
		{
			get { return (TestListLoader)base.BusinessEntity; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			OpenFileDialogButton_Click(this, EventArgs.Empty);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		void loadTestsButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			Close();
		}

		void OpenFileDialogButton_Click(object sender, EventArgs e)
		{
			this.openFileDialog.FileName = FileNameTextBox.Text;
			if (this.openFileDialog.ShowDialog() == DialogResult.OK)
			{
				if (BusinessEntity == null)
				{
					FileNameTextBox.Text = this.openFileDialog.UnmappedFileName;
				}
				else
				{
					BusinessEntity.FileName = this.openFileDialog.UnmappedFileName;
				}
			}
		}
	}
}

#endif
