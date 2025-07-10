using System;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.Module
{
	public partial class UPEPrintBatchFilterControl : ZFilterStripControl
	{
		public UPEPrintBatchFilterControl(UPEPrintBatchModule module, UPEPrintBatchCollection gridCollection, UPEPrintBatchFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			this.Module = module;
			FilteredGrid.DoubleClick += new EventHandler(OnFilteredGrid_DoubleClick);
		}

		readonly UPEPrintBatchModule Module;

		#region Print Batch Type

		void OnFilteredGrid_DoubleClick(object sender, EventArgs e)
		{
			if (FilteredGrid.SelectedElements.Length == 1)
			{
				UPEPrintBatch selectedBatch = (UPEPrintBatch)FilteredGrid.SelectedElements[0];
				Module.PrintBatch(selectedBatch);
			}
		}

		#endregion
	}
}
