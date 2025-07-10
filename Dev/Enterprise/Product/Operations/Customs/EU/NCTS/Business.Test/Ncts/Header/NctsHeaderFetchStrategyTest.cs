using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(NctsHeader.Job) + "+" + JobHeader.Schema.JH_HoldReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(NctsHeader.Job) + "+" + JobHeader.Schema.JH_Status, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(NctsHeader.Job) + "+" + JobHeader.Schema.JH_Status;
			var shipment = Factory.New<NctsHeader>();
			shipment.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(NctsHeader.Job) + "+" + nameof(NctsHeader.Job.JH_HoldReason);
			shipment = Factory.New<NctsHeader>();
			shipment.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_ContactFullName()
		{
			AssertLocationContactFetchForView(nameof(NctsHeader.ContactFullName));
		}

		public void TestFetchForView_ContactPhone()
		{
			AssertLocationContactFetchForView(nameof(NctsHeader.ContactPhone));
		}

		public void TestFetchForView_ContactEmail()
		{
			AssertLocationContactFetchForView(nameof(NctsHeader.ContactEmail));
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			CreateNctsHeader();
			return new NctsHeaderCollection(Factory);
		}

		void AssertLocationContactFetchForView(string propertyName)
		{
			AssertFetchForView(propertyName, new Dictionary<string, int>
			{
				{ CusInBondPerson.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateNctsHeader();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var headers = newFactory.Load<NctsHeader>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var header in headers)
			{
				header.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var header in headers)
			{
				_ = header.ZPropertyInfoHash.GetPropertySafe(propertyName).Value;
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		protected virtual NctsHeader CreateNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			_ = header.LocationContact;
			return header;
		}
	}
}
