using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AeoCertificateValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter IAeoCertificateSupporter is null", () => new AeoCertificateValidator(null, new Mock<IAeoCertificateOrganisationCaptionProvider>().Object));
		AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter IAeoCertificateOrganisationCaptionProvider is null", () => new AeoCertificateValidator(new Mock<IAeoCertificateSupporter>().Object, null));
	}

	public void TestCheckAEOCertificateWhenInpurtParameterIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter is null", () => aeoCertificateValidator.CheckAEOCertificate(null));
	}

	protected override void SetUp()
	{
		base.SetUp();

		aeoCertificateSupporterMock = new Mock<IAeoCertificateSupporter>();
		aeoCertificateValidator = new AeoCertificateValidator(aeoCertificateSupporterMock.Object, new Mock<IAeoCertificateOrganisationCaptionProvider>().Object);
	}
	Mock<IAeoCertificateSupporter> aeoCertificateSupporterMock;
	AeoCertificateValidator aeoCertificateValidator;
}
