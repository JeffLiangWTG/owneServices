using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ReceivablesTaxParentFromPostingChargeTest : TestCaseWithFactory
	{
		public void TestConstructorAndProperties()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ReceivablesTaxParentFromPostingCharge(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ReceivablesTaxParentFromPostingCharge(new IReceivablesPostingChargeCollection(), null));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => _ = new ReceivablesTaxParentFromPostingCharge(new IReceivablesPostingChargeCollection(), new ReadOnlyBusinessObjectFactory()));

			var charges = new IReceivablesPostingChargeCollection();
			var key = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);
			charges.Key = key;

			var charge = Factory.New<Charge>();
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charges.Add(charge);

			ITaxRecordParentBase taxParentFromPostingCharge = null;
			AssertNoExceptionThrown(() => taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, new ReadOnlyBusinessObjectFactory()));

			AssertNoExceptionThrown(() => _ = ((ReceivablesTaxParentFromPostingCharge)taxParentFromPostingCharge).Key);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.PK);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Factory);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Org);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Ledger);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Currency);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.PostDate);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Company);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Branch);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.Department);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.IsPosted);
			AssertNoExceptionThrown(() => _ = taxParentFromPostingCharge.GetLines());
		}

		[TestDate(2021, 02, 28)]
		public void TestProperties()
		{
			var charges = new IReceivablesPostingChargeCollection();
			var key = new PostingChargeKey(ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, 0);
			charges.Key = key;

			var charge = Factory.New<Charge>();
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charges.Add(charge);

			var factory = new ReadOnlyBusinessObjectFactory();

			var taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, factory);
			var taxParent = (ITaxRecordParentBase)taxParentFromPostingCharge;
			AssertEquals("KEY", key, taxParentFromPostingCharge.Key);

			var pk = taxParent.PK;
			AssertEquals("PK", pk, taxParent.PK);

			AssertEquals("Factory", factory, taxParent.Factory);
			AssertEquals("Org", TestObjectCreator.Debtor.PK, taxParent.Org.PK);
			AssertEquals("Ledger", LedgerTypes.AccountsReceivable, taxParent.Ledger);
			AssertEquals("Currency", TestObjectCreator.AUD.RX_Code, taxParent.Currency);
			AssertEquals("PostDate", ZDateTime.Now.Date, taxParent.PostDate.Date);
			AssertEquals("Company PK",GlbCompany.CurrentCompany.PK, taxParent.Company.PK);
			AssertEquals("Branch PK", GlbBranch.CurrentBranch.PK, taxParent.Branch.PK);
			AssertEquals("Department PK", GlbDepartment.CurrentDepartment.PK, taxParent.Department.PK);
			AssertEquals("IsPosted", false, taxParent.IsPosted);
			var lines = taxParent.GetLines();
			AssertEquals("Line count", 1, lines.Count);
			AssertEquals("Line pk", lines[0].PK, taxParent.GetLines()[0].PK);
		}

		public void TestTaxParentBranchAndCompany()
		{
			var branch1 = TestObjectCreator.CreateBranch("XXX", GlbCompany.CurrentCompany);

			var charges = new IReceivablesPostingChargeCollection();
			var charge = Factory.New<Charge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GB_SellTaxBranch = branch1.PK;

			charges.Add(charge);

			var factory = new ReadOnlyBusinessObjectFactory();

			var taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, factory);
			var taxParent = (ITaxRecordParentBase)taxParentFromPostingCharge;

			AssertEquals("Correct company is returned", GlbCompany.CurrentCompany.PK, taxParent.Company.PK);
			AssertEquals("Correct branch is returned", branch1.PK, taxParent.Branch.PK);

			charges = new IReceivablesPostingChargeCollection();
			charge = Factory.New<Charge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;

			charges.Add(charge);

			taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, factory);
			taxParent = taxParentFromPostingCharge;

			AssertEquals("Correct branch is returned", GlbBranch.CurrentBranch.PK, taxParent.Branch.PK);

			charges = new IReceivablesPostingChargeCollection();
			charge = Factory.New<Charge>();
			charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;

			charges.Add(charge);

			taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, factory);
			taxParent = taxParentFromPostingCharge;

			AssertEquals("Correct company is returned", TestObjectCreator.NonCurrentBranch.Company.PK, taxParent.Company.PK);
			AssertEquals("Correct branch is returned", TestObjectCreator.NonCurrentBranch.PK, taxParent.Branch.PK);
		}

		public void TestTaxParentDepartment()
		{
			var charges = new IReceivablesPostingChargeCollection();
			var charge = Factory.New<Charge>();
			charge.JR_GE = ZGuid.Empty;

			charges.Add(charge);

			var factory = new ReadOnlyBusinessObjectFactory();

			var taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, factory);
			var taxParent = (ITaxRecordParentBase)taxParentFromPostingCharge;

			AssertNull("If charge does not have department set then null will be returned", taxParent.Department);

			charges = new IReceivablesPostingChargeCollection();
			charge = Factory.New<Charge>();
			charge.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			charges.Add(charge);

			taxParentFromPostingCharge = new ReceivablesTaxParentFromPostingCharge(charges, factory);
			taxParent = taxParentFromPostingCharge;

			AssertEquals("Correct company is returned", TestObjectCreator.NonCurrentDepartment.PK, taxParent.Department.PK);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
