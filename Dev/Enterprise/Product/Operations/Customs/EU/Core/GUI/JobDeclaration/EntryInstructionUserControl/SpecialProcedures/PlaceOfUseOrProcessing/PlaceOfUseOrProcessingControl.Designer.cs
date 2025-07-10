namespace Enterprise.Customs.EU.GUI
{
	partial class PlaceOfUseOrProcessingControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PlaceOfUseOrProcessingDescription = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.IFirstPlaceOfUseOrProcessingProvider);
			// 
			// MoreButton
			// 
			this.MoreButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3D9A455D-1F55-40EA-A6D6-BC3FDCDAD261", "More..");
			this.MoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 0, true);
			this.MoreButton.Name = "MoreButton";
			this.MoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.MoreButton.TabIndex = 1;
			this.MoreButton.ToolTipCaption = null;
			this.MoreButton.Click += new System.EventHandler(this.MoreButton_Click);
			// 
			// PlaceOfUseOrProcessingDescription
			// 
			this.BindingSource.SetBindingMember(this.PlaceOfUseOrProcessingDescription, "FirstPlaceOfUseOrProcessingDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.IFirstPlaceOfUseOrProcessingProvider)(null)).FirstPlaceOfUseOrProcessingDescription)));
			this.PlaceOfUseOrProcessingDescription.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("5e316bb6-c920-47c1-b6e5-b751f947b875", "", "[4/5] First Place of Processing");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfUseOrProcessingDescription, false);
			this.PlaceOfUseOrProcessingDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceOfUseOrProcessingDescription.Name = "PlaceOfUseOrProcessingDescription";
			this.PlaceOfUseOrProcessingDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.PlaceOfUseOrProcessingDescription.TabIndex = 0;
			// 
			// PlaceOfUseOrProcessingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MoreButton);
			this.Controls.Add(this.PlaceOfUseOrProcessingDescription);
			this.Name = "PlaceOfUseOrProcessingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.GUI.ZButton MoreButton;
		protected internal ZArchitecture.ZTextBox PlaceOfUseOrProcessingDescription;
	}
}
