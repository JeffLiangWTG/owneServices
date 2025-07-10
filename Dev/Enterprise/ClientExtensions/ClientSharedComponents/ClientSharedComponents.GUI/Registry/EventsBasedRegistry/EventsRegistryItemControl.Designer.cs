using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class EventsRegistryItemControl : RegistryZUserControl
	{
		Enterprise.ZArchitecture.ZGrid zGrid1;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObjectCollection);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObject)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObject)(null)).EventsCodeDescriptionPairList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObject)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.EventRegistryBusinessObject)(null)).Reference)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "EventsCodeDescriptionPairList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("EventsRegistryItemControl|96c3c0c2-6c2e-421c-ac1b-d8bd6de1d9bf", "Event Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("07eb213a-5792-48fd-a144-96b04f580f53", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("EventsRegistryItemControl|7c097f99-86de-458b-b085-b657e53486ef", "Event Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "Reference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "feb95d25-925e-41d0-81da-b73092330b81";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			this.zGrid1.TabIndex = 0;
			// 
			// EventsRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Name = "EventsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
