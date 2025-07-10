using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(GeneralLedgerBalanceLineCollectionView))]
	public class GeneralLedgerBalanceLineCollectionViewTest : BusinessObjectCollectionViewTestCase<GeneralLedgerBalanceLineCollectionView>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var collectionView = GetCollectionToTest();
			AssertEquals(0, collectionView.Count);

			var element = new GeneralLedgerBalanceLine(Factory, GeneralLedgerBalanceLineTest.GetDataRow(Factory));
			Assert(element.GeneralLedgerAmountDR.IsEmpty);
			Assert(element.GeneralLedgerAmountCR.IsEmpty);
			collectionView.Add(element);
			AssertEquals(0, collectionView.Count);

			var row1 = GeneralLedgerBalanceLineTest.GetDataRow(Factory);
			row1[GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountDR] = 100m;
			row1.AcceptChanges();
			var element1 = new GeneralLedgerBalanceLine(Factory, row1);
			AssertEquals(100m, element1.GeneralLedgerAmountDR);
			Assert(element1.GeneralLedgerAmountCR.IsEmpty);
			collectionView.Add(element1);
			AssertEquals(1, collectionView.Count);

			var row2 = GeneralLedgerBalanceLineTest.GetDataRow(Factory);
			row2[GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountCR] = 100m;
			row2.AcceptChanges();
			var element2 = new GeneralLedgerBalanceLine(Factory, row2);
			Assert(element2.GeneralLedgerAmountDR.IsEmpty);
			AssertEquals(100m, element2.GeneralLedgerAmountCR);
			collectionView.Add(element2);
			AssertEquals(2, collectionView.Count);
		}

		protected override GeneralLedgerBalanceLineCollectionView GetCollectionToTest()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			return new GeneralLedgerBalanceLineCollectionView(new AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine>(report));
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(GeneralLedgerBalanceLineCollectionView);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GeneralLedgerBalanceLine(Factory, GetDataRow(Factory));
		}

		internal static DataRow GetDataRow(BusinessObjectFactory factory)
		{
			var result = GeneralLedgerBalanceLineTest.GetDataRow(factory);
			result[GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountDR] = 100m;
			result.AcceptChanges();
			return result;
		}
	}
}
