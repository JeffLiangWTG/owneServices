using System;
using System.IO;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.EInvoicing.HardwareTokenSigning
{
	sealed class ESigningValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateChipsetType_ProductionSystem()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var eSignging = new ESigningBusinessObject(null, null, null);
			eSignging.ChipsetType = string.Empty;
			AssertHasErrors("Should be errors", eSignging.ChipsetTypeInfo);

			eSignging.ChipsetType = "Æāß";
			AssertHasErrors("Should be errors", eSignging.ChipsetTypeInfo);

			eSignging.ChipsetType = ESigningBusinessObject.WindowsToken;
			AssertHasErrors("Should be errors", eSignging.ChipsetTypeInfo);

			eSignging.ChipsetType = "EPASS2003";
			AssertNoErrors("Should be no errors", eSignging.ChipsetTypeInfo);
		}

		public void TestValidateChipsetType_NonProductionSystem()
		{
			var eSignging = new ESigningBusinessObject(null, null, null);
			eSignging.ChipsetType = string.Empty;
			AssertHasErrors("Should be errors", eSignging.ChipsetTypeInfo);

			eSignging.ChipsetType = "Æāß";
			AssertHasErrors("Should be errors", eSignging.ChipsetTypeInfo);

			eSignging.ChipsetType = ESigningBusinessObject.WindowsToken;
			AssertNoErrors("Should be no errors", eSignging.ChipsetTypeInfo);

			eSignging.ChipsetType = "EPASS2003";
			AssertNoErrors("Should be no errors", eSignging.ChipsetTypeInfo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateCertificateCode()
		{
			var mockCryptoApi = new Moq.Mock<ICryptoApi>();
			var signatureData = ZBlob.FromUTF8("exactly 32 byte fake signature.");
			mockCryptoApi.Setup(x => x.SignWithToken(Moq.It.IsAny<Chipset>(), Moq.It.IsNotNull<string>(), Moq.It.IsNotNull<byte[]>(), Moq.It.IsNotNull<byte[]>()))
						.Returns(signatureData);
			string base64CertificateData;
			using (StreamReader streamReader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\eInvoicing\HardwareTokenSigning\TestFiles\TestCertificateData.cer"))
			{
				base64CertificateData = streamReader.ReadToEnd();
			}
			var certificateData = Convert.FromBase64String(base64CertificateData);
			mockCryptoApi.Setup(x => x.GetCertificatesFromToken(Moq.It.IsAny<Chipset>()))
						.Returns(new CertificateInfo[] { new CertificateInfo() { Content = certificateData } });

			var eSignging = new ESigningBusinessObject(null, null, null, cryptoApi: mockCryptoApi.Object);
			eSignging.CertificateCode = ZString.Empty;
			AssertHasErrors("Should be errors", eSignging.CertificateCodeInfo);

			eSignging.CertificateCode = "5";
			AssertHasErrors("Should be errors", eSignging.CertificateCodeInfo);

			eSignging.CertificateCode = "1";
			AssertNoErrors("Should be no errors", eSignging.ChipsetTypeInfo);
		}

		public void TestValidateEnteredPin()
		{
			var eSignging = new ESigningBusinessObject(null, null, null);
			eSignging.EnteredPin = ZString.Empty;
			AssertHasErrors("Should be errors", eSignging.EnteredPinInfo);

			eSignging.EnteredPin = "123";
			AssertHasErrors("Should be errors", eSignging.EnteredPinInfo);

			eSignging.EnteredPin = "1234";
			AssertNoErrors("Should be no errors", eSignging.EnteredPinInfo);
		}
	}
}
