using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class GLAccountSelectionForm : ZChildForm
	{
		public GLAccountSelectionForm(AccGLHeaderCollection gridCollection, List<AccGLHeader> gLHeaderList) : base(gridCollection)
		{
			BindingSource.DataSource = this;
			this.gridCollection = gridCollection;
			GLHeaderList = gLHeaderList;
		}

		void OK_Button_Click(object sender, EventArgs e)
		{
			var selectedBusinessObjects = Grid.GetSelectedElements<BusinessObject>();
			if (selectedBusinessObjects != null)
			{
				if (selectedBusinessObjects.Length != 1)
				{
					Globals.Message.ShowError(Res.GetString("GLAccountSelectionForm|7B28F0F9-F4A8-4E43-87F5-DD9702864768", "Please select a Parent Account from the list"));
				}
				else
				{
					var glHeader = selectedBusinessObjects.Cast<AccGLHeader>().First();
					GLHeaderList.Add(glHeader);
					DialogResult = System.Windows.Forms.DialogResult.OK;
				}
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		readonly List<AccGLHeader> GLHeaderList;
		protected ZPanel FilterControlPanel;
		protected ZPanel ButtonPanel;
		protected ZLabel MessageLabel;
		ZPanel panelOkCancelButtons;
		protected ZButton OK_Button;
		ZButton Cancel_Button;
		internal ZGrid Grid;
	}
}
