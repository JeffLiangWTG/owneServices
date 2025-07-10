namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMultiControlTestForm : ZForm
	{
		public ZMultiControlTestForm(GridTestParentBusinessObject parentFields, bool bindToDecimal = false)
			: base(parentFields)
		{
			this.bindToDecimal = bindToDecimal;
			InitializeComponent();
		}

		readonly bool bindToDecimal;

		public ZGrid zGrid1;

		public GridTestParentBusinessObject ParentFields
		{
			get { return (GridTestParentBusinessObject)BusinessEntity; }
		}

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

#pragma warning disable IDE0001 // Simplify Names
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			var zMultiControlColumnStyleInfo1 = new GUI.ZMultiControlColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.zGrid1 = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = new System.Drawing.Point(0, 401);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = new System.Drawing.Size(520, 24);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 252;
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = 253;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "Fields";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Field Name";
			zTextBoxColumnStyleInfo1.ColumnName = "FieldName";
			zMultiControlColumnStyleInfo1.Caption = "Field Value";
			zMultiControlColumnStyleInfo1.ColumnName = "FieldValue";
			zMultiControlColumnStyleInfo1.BindToList = "TestList";
			if (this.bindToDecimal)
			{
				zMultiControlColumnStyleInfo1.BindToDecimalPlaces = "Decimal";
			}
			zMultiControlColumnStyleInfo1.ModuleID = Modules.Testing.DummyModuleIDs.Dummy;
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "FieldType";
			zTextBoxColumnStyleInfo2.ColumnName = "FieldType";
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = new System.Drawing.Point(32, 32);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = new System.Drawing.Size(448, 352);
			this.zGrid1.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// Form1
			// 

			this.ClientSize = new System.Drawing.Size(520, 425);
			this.Controls.Add(this.zGrid1);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.GUI.Internal.Testing.GridTestParentBusinessObject";
			this.Name = "Form1";
			this.Text = "Form1";
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
#pragma warning restore IDE0001 // Simplify Names
	}
}
