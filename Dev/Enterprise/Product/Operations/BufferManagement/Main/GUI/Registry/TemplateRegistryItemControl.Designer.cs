using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	partial class TemplateRegistryItemControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			CaptionRenderingEnabled = true;
			Size = ControlDpiScalingHelper.NewScaledSize(300, 250, true);

			GroupBox = new ZGroupBox();
			CheckBox = new ZCheckBox();
			Criterion1Label = new ZLabel();
			Criterion1Box = new ZDropEdit();
			Criterion2Label = new ZLabel();
			Criterion2Box = new ZDropEdit();
			Criterion3Label = new ZLabel();
			Criterion3Box = new ZDropEdit();
			Criterion4Label = new ZLabel();
			Criterion4Box = new ZDropEdit();
			Criterion5Label = new ZLabel();
			Criterion5Box = new ZDropEdit();

			GroupBox.Controls.Add(CheckBox);
			GroupBox.Controls.Add(Criterion1Label);
			GroupBox.Controls.Add(Criterion1Box);
			GroupBox.Controls.Add(Criterion2Label);
			GroupBox.Controls.Add(Criterion2Box);
			GroupBox.Controls.Add(Criterion3Label);
			GroupBox.Controls.Add(Criterion3Box);
			GroupBox.Controls.Add(Criterion4Label);
			GroupBox.Controls.Add(Criterion4Box);
			GroupBox.Controls.Add(Criterion5Label);
			GroupBox.Controls.Add(Criterion5Box);

			GroupBox.CaptionResourceString = Res.GetData("22A2C947-80CC-4BBB-9107-CF16557EB3E6", "Template Criteria");
			GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			GroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			GroupBox.Name = "GroupBox";
			GroupBox.TabStop = false;

			BindingSource.SetBindingMember(CheckBox, "Enabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZBool)(((TemplateCriteria)(null)).Enabled)));
			CheckBox.CaptionResourceString = Res.GetData("F3346968-70BF-4712-AAC9-673B9F7092BA", "Enabled");
			CheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			CheckBox.Name = "EnabledCheckBox";
			CheckBox.TabIndex = 1;

			BindingSource.SetBindingMember(Criterion1Label, "Criterion1Label");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion1Label)));
			Criterion1Label.Location = ControlDpiScalingHelper.NewScaledPoint(10, 50,true);
			Criterion1Label.Name = "Criterion1Label";
			Criterion1Label.Size = ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			Criterion1Label.TextAlign = ContentAlignment.MiddleRight;
			Criterion1Label.TabStop = false;

			BindingSource.SetBindingMember(Criterion1Box, "Criterion1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion1)));
			Criterion1Box.CaptionResourceString = Res.GetData("397B3B5A-567D-4BDE-AED7-9EA3A813EE73", "Criteria 1");
			Criterion1Box.Location = ControlDpiScalingHelper.NewScaledPoint(130, 50, true);
			Criterion1Box.Name = "Criterion1Box";
			Criterion1Box.Size = ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			Criterion1Box.TabIndex = 2;

			BindingSource.SetBindingMember(Criterion2Label, "Criterion2Label");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion2Label)));
			Criterion2Label.Location = ControlDpiScalingHelper.NewScaledPoint(10, 80, true);
			Criterion2Label.Name = "Criterion2Label";
			Criterion2Label.Size = ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			Criterion2Label.TextAlign = ContentAlignment.MiddleRight;
			Criterion2Label.TabStop = false;

			BindingSource.SetBindingMember(Criterion2Box, "Criterion2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion2)));
			Criterion2Box.CaptionResourceString = Res.GetData("0F8B810B-3A8F-4CDF-A493-A493A9521A8E", "Criteria 2");
			Criterion2Box.Location = ControlDpiScalingHelper.NewScaledPoint(130, 80, true);
			Criterion2Box.Name = "Criterion2Box";
			Criterion2Box.Size = ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			Criterion2Box.TabIndex = 3;

			BindingSource.SetBindingMember(Criterion3Label, "Criterion3Label");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion3Label)));
			Criterion3Label.Location = ControlDpiScalingHelper.NewScaledPoint(10, 110, true);
			Criterion3Label.Name = "Criterion3Label";
			Criterion3Label.Size = ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			Criterion3Label.TextAlign = ContentAlignment.MiddleRight;
			Criterion3Label.TabStop = false;

			BindingSource.SetBindingMember(Criterion3Box, "Criterion3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion3)));
			Criterion3Box.CaptionResourceString = Res.GetData("9FA7FAC6-0562-4F6A-A3DF-D4B41C872E48", "Criteria 3");
			Criterion3Box.Location = ControlDpiScalingHelper.NewScaledPoint(130, 110, true);
			Criterion3Box.Name = "Criterion3Box";
			Criterion3Box.Size = ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			Criterion3Box.TabIndex = 4;

			BindingSource.SetBindingMember(Criterion4Label, "Criterion4Label");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion4Label)));
			Criterion4Label.Location = ControlDpiScalingHelper.NewScaledPoint(10, 140, true);
			Criterion4Label.Name = "Criterion4Label";
			Criterion4Label.Size = ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			Criterion4Label.TextAlign = ContentAlignment.MiddleRight;
			Criterion4Label.TabStop = false;

			BindingSource.SetBindingMember(Criterion4Box, "Criterion4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion4)));
			Criterion4Box.CaptionResourceString = Res.GetData("C639F260-0C58-4D6C-9043-3A41C27389A8", "Criteria 4");
			Criterion4Box.Location = ControlDpiScalingHelper.NewScaledPoint(130, 140, true);
			Criterion4Box.Name = "Criterion4Box";
			Criterion4Box.Size = ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			Criterion4Box.TabIndex = 5;

			BindingSource.SetBindingMember(Criterion5Label, "Criterion5Label");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion5Label)));
			Criterion5Label.Location = ControlDpiScalingHelper.NewScaledPoint(10, 170, true);
			Criterion5Label.Name = "Criterion5Label";
			Criterion5Label.Size = ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			Criterion5Label.TextAlign = ContentAlignment.MiddleRight;
			Criterion5Label.TabStop = false;

			BindingSource.SetBindingMember(Criterion5Box, "Criterion5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((ZString)(((TemplateCriteria)(null)).Criterion5)));
			Criterion5Box.CaptionResourceString = Res.GetData("B2E97D4A-04F1-4EE4-98D3-F146A23C944A", "Criteria 5");
			Criterion5Box.Location = ControlDpiScalingHelper.NewScaledPoint(130, 170, true);
			Criterion5Box.Name = "Criterion5Box";
			Criterion5Box.Size = ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			Criterion5Box.TabIndex = 6;

			Controls.Add(GroupBox);
		}

		#endregion
	}
}
