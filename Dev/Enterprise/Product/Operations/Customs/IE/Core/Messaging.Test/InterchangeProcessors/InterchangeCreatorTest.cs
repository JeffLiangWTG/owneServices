using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class InterchangeCreatorTest : TestCaseWithFactory
	{
		public void TestCreateOutgoingInterchange_CredentialPKProvided_Production()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var credentialPK = ZGuid.NewZGuid();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, "ZX!", "KD$", branch.PK, "https://www.endpoint.com", "<Greeting>Hello</Greeting>", credentialPK: credentialPK);
			AssertOutgoingInterchange(outgoingInterchange, branch.PK, company.LicenceKeyIdentifier, "IECustomsROS", credentialPK);
		}

		public void TestCreateOutgoingInterchange_CredentialPKNotProvided_Production()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var wrapper = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var certificationData = wrapper.GetGlbExternalPasswordOrCreateNew();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, "ZX!", "KD$", branch.PK, "https://www.endpoint.com", "<Greeting>Hello</Greeting>");
			AssertOutgoingInterchange(outgoingInterchange, branch.PK, company.LicenceKeyIdentifier, "IECustomsROS", certificationData.PK);
		}

		public void TestCreateOutgoingInterchange_CredentialPKProvided_NonProduction()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var credentialPK = ZGuid.NewZGuid();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, "ZX!", "KD$", branch.PK, "https://www.endpoint.com", "<Greeting>Hello</Greeting>", credentialPK: credentialPK);
			AssertOutgoingInterchange(outgoingInterchange, branch.PK, company.LicenceKeyIdentifier, "IECustomsTest", credentialPK);
		}
		public void TestCreateOutgoingInterchange_CredentialPKNotProvided_NonProduction()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var wrapper = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var certificationData = wrapper.GetGlbExternalPasswordOrCreateNew();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, "ZX!", "KD$", branch.PK, "https://www.endpoint.com", "<Greeting>Hello</Greeting>");
			AssertOutgoingInterchange(outgoingInterchange, branch.PK, company.LicenceKeyIdentifier, "IECustomsTest", certificationData.PK);
		}

		void AssertOutgoingInterchange(EDIInterchange outgoingInterchange, ZGuid expectBranchPK, ZString expectedFrom, ZString expectedTo, ZGuid expectedCredentialPK)
		{
			CombineAssertions("Production", () =>
			{
				AssertEquals("outgoingInterchange.EI_ApplicationCode", "ZX!", outgoingInterchange.EI_ApplicationCode);
				AssertEquals("outgoingInterchange.EI_InterchangeType", "KD$", outgoingInterchange.EI_InterchangeType);
				AssertEquals("outgoingInterchange.EI_ReceiveTransmit", "TRX", outgoingInterchange.EI_ReceiveTransmit);
				AssertEquals("outgoingInterchange.EI_TransportType", "XTT", outgoingInterchange.EI_TransportType);
				AssertNotEquals("outgoingInterchange.EI_SessionGUID", ZGuid.Empty, outgoingInterchange.EI_SessionGUID);
				AssertEquals("outgoingInterchange.EI_Priority", "HGH", outgoingInterchange.EI_Priority);
				AssertEquals("outgoingInterchange.EI_IsActive", ZBool.True, outgoingInterchange.EI_IsActive);
				AssertEquals("outgoingInterchange.EI_To", expectedTo, outgoingInterchange.EI_To);
				AssertEquals("outgoingInterchange.EI_GB", expectBranchPK, outgoingInterchange.EI_GB);
				AssertEquals("outgoingInterchange.EI_From", expectedFrom, outgoingInterchange.EI_From);
				AssertEquals("outgoingInterchange.EI_GP", expectedCredentialPK, outgoingInterchange.EI_GP);
				AssertEquals("outgoingInterchange.EI_BodyText", "<Greeting>Hello</Greeting>", outgoingInterchange.EI_BodyText);
				AssertEquals("outgoingInterchange.EI_Status", "QUE", outgoingInterchange.EI_Status);
				AssertEquals("outgoingInterchange.EI_HeaderText", @"{""custom.IE.Endpoint"":""https://www.endpoint.com""}", outgoingInterchange.EI_HeaderText);
			});
		}
	}
}
