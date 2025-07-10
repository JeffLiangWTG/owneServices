namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class AdditionalTransportBorderUserControl
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
			this.MoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdditionalTransportBorderMeansCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// MoreButton
			// 
			this.MoreButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("31acdcf9-8e83-42af-8782-314b05065d0e", "More...");
			this.MoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MoreButton.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 22, true);
			this.MoreButton.Name = "MoreButton";
			this.MoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 22, true);
			this.MoreButton.TabIndex = 11;
			this.MoreButton.ToolTipCaption = null;
			this.MoreButton.Click += new System.EventHandler(this.MoreButton_OnClick);
			// 
			// AdditionalTransportBorderMeansCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AdditionalTransportBorderMeansCountCalcEdit, "AdditionalTransportAtBorderListCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).AdditionalTransportAtBorderListCount)));
			this.AdditionalTransportBorderMeansCountCalcEdit.DecimalPlaces = 2;
			this.AdditionalTransportBorderMeansCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 5, true);
			this.AdditionalTransportBorderMeansCountCalcEdit.Name = "AdditionalTransportBorderMeansCountCalcEdit";
			this.AdditionalTransportBorderMeansCountCalcEdit.ReadOnly = true;
			this.AdditionalTransportBorderMeansCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.AdditionalTransportBorderMeansCountCalcEdit.TabIndex = 12;
			this.AdditionalTransportBorderMeansCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AdditionalTransportBorderMeansCountCalcEdit.TrackDisposedAccess = true;
			// 
			// AdditionalTransportBorderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MoreButton);
			this.Controls.Add(this.AdditionalTransportBorderMeansCountCalcEdit);
			this.Name = "AdditionalTransportBorderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 33, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton MoreButton;
		internal ZArchitecture.ZCalcEdit AdditionalTransportBorderMeansCountCalcEdit;
	}
}
