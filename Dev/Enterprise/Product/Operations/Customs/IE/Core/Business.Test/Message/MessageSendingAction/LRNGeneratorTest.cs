using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class LRNGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2021, 01, 01)]
		public void TestNewLRNGenerated()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branch = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");
			var generator = new LRNGenerator(Factory, branch);
			var dummy = Factory.New<DummyBusinessObject>();
			generator.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_DescriptionInfo, () => false);
			AssertEquals(AddPrefix("BRX2100000001V01"), dummy.Z0_Description);
		}

		[TestDate(2024, 05, 28)]
		public void TestNewTestNewLRNGenerated_PartByPart()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branch = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");
			var generator = new LRNGenerator(Factory, branch, LRNGenerator.Constants.TemporaryStorage.LrnPrefix, LRNGenerator.Constants.TemporaryStorage.FountainPrefix);
			var dummy = Factory.New<DummyBusinessObject>();
			generator.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_DescriptionInfo, () => false);

			var output = dummy.Z0_Description;

			CombineAssertions("Output LRN part by part.", () =>
			{
				var checkIndex = 0;
				AssertEquals("Prefix", LRNGenerator.Constants.TemporaryStorage.LrnPrefix, output.Substring(checkIndex, 2));

				checkIndex += 2;
				var licenceServerID = company.LicenceServerID;
				AssertEquals("LicenceServerID", licenceServerID, output.Substring(checkIndex, licenceServerID.Length));

				checkIndex += licenceServerID.Length;
				AssertEquals("BranchCode", "BRX", output.Substring(checkIndex, 3));

				checkIndex += 3;
				AssertEquals("Year", "24", output.Substring(checkIndex, 2));

				checkIndex += 2;
				AssertEquals("Number sequence", "000000001", output.Substring(checkIndex, 9));

				checkIndex += 9;
				AssertEquals("Subsequence", "V01", output.Substring(checkIndex, output.Length - checkIndex));
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestNewLRNGeneratedForMultipleDeclarations()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branch = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");

			var generator = new LRNGenerator(Factory, branch);
			var dummy = Factory.New<DummyBusinessObject>();
			CombineAssertions(() =>
			{
				generator.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_DescriptionInfo, () => false);
				AssertEquals(AddPrefix("BRX2100000001V01"), dummy.Z0_Description);

				generator.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_VarCharMaxInfo, () => false);
				AssertEquals("Second LRN Generated", AddPrefix("BRX2100000002V01"), dummy.Z0_VarCharMax);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestNewLRNStartingSequenceForDifferentBranches()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branchX = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");
			var branchY = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRY");

			var generatorBranchX = new LRNGenerator(Factory, branchX);
			var generatorBranchY = new LRNGenerator(Factory, branchY);
			var dummy = Factory.New<DummyBusinessObject>();
			CombineAssertions(() =>
			{
				generatorBranchX.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_DescriptionInfo, () => false);
				AssertEquals("BRX Branch LRN", AddPrefix("BRX2100000001V01"), dummy.Z0_Description);
				generatorBranchY.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_VarCharMaxInfo, () => false);
				AssertEquals("BRY Branch LRN", AddPrefix("BRY2100000001V01"), dummy.Z0_VarCharMax);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestNewLRNVersionIncremented()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branch = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = AddPrefix("BRX2100000001V01");
			Factory.Save();
			var generator = new LRNGenerator(Factory, branch);
			generator.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_DescriptionInfo, () => false);
			AssertEquals(AddPrefix("BRX2100000001V02"), dummy.Z0_Description);
		}

		[TestDate(2021, 01, 01)]
		public void TestNewLRNVersionNumberOnOldFormat()
		{
			var company = CreateCompany(Factory, Core.Constants.CountryCodes.Ireland, "CO1");
			var branch = CreateBranch(company, Core.Constants.CountryCodes.Ireland, "BRX");
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "BRX2100000001V001";
			Factory.Save();
			var generator = new LRNGenerator(Factory, branch);
			generator.GetLRNAndSetIfNeeded((ZPropertyInfoString)dummy.Z0_DescriptionInfo, () => false);
			AssertEquals("BRX2100000001V002", dummy.Z0_Description);
		}

		internal static GlbCompany CreateCompany(BusinessObjectFactory factory, ZString countryCode, ZString code)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = $"TEST {code} COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = countryCode;
			return company;
		}

		internal static GlbBranch CreateBranch(GlbCompany company, ZString countryCode, ZString code)
		{
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_BranchName = $"TEST {code} BRANCH";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = countryCode;
			return branch;
		}

		static string AddPrefix(string lrn)
		{
			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			return prefix + lrn;
		}
	}
}
