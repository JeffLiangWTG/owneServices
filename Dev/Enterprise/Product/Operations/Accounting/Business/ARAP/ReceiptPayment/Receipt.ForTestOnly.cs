#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class Receipt
	{
		public AccTransactionHeaderValidation GetNewMatchingValidation_ForTestOnly()
		{
			return GetNewMatchingValidation();
		}

		public void SaveChequeDetailsAsDefault_ForTestOnly()
		{
			SaveChequeDetailsAsDefault();
		}

		public ZString Default_AH_ChequeDrawer_ForTestOnly
		{
			get { return Default_AH_ChequeDrawer; }
			set { Default_AH_ChequeDrawer = value; }
		}

		public ZString Default_AH_DrawerBank_ForTestOnly
		{
			get { return Default_AH_DrawerBank; }
			set { Default_AH_DrawerBank = value; }
		}

		public ZString Default_AH_DrawerBranch_ForTestOnly
		{
			get { return Default_AH_DrawerBranch; }
			set { Default_AH_DrawerBranch = value; }
		}
	}
}

#endif
