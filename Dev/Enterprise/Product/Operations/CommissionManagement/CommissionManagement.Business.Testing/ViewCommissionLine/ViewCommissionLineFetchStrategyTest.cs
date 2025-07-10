using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class ViewCommissionLineFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_CommissionHeader()
		{
			var rate = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>();
			for (var i = 0; i < 7; i++)
			{
				var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
				var lineA = commissionHeader.Lines.AddNew();
				lineA.CL0_CAT = rate.PK;
				var lineB = commissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
				lineB.CL0_CAT = rate.PK;
			}

			Factory.Save();

			var properties = new[]
				{
					"CommissionHeader.CH0_Product",
					"CommissionHeader.CH0_OH_Customer",
				};

			var expectedDbHits = new Dictionary<string, int>
			{
				{ AccCommissionHeaderSchema.Constants.TableName, 1 },
			};

			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		public void TestFetchForView_ChargeCode()
		{
			var rate = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>();
			for (var i = 0; i < 7; i++)
			{
				var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
				var lineA = commissionHeader.Lines.AddNew();
				lineA.CL0_CAT = rate.PK;
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_IsCommissionable = true;
				var lineB = commissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
				lineB.CL0_CAT = rate.PK;
			}

			Factory.Save();

			var properties = new[] { "ChargeCode.AC_Code" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(AccChargeCodeSchema.Constants.TableName, 1);
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		public void TestFetchForView_ApprovalRequestPk()
		{
			var rate = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>();
			for (var i = 0; i < 7; i++)
			{
				var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
				var lineA = commissionHeader.Lines.AddNew();
				lineA.CL0_CAT = rate.PK;
				var lineB = commissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
				lineB.CL0_CAT = rate.PK;

				var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
				approvalRequest.Items.AddNew().CRI_CL0 = lineA.PK;
				approvalRequest.Items.AddNew().CRI_CL0 = lineB.PK;
			}

			Factory.Save();

			var properties = new[] { "ApprovalRequestPk" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(AccCommissionApprovalRequestItemSchema.Constants.TableName, 1);
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testCommissionLines = viewFactory.Load<ViewCommissionLine>(new ZQuery());
			viewFactory.ResetDatabaseLoadCount();

			foreach (var testLine in testCommissionLines)
			{
				testLine.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testLine in testCommissionLines)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testLine[columnName];
				}
			}

			AssertDbHits(expectedDbHits, viewFactory);
		}
	}
}
