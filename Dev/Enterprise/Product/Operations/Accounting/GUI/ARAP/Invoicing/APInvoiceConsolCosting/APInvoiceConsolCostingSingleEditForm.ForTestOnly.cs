#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class APInvoiceConsolCostingSingleEditForm
	{
		public ZArchitecture.GUI.ZDropEdit FixedPlaceOfSupplyDropEdit_ForTestOnly
		{
			get { return fixedPlaceOfSupplyDropEdit; }
			set { fixedPlaceOfSupplyDropEdit = value; }
		}

		public ZArchitecture.ZGrid ApportionedChargesGrid_ForTestOnly
		{
			get { return ApportionedChargesGrid; }
			set { ApportionedChargesGrid = value; }
		}

		public ZArchitecture.ZTextBox GovtChargeCodeTextBox_ForTestOnly
		{
			get { return govtChargeCodeTextBox; }
			set { govtChargeCodeTextBox = value; }
		}

		public ZArchitecture.GUI.ZGroupBox CostDetails_ForTestOnly
		{
			get { return CostDetails; }
			set { CostDetails = value; }
		}
	}
}

#endif
