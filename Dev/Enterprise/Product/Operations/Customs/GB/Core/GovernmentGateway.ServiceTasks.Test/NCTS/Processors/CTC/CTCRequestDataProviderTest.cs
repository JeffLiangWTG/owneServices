using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.GovernmentGateway.GatewayXml.ServiceTask.Ncts;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class CTCRequestDataProviderTest : TestCaseWithFactory
	{
		public void TestServiceReference()
		{
			var header = Factory.New<NctsHeader>();
			NctsHeaderTest.CreateMessagesForServiceReferenceTest(header);
			var outboundMessage = Factory.New<EDIMessage>();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			outboundMessage.EM_MessageSubType = "007";

			var provider = new CTCRequestDataProviderForTest(header, outboundMessage);
			AssertEquals("The latest outbound arrival message has a reference of 2", "2", provider.ServiceReference);

			outboundMessage.EM_MessageSubType = "015";
			provider = new CTCRequestDataProviderForTest(header, outboundMessage);
			AssertEquals("The latest outbound arrival message has a reference of 5", "5", provider.ServiceReference);
		}

		public void TestGateway()
		{
			var header = Factory.New<NctsHeader>();
			var provider = new CTCRequestDataProviderForTest(header, Factory.New<EDIMessage>());
			AssertEquals("GovernmentGateway", provider.Gateway);
		}

		public void TestJobNumber()
		{
			var header = Factory.New<NctsHeader>();
			var provider = new CTCRequestDataProviderForTest(header, Factory.New<EDIMessage>());

			header.BH_JobReference = "JOB456789";

			AssertEquals("JOB456789", provider.JobNumber);
		}

		public void TestGetCredentialsKey()
		{
			var enterpriseCode = GBExtensions.GetEnterpriseCode();
			var header = Factory.New<NctsHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ImporterA";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB1234567890", Core.Constants.CountryCodes.UnitedKingdom);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "ABC";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = orgHeader.PK;
			Factory.Save();

			header.BH_GB = aaaBranch.PK;

			var provider = new CTCRequestDataProviderForTest(header, Factory.New<EDIMessage>());

			var collection = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(header.Company).GBBPasswordCollection;
			var pwd = collection.AddNew();

			pwd.Badge = "ABC";
			pwd.EORI = "GB1234567890";
			pwd.Status = PasswordStatusList.Codes.Invalid;
			pwd.GP_ExpiryDate = ZDateTime.Today.AddMonths(1);

			var actual = provider.GetCredentialsKey_Exposed();
			var expected = $"{enterpriseCode}..NOVALIDTOKENFOUND";
			AssertEquals("No token", expected, actual);

			pwd.Status = PasswordStatusList.Codes.Valid;
			pwd.IsTokenForNCTS = true;
			actual = provider.GetCredentialsKey_Exposed();
			expected = $"{enterpriseCode}.GB1234567890.ABC";
			AssertEquals("valid token", expected, actual);

			var pwd2 = collection.AddNew();

			pwd2.Badge = "XYZ";
			pwd2.EORI = "GB1234567890";
			pwd2.Status = PasswordStatusList.Codes.Valid;
			pwd2.GP_ExpiryDate = ZDateTime.Today.AddYears(1);

			actual = provider.GetCredentialsKey_Exposed();
			expected = $"{enterpriseCode}.GB1234567890.ABC";
			AssertEquals("valid token", expected, actual);

			var pwd3 = collection.AddNew();

			pwd3.Badge = "ABC";
			pwd3.EORI = "GB0123456789";
			pwd3.Status = PasswordStatusList.Codes.Valid;
			pwd3.GP_ExpiryDate = ZDateTime.Today.AddYears(1);

			actual = provider.GetCredentialsKey_Exposed();
			expected = $"{enterpriseCode}.GB1234567890.ABC";
			AssertEquals("valid token", expected, actual);
		}

		public void TestGetCredentialSetting()
		{
			var header = Factory.New<NctsHeader>();
			var provider = new CTCRequestDataProviderForTest(header, Factory.New<EDIMessage>());

			AssertNull("No credential settings", provider.GetCredentialsSetting_Exposed());
		}

		class CTCRequestDataProviderForTest : CTCRequestDataProvider
		{
			public CTCRequestDataProviderForTest(NctsHeader header, EDIMessage outboundMessage) : base(header, outboundMessage)
			{
			}

			public ZString GetCredentialsKey_Exposed() => base.GetCredentialsKey();
			public CredentialsSetting GetCredentialsSetting_Exposed() => base.GetCredentialsSetting();
		}
	}
}
