#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectCashBookBaseForm
	{
		public ZArchitecture.ZGrid CashBookLineBoundGrid_ForTestOnly
		{
			get { return CashBookLineBoundGrid; }
			set { CashBookLineBoundGrid = value; }
		}

		public ZGuidFindBox ChequeBookFindBox_ForTestOnly
		{
			get { return ChequeBookFindBox; }
			set { ChequeBookFindBox = value; }
		}

		public Business.CashBook.DirectTransactionHeaderBase Direct_ForTestOnly => Direct;

		public ZCalcFindBox AH_LocalExTaxAmountCalcFindBox_ForTestOnly
		{
			get { return AH_LocalExTaxAmountCalcFindBox; }
			set { AH_LocalExTaxAmountCalcFindBox = value; }
		}

		public ZGuidFindBox AH_GB_TaxBranchGuidFindBox_ForTestOnly => AH_GB_TaxBranchGuidFindBox;

		public ZTabPage CashBookTransactionTabPage1_ForTestOnly
		{
			get { return CashBookTransactionTabPage1; }
			set { CashBookTransactionTabPage1 = value; }
		}

		public ZDropEdit ZDropEditPlaceOfSupply_ForTestOnly
		{
			get { return zDropEditPlaceOfSupply; }
			set { zDropEditPlaceOfSupply = value; }
		}
	}
}

#endif
