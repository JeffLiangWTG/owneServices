using System;
using System.ComponentModel;

using CargoWise.ComponentModel;

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	/// <summary>
	/// Grid control for Allocate Documents Form 
	/// </summary>
	public class BindableGridControl /*SuppressCodeSmell Reason = this is not a concrete class*/ : ZUserControl, IBindTo
	{
		public BindableGridControl()
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			this.InnerGrid = new Enterprise.DocumentScanning.GUI.DocumentsZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// InnerGrid
			// 
			this.InnerGrid.AllowDrop = true;
			this.InnerGrid.AllowNavigation = false;
			this.InnerGrid.CaptionVisible = false;
			this.InnerGrid.GridId = "f04d1e6b-53dd-4654-b146-a7055262ae8d";
			this.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InnerGrid.DocumentManipulationTarget = null;
			this.InnerGrid.DragDropTarget = null;
			this.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InnerGrid.LayoutKey = "documentsZGrid1";
			this.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InnerGrid.Name = "InnerGrid";
			this.InnerGrid.ShowDeliverDocumentMenuItem = true;
			this.InnerGrid.ShowRestoreMenuItem = true;
			this.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 464, true);
			this.InnerGrid.TabIndex = 0;
			// 
			// BindableGridControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InnerGrid);
			this.Name = "BindableGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}

		public event EventHandler BindingFinished
		{
			add
			{
				if (bindingFinished == null)
				{
					Grid.AfterBind += new EventHandler(OnBindingFinished);
				}
				bindingFinished += value;
			}
			remove
			{
				bindingFinished -= value;
				if (bindingFinished == null)
				{
					Grid.AfterBind -= new EventHandler(OnBindingFinished);
				}
			}
		}
		EventHandler bindingFinished;

		void OnBindingFinished(object sender, EventArgs e) => bindingFinished?.Invoke(this, e);

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DocumentsZGrid Grid
		{
			get { return InnerGrid; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string BindTo
		{
			get { return InnerGrid.BindTo; }
			set { InnerGrid.BindTo = value; }
		}

		Enterprise.DocumentScanning.GUI.DocumentsZGrid InnerGrid;
	}
}
