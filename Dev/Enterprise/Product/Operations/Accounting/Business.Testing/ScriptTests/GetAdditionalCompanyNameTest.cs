using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetAdditionalCompanyNameTest : ScriptTest
	{
		public void TestGetAdditionalCompanyNameWhenCompanyOrgProxyIsNull()
		{
			var expectedAdditionalCompanyName = "人生得意须尽欢";
			var chineseAddress = "莫使金樽空对月";

			var companyWithoutOrgProxy = TestObjectCreator.CreateNewCompany("ABC");
			companyWithoutOrgProxy.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			companyWithoutOrgProxy.GC_OH_OrgProxy = ZGuid.Empty;

			var branchWithoutOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB1");
			branchWithoutOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGBB2", false, true);
			branchOrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
			var branchWithOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB2");
			branchWithOrgProxy.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			AssertEquals("Precondition", ZGuid.Empty, companyWithoutOrgProxy.GC_OH_OrgProxy);
			AssertEquals("Precondition", ZGuid.Empty, branchWithoutOrgProxy.GB_OH_OrgProxy);
			AssertEquals("Precondition", branchOrgProxy.PK, branchWithOrgProxy.GB_OH_OrgProxy);

			var debtorOrgHeader = TestObjectCreator.CreateOrgHeader("ORG123", false, true);
			debtorOrgHeader.Addresses.RemoveAndDeleteAll();
			var orgAddress = debtorOrgHeader.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
			orgAddress.Address1 = chineseAddress;
			orgAddress.CompanyName = expectedAdditionalCompanyName;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
			orgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
			Factory.Save();

			AssertEquals(expectedAdditionalCompanyName, RunScript(debtorOrgHeader.PK, companyWithoutOrgProxy.PK, branchWithOrgProxy.PK, LedgerTypes.AccountsReceivable));

			AssertNull(RunScript(debtorOrgHeader.PK, companyWithoutOrgProxy.PK, branchWithoutOrgProxy.PK, LedgerTypes.AccountsReceivable));
		}

		public void TestGetAdditionalCompanyName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				AssertAdditionalCompanyName(OrgAddressType.Payables, LedgerTypes.AccountsPayable);
				AssertAdditionalCompanyName(OrgAddressType.Receivables, LedgerTypes.AccountsReceivable);
			}
		}

		void AssertAdditionalCompanyName(string addressType, string ledgerType)
		{
			var expectedAdditionalCompanyName = "人生得意须尽欢";
			var chineseAddress = "莫使金樽空对月";

			// Main Address
			var orgHeader = TestObjectCreator.CreateOrgHeader("ABC" + addressType, true, true);
			var orgAddress = TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, chineseAddress, expectedAdditionalCompanyName, addressType, addressType);

			Factory.Save();

			AssertEquals("The additional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, RunScript(orgHeader.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ledgerType));

			// Main Translated Address
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("DEF" + addressType, true, true);
			var orgAddress1 = TestObjectCreator.CreateAddress(orgHeader1, Constants.CountryCodes.China, SharedConstants.Languages.English, "Test Address", "", addressType, addressType);
			var translatedAddress1 = TestObjectCreator.CreateTranslatedAddress(orgAddress1, SharedConstants.Languages.ChineseSimplified, expectedAdditionalCompanyName, chineseAddress);

			Factory.Save();

			AssertEquals("The additional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, RunScript(orgHeader1.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ledgerType));

			// Non-Main Address
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("GHI" + addressType, true, true);
			var orgAddress2 = TestObjectCreator.CreateAddress(orgHeader2, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, chineseAddress, expectedAdditionalCompanyName, addressType, "");

			Factory.Save();

			AssertEquals("The additional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, RunScript(orgHeader2.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ledgerType));

			// Non-Main Translated Address
			var orgHeader3 = TestObjectCreator.CreateOrgHeader("LMN" + addressType, true, true);
			var orgAddress3 = TestObjectCreator.CreateAddress(orgHeader3, Constants.CountryCodes.China, SharedConstants.Languages.English, "Test Address", "", addressType, "");
			var translatedAddress3 = TestObjectCreator.CreateTranslatedAddress(orgAddress3, SharedConstants.Languages.ChineseSimplified, expectedAdditionalCompanyName, chineseAddress);

			Factory.Save();

			AssertEquals("The additional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, RunScript(orgHeader3.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ledgerType));

			// Empty
			var orgHeader4 = TestObjectCreator.CreateOrgHeader("OPQ" + addressType, true, true);

			Factory.Save();

			AssertNullOrEmpty("The additional company name should be null.", RunScript(orgHeader4.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, ledgerType));
		}

		public void TestGetAdditionalCompanyNameShouldFilterEmptyValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				var addressType = OrgAddressType.Receivables;

				var orgHeader = TestObjectCreator.CreateOrgHeader("ABC" + addressType, true, true);
				var orgAddress = TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "黄河之水天上来", "", addressType, addressType);

				Factory.Save();

				AssertNullOrEmpty(RunScript(orgHeader.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, LedgerTypes.AccountsReceivable));
			}
		}

		string RunScript(ZGuid orgHeader, ZGuid company, ZGuid branch, string ledgerType)
		{
			return DataUtils.GetListOfValuesFromQuery(Db.Connection, string.Format($@"
SELECT CompanyName FROM [dbo].[GetAdditionalCompanyName]
(
'{orgHeader}',
'{company}',
'{branch}',
'{ledgerType}'
)")).FirstOrDefault();
		}
	}
}
