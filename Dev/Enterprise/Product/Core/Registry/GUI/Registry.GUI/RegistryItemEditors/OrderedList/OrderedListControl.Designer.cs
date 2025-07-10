using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrderedListControl : ZUserControl
	{
		CargoWise.Windows.UI.KListBox ItemListBox;
		Enterprise.ZArchitecture.GUI.ZButton MoveUpButton;
		Enterprise.ZArchitecture.GUI.ZButton MoveDownButton;
		CargoWise.Windows.UI.KTextBox ItemValue;
		CargoWise.Windows.UI.KButton AddItemButton;
		CargoWise.Windows.UI.KButton DeleteItemButton;
		CargoWise.Windows.UI.KLabel MessageText;

		void InitializeComponent()
		{
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ItemValue = new CargoWise.Windows.UI.KTextBox();
			this.AddItemButton = new CargoWise.Windows.UI.KButton();
			this.DeleteItemButton = new CargoWise.Windows.UI.KButton();
			this.MessageText = new CargoWise.Windows.UI.KLabel();
			this.ItemListBox = new CargoWise.Windows.UI.KListBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 3, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveUpButton.TabIndex = 1;
			this.MoveUpButton.Text = "Move Up";
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 35, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveDownButton.TabIndex = 2;
			this.MoveDownButton.Text = "Move Down";
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// ItemValue
			// 
			this.ItemValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ItemValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 327, true);
			this.ItemValue.Name = "ItemValue";
			this.ItemValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 20, true);
			this.ItemValue.TabIndex = 3;
			// 
			// AddItemButton
			// 
			this.AddItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 327, true);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AddItemButton.TabIndex = 4;
			this.AddItemButton.Text = "Add";
			this.AddItemButton.UseVisualStyleBackColor = true;
			this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
			// 
			// DeleteItemButton
			// 
			this.DeleteItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 295, true);
			this.DeleteItemButton.Name = "DeleteItemButton";
			this.DeleteItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeleteItemButton.TabIndex = 5;
			this.DeleteItemButton.Text = "Delete";
			this.DeleteItemButton.UseVisualStyleBackColor = true;
			this.DeleteItemButton.Click += new System.EventHandler(this.DeleteItemButton_Click);
			// 
			// MessageText
			// 
			this.MessageText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageText.AutoSize = true;
			this.MessageText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.MessageText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 367, true);
			this.MessageText.Name = "MessageText";
			this.MessageText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MessageText.TabIndex = 6;
			// 
			// ItemListBox
			// 
			this.ItemListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ItemListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemListBox.Name = "ItemListBox";
			this.ItemListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 316, true);
			this.ItemListBox.TabIndex = 0;
			this.ItemListBox.SelectedIndexChanged += new System.EventHandler(this.ItemListBox_SelectedIndexChanged);
			// 
			// OrderedListControl
			// 
			this.AutoSize = true;
			this.Controls.Add(this.MessageText);
			this.Controls.Add(this.DeleteItemButton);
			this.Controls.Add(this.AddItemButton);
			this.Controls.Add(this.ItemValue);
			this.Controls.Add(this.MoveDownButton);
			this.Controls.Add(this.MoveUpButton);
			this.Controls.Add(this.ItemListBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 500, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.Name = "OrderedListControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing )
		{
			if (disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing );
		}
	}
}
