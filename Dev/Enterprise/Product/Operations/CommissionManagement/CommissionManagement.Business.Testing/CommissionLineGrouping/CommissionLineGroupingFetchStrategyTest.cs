using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionLineSourceGroupingFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_SourceNumber()
		{
			var rate = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>();
			for (var i = 0; i < 10; i++)
			{
				var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
				if (i < 5)
				{
					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					commissionHeader.CH0_GroupingSourceID = invoice.PK;
					commissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
				}
				else
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					var job = new Job.Loader(shipment).TryCreate();
					commissionHeader.CH0_GroupingSourceID = job.PK;
					commissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
					commissionHeader.CH0_JobNumber = job.JH_JobNum;
				}

				var lineA = commissionHeader.Lines.AddNew();
				lineA.CL0_CAT = rate.PK;
				var lineB = commissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
				lineB.CL0_CAT = rate.PK;
			}

			Factory.Save();

			var properties = new[] { CommissionLineGroupingForTest.Schema.SourceNumber };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(AccTransactionHeaderSchema.Constants.TableName, 1);
			expectedDbHits.Add(JobHeaderSchema.Constants.TableName, 0);
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testCommissionLines = viewFactory.Load<ViewCommissionLine>(new ZQuery());
			var testCommissionSourceGroupings = testCommissionLines.Select(x =>
				{
					var grouping = new CommissionLineGroupingForTest(viewFactory);
					grouping.Init(new[] { x });
					return grouping;
				});

			viewFactory.ResetDatabaseLoadCount();

			foreach (var testGrouping in testCommissionSourceGroupings)
			{
				testGrouping.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testGrouping in testCommissionSourceGroupings)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testGrouping[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}
	}
}
