using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ProfitLossFilterProvider))]
	public class ProfitLossFilterProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDepartment()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));

			GlbDepartmentCollection departments = new GlbDepartmentCollection(Factory);
			filter.DepartmentFilter = departments[0].PK;
			AssertNoErrors("Valid", filter.DepartmentFilterInfo);

			filter.DepartmentFilter = ZGuid.NewZGuid();
			AssertHasErrors("Invalid", filter.DepartmentFilterInfo);

			filter.DepartmentFilter = ZGuid.Empty;
			AssertNoErrors("Empty is valid", filter.DepartmentFilterInfo);
		}

		public void TestBranch()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));

			GlbBranchCollection branches = new GlbBranchCollection(Factory);
			branches.Load();
			filter.BranchFilter = branches[0].PK;
			AssertNoErrors("Valid", filter.BranchFilterInfo);

			filter.BranchFilter = ZGuid.NewZGuid();
			AssertHasErrors("Invalid", filter.BranchFilterInfo);

			filter.BranchFilter = ZGuid.Empty;
			AssertNoErrors("Empty is valid", filter.BranchFilterInfo);
		}

		public void TestChargeCode()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));

			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			filter.ChargeCodeFilter = chargeCode.AC_Code;
			AssertNoErrors("Valid", filter.ChargeCodeFilterInfo);

			filter.ChargeCodeFilter = "BLAH!";
			AssertHasErrors("Invalid", filter.ChargeCodeFilterInfo);

			filter.ChargeCodeFilter = ZString.Empty;
			AssertNoErrors("Empty is valid", filter.ChargeCodeFilterInfo);
		}

		public void TestJob()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));

			Job testJob = Factory.NewJobForTesting<Job>();
			filter.JobNumberFilter = testJob.PK;
			AssertNoErrors("Valid", filter.JobNumberFilterInfo);

			filter.JobNumberFilter = ZGuid.NewZGuid();
			AssertHasErrors("Invalid", filter.JobNumberFilterInfo);

			filter.JobNumberFilter = ZGuid.Empty;
			AssertNoErrors("Empty is valid", filter.JobNumberFilterInfo);
		}

		public void TestCompany()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));

			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			filter.CompanyFilter = companies[0].PK;
			AssertNoErrors("Valid", filter.CompanyFilterInfo);

			filter.CompanyFilter = ZGuid.NewZGuid();
			AssertHasErrors("Invalid", filter.CompanyFilterInfo);

			filter.CompanyFilter = ZGuid.Empty;
			AssertNoErrors("Empty is valid", filter.CompanyFilterInfo);
		}

		public void TestRecognizedCharges()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));

			filter.RecognizedChargesFilter = "ALL";
			AssertNoErrors("Valid", filter.RecognizedChargesFilterInfo);

			filter.RecognizedChargesFilter = "REC";
			AssertNoErrors("Valid", filter.RecognizedChargesFilterInfo);

			filter.RecognizedChargesFilter = "NRC";
			AssertNoErrors("Valid", filter.RecognizedChargesFilterInfo);

			filter.RecognizedChargesFilter = "XYZ";
			AssertHasErrors("Invalid", filter.RecognizedChargesFilterInfo);

			filter.RecognizedChargesFilter = ZString.Empty;
			AssertNoErrors("Empty is valid", filter.RecognizedChargesFilterInfo);
		}

		public void TestClearFilterValues()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));
			filter.DepartmentFilter = ZGuid.NewZGuid();
			filter.BranchFilter = ZGuid.NewZGuid();
			filter.ChargeCodeFilter = "BLA";
			filter.JobNumberFilter = ZGuid.NewZGuid();
			filter.CompanyFilter = ZGuid.NewZGuid();
			filter.ShowReversedFilter = false;
			filter.RecognizedChargesFilter = "REC";

			filter.ClearFilterValues();

			AssertEquals(ZGuid.Empty, filter.DepartmentFilter);
			AssertEquals(ZGuid.Empty, filter.BranchFilter);
			AssertEquals(ZString.Empty, filter.ChargeCodeFilter);
			AssertEquals(ZGuid.Empty, filter.JobNumberFilter);
			AssertEquals(ZGuid.Empty, filter.CompanyFilter);
			AssertEquals(true, filter.ShowReversedFilter);
			AssertEquals(ZString.Empty, filter.RecognizedChargesFilter);
		}

		public void TestDefaultValues()
		{
			ProfitLossFilterProvider filter = new ProfitLossFilterProvider(new JobProfitLoss(Factory));
			AssertEquals("RecognizedChargesFilter", "ALL", filter.RecognizedChargesFilter);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProfitLossFilterProvider(new JobProfitLoss(Factory));
		}
	}
}
