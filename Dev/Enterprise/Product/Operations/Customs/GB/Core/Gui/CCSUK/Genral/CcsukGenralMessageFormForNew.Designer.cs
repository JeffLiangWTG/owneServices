namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukGenralMessageFormForNew
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BadgeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AirportTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsPimasDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RecipientGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RadioRollYourOwn = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RadioCustoms = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RadioAgent = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RadioShed = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendingBadgeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GroupBoxSender = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PayloadTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.preformattedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RecipientGroupBox.SuspendLayout();
			this.GroupBoxSender.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 479, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew);
			// 
			// CreateButton
			//
			this.CreateButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("325afbf5-dc51-4d3d-8efc-fe442db14ca2", "Create GENRAL");
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 449, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CreateButton.TabIndex = 3;
			this.CreateButton.UseVisualStyleBackColor = true;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// BadgeTextBox
			// 
			this.BindingSource.SetBindingMember(this.BadgeTextBox, "ShedOrBadge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).ShedOrBadge)));
			this.BadgeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 124, true);
			this.BadgeTextBox.Name = "BadgeTextBox";
			this.BadgeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.BadgeTextBox.TabIndex = 4;
			// 
			// AirportTextBox
			// 
			this.BindingSource.SetBindingMember(this.AirportTextBox, "Airport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).Airport)));
			this.AirportTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 152, true);
			this.AirportTextBox.Name = "AirportTextBox";
			this.AirportTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.AirportTextBox.TabIndex = 5;
			// 
			// CustomsPimasDropEdit
			// 
			this.CustomsPimasDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsPimasDropEdit, "Pima");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).Pima)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).RecipientPimasList)));
			this.CustomsPimasDropEdit.BindToList = "RecipientPimasList";
			this.CustomsPimasDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 178, true);
			this.CustomsPimasDropEdit.Name = "CustomsPimasDropEdit";
			this.CustomsPimasDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.CustomsPimasDropEdit.TabIndex = 6;
			// 
			// RecipientGroupBox
			//
			this.RecipientGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6fcdd1f7-3a2b-4627-a9ee-eea76258b046", "Recipient");
			this.RecipientGroupBox.Controls.Add(this.RadioRollYourOwn);
			this.RecipientGroupBox.Controls.Add(this.RadioCustoms);
			this.RecipientGroupBox.Controls.Add(this.RadioAgent);
			this.RecipientGroupBox.Controls.Add(this.RadioShed);
			this.RecipientGroupBox.Controls.Add(this.CustomsPimasDropEdit);
			this.RecipientGroupBox.Controls.Add(this.BadgeTextBox);
			this.RecipientGroupBox.Controls.Add(this.AirportTextBox);
			this.RecipientGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.RecipientGroupBox.Name = "RecipientGroupBox";
			this.RecipientGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 209, true);
			this.RecipientGroupBox.TabIndex = 0;
			this.RecipientGroupBox.TabStop = false;
			// 
			// RadioRollYourOwn
			// 
			this.RadioRollYourOwn.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.RadioRollYourOwn, "IsRecipientRollYourOwn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).IsRecipientRollYourOwn)));
			this.RadioRollYourOwn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7eea3a6c-c9b5-4bb2-8091-fac333b77107", "I will specify the recipient's full PIMA myself");
			this.RadioRollYourOwn.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RadioRollYourOwn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 100, true);
			this.RadioRollYourOwn.Name = "RadioRollYourOwn";
			this.RadioRollYourOwn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 18, true);
			this.RadioRollYourOwn.TabIndex = 3;
			this.RadioRollYourOwn.TabStop = true;
			this.RadioRollYourOwn.UseVisualStyleBackColor = true;
			// 
			// RadioCustoms
			// 
			this.RadioCustoms.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.RadioCustoms, "IsRecipientCustoms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).IsRecipientCustoms)));
			this.RadioCustoms.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0ca94dc4-efbb-4763-872f-63372028434c", "Customs officers (CTM)");
			this.RadioCustoms.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RadioCustoms.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 73, true);
			this.RadioCustoms.Name = "RadioCustoms";
			this.RadioCustoms.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 18, true);
			this.RadioCustoms.TabIndex = 2;
			this.RadioCustoms.TabStop = true;
			this.RadioCustoms.UseVisualStyleBackColor = true;
			// 
			// RadioAgent
			// 
			this.RadioAgent.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.RadioAgent, "IsRecipientAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).IsRecipientAgent)));
			this.RadioAgent.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d143431f-9619-4b55-894e-5ae25a5c2475", "Agent (FFW)");
			this.RadioAgent.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RadioAgent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 46, true);
			this.RadioAgent.Name = "RadioAgent";
			this.RadioAgent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 18, true);
			this.RadioAgent.TabIndex = 1;
			this.RadioAgent.TabStop = true;
			this.RadioAgent.UseVisualStyleBackColor = true;
			// 
			// RadioShed
			// 
			this.RadioShed.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.RadioShed, "IsRecipientShed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).IsRecipientShed)));
			this.RadioShed.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8715660a-185a-4344-b92f-1ba3b0a226a1", "Airline/Shed (AIR)");
			this.RadioShed.Checked = true;
			this.RadioShed.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RadioShed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 19, true);
			this.RadioShed.Name = "RadioShed";
			this.RadioShed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 18, true);
			this.RadioShed.TabIndex = 0;
			this.RadioShed.TabStop = true;
			this.RadioShed.UseVisualStyleBackColor = true;
			// 
			// SendingBadgeDropEdit
			// 
			this.SendingBadgeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendingBadgeDropEdit, "SendingProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).SendingProfile)));
			this.SendingBadgeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 18, true);
			this.SendingBadgeDropEdit.Name = "SendingBadgeDropEdit";
			this.SendingBadgeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.SendingBadgeDropEdit.TabIndex = 0;
			// 
			// GroupBoxSender
			//
			this.GroupBoxSender.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("429c0b3b-9368-4f9e-a97c-d4e0c5be426e", "Sender");
			this.GroupBoxSender.Controls.Add(this.SendingBadgeDropEdit);
			this.GroupBoxSender.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 219, true);
			this.GroupBoxSender.Name = "GroupBoxSender";
			this.GroupBoxSender.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 44, true);
			this.GroupBoxSender.TabIndex = 1;
			this.GroupBoxSender.TabStop = false;
			// 
			// PayloadTextBox
			// 
			this.BindingSource.SetBindingMember(this.PayloadTextBox, "Payload");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).Payload)));
			this.PayloadTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PayloadTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 12, true);
			this.PayloadTextBox.Multiline = true;
			this.PayloadTextBox.Name = "PayloadTextBox";
			this.PayloadTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.PayloadTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 128, true);
			this.PayloadTextBox.TabIndex = 2;
			// 
			// zGroupBox1
			//
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("61182ca5-65a0-4866-a0f9-2d8e3c0b8a3f", "Message Text");
			this.zGroupBox1.Controls.Add(this.preformattedCheckBox);
			this.zGroupBox1.Controls.Add(this.PayloadTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 269, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 174, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			// 
			// preformattedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.preformattedCheckBox, "PreformattedLinesOf70");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew)(null)).PreformattedLinesOf70)));
			this.preformattedCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ffc866a5-7d60-4323-9646-0ffe9acf5414", "Preformatted");
			this.preformattedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.preformattedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 144, true);
			this.preformattedCheckBox.Name = "preformattedCheckBox";
			this.preformattedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 24, true);
			this.preformattedCheckBox.TabIndex = 4;
			this.preformattedCheckBox.UseVisualStyleBackColor = true;
			// 
			// CcsukGenralMessageFormForNew
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5f73e99d-e457-410c-a831-b48cba668d1b", "CCS-UK new GENRAL message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 503, true);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.CreateButton);
			this.Controls.Add(this.GroupBoxSender);
			this.Controls.Add(this.RecipientGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.NonPersistentGenralEdiMessageForNew);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CcsukGenralMessageFormForNew";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.RecipientGroupBox, 0);
			this.Controls.SetChildIndex(this.GroupBoxSender, 0);
			this.Controls.SetChildIndex(this.CreateButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RecipientGroupBox.ResumeLayout(false);
			this.RecipientGroupBox.PerformLayout();
			this.GroupBoxSender.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion 

		public ZArchitecture.GUI.ZButton CreateButton;
		private ZArchitecture.ZTextBox BadgeTextBox;
		private ZArchitecture.ZTextBox AirportTextBox;
		private ZArchitecture.GUI.ZDropEdit CustomsPimasDropEdit;
		private ZArchitecture.GUI.ZGroupBox RecipientGroupBox;
		private ZArchitecture.GUI.ZDropEdit SendingBadgeDropEdit;
		private ZArchitecture.GUI.ZGroupBox GroupBoxSender;
		private ZArchitecture.GUI.ZRadioButton RadioRollYourOwn;
		private ZArchitecture.GUI.ZRadioButton RadioCustoms;
		private ZArchitecture.GUI.ZRadioButton RadioAgent;
		private ZArchitecture.GUI.ZRadioButton RadioShed;
		private ZArchitecture.ZTextBox PayloadTextBox;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZCheckBox preformattedCheckBox;
	}
}
