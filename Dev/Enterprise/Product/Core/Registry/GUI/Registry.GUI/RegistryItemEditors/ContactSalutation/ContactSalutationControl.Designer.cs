using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Enterprise.Registry.GUI
{
	public partial class ContactSalutationControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.contactSalutationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contactSalutationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ContactSalutationCollection);
			// 
			// contactSalutationsGrid
			// 
			this.contactSalutationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.contactSalutationsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ContactSalutation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ContactSalutation)(null)).EnglishSalutation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ContactSalutation)(null)).Gender)));
			this.contactSalutationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e4f8a30d-770d-46d7-ba26-f28c0f504151", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishSalutation";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c5304deb-37a0-4d1f-b06b-94ccb526934b", "Gender");
			zDropEditColumnStyleInfo1.ColumnName = "Gender";
			zDropEditColumnStyleInfo1.GroupName = null;
			this.contactSalutationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.contactSalutationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.contactSalutationsGrid.CopySelectedRowsAllowed = true;
			this.contactSalutationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contactSalutationsGrid.GridId = "f147720a-b089-4b80-b8a4-1ecf5254d75d";
			this.contactSalutationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.contactSalutationsGrid.LayoutKey = "contactSalutationsGrid";
			this.contactSalutationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.contactSalutationsGrid.Name = "contactSalutationsGrid";
			this.contactSalutationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			this.contactSalutationsGrid.TabIndex = 0;
			// 
			// ContactSalutationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.contactSalutationsGrid);
			this.Name = "ContactSalutationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contactSalutationsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid contactSalutationsGrid;
	}
}
