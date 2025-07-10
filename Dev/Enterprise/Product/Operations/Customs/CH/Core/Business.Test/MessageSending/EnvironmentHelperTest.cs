using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class EnvironmentHelperTest : TestCaseWithFactory
{
	public void TestCheckMessageSendingEnvironmentForEdec()
	{
		const string noRegistrationNoMessage = "Customs Registration Number is not configured for the current company. Please contact your system administrator.";

		CombineAssertions(() =>
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			AssertEquals("No customs registration number", noRegistrationNoMessage, EnvironmentHelper.CheckMessageSendingEnvironmentForEdec());

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
			AssertEquals("Has customs registration number", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForEdec());
		});
	}

	public void TestGetBusinessPartnerId()
	{
		CombineAssertions(() =>
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "234567");
			AssertEquals("BusinessPartnerId on Current Company Proxy", "234567", EnvironmentHelper.GetBusinessPartnerId());

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = OrgHeader.New(GlbBranch.CurrentBranch.Factory).PK;
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123456");
			AssertEquals("BusinessPartnerId on Current Branch Proxy", "123456", EnvironmentHelper.GetBusinessPartnerId());
		});
	}

	public void TestCheckMessageSendingEnvironmentForPassarAndChartera()
	{
		const string noBIDMessage = "Business Partner ID (BID) is not configured for the company or branch. Please contact your system administrator.";
		const string noTokenMessage = "Communication Tokens are not configured for the company. Please contact your system administrator.";

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);

		CombineAssertions(() =>
		{
			void SetProperties(bool branchBID = true, bool companyBID = true, bool communicationToken = true)
			{
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
				if (branchBID)
				{
					GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
				}
				if (companyBID)
				{
					GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
				}
				companyWrapper.TokenCredentialsEnabled = communicationToken;
			}

			SetProperties();
			AssertEquals("Configuration is complete", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

			SetProperties(branchBID: false);
			AssertEquals("No branch BID", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

			SetProperties(companyBID: false);
			AssertEquals("No company BID", ZString.Empty, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

			SetProperties(branchBID: false, companyBID: false);
			AssertEquals("No BID at all", noBIDMessage, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());

			SetProperties(communicationToken: false);
			AssertEquals("No communication token", noTokenMessage, EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera());
		});
	}
}
