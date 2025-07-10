#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class BulkCostApportionmentFormHollywood
	{
		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}

		public IBusiness BusinessEntityForValidation_ForTestOnly
		{
			get { return BusinessEntityForValidation; }
		}

		public ZArchitecture.ZGrid InvoicesGrid_ForTestOnly
		{
			get { return InvoicesGrid; }
			set { InvoicesGrid = value; }
		}

		public ZArchitecture.GUI.ZTabControl TabControl_ForTestOnly
		{
			get { return TabControl; }
			set { TabControl = value; }
		}

		public AccountingOnFormFilterControl FilterControl_ForTestOnly
		{
			get { return FilterControl; }
			set { FilterControl = value; }
		}

		public ZArchitecture.ZGrid AccrualsGrid_ForTestOnly
		{
			get { return AccrualsGrid; }
			set { AccrualsGrid = value; }
		}

		public ZArchitecture.GUI.ZButton PostButton_ForTestOnly
		{
			get { return PostButton; }
			set { PostButton = value; }
		}
	}
}

#endif
