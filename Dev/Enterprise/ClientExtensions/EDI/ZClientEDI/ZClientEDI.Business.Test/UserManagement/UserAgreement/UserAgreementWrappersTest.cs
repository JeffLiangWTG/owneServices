using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(EdiUserAgreementWrapper))]
	public class EdiUserAgreementWrapperTest : NonPersistentBusinessObjectTestCase
	{
	}

	[TestedType(typeof(DocEdiUserAgreement))]
	public class DocEdiUserAgreementTest : DocumentWrapperTestCase
	{
		public void TestMacros()
		{
			AssertEquals("User1 Jim", Wrapper.UserFullName);
			AssertEquals("user1@cw1.com", Wrapper.UserEmail);
			AssertEquals("Title001", Wrapper.AgreementTitle);
			AssertEquals("Content002", Wrapper.AgreementContent);
			AssertEquals("3", Wrapper.AgreementVersionNumber);
			AssertEquals("TestOrg004", Wrapper.ClientName);
			AssertEquals("#1, New South Wales, Australia", Wrapper.ClientMainAddressInSingleLine);
			AssertEquals("Not on file", Wrapper.ClientBusinessRegistration);
		}

		DocEdiUserAgreement Wrapper
		{
			get { return (DocEdiUserAgreement)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "111", "SD1");
			var org1 = licence1.Company.Header;
			var db1 = licence1.Database;
			db1.LD_Product = ProductTypes.Codes.Enterprise;
			db1.LD_LicenceType = DatabaseTypes.Codes.Test;
			db1.LD_DatabaseNumber = 1001;
			db1.LD_OH_WebAccessOrg = org1.PK;

			var ediUserAgreementWrapper = new EdiUserAgreementWrapper()
			{
				UserAgreement = Factory.New<EdiUserAgreement>(),
				UserInfo = new WTG.TrustedMessaging.MyAccount.Models.TrustedUserInfo()
				{
					Email = "user1@cw1.com",
					FullName = "User1 Jim",
				},
				Database = db1,
			};

			ediUserAgreementWrapper.AgreementTitle = "Title001";
			ediUserAgreementWrapper.AgreementContent = "Content002";
			ediUserAgreementWrapper.UserAgreement.ERA_VersionNumber = 3;
			ediUserAgreementWrapper.Database.WebAccessOrg.OH_FullName = "TestOrg004";

			return new DocumentWrapper[] { DocEdiUserAgreement.New(ediUserAgreementWrapper, Factory) };
		}
	}
}
