
namespace Enterprise.Customs.EU.GUI.SADH
{
	partial class SADHEntryForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

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
		protected new void InitializeComponent()
		{
			this.Section1EntryStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();

			this.Section14RepresentationTypeLabel = new CargoWise.Windows.UI.KLabel();
			this.Section30TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel.SuspendLayout();
			this.SectionAPanel.SuspendLayout();
			this.Section1Panel.SuspendLayout();
			this.Section2Panel.SuspendLayout();
			this.Section3Panel.SuspendLayout();
			this.Section4Panel.SuspendLayout();
			this.Section8Panel.SuspendLayout();
			this.Section7Panel.SuspendLayout();
			this.Section6Panel.SuspendLayout();
			this.Section5Panel.SuspendLayout();
			this.Section9Panel.SuspendLayout();
			this.Section10Panel.SuspendLayout();
			this.Section12Panel.SuspendLayout();
			this.Section11Panel.SuspendLayout();
			this.Section13Panel.SuspendLayout();
			this.Section16Panel.SuspendLayout();
			this.Section15Panel.SuspendLayout();
			this.Section17Panel.SuspendLayout();
			this.Section17bPanel.SuspendLayout();
			this.Section15bPanel.SuspendLayout();
			this.Section14Panel.SuspendLayout();
			this.Section19Panel.SuspendLayout();
			this.Section20Panel.SuspendLayout();
			this.Section22Panel.SuspendLayout();
			this.Section21Panel.SuspendLayout();
			this.Section24Panel.SuspendLayout();
			this.Section23Panel.SuspendLayout();
			this.Section27Panel.SuspendLayout();
			this.Section26Panel.SuspendLayout();
			this.Section25Panel.SuspendLayout();
			this.Section28Panel.SuspendLayout();
			this.Section31bPanel.SuspendLayout();
			this.Section32Panel.SuspendLayout();
			this.Section31Panel.SuspendLayout();
			this.Section34Panel.SuspendLayout();
			this.Section35Panel.SuspendLayout();
			this.Section36Panel.SuspendLayout();
			this.Section39Panel.SuspendLayout();
			this.Section38Panel.SuspendLayout();
			this.Section37Panel.SuspendLayout();
			this.Section40Panel.SuspendLayout();
			this.Setion42Panel.SuspendLayout();
			this.Section41Panel.SuspendLayout();
			this.Section43Panel.SuspendLayout();
			this.Section44Panel.SuspendLayout();
			this.SectionA1Panel.SuspendLayout();
			this.Section45Panel.SuspendLayout();
			this.Section47Panel.SuspendLayout();
			this.Section46Panel.SuspendLayout();
			this.Section49Panel.SuspendLayout();
			this.Section48Panel.SuspendLayout();
			this.SectionBPanel.SuspendLayout();
			this.Section50Panel.SuspendLayout();
			this.Section51Panel.SuspendLayout();
			this.SectionCPanel.SuspendLayout();
			this.Section52Panel.SuspendLayout();
			this.Section52CodePanel.SuspendLayout();
			this.Section53Panel.SuspendLayout();
			this.Section54Panel.SuspendLayout();
			this.SectionJPanel.SuspendLayout();
			this.Section33Panel.SuspendLayout();
			this.Section18Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Section1Panel
			// 
			this.Section1Panel.Controls.Add(this.Section1EntryStyleDropEdit);
			this.Section1Panel.Controls.SetChildIndex(this.Section1DescriptionLabel, 0);
			this.Section1Panel.Controls.SetChildIndex(this.Section1EntryStyleDropEdit, 0);
			// 
			// Section14Panel
			// 
			this.Section14Panel.Controls.Add(this.Section14RepresentationTypeLabel);
			this.Section14Panel.Controls.SetChildIndex(this.Section14DescriptionLabel, 0);
			this.Section14Panel.Controls.SetChildIndex(this.Section14TextBox, 0);
			this.Section14Panel.Controls.SetChildIndex(this.Section14RepresentationTypeLabel, 0);
			// 
			// Section30Panel
			// 
			this.Section30Panel.Controls.Add(this.Section30TextBox);
			this.Section30Panel.Controls.SetChildIndex(this.Section30TextBox, 0);
			this.Section30Panel.Controls.SetChildIndex(this.Section30DescriptionLabel, 0);
			// 
			// Section33Panel
			// 
			this.Section33Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 41, true);
			this.Section33Panel.Controls.SetChildIndex(this.Section33DescriptionLabel, 0);
			// 
			// Section1EntryStyleDropEdit
			// 
			this.Section1EntryStyleDropEdit.BindTo = "D1_EntryType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.EU.Business.SADH.SADHFormData)(null)).D1_EntryTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.SADH.SADHFormData)(null)).D1_EntryType)));
			this.Section1EntryStyleDropEdit.BindToList = "Lookups+EntryStyleList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.EU.Business.SADH.SADHFormData)(null)).Lookups.EntryStyleList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section1EntryStyleDropEdit, false);
			this.Section1EntryStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.Section1EntryStyleDropEdit.Name = "Section1EntryStyleDropEdit";
			this.Section1EntryStyleDropEdit.PreBoundMaxLength = 2;
			this.Section1EntryStyleDropEdit.ShowDescriptionBox = false;
			this.Section1EntryStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.Section1EntryStyleDropEdit.TabIndex = 2;
			// 
			// Section14RepresentationTypeLabel
			// 
			this.Section14RepresentationTypeLabel.AutoSize = true;
			this.Section14RepresentationTypeLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section14RepresentationTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 7, true);
			this.Section14RepresentationTypeLabel.Name = "Section14RepresentationTypeLabel";
			this.Section14RepresentationTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 10, true);
			this.Section14RepresentationTypeLabel.TabIndex = 39;
			this.Section14RepresentationTypeLabel.Text = "Type";
			// 
			// Section30TextBox
			// 
			this.Section30TextBox.BindTo = "D1_LocationOfGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.EU.Business.SADH.SADHFormData)(null)).D1_LocationOfGoodsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.SADH.SADHFormData)(null)).D1_LocationOfGoods)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section30TextBox, false);
			this.Section30TextBox.DecimalPlaces = -1;
			this.Section30TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section30TextBox.Name = "Section30TextBox";
			this.Section30TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Section30TextBox.TabIndex = 2;
			// 
			// SADHEntryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 760, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.EU.Business";
			this.DataSourceTypeName = "Enterprise.Customs.EU.Business.SADH.SADHFormData";
			this.Name = "SADHEntryForm";
			this.Text = "SADHEntryForm";
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SectionAPanel.ResumeLayout(false);
			this.SectionAPanel.PerformLayout();
			this.Section1Panel.ResumeLayout(false);
			this.Section1Panel.PerformLayout();
			this.Section2Panel.ResumeLayout(false);
			this.Section2Panel.PerformLayout();
			this.Section3Panel.ResumeLayout(false);
			this.Section3Panel.PerformLayout();
			this.Section4Panel.ResumeLayout(false);
			this.Section4Panel.PerformLayout();
			this.Section8Panel.ResumeLayout(false);
			this.Section8Panel.PerformLayout();
			this.Section7Panel.ResumeLayout(false);
			this.Section7Panel.PerformLayout();
			this.Section6Panel.ResumeLayout(false);
			this.Section6Panel.PerformLayout();
			this.Section5Panel.ResumeLayout(false);
			this.Section5Panel.PerformLayout();
			this.Section9Panel.ResumeLayout(false);
			this.Section9Panel.PerformLayout();
			this.Section10Panel.ResumeLayout(false);
			this.Section10Panel.PerformLayout();
			this.Section12Panel.ResumeLayout(false);
			this.Section12Panel.PerformLayout();
			this.Section11Panel.ResumeLayout(false);
			this.Section11Panel.PerformLayout();
			this.Section13Panel.ResumeLayout(false);
			this.Section13Panel.PerformLayout();
			this.Section16Panel.ResumeLayout(false);
			this.Section16Panel.PerformLayout();
			this.Section15Panel.ResumeLayout(false);
			this.Section15Panel.PerformLayout();
			this.Section17Panel.ResumeLayout(false);
			this.Section17Panel.PerformLayout();
			this.Section17bPanel.ResumeLayout(false);
			this.Section17bPanel.PerformLayout();
			this.Section15bPanel.ResumeLayout(false);
			this.Section15bPanel.PerformLayout();
			this.Section14Panel.ResumeLayout(false);
			this.Section14Panel.PerformLayout();
			this.Section19Panel.ResumeLayout(false);
			this.Section19Panel.PerformLayout();
			this.Section20Panel.ResumeLayout(false);
			this.Section20Panel.PerformLayout();
			this.Section22Panel.ResumeLayout(false);
			this.Section22Panel.PerformLayout();
			this.Section21Panel.ResumeLayout(false);
			this.Section21Panel.PerformLayout();
			this.Section24Panel.ResumeLayout(false);
			this.Section24Panel.PerformLayout();
			this.Section23Panel.ResumeLayout(false);
			this.Section23Panel.PerformLayout();
			this.Section27Panel.ResumeLayout(false);
			this.Section27Panel.PerformLayout();
			this.Section26Panel.ResumeLayout(false);
			this.Section26Panel.PerformLayout();
			this.Section25Panel.ResumeLayout(false);
			this.Section25Panel.PerformLayout();
			this.Section28Panel.ResumeLayout(false);
			this.Section28Panel.PerformLayout();
			this.Section30Panel.ResumeLayout(false);
			this.Section30Panel.PerformLayout();
			this.Section31bPanel.ResumeLayout(false);
			this.Section31bPanel.PerformLayout();
			this.Section32Panel.ResumeLayout(false);
			this.Section32Panel.PerformLayout();
			this.Section31Panel.ResumeLayout(false);
			this.Section31Panel.PerformLayout();
			this.Section34Panel.ResumeLayout(false);
			this.Section34Panel.PerformLayout();
			this.Section35Panel.ResumeLayout(false);
			this.Section35Panel.PerformLayout();
			this.Section36Panel.ResumeLayout(false);
			this.Section36Panel.PerformLayout();
			this.Section39Panel.ResumeLayout(false);
			this.Section39Panel.PerformLayout();
			this.Section38Panel.ResumeLayout(false);
			this.Section38Panel.PerformLayout();
			this.Section37Panel.ResumeLayout(false);
			this.Section37Panel.PerformLayout();
			this.Section40Panel.ResumeLayout(false);
			this.Section40Panel.PerformLayout();
			this.Setion42Panel.ResumeLayout(false);
			this.Setion42Panel.PerformLayout();
			this.Section41Panel.ResumeLayout(false);
			this.Section41Panel.PerformLayout();
			this.Section43Panel.ResumeLayout(false);
			this.Section43Panel.PerformLayout();
			this.Section44Panel.ResumeLayout(false);
			this.Section44Panel.PerformLayout();
			this.SectionA1Panel.ResumeLayout(false);
			this.SectionA1Panel.PerformLayout();
			this.Section45Panel.ResumeLayout(false);
			this.Section45Panel.PerformLayout();
			this.Section47Panel.ResumeLayout(false);
			this.Section47Panel.PerformLayout();
			this.Section46Panel.ResumeLayout(false);
			this.Section46Panel.PerformLayout();
			this.Section49Panel.ResumeLayout(false);
			this.Section49Panel.PerformLayout();
			this.Section48Panel.ResumeLayout(false);
			this.Section48Panel.PerformLayout();
			this.SectionBPanel.ResumeLayout(false);
			this.SectionBPanel.PerformLayout();
			this.Section50Panel.ResumeLayout(false);
			this.Section50Panel.PerformLayout();
			this.Section51Panel.ResumeLayout(false);
			this.Section51Panel.PerformLayout();
			this.SectionCPanel.ResumeLayout(false);
			this.SectionCPanel.PerformLayout();
			this.Section52Panel.ResumeLayout(false);
			this.Section52Panel.PerformLayout();
			this.Section52CodePanel.ResumeLayout(false);
			this.Section52CodePanel.PerformLayout();
			this.Section53Panel.ResumeLayout(false);
			this.Section53Panel.PerformLayout();
			this.Section54Panel.ResumeLayout(false);
			this.Section54Panel.PerformLayout();
			this.SectionJPanel.ResumeLayout(false);
			this.SectionJPanel.PerformLayout();
			this.Section33Panel.ResumeLayout(false);
			this.Section33Panel.PerformLayout();
			this.Section18Panel.ResumeLayout(false);
			this.Section18Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit Section1EntryStyleDropEdit;
		private CargoWise.Windows.UI.KLabel Section14RepresentationTypeLabel;
		private Enterprise.ZArchitecture.ZTextBox Section30TextBox;

	}
}
