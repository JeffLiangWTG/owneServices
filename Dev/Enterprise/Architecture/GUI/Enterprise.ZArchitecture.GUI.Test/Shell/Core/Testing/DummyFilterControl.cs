using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[SuppressFormsLocalizedTest]
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class DummyFilterControl : ZFilterStripControl, IDummyFilterControl
	{
		public DummyFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Exposed Stuff for Testing

		public DummyFilterBusinessObject DummyFilterBizObj
		{
			get { return (DummyFilterBusinessObject)FilterBusinessObject; }
		}

		public void ResetFilter()
		{
			FilterBusinessObject.ResetToDefaultValues();
		}

		public void AddOrUpdateExistingFindDropListItemExposed(StmModuleFilter filter)
		{
			this.AddOrUpdateExistingFindDropListItem(filter);
		}

		public bool CanAcceptCommonDataExposed(IDataObject dataObject)
		{
			return CanAcceptCommonData(dataObject);
		}

		#endregion

#pragma warning disable IDE0001 // Simplify Names
		#region Component Designer generated code

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo2.ColumnName = "Z0_Description";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Z0_Number";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.TabIndex = 5;
			// 
			// DummyFilterControl
			// 
			this.Name = "DummyFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
#pragma warning restore IDE0001 // Simplify Names

		#region IDummyFilterControl Members

		public IBusinessObjectCollection GridList
		{
			get { return (BusinessObjectCollection)FilteredGrid.List; }
		}

		public void SelectRow(int rowIndex)
		{
			FilteredGrid.Select(rowIndex);
		}

		public void UnselectRow(int rowIndex)
		{
			FilteredGrid.UnSelect(rowIndex);
		}

		public ToolStripSplitButton FindButton
		{
			get { return base.ToolStripFindDropButton; }
		}

		public ZLabel AutoRefreshWarningLabelExposed
		{
			get { return AutoRefreshWarningLabel; }
		}

		#endregion
	}
}
