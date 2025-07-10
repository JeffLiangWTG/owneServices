namespace Enterprise.Client.EDI.IdentityCertificate.Business.Testing
{
	using System;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Client.EDI.Registry.Business;
	using Enterprise.Registry.Business;

	class EdiIdentityCertificateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCARootValidation()
		{
			Assert("IDA_LD has value.", !ediIdentityCertificateWithLicense.Application.IDA_LD.IsEmpty);
			ediIdentityCertificateWithLicense.Validation.ValidateICE_CARoot();
			Assert("caroot is not mandatory if IDA_LD has value.", !ediIdentityCertificateWithLicense.ICE_CARootInfo.HasErrors());

			Assert("IDA_LD has no value.", ediIdentityCertificateWithoutLicense.Application.IDA_LD.IsEmpty);
			ediIdentityCertificateWithoutLicense.Validation.ValidateICE_CARoot();
			Assert("caroot is mandatory if IDA_LD has no value.", ediIdentityCertificateWithoutLicense.ICE_CARootInfo.HasErrors());
			Assert(ediIdentityCertificateWithoutLicense.ICE_CARootInfo.GetErrors().Contains("Please enter an AWS Issuing CA."));

			var caList = new AWSPrivateCACollection();
			caList.Add(new AWSPrivateCA() { IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust, Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:123123123", IsEnabled = true, AccessKey = "Test01", SecretKey = "Test02" });
			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, caList))
			{
				ediIdentityCertificateWithoutLicense.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
				Assert(!ediIdentityCertificateWithoutLicense.ICE_CARootInfo.HasErrors());

				ediIdentityCertificateWithoutLicense.ICE_CARoot = "Arn2";
				Assert(ediIdentityCertificateWithoutLicense.ICE_CARootInfo.HasErrors());
				Assert(ediIdentityCertificateWithoutLicense.ICE_CARootInfo.GetErrors().Contains("Enter a valid AWS Issuing CA."));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ediIdentityCertificateWithLicense = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificateWithoutLicense = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificateWithoutLicense.Application.IDA_LD = ZGuid.Empty;
		}

		EdiIdentityCertificate ediIdentityCertificateWithLicense;
		EdiIdentityCertificate ediIdentityCertificateWithoutLicense;
	}
}
