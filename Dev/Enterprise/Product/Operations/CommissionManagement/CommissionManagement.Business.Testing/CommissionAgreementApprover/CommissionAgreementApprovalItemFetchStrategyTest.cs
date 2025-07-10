using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class CommissionAgreementApprovalItemFetchStrategyTest : TestCaseWithFactory
	{
		#region FetchForView

		public void TestFetchForView_Opportunity()
		{
			var properties = new[] { "CommissionAgreement+Opportunity+P8_OpportunityID" };
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgOpportunitySchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		public void TestFetchForView_Customer()
		{
			var properties = new[] { "CommissionAgreement+Customer+OH_Code" };
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		#endregion

		#region Implementation

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testAgreements = viewFactory.Load<OrgCommissionAgreement>(new ZQuery(OrgCommissionAgreementSchema.PK, testAgreementPks));
			var wizard = new CommissionAgreementApprovalWizard(viewFactory);
			var testBackdateItems = testAgreements.Select(x => new CommissionAgreementApprovalItem(wizard, x)).ToArray();
			viewFactory.ResetDatabaseLoadCount();

			foreach (var testItem in testBackdateItems)
			{
				testItem.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testItem in testBackdateItems)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testItem[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testAgreementPks = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
				testAgreementPks.Add(agreement.PK);
			}

			Factory.Save();
		}

		List<ZGuid> testAgreementPks;

		#endregion
	}
}
