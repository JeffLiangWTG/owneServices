using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReportItemCollection<CusExitReportItem>))]
	sealed class CusExitReportItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitReportItemCollection<CusExitReportItem>>
	{
		public void TestCusExitReportAsMaster()
		{
			var report = Factory.New<CusExitReport>();
			var exitReportItem = report.CusExitReportItems.AddNew();
			exitReportItem.ERI_CCI_ConsignmentItem = ZGuid.NewZGuid();
			var exitReportItem2 = report.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CXP_Package = ZGuid.NewZGuid();

			var collection = new CusExitReportItemCollection<CusExitReportItem>(report, new ZQuery());
			CombineAssertions(() =>
			{
				AssertEquals("FKSchemaColumnInDependent", CusExitReportItemSchema.ERI_CER_Report, ((DependentRelationship)collection.Relationship).FKSchemaColumnInDependent);

				var completeFilter = collection.CompleteFilter;
				AssertEquals("exitReportItem", true, exitReportItem.MatchesFilter(completeFilter));
				AssertEquals("exitReportItem2", true, exitReportItem2.MatchesFilter(completeFilter));

				collection = new CusExitReportItemCollection<CusExitReportItem>(report, new ZQuery(CusExitReportItemSchema.ERI_CCI_ConsignmentItem, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				completeFilter = collection.CompleteFilter;
				AssertEquals("Filter ConsignmentItem, exitReportItem", true, exitReportItem.MatchesFilter(completeFilter));
				AssertEquals("Filter ConsignmentItem, exitReportItem2", false, exitReportItem2.MatchesFilter(completeFilter));

				collection = new CusExitReportItemCollection<CusExitReportItem>(report, new ZQuery(CusExitReportItemSchema.ERI_CXP_Package, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				completeFilter = collection.CompleteFilter;
				AssertEquals("Filter Package, exitReportItem", false, exitReportItem.MatchesFilter(completeFilter));
				AssertEquals("Filter Package, exitReportItem2", true, exitReportItem2.MatchesFilter(completeFilter));
			});
		}

		public void TestCusExitConsignmentItemAsMaster()
		{
			var consignmentItem = Factory.New<CusExitConsignmentItem>();
			consignmentItem.CCI_LineNumber = 1;
			var collection = new CusExitReportItemCollection<CusExitReportItem>(consignmentItem);
			AssertEquals(CusExitReportItemSchema.ERI_CCI_ConsignmentItem, ((DependentRelationship)collection.Relationship).FKSchemaColumnInDependent);
		}

		public void TestCusExitConsignmentPackageAsMaster()
		{
			var package = Factory.New<CusExitConsignmentPackage>();
			var collection = new CusExitReportItemCollection<CusExitReportItem>(package);
			AssertEquals(CusExitReportItemSchema.ERI_CXP_Package, ((DependentRelationship)collection.Relationship).FKSchemaColumnInDependent);
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, ((IBindingList)collection).AllowRemove);
		}

		protected override CusExitReportItemCollection<CusExitReportItem> GetCollectionToTest()
		{
			var master = Factory.New<CusExitReport>();
			return new CusExitReportItemCollection<CusExitReportItem>(master, new ZQuery());
		}
	}
}
