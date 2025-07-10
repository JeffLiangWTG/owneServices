using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APContraRow))]
	public class APContraRowTest : ContraRowTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		protected override ContraRow TestContraRow
		{
			get { return Header as APContraRow; }
		}

		protected override Type TypeOfOtherContraRow
		{
			get { return typeof(ARContraRow); }
		}

		public override void TestBeforeContra()
		{
			base.TestBeforeContra();
			AssertEquals("should be -30", new ZDecimal(-30), TestContraRow.AH_BeforeContra);
		}

		public override void TestSetAfterContraValues()
		{
			base.TestBeforeContra();
			TestContraRow.AH_LocalExTaxAmount = 15;
			TestContraRow.SetAfterContraValues();
			AssertEquals("should be -30 - 15 = -45", new ZDecimal(-45), TestContraRow.AH_AfterContra);
		}

		public override void TestGetTopLevelTransaction()
		{
			SetupForSave();
			ARContraRow testARContraRow = Factory.NewWithValidTestData(typeof(ARContraRow)) as ARContraRow;
			testARContraRow.AH_TransactionNum = Header.AH_TransactionNum;

			Factory.Save();

			IMatching topLevelTrans = TestContraRow.GetTopLevelTransaction;
			Assert("TestContraRow's top level transaction should be a Contra", topLevelTrans is Contra);
			Contra topLevelContra = topLevelTrans as Contra;
			AssertEquals("TopLevelContra should have correct APRow", TestContraRow.PK,
				topLevelContra.APRow.PK);
			AssertEquals("TopLevelContra should have correct ARRow", testARContraRow.PK,
				topLevelContra.ARRow.PK);
			AssertEquals(LedgerTypes.AccountsPayable, topLevelContra.ControllerLedger);
		}
	}
}
