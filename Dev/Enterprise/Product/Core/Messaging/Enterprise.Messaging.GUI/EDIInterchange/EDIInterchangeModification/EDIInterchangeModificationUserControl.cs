using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIInterchangeModificationUserControl : ZUserControl
	{
		public EDIInterchangeModificationUserControl()
		{
			InitializeComponent();
			TopSplitContainer.Panel2MinSize = 200;
			BottomSplitContainer.Panel2MinSize = 100;
		}

		EDIInterchange Interchange
		{
			get
			{
				return (EDIInterchange)this.BindingSource.Current;
			}
		}

		void EDIInterchangeModificationUserControl_Load(object sender, System.EventArgs e)
		{
			if (Interchange.SaveToDisk)
			{
				this.zButtonUploadFile.Visible = true;
			}
			else
			{
				this.BodyTextTextBox.Visible = true;
			}
		}

		void zButtonUploadFile_Click(object sender, System.EventArgs e)
		{
			if (fileUploadDialog.ShowDialog() == DialogResult.OK)
			{
				Interchange.EI_BodyText = "Updating...";
				Interchange.SetEI_BodyTextSource(new LargeFileHolder(fileUploadDialog.ForceLocalFile()));
				Interchange.Factory.Save();
			}
		}
	}
}
