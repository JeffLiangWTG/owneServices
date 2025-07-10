using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	[TestedType(typeof(ChinaReconciliationExportWrapper))]
	public class ChinaReconciliationExportWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBranches()
		{
			BizObj.Branches.Load();

			var collection = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK));
			collection.Load();

			Assert("Precondition Current Company has at least one Branch", collection.Count > 0);

			var result = collection.All(branch => BizObj.Branches.Contains(branch));

			Assert("Test Branches is OK", result);
		}

		public void TestValidateBranchCode()
		{
			BizObj.Branches.Load();

			BizObj.Branch = BizObj.Branches[0].PK;
			AssertEquals("Should have value", BizObj.Branches[0].GB_Code, BizObj.BranchCode);

			BizObj.Branch = ZGuid.NewZGuid();
			AssertHasError(BizObj.BranchInfo, "Enter a valid selection.");
		}

		public void TestPostDateFrom()
		{
			BizObj.PostDateFrom = ZDate.Empty;
			AssertHasError(BizObj.PostDateFromInfo, "Please enter a value.");

			BizObj.PostDateFrom = new ZDate(2016, 1, 1);
			AssertHasError(BizObj.PostDateFromInfo, "The date entered is not in the accounting periods");

			SetUpPeriod();
			BizObj.PostDateFrom = new ZDate(2016, 1, 2);
			AssertNoErrors(BizObj.PostDateFromInfo);

			BizObj.PostDateTo = new ZDate(2016, 1, 1);
			BizObj.PostDateFrom = new ZDate(2016, 1, 3);
			AssertHasError(BizObj.PostDateFromInfo, "'Post Date To' must be greater than 'Post Date From'");
			BizObj.PostDateFrom = new ZDate(2016, 1, 1);
			AssertNoErrors(BizObj.PostDateFromInfo);

			SetUpAnotherPeriod();
			BizObj.PostDateFrom = new ZDate(2015, 12, 1);
			AssertHasError(BizObj.PostDateFromInfo, "'Post Date From' and 'Post Date To' must be within a same financial year");
		}

		public void TestPostDateTo()
		{
			BizObj.PostDateTo = ZDate.Empty;
			AssertHasError(BizObj.PostDateToInfo, "Please enter a value.");

			BizObj.PostDateTo = new ZDate(2016, 1, 1);
			AssertHasError(BizObj.PostDateToInfo, "The date entered is not in the accounting periods");

			SetUpPeriod();
			BizObj.PostDateTo = new ZDate(2016, 1, 2);
			AssertNoErrors(BizObj.PostDateToInfo);

			BizObj.PostDateFrom = new ZDate(2016, 1, 3);
			BizObj.PostDateTo = new ZDate(2016, 1, 1);
			AssertHasError(BizObj.PostDateToInfo, "'Post Date To' must be greater than 'Post Date From'");

			BizObj.PostDateTo = new ZDate(2016, 1, 3);
			AssertNoErrors(BizObj.PostDateToInfo);

			SetUpAnotherPeriod();
			BizObj.PostDateTo = new ZDate(2015, 12, 3);
			AssertHasError(BizObj.PostDateToInfo, "'Post Date From' and 'Post Date To' must be within a same financial year");
		}

		public void TestComplianceSubType()
		{
			BizObj.ComplianceSubType = "TXT";
			AssertHasError(BizObj.ComplianceSubTypeInfo, "Enter a valid selection.");

			foreach (ICodeDescription complianceSubType in BizObj.ComplianceSubTypeList)
			{
				BizObj.ComplianceSubType = complianceSubType.Code;
				AssertNoErrors(BizObj.ComplianceSubTypeInfo);
			}
		}

		public void TestExportStatus()
		{
			BizObj.ExportStatus = "TXT";
			AssertHasError(BizObj.ExportStatusInfo, "Enter a valid selection.");

			foreach (ICodeDescription exportStatus in BizObj.ExportStatusList)
			{
				BizObj.ExportStatus = exportStatus.Code;
				AssertNoErrors(BizObj.ExportStatusInfo);
			}
		}

		void SetUpPeriod()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 31));
		}

		void SetUpAnotherPeriod()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(201512, new ZDateTime(2015, 12, 1), new ZDateTime(2015, 12, 31));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChinaReconciliationExportWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizObj = new ChinaReconciliationExportWrapper(Factory);
			BizObj.RunPreSaveValidation();
		}

		protected ChinaReconciliationExportWrapper BizObj;

		#endregion
	}
}
