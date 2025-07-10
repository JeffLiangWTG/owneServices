using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	[TestedType(typeof(DirectReceipt))]
	class DirectReceiptValidationTest : DirectTransactionHeaderBaseValidationTest
	{
		protected override bool IsChequeDrawerAlwaysEditable => false;

		public void TestAH_ReceiptType()
		{
			foreach (ICodeDescription receiptMethod in TestBizO.ReceiptMethods)
			{
				TestBizO.AH_ReceiptType = receiptMethod.Code;
				AssertEquals(false, TestBizO.AH_ReceiptTypeInfo.HasErrors());
			}

			TestBizO.AH_ReceiptType = "ABC";
			AssertEquals(true, TestBizO.AH_ReceiptTypeInfo.HasErrors());
		}

		public void TestCheckAH_DrawerBank()
		{
			AssertEquals("Percondition", false, TestBizO.AH_DrawerBankInfo.ReadOnly);

			TestBizO.AH_DrawerBank = string.Empty;
			AssertEquals(true, TestBizO.AH_DrawerBankInfo.HasError("Please enter a Drawer Bank."));

			TestBizO.AH_DrawerBank = "abc";
			AssertEquals(false, TestBizO.AH_DrawerBankInfo.HasErrors());

			TestBizO.AH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals("Percondition", true, TestBizO.AH_DrawerBankInfo.ReadOnly);

			TestBizO.AH_DrawerBank = string.Empty;
			AssertEquals(false, TestBizO.AH_DrawerBankInfo.HasErrors());
		}

		public void TestCheckAH_DrawerBranch()
		{
			AssertEquals("Percondition", false, TestBizO.AH_DrawerBranchInfo.ReadOnly);

			TestBizO.AH_DrawerBranch = string.Empty;
			AssertEquals(true, TestBizO.AH_DrawerBranchInfo.HasError("Please enter a Drawer Branch."));

			TestBizO.AH_DrawerBranch = "abc";
			AssertEquals(false, TestBizO.AH_DrawerBranchInfo.HasErrors());

			TestBizO.AH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals("Percondition", true, TestBizO.AH_DrawerBranchInfo.ReadOnly);

			TestBizO.AH_DrawerBranch = string.Empty;
			AssertEquals(false, TestBizO.AH_DrawerBranchInfo.HasErrors());
		}
	}
}
