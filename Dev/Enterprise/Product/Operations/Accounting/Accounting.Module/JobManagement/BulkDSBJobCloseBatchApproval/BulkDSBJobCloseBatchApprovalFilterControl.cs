using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class BulkDSBJobCloseBatchApprovalFilterControl : ZFilterStripControl
	{
		public BulkDSBJobCloseBatchApprovalFilterControl()
		{
			InitializeComponent();
		}

		public BulkDSBJobCloseBatchApprovalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.IContainer components;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}
	}
}

