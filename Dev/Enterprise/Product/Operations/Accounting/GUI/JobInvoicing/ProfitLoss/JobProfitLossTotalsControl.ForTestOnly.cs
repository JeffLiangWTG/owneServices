#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitLossTotalsControl
	{
		public ZArchitecture.ZCalcEdit TotalRevenue_ForTestOnly
		{
			get { return TotalRevenue; }
			set { TotalRevenue = value; }
		}

		public ZArchitecture.ZCalcEdit TotalRevenueRecognized_ForTestOnly
		{
			get { return TotalRevenueRecognized; }
			set { TotalRevenueRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalRevenueNotRecognized_ForTestOnly
		{
			get { return TotalRevenueNotRecognized; }
			set { TotalRevenueNotRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalWIP_ForTestOnly
		{
			get { return TotalWIP; }
			set { TotalWIP = value; }
		}

		public ZArchitecture.ZCalcEdit TotalWIPRecognized_ForTestOnly
		{
			get { return TotalWIPRecognized; }
			set { TotalWIPRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalWIPNotRecognized_ForTestOnly
		{
			get { return TotalWIPNotRecognized; }
			set { TotalWIPNotRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalAccrual_ForTestOnly
		{
			get { return TotalAccrual; }
			set { TotalAccrual = value; }
		}

		public ZArchitecture.ZCalcEdit TotalAccrualRecognized_ForTestOnly
		{
			get { return TotalAccrualRecognized; }
			set { TotalAccrualRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalAccrualNotRecognized_ForTestOnly
		{
			get { return TotalAccrualNotRecognized; }
			set { TotalAccrualNotRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalCost_ForTestOnly
		{
			get { return TotalCost; }
			set { TotalCost = value; }
		}

		public ZArchitecture.ZCalcEdit TotalCostRecognized_ForTestOnly
		{
			get { return TotalCostRecognized; }
			set { TotalCostRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalCostNotRecognized_ForTestOnly
		{
			get { return TotalCostNotRecognized; }
			set { TotalCostNotRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalLineAmount_ForTestOnly
		{
			get { return TotalLineAmount; }
			set { TotalLineAmount = value; }
		}

		public ZArchitecture.ZCalcEdit TotalLineAmountRecognized_ForTestOnly
		{
			get { return TotalLineAmountRecognized; }
			set { TotalLineAmountRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalLineAmountNotRecognized_ForTestOnly
		{
			get { return TotalLineAmountNotRecognized; }
			set { TotalLineAmountNotRecognized = value; }
		}

		public ZArchitecture.ZCalcEdit TotalTaxExpenseRevenue_ForTestOnly
		{
			get { return TotalTaxExpenseRevenue; }
			set { TotalTaxExpenseRevenue = value; }
		}

		public ZArchitecture.ZCalcEdit TotalTaxExpenseCost_ForTestOnly
		{
			get { return TotalTaxExpenseCost; }
			set { TotalTaxExpenseCost = value; }
		}

		public ZPanel TaxExpensePanel_ForTestOnly
		{
			get { return TaxExpensePanel; }
			set { TaxExpensePanel = value; }
		}
	}
}

#endif
