using NUnit.Framework;

namespace Enterprise.Customs.Common.CA.Testing
{
	class TariffTreatmentCodesTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsCountryOfOriginAndExportRequiredForLVS()
		{
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(TariffTreatmentCodes.Codes.MostFavouredNation), Is.EqualTo(false), TariffTreatmentCodes.Descriptions.MostFavouredNation.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(TariffTreatmentCodes.Codes.UnitedStates), Is.EqualTo(false), TariffTreatmentCodes.Descriptions.UnitedStates.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(TariffTreatmentCodes.Codes.CanadaIsraelAgreement), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.CanadaIsraelAgreement.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(TariffTreatmentCodes.Codes.Iceland), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.Iceland.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(TariffTreatmentCodes.Codes.Australia), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.Australia.ToString());
		}

		[ExpectNoExceptions]
		public void TestIsNeedCheckForValidCertificateOfOrigin()
		{
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(TariffTreatmentCodes.Codes.MostFavouredNation), Is.EqualTo(false), TariffTreatmentCodes.Descriptions.MostFavouredNation.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(TariffTreatmentCodes.Codes.General), Is.EqualTo(false), TariffTreatmentCodes.Descriptions.General.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(TariffTreatmentCodes.Codes.UnitedStates), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.UnitedStates.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(TariffTreatmentCodes.Codes.CanadaIsraelAgreement), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.CanadaIsraelAgreement.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(TariffTreatmentCodes.Codes.Iceland), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.Iceland.ToString());
			NUnit.Framework.Assert.That(TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(TariffTreatmentCodes.Codes.Australia), Is.EqualTo(true), TariffTreatmentCodes.Descriptions.Australia.ToString());
		}
	}
}
