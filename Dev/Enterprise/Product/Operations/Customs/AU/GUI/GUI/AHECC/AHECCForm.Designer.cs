using System.Windows.Forms;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AHECCForm
	{
		void InitializeComponent()
		{
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.zTextBoxUnitOfQuantity = new ZArchitecture.ZTextBox();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.zTextBoxFullDescription = new ZArchitecture.ZTextBox();
			this.SuspendLayout();
			// 
			// TreeView
			// 
			this.TreeView.Name = "TreeView";
			this.TreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 296, true);
			this.TreeView.AfterSelect += new TreeViewEventHandler(this.TreeView_AfterSelect);
			// 
			// label1
			// 
			this.label1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 379, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.label1.TabIndex = 3;
			this.label1.Text = "Unit of Quantity";
			// 
			// zTextBoxUnitOfQuantity
			// 
			this.zTextBoxUnitOfQuantity.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zTextBoxUnitOfQuantity.BackColor = System.Drawing.SystemColors.Control;
			this.zTextBoxUnitOfQuantity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 376, true);
			this.zTextBoxUnitOfQuantity.Name = "zTextBoxUnitOfQuantity";
			this.zTextBoxUnitOfQuantity.TabIndex = 4;
			this.zTextBoxUnitOfQuantity.TabStop = false;
			this.zTextBoxUnitOfQuantity.Text = "";
			// 
			// label2
			// 
			this.label2.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 312, true);
			this.label2.Name = "label2";
			this.label2.TabIndex = 4;
			this.label2.Text = "Full Description";
			// 
			// zTextBoxFullDescription
			// 
			this.zTextBoxFullDescription.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.zTextBoxFullDescription.BackColor = System.Drawing.SystemColors.Control;
			this.zTextBoxFullDescription.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBoxFullDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 312, true);
			this.zTextBoxFullDescription.Multiline = true;
			this.zTextBoxFullDescription.Name = "zTextBoxFullDescription";
			this.zTextBoxFullDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 56, true);
			this.zTextBoxFullDescription.TabIndex = 3;
			this.zTextBoxFullDescription.TabStop = false;
			this.zTextBoxFullDescription.Text = "";
			// 
			// AHECCForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 414, true);
			this.Controls.Add(this.zTextBoxFullDescription);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.zTextBoxUnitOfQuantity);
			this.Controls.Add(this.label1);
			this.Name = "AHECCForm";
			this.Text = "Select an Export Tariff";
			this.VisibleChanged += new System.EventHandler(this.AHECCForm_VisibleChanged);
			this.Controls.SetChildIndex(this.TreeView, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.zTextBoxUnitOfQuantity, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.zTextBoxFullDescription, 0);
			this.ResumeLayout(false);
		}

		ZArchitecture.ZTextBox zTextBoxUnitOfQuantity;
		CargoWise.Windows.UI.KLabel label2;
		ZArchitecture.ZTextBox zTextBoxFullDescription;
		CargoWise.Windows.UI.KLabel label1;
	}
}
