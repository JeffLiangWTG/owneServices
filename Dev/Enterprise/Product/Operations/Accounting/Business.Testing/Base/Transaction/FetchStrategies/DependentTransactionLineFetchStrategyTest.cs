using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class DependentTransactionLineFetchStrategyTest : TestCaseWithFactory
	{
		protected TestObjectCreator TestObjectCreator;

		#region TestFetchForView

		public void TestFetchForView_BranchName()
		{
			var columnNamesToView = new[] { DependentTransactionLine.Schema.BranchName };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbBranchSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNamesToView, expectedDbHits, true);
		}

		public void TestFetchForView_DepartmentDescription()
		{
			var columnNamesToView = new[] { DependentTransactionLine.Schema.DepartmentDescription };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbDepartmentSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNamesToView, expectedDbHits, true);
		}

		public void TestFetchForViewWhenColumnsNotVisible()
		{
			var columnNamesToView = new[] { DependentTransactionLine.Schema.BranchName, DependentTransactionLine.Schema.DepartmentDescription };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbDepartmentSchema.Constants.TableName, 0);
			expectedDbHits.Add(GlbBranchSchema.Constants.TableName, 0);

			AssertFetchForViewDbHits(columnNamesToView, expectedDbHits, false);
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits, bool areColumnsVisible)
		{
			var viewFactory = new BusinessObjectFactory();
			var invoiceLineCollection = viewFactory.Load<ARInvoiceLine>(new ZQuery(AccTransactionLinesSchema.PK, CreateTestInvoiceLineCollection()));
			viewFactory.ResetDatabaseLoadCount();

			foreach (var invoiceLine in invoiceLineCollection)
			{
				invoiceLine.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			if (areColumnsVisible)
			{
				foreach (var invoiceLine in invoiceLineCollection)
				{
					object hitProperty;
					foreach (var columnName in viewColumnNames)
					{
						hitProperty = invoiceLine[columnName];
					}
				}
			}

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(expectedDbHits, viewFactory);
		}

		List<ZGuid> CreateTestInvoiceLineCollection()
		{
			var result = new List<ZGuid>();
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			job.JH_GB = Factory.LoadTop1<GlbBranch>(new ZQuery()).PK;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

			for (int i = 0; i < 10; i++)
			{
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
				line.AL_JH = job.PK;
				line.AL_GE = TestObjectCreator.CreateDepartment(string.Format("A{0}", i.ToString("D2"))).PK;
				line.AL_GB = TestObjectCreator.CreateBranch(string.Format("B{0}", i.ToString("D2")), GlbCompany.CurrentCompany).PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.LocalCurrency);

				result.Add(line.PK);
			}

			Factory.Save();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		#endregion
	}
}
