using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.Testing
{
	[UseSnapshotProtection]
	public abstract class BaseRollBackTestWithoutFactory : TestCase
	{
		protected void ChequeBook_Reloaded(object sender, EventArgs e)
		{
			Test_ChequeBookReloads_Counter++;
		}
		protected ZInt Test_ChequeBookReloads_Counter;

		protected override void SetUp()
		{
			base.SetUp();
			ExceptionOccured = ZBool.False;
			TestBookWithAutoAllocation = null;
		}

		protected virtual void SetUpTestObjectsAndBeginTransaction(ZDecimal startNo, ZDecimal currentNo, ZDecimal lastNo, ZBool createOrgHeader)
		{
			SetUpTestObjects(startNo, currentNo, lastNo, createOrgHeader);
		}

		protected virtual void SetUpTestObjects(ZDecimal startNo, ZDecimal currentNo, ZDecimal lastNo, ZBool createOrgHeader)
		{
			BankAccount = TestHelper.TestObjectCreator.CreateBankAccount("TestBank", "Test Bank", "AUD Bank", "TB", GlbCompany.CurrentCompany.LocalCurrency, "123", "13131313", TestHelper.TestObjectCreator.GLHeader1);
			TestBookWithAutoAllocation = TestHelper.TestObjectCreator.CreateChequeBook(startNo, currentNo, lastNo, BankAccount);
			TestHelper.TestObjectCreator.SetupAutoPrintChequeBook(BankAccount, TestBookWithAutoAllocation, Factory);

			if (createOrgHeader)
			{
				TestOrgHeader = TestHelper.TestObjectCreator.TestOrganisation;
			}

			Factory.Save();
		}

		protected ZBool ExceptionOccured;
		protected AccBankAccount BankAccount;
		protected AccChequeBook TestBookWithAutoAllocation;
		protected OrgHeader TestOrgHeader;

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		protected TransactionAllocateTestHelper TestHelper
		{
			get
			{
				if (fTestHelper == null)
				{
					fTestHelper = new TransactionAllocateTestHelper(Factory);
				}
				return fTestHelper;
			}
		}
		TransactionAllocateTestHelper fTestHelper;
	}
}
