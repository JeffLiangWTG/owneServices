using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferChargeLine))]
	class BankTransferChargeLineEmptyValidationTest : DirectTransactionLineBaseValidationTest
	{
		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(BankTransferCharge);
		}

		public override void TestCheckAL_AT()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			TestBizO.AL_AT = ZGuid.Empty;
			AssertEquals(false, TestBizO.AL_ATInfo.HasErrors());
		}

		public override void TestCheckAL_OSExTaxAmount()
		{
			TestBizO.AL_OSExTaxAmount = -1m;
			AssertEquals(false, TestBizO.AL_OSExTaxAmountInfo.HasErrors());
		}

		public override void TestCheckAL_AG()
		{
			AssertEquals(false, TestBizO.AL_AG.IsEmpty);
		}

		public override void TestCheckAL_Desc()
		{
			TestBizO.AL_Desc = string.Empty;
			AssertEquals(false, TestBizO.AL_DescInfo.HasErrors());
		}

		[TestDate(2020, 5, 6)]
		public void CheckAL_GovtChargeCode()
		{
			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 0m, 1m);
			foreach (var enableGovernmentChargeCode in new bool[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableGovernmentChargeCode))
				{
					transfer.FinanceChargeTaxID = TestObjectCreator.GST1.PK;
					transfer.EnableFinanceCharge = true;

					transfer.FinanceChargeGovtChargeCode = string.Empty;
					if (enableGovernmentChargeCode)
					{
						AssertHasErrors(transfer.FinanceChargeGovtChargeCodeInfo);
					}
					else
					{
						AssertNoErrors(transfer.FinanceChargeGovtChargeCodeInfo);
					}

					transfer.FinanceChargeGovtChargeCode = "CCC";
					AssertNoErrors(transfer.FinanceChargeGovtChargeCodeInfo);

					transfer.EnableFinanceCharge = false;

					transfer.FinanceChargeGovtChargeCode = "";
					AssertNoErrors(transfer.FinanceChargeGovtChargeCodeInfo);

					transfer.FinanceChargeGovtChargeCode = "DDD";
					AssertNoErrors(transfer.FinanceChargeGovtChargeCodeInfo);
				}
			}
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
