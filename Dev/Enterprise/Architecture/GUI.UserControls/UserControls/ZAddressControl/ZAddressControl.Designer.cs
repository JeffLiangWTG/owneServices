using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A control for selecting an organisation and address.
	/// SetDataBinding() takes a dataSource and dataMember that point to the OrgAddress foreign key.
	/// For example, SetDataBinding(shipment, "JS_OA_NotifyParty");
	/// In this example, property JS_OA_NotifyParty_ZAddress of type ZAddress is also expected.
	/// </summary>
	public partial class ZAddressControl
	{
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.OrganisationFindBox = new ZAddressFindBox.Bare();
			this.AddressDropEdit = new ZAddressDropEdit.Bare();
			this.AddressTextBox = new ZTextBox.Bare();
			this.AddressStatusButton = new ZButton();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationFindBox.SuspendLayout();
			this.AddressDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// OrganisationFindBox
			//
			this.OrganisationFindBox.AllowDrop = true;
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationFindBox.ModuleID = ((OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.ShowDescriptionBox = false;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.OrganisationFindBox.TabIndex = 0;
			//
			// AddressDropEdit
			//
			this.AddressDropEdit.AllowDrop = true;
			this.AddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			this.AddressDropEdit.Name = "AddressDropEdit";
			this.AddressDropEdit.PreBoundMaxLength = 28;
			this.AddressDropEdit.ShowDescriptionBox = false;
			this.AddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.AddressDropEdit.TabIndex = 1;
			//
			// AddressTextBox
			//
			this.AddressTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.AddressTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.AddressTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.AddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.AddressTextBox.Multiline = true;
			this.AddressTextBox.Name = "AddressTextBox";
			this.AddressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 26, true);
			this.AddressTextBox.TabIndex = 2;
			//
			// AddressStatusButton
			//
			this.AddressStatusButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.AddressStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 0, true);
			this.AddressStatusButton.Name = "AddressStatusButton";
			this.AddressStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 19, true);
			this.AddressStatusButton.TabIndex = 39;
			this.AddressStatusButton.UseVisualStyleBackColor = true;
			this.AddressStatusButton.Text = " ";
			this.AddressStatusButton.Image = Properties.Resources.AddressOriginal;
			this.AddressStatusButton.Click += AddressStatusButton_Click;
			//
			// ZAddressControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressStatusButton);
			this.Controls.Add(this.AddressTextBox);
			this.Controls.Add(this.AddressDropEdit);
			this.Controls.Add(this.OrganisationFindBox);
			this.Name = "ZAddressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 60, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.AddressDropEdit.ResumeLayout(true);
			this.AddressDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
