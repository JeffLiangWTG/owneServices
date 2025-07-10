using CargoWiseOne.ResourceStrings;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class RenominationUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenominationUserControl));
			this.ButtonGenerate = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewAgentCodeFind = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SendGenralCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ButtonGenerate
			// 
			this.ButtonGenerate.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6481c5e9-fcd1-4ae1-88ee-5531828ef81b", "Request Renomination");
			this.ButtonGenerate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 129, true);
			this.ButtonGenerate.Name = "ButtonGenerate";
			this.ButtonGenerate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ButtonGenerate.TabIndex = 3;
			this.ButtonGenerate.UseVisualStyleBackColor = true;
			this.ButtonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
			// 
			// NewAgentCodeFind
			// 
			this.NewAgentCodeFind.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewAgentCodeFind, "NewAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.NewAgentCodeFind.CaptionResourceString = NewAgentCaption;
			this.NewAgentCodeFind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 79, true);
			this.NewAgentCodeFind.Name = "NewAgentCodeFind";
			this.NewAgentCodeFind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.NewAgentCodeFind.TabIndex = 1;
			// 
			// SendGenralCheckbox
			// 
			this.SendGenralCheckbox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4dfc2be3-7cc4-4999-86fc-b6f2c4b51ae9", "Also send GENRAL text message to new agent?");
			this.SendGenralCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendGenralCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 104, true);
			this.BindingSource.SetBindingMember(SendGenralCheckbox, "SendNewAgentGenral");
			this.SendGenralCheckbox.Name = "SendGenralCheckbox";
			this.SendGenralCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.SendGenralCheckbox.TabIndex = 2;
			this.SendGenralCheckbox.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			//
			this.zLabel1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("b3885698-916c-4e9b-9555-4f88d34c2702", "You are about to REQUEST that the shed renominates the record to the new agent. They may do so automatically, manually, or not at all. Having sent this request you may receive an FRC message advising that you're no longer nominated.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 72, true);
			this.zLabel1.TabIndex = 0;
			// 
			// RenominationUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.SendGenralCheckbox);
			this.Controls.Add(this.NewAgentCodeFind);
			this.Controls.Add(this.ButtonGenerate);
			this.Name = "RenominationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 161, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public static ResourceStringData NewAgentCaption
		{
			get { return Enterprise.Customs.GB.GUI.Res.GetData("RenominationUserControl|309133b8-7065-4f06-8c6b-99ec62348c37", "New Agent");}
		}

		private ZArchitecture.GUI.ZButton ButtonGenerate;
		private ZArchitecture.GUI.ZDropEdit NewAgentCodeFind;
		private ZArchitecture.GUI.ZCheckBox SendGenralCheckbox;
		private ZArchitecture.ZLabel zLabel1;
	}
}
