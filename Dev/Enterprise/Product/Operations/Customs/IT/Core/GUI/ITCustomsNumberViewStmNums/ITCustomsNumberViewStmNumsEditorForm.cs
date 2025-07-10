using System;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public partial class ITCustomsNumberViewStmNumsEditorForm : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyEditorForm
{
	[Obsolete("Required by the designer on subclass. Please do not use.")]
	public ITCustomsNumberViewStmNumsEditorForm()
	{
		InitializeComponent();
	}

	public ITCustomsNumberViewStmNumsEditorForm(ITCustomsNumberViewStmNumsWrapper stmNums)
		: base(stmNums)
	{
		InitializeComponent();
		FountainNameTextBox.Visible = false;
	}
}
