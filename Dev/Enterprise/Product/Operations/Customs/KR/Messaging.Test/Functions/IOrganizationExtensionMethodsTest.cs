using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class IOrganizationExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetRegistrationNumber()
		{
			var organisation = new Mock<IOrganization>();

			organisation.Setup(m => m.BusinessRegNo).Returns("1");
			organisation.Setup(m => m.KoreanRegNoForResident).Returns("2");//CEO's number can be present for non-individual organisations
			organisation.Setup(m => m.UnipassIDForOrganization).Returns("3");

			AssertEquals("1", organisation.Object.GetFirstMatchedRegistrationNumber(new string[] { IdentificationType.BusinessRegNo, IdentificationType.UnipassIDForOrganization }));
			AssertEquals("3", organisation.Object.GetFirstMatchedRegistrationNumber(new string[] { IdentificationType.UnipassIDForIndividual, IdentificationType.UnipassIDForOrganization }));
			AssertEquals("", organisation.Object.GetFirstMatchedRegistrationNumber(new string[] { IdentificationType.UnipassIDForIndividual, IdentificationType.ForeignCompanyID }));
			AssertEquals("1", organisation.Object.GetRegistrationNumber(IdentificationType.BusinessRegNo));
			AssertEquals("2", organisation.Object.GetRegistrationNumber(IdentificationType.KoreanRegNoForResident));
			AssertEquals("3", organisation.Object.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization));
			AssertEquals("", organisation.Object.GetRegistrationNumber(IdentificationType.ForeignCompanyID));
			organisation.VerifyAll();
		}

		public void TestGetRegistrationTypeNumber()
		{
			var organisation = new Mock<IOrganization>();

			organisation.Setup(m => m.KoreanRegNoForForeigner).Returns("1");
			organisation.Setup(m => m.UnipassIDForIndividual).Returns("2");

			var regoNumber = organisation.Object.GetFirstMatchedRegistrationTypeNumber(new string[] { IdentificationType.BusinessRegNo, IdentificationType.KoreanRegNoForForeigner, IdentificationType.UnipassIDForIndividual });
			AssertEquals(IdentificationType.KoreanRegNoForForeigner, regoNumber.Type);
			AssertEquals("1", regoNumber.Number);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, regoNumber.CountryOfIssue);

			regoNumber = organisation.Object.GetFirstMatchedRegistrationTypeNumber(new string[] { IdentificationType.UnipassIDForIndividual, IdentificationType.KoreanRegNoForForeigner });
			AssertEquals(IdentificationType.UnipassIDForIndividual, regoNumber.Type);
			AssertEquals("2", regoNumber.Number);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, regoNumber.CountryOfIssue);

			regoNumber = organisation.Object.GetFirstMatchedRegistrationTypeNumber(new string[] { IdentificationType.BusinessRegNo, IdentificationType.KoreanRegNoForResident });
			AssertNull(regoNumber);

			regoNumber = organisation.Object.GetRegistrationTypeAndNumber(IdentificationType.UnipassIDForIndividual);
			AssertEquals(IdentificationType.UnipassIDForIndividual, regoNumber.Type);
			AssertEquals("2", regoNumber.Number);

			regoNumber = organisation.Object.GetRegistrationTypeAndNumber(IdentificationType.KoreanRegNoForResident);
			AssertNull(regoNumber);
			organisation.VerifyAll();
		}

		public void TestGetBusinessOrIndividualRegoNumber()
		{
			var organisation = new Mock<IOrganization>();

			organisation.Setup(m => m.PassportNo).Returns("2");
			organisation.Setup(m => m.BusinessRegNo).Returns("1");
			organisation.Setup(m => m.IsIndividual).Returns(ZBool.False);
			var result = organisation.Object.GetBusinessOrIndividualRegistrationNumber();
			AssertEquals("When business", "1", result.Number);
			AssertEquals("When business", IOrganizationExtensionMethods.BusinessNoTypeForKRC, result.Type);

			organisation.Setup(m => m.IsIndividual).Returns(ZBool.True);
			result = organisation.Object.GetBusinessOrIndividualRegistrationNumber();
			AssertEquals("When individual", "2", result.Number);
			AssertEquals("When individual. Passport number type is 02 for KR Customs", IOrganizationExtensionMethods.PassportNoTypeForKRC, result.Type);

			organisation.VerifyAll();
		}

		public void TestGetBusinessOrIndividualRegistrationNumberTypes()
		{
			var result = IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(true);
			AssertEquals(4, result.Length);
			AssertEquals(IdentificationType.KoreanRegNoForResident, result[0]);
			AssertEquals(IdentificationType.PassportNo, result[1]);
			AssertEquals(IdentificationType.KoreanRegNoForForeigner, result[2]);
			AssertEquals(IdentificationType.UnipassIDForIndividual, result[3]);

			result = IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(false);
			AssertEquals(1, result.Length);
			AssertEquals(IdentificationType.BusinessRegNo, result[0]);
		}

		public void TestCorporationCode()
		{
			var organisation = new Mock<IOrganization>();

			organisation.Setup(m => m.CorporationCode).Returns("3");
			var result = organisation.Object.GetFirstMatchedRegistrationTypeNumber(new string[] { IdentificationType.CorporationCode });
			AssertEquals("3", result.Number);
			AssertEquals(IOrganizationExtensionMethods.CorporationNoTypeForKRC, result.Type);
			organisation.VerifyAll();
		}
	}
}
