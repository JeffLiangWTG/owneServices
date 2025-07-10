using CargoWise.Windows.UI;

namespace Enterprise.Customs.EU.Module
{
	public partial class PermitTypeFilterStrip : Customs.Module.PermitTypeFilterStrip
	{
		public PermitTypeFilterStrip()
		{
			InitializeComponent();
			InitializeCountrySpecificComponent();
		}

		void InitializeCountrySpecificComponent()
		{
			ChangePermitTypeFromDropEditorToCodeFindBox();
		}

		void ChangePermitTypeFromDropEditorToCodeFindBox()
		{
			typeCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			typeCodeFindBox.SuspendLayout();
			Controls.Add(typeCodeFindBox);
			typeCodeFindBox.AllowDrop = true;
			BindingSource.SetBindingMember(typeCodeFindBox, nameof(PermitTypeModuleFilter.Property3));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PermitTypeModuleFilter)(null)).Property3);
			typeCodeFindBox.Location = TypeDropEdit.Location;
			typeCodeFindBox.Name = "TypeCodeFindBox";
			typeCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(TypeDropEdit.Width + TypeDropEdit.Margin.Right + SubTypeDropEdit.Width, TypeDropEdit.Height);
			typeCodeFindBox.PreBoundMaxLength = TypeDropEdit.PreBoundMaxLength;
			typeCodeFindBox.TabIndex = TypeDropEdit.TabIndex;
			typeCodeFindBox.ResumeLayout(true);
			typeCodeFindBox.PerformLayout();

			Controls.Remove(TypeDropEdit);
			Controls.Remove(SubTypeDropEdit);
		}

		ZArchitecture.GUI.ZCodeFindBox typeCodeFindBox;
	}
}
