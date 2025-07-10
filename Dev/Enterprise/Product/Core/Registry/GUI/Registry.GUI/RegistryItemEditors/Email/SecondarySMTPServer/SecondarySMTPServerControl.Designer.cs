using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class SecondarySMTPServerControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();

			this.SecondarySMTPServersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SecondarySMTPServersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SecondarySMTPServerCollection);
			// 
			// SecondarySMTPServersGrid
			// 
			this.SecondarySMTPServersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SecondarySMTPServersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SecondarySMTPServerCollection)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SMTPServer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SMTPPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SMTPSecureConnection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SMTPUsername)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SMTPPassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).AllowEmailsToBeSentFromUsersAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SMTPSenderAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SecondarySMTPServer)(null)).SupportedDomains)));
			this.SecondarySMTPServersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|45f212da-5885-4251-ac5e-3f276839c8f6", "Server", "SMTP Server", "SMTP Server Address");
			zTextBoxColumnStyleInfo1.ColumnName = "SMTPServer";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|d4324da2-898a-4a42-9b11-bb8b4b5bb5c5", "Port", "SMTP Port", "SMTP Port Number");
			zCalcEditColumnStyleInfo1.ColumnName = "SMTPPort";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|7363ad11-3aad-4d76-8c6a-ce07875f5790", "Secure Connection", "SMTP Secure Connection", "SMTP Secure Connection Type");
			zDropEditColumnStyleInfo1.ColumnName = "SMTPSecureConnection";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|1a0f9ed9-00c3-4522-a454-33fb08d5ce66", "User", "SMTP User", "SMTP User Name");
			zTextBoxColumnStyleInfo2.ColumnName = "SMTPUsername";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|5099a6d6-f182-4312-913b-2a2ba7054552", "Password", "SMTP Password", "SMTP User Password");
			zTextBoxColumnStyleInfo3.ColumnName = "SMTPPassword";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.PasswordChar = '*';
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|a59c1024-7b35-4fd4-b3c2-9ad41faf91b3", "Send From User", "Allow to Send From User", "Allow Emails To Be Sent From Users Address");
			zCheckBoxColumnStyleInfo1.ColumnName = "AllowEmailsToBeSentFromUsersAddress";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|ab58456d-3df5-4896-8015-2177edbaa3c0", "Sender", "Sender Address", "SMTP Sender Address");
			zTextBoxColumnStyleInfo4.ColumnName = "SMTPSenderAddress";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SecondarySMTPServerControl|690c2a98-3b56-49f3-a322-d40b565225aa", "Domains", "Supported Domains", "Supported Mail Domains");
			zMultiLineTextBoxColumnInfo1.ColumnName = "SupportedDomains";
			zMultiLineTextBoxColumnInfo1.WordWrap = true;

			this.SecondarySMTPServersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SecondarySMTPServersGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.SecondarySMTPServersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondarySMTPServersGrid.GridId = "4c04818a-bd95-445f-9a87-b833b9a86013";
			this.SecondarySMTPServersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecondarySMTPServersGrid.LayoutKey = "zGrid1";
			this.SecondarySMTPServersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecondarySMTPServersGrid.Name = "SecondarySMTPServersGrid";
			this.SecondarySMTPServersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 293, true);
			this.SecondarySMTPServersGrid.TabIndex = 0;
			// 
			// SecondarySMTPServerControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SecondarySMTPServersGrid);
			this.Name = "SecondarySMTPServerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SecondarySMTPServersGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		Enterprise.ZArchitecture.ZGrid SecondarySMTPServersGrid;
	}
}
