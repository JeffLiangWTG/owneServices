using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class LineOnlyVoucherTestCase : VoucherProviderTestCase
	{
		public override void TestCompanyName()
		{
			var companyOrgProxy = TestObjectCreator.CreateOrgHeader("COMPROXY", true, true);
			var companyOrgProxyOFCAddressCHS = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Office, true);
			companyOrgProxyOFCAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			companyOrgProxyOFCAddressCHS.OA_CompanyNameOverride = "Company > Org Proxy > Main OFC Address’s Company Name in CHS";

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("BRNPROXY", true, true);
			var branchOrgProxyOFCAddressCHS = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Office, true);
			branchOrgProxyOFCAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			branchOrgProxyOFCAddressCHS.OA_CompanyNameOverride = "Branch > Org Proxy > Main OFC Address’s Company Name in CHS";

			var company = TestObjectCreator.CreateNewCompany("DCN", Core.Constants.CountryCodes.China, orgProxy: companyOrgProxy);
			var branch = TestObjectCreator.CreateBranch("SHA", "Shanghai", company, branchOrgProxy);
			var loginBranch = TestObjectCreator.CreateBranch("BEI", "Beijing", company);

			Factory.Save();

			AccTransactionHeader transaction;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				transaction = GetTestTransaction();
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, loginBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var provider = GetVoucherProvider(transaction);

				companyOrgProxy.OH_FullName = "Company > Org Proxy > Details > Details > Full Name";
				Factory.Save();
				AssertEquals(companyOrgProxy.OH_FullName, provider.CompanyName);

				var companyOrgProxyARMAddressENG = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Receivables, true);
				companyOrgProxyARMAddressENG.OA_Language = Core.SharedConstants.Languages.English;
				companyOrgProxyARMAddressENG.OA_CompanyNameOverride = "Company > Org Proxy > Main ARM Address’s Company Name in ENG";
				var translatedAddressCHSForCompanyOrgProxyARMAddressENG = TestObjectCreator.CreateTranslatedAddress(companyOrgProxyARMAddressENG, Core.SharedConstants.Languages.ChineseSimplified, "Company > Org Proxy > Main ARM Address’s Company Name in ENG -> Translated Address in CHS", "OTA_Address1");
				Factory.Save();
				AssertEquals(translatedAddressCHSForCompanyOrgProxyARMAddressENG.CompanyName, provider.CompanyName);

				var companyOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Receivables, true);
				companyOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				companyOrgProxyARMAddressCHS.OA_CompanyNameOverride = "Company > Org Proxy > Main ARM Address’s Company Name in CHS";
				Factory.Save();
				AssertEquals(companyOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);

				var branchOrgProxyARMAddressENG = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
				branchOrgProxyARMAddressENG.OA_Language = Core.SharedConstants.Languages.English;
				branchOrgProxyARMAddressENG.OA_CompanyNameOverride = "Branch > Org Proxy > Main ARM Address’s Company Name in ENG";
				var translatedAddressCHSForBranchOrgProxyARMAddressENG = TestObjectCreator.CreateTranslatedAddress(branchOrgProxyARMAddressENG, Core.SharedConstants.Languages.ChineseSimplified, "Branch > Org Proxy > Main ARM Address’s Company Name in ENG -> Translated Address in CHS", "OTA_Address1");
				Factory.Save();
				AssertEquals("Line Only Voucher doesn't belong to any branch", companyOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);

				var branchOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
				branchOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				branchOrgProxyARMAddressCHS.OA_CompanyNameOverride = "Branch > Org Proxy > Main ARM Address’s Company Name in CHS";
				Factory.Save();
				AssertEquals("Line Only Voucher doesn't belong to any branch", companyOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);
			}
		}

		public override void TestBranchCode()
		{
			var testLineOnlyProvider = GetVoucherProvider(GetTestTransaction());
			AssertEquals(ZString.Empty, testLineOnlyProvider.BranchCode);
		}

		public void TestPostDate()
		{
			VoucherProvider testLineOnlyProvider = GetVoucherProvider(GetTestTransaction());
			ZDateTime expectedDate = new AccountingPeriodCalculator(Factory).GetLastDayForPeriod(testLineOnlyProvider.Period);
			AssertEquals(expectedDate, testLineOnlyProvider.PostDate);
		}

		public void TestPostedByPerson()
		{
			VoucherProvider testLineOnlyProvider = GetVoucherProvider(GetTestTransaction());
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, testLineOnlyProvider.PostedBy);
		}

		protected override void PrepareDataForTestForNewFieldsTest()
		{
			base.PrepareDataForTestForNewFieldsTest();
			Job = TestObjectCreator.CreateJobHeader();
			AccTransactionLines wip = TestObjectCreator.CreateWIP(Job, ChargeCode1, 1.0M, "LINE DESCRIPTION", 100M);
			wip.AL_PostDate = new ZDateTime(2004, 01, 01);
			AccTransactionLines accrual = TestObjectCreator.CreateAccrual(Job, ChargeCode1, 1.0M, "LINE DESCRIPTION", 100M);
			accrual.AL_PostDate = new ZDateTime(2004, 01, 01);
		}

		protected override void CoreTestForVoucherCurrency(RefCurrency currency)
		{
			Assert(true);
		}
	}
}
