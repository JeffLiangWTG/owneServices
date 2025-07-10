using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class MTDSubmissionDataTestEnvironmentCreator
	{
		public static void CreateCustomsCode(TestObjectCreator objectCreator)
		{
			var cusCode = objectCreator.Debtor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = MockVRN;
			cusCode.OK_RN_NKCodeCountry = "GB";
			objectCreator.Factory.Save();
		}

		public static GlbCompany CreateUKCompany(TestObjectCreator objectCreator)
		{
			var cusCode = objectCreator.Debtor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = MockVRN;
			cusCode.OK_RN_NKCodeCountry = "GB";
			var company = objectCreator.CreateNewCompany("UK1", "GB", objectCreator.Debtor);
			company.SetCurrency(Constants.CurrencyCodes.UnitedKingdom);
			var branch = objectCreator.CreateNewBranch(company, "ZBR");
			objectCreator.Factory.Save();
			return company;
		}

		public static GlbCompany CreateUKCompany(TestObjectCreator objectCreator, string companyName, string branchCode)
		{
			var company = objectCreator.CreateNewCompany(companyName, "GB", objectCreator.Debtor);
			company.SetCurrency(Constants.CurrencyCodes.UnitedKingdom);
			objectCreator.CreateNewBranch(company, branchCode);
			objectCreator.Factory.Save();
			return company;
		}

		public static (GlbCompany GroupCompany, GlbCompany MemberCompany) CreateUKGroupAndMemberCompany(BusinessObjectFactory businessObjectFactory, string groupCompanyName, string memberCompanyName)
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(businessObjectFactory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			CreateCustomsCode(testObjectCreator);
			return (CreateUKCompany(testObjectCreator, groupCompanyName, groupCompanyName), CreateUKCompany(testObjectCreator, memberCompanyName, memberCompanyName));
		}

		public const string MockVRN = "1125463";
	}
}
