using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARContraRow))]
	public class ARContraRowTest : ContraRowTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		protected override ContraRow TestContraRow
		{
			get { return Header as ARContraRow; }
		}

		protected override Type TypeOfOtherContraRow
		{
			get { return typeof(APContraRow); }
		}

		public override void TestBeforeContra()
		{
			base.TestBeforeContra();
			AssertEquals("should be -70", new ZDecimal(-70), TestContraRow.AH_BeforeContra);
		}

		public override void TestSetAfterContraValues()
		{
			base.TestBeforeContra();
			TestContraRow.AH_LocalExTaxAmount = 15;
			TestContraRow.SetAfterContraValues();
			AssertEquals("should be -70 - 15 = -85", new ZDecimal(-85), TestContraRow.AH_AfterContra);
		}

		public override void TestGetTopLevelTransaction()
		{
			SetupForSave();
			APContraRow testAPContraRow = Factory.NewWithValidTestData(typeof(APContraRow)) as APContraRow;
			testAPContraRow.AH_TransactionNum = Header.AH_TransactionNum;

			Factory.Save();

			IMatching topLevelTrans = TestContraRow.GetTopLevelTransaction;
			Assert("TestContraRow's top level transaction should be a Contra", topLevelTrans is Contra);
			Contra topLevelContra = topLevelTrans as Contra;
			AssertEquals("TopLevelContra should have correct APRow", testAPContraRow.PK,
				topLevelContra.APRow.PK);
			AssertEquals("TopLevelContra should have correct ARRow", TestContraRow.PK,
				topLevelContra.ARRow.PK);
			AssertEquals(LedgerTypes.AccountsReceivable, topLevelContra.ControllerLedger);
		}

		[TestedType(typeof(ARContraRow))]
		public class ARContraRowDocumentTest : ContraRowDocumentTest
		{
			protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
			{
				return Factory.New<ARContraRow>();
			}
		}

		[TestedType(typeof(ARContraRow))]
		public class ARContraRowMatchingTest : ContraRowMatchingTest
		{
			protected override ContraRow GetNewContraRow()
			{
				return Factory.New<ARContraRow>();
			}
		}
	}
}
