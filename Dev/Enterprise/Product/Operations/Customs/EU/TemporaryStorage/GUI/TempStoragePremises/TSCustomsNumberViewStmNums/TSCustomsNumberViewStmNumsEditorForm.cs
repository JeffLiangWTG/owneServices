using System;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class TSCustomsNumberViewStmNumsEditorForm : MasterFiles.GUI.CustomsNumberViewStmNumsEditorForm
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public TSCustomsNumberViewStmNumsEditorForm()
		{
			InitializeComponent();
		}

		public TSCustomsNumberViewStmNumsEditorForm(TSCustomsNumberViewStmNumsWrapper stmNums)
		: base(stmNums)
		{
			InitializeComponent();
			FountainNameTextBox.Visible = false;
		}
	}
}
