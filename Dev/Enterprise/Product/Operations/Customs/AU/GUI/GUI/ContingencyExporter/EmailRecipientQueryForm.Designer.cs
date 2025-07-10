using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
    public partial class EmailRecipientQueryForm
    {
		protected override void InitializeComponent()
		{
			this.recipientDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.cancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 164, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 10, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection);
			// 
			// RecipientDropEdit
			// 
			this.recipientDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.recipientDropEdit, "SelectedRecipientName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection)(null)).SelectedRecipientName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection)(null)).Recipients)));
			this.recipientDropEdit.BindToList = "Recipients";
			this.recipientDropEdit.CaptionResourceString = null;
			this.recipientDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 52, true);
			this.recipientDropEdit.MaxItemsToShowInDropDown = 20;
			this.recipientDropEdit.Name = "RecipientDropEdit";
			this.recipientDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 20, true);
			this.recipientDropEdit.TabIndex = 2;
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = null;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 135, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.SendButton.TabIndex = 7;
			this.SendButton.Text = "Send";
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "RecipientName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection)(null)).RecipientName)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 74, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zTextBox1.TabIndex = 4;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "RecipientEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection)(null)).RecipientEmail)));
			this.zTextBox2.CaptionResourceString = null;
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 97, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 20, true);
			this.zTextBox2.TabIndex = 6;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = null;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 96, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.zLabel1.TabIndex = 5;
			this.zLabel1.Text = "Email Address:";
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = null;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 74, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.Text = "Name:";
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = null;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 52, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.zLabel3.TabIndex = 1;
			this.zLabel3.Text = "Recipient:";
			// 
			// zLabel4
			// 
			this.zLabel4.CaptionResourceString = null;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 15, true);
			this.zLabel4.TabIndex = 0;
			this.zLabel4.Text = "Please choose which customs email address you want to send the Contingency data t" +
	"o. Optionally, you can enter a different name and address - this ";
			// 
			// CancelButtonX
			// 
			this.cancelButtonX.CaptionResourceString = null;
			this.cancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 135, true);
			this.cancelButtonX.Name = "CancelButtonX";
			this.cancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.cancelButtonX.TabIndex = 8;
			this.cancelButtonX.Text = "Cancel";
			// 
			// zLabel5
			// 
			this.zLabel5.CaptionResourceString = null;
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 15, true);
			this.zLabel5.TabIndex = 10;
			this.zLabel5.Text = "will then be added to the list and be available next time you Create Contingency " +
	"Data.";
			// 
			// EmailRecipientQueryForm
			// 
			this.AcceptButton = this.SendButton;

			this.CancelButton = this.cancelButtonX;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 174, true);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.cancelButtonX);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.recipientDropEdit);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.EmailRecipientSelection";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "EmailRecipientQueryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Customs Contingency Data";
			this.Controls.SetChildIndex(this.recipientDropEdit, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.cancelButtonX, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZButton SendButton;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZTextBox zTextBox2;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZLabel zLabel5;
		private ZButton cancelButtonX;
		private ZDropEdit recipientDropEdit;
	}
}
