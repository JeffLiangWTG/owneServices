using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Registry.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public partial class FileSelectorFindBox : ZGridFindBox
	{
		public FileSelectorFindBox(ZString fileType)
		{
			InitializeComponent();
			this.fileType = fileType;
			this.SetReadOnly(true);
			this.PopupButtonReadonlyCanBeDifferent = true;
			this.PopupButton.ReadOnly = false;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			if (keyData == Keys.Delete)
			{
				OnKeyPress(new KeyPressEventArgs((Char)Keys.Delete));
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (e.KeyChar == (Char)Keys.Delete)
			{
				this.Code = ZString.Empty;
			}

			base.OnKeyPress(e);
		}

		readonly ZString fileType;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return new FileManager(fileType);
		}

		protected override IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}
	}
}
