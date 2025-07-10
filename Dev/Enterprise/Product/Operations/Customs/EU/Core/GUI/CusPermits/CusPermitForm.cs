using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CusPermitForm : Customs.GUI.CusPermitForm
	{
		public CusPermitForm() : base()
		{
		}

		public CusPermitForm(CusPermitHeader header) : base(header)
		{
			InitializeComponent();
			InitializeCountrySpecificComponent();
		}

		void InitializeCountrySpecificComponent()
		{
			AddPermitFullTypeCodeFindBox();
			ChangeUnitOfMeasureToDropDown();
			MoveDownOtherControls();
		}

		void ChangeUnitOfMeasureToDropDown()
		{
			UnitOfMeasureZTextBox.Visible = false;

			unitOfMeasureZDropEdit = new ZArchitecture.GUI.ZDropEdit();
			unitOfMeasureZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.unitOfMeasureZDropEdit, "CPH_UnitOfMeasure");
			unitOfMeasureZDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CusPermitForm|22641BD3-A5C9-49F6-8776-D529600E66B1", "Unit of Measure");
			unitOfMeasureZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 97, true);
			unitOfMeasureZDropEdit.Name = "UnitOfMeasureZDropEdit";
			unitOfMeasureZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			unitOfMeasureZDropEdit.Visible = true;
			unitOfMeasureZDropEdit.TabIndex = 9;

			PermitDetailsGroupBox.Controls.Add(this.unitOfMeasureZDropEdit);
		}

		void AddPermitFullTypeCodeFindBox()
		{
			permitFullTypeCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			permitFullTypeCodeFindBox.SuspendLayout();
			PermitDetailsGroupBox.Controls.Add(permitFullTypeCodeFindBox);
			permitFullTypeCodeFindBox.AllowDrop = true;
			BindingSource.SetBindingMember(permitFullTypeCodeFindBox, "CPH_FullType");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusPermitHeader)(null)).CPH_FullType);
			permitFullTypeCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CusPermitForm|9c201369-6e5f-477e-b050-0ad8062ee9f9", "Permit Full Type");
			permitFullTypeCodeFindBox.Location = PermitTypeZDropEdit.Location;
			permitFullTypeCodeFindBox.Name = "PermitTypeZCodeFindBox";
			permitFullTypeCodeFindBox.PreBoundMaxLength = PermitTypeZDropEdit.PreBoundMaxLength;
			permitFullTypeCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(PermitSubTypeZDropEdit.Right - PermitTypeZDropEdit.Left, PermitTypeZDropEdit.Height, false);
			permitFullTypeCodeFindBox.Margin = PermitTypeZDropEdit.Margin;
			permitFullTypeCodeFindBox.TabIndex = PermitTypeZDropEdit.TabIndex;
			permitFullTypeCodeFindBox.ResumeLayout(true);
			permitFullTypeCodeFindBox.PerformLayout();
		}

		void MoveDownOtherControls()
		{
			var offset = permitFullTypeCodeFindBox.Height + permitFullTypeCodeFindBox.Margin.Bottom;
			PermitDetailsGroupBox.Controls.Cast<Control>().Where(x => x != permitFullTypeCodeFindBox).ForEach(x => ControlDpiScalingHelper.SetTop(x, x.Top + offset, false));
			ControlDpiScalingHelper.SetHeight(PermitDetailsGroupBox, PermitDetailsGroupBox.Height + offset, false);
			ControlDpiScalingHelper.SetHeight(zPanel1, zPanel1.Height + offset, false);
		}

		ZArchitecture.GUI.ZCodeFindBox permitFullTypeCodeFindBox;
		protected ZArchitecture.GUI.ZDropEdit unitOfMeasureZDropEdit;

		protected override Control unitOfMeasureControl => unitOfMeasureZDropEdit;
	}
}
