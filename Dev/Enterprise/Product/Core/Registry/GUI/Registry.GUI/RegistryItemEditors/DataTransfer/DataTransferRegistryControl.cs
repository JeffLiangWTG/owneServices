using System;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DataTransferRegistryControl : AutomaticProcessRegistryControl
	{
		public DataTransferRegistryControl()
		{
			InitializeComponent();
		}

		void DirectorySelectorButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZFolderBrowserDialog())
			{
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					CurrentDataItem.Directory = dialog.UnmappedSelectedPath;
				}
			}
		}

		new DataTransferRegistryBusinessObject CurrentDataItem
		{
			get { return (DataTransferRegistryBusinessObject)base.CurrentDataItem; }
		}
	}
}
