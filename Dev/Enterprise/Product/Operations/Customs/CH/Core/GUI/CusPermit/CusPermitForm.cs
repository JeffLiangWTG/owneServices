using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class CusPermitForm : Customs.GUI.CusPermitForm
{
	public CusPermitForm()
	{
	}

	public CusPermitForm(CusPermitHeader header) : base(header)
	{
		InitializeComponent();
		ChangeControlVisibility();
		ChangeUnitOfMeasureToDropDown();
	}

	void ChangeControlVisibility()
	{
		PermitSubTypeZDropEdit.Visible = false;
	}

	void ChangeUnitOfMeasureToDropDown()
	{
		UnitOfMeasureZTextBox.Visible = false;

		UnitOfMeasureZDropEdit = new ZDropEdit();
		UnitOfMeasureZDropEdit.Name = nameof(UnitOfMeasureZDropEdit);
		UnitOfMeasureZDropEdit.AllowDrop = UnitOfMeasureZTextBox.AllowDrop;
		UnitOfMeasureZDropEdit.CaptionResourceString = UnitOfMeasureZTextBox.CaptionResourceString;
		UnitOfMeasureZDropEdit.Location = UnitOfMeasureZTextBox.Location;
		UnitOfMeasureZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
		UnitOfMeasureZDropEdit.TabIndex = UnitOfMeasureZTextBox.TabIndex;
		BindingSource.SetBindingMember(UnitOfMeasureZDropEdit, nameof(CusPermitHeader.CPH_UnitOfMeasure));

		PermitDetailsGroupBox.Controls.Add(UnitOfMeasureZDropEdit);
	}

	internal ZDropEdit UnitOfMeasureZDropEdit;
	protected override Control unitOfMeasureControl => UnitOfMeasureZDropEdit;
}
