using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILEDIInterchange))]
	sealed class ILEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<ILEDIInterchange>();
			AssertEquals("ILC", interchange.EI_ApplicationCode);
		}

		public void TestIEDIInterchange()
		{
			Assert(Factory.New<ILEDIInterchange>() is Integration.Customs.IL.IEDIInterchange);
		}

		public void TestGetMessageAttrDictionary_ReturnsNoneSignature_WhenCredentialIsNull()
		{
			var interchange = Factory.New<ILEDIInterchange>();
			var messageAttrDictionary = ((IxTMessageAttributeProvider)interchange).GetMessageAttrDictionary();

			AssertNotNull(messageAttrDictionary);
			AssertNotNull(messageAttrDictionary[MessageSignatureProperty.SignatureType]);
			AssertEquals(string.Empty, messageAttrDictionary[MessageSignatureProperty.SignatureType]);
		}

		public void TestGetMessageAttrDictionary_ReturnsNoneSignature_WhenCredentialFieldsAreEmpty()
		{
			var factory = Factory;
			var interchange = factory.New<ILEDIInterchange>();
			var credential = factory.New<GlbILStaffExternalPassword>();
			credential.GP_UserID = "BAD";
			interchange.EI_GP = credential.PK;
			var messageAttrDictionary = ((IxTMessageAttributeProvider)interchange).GetMessageAttrDictionary();

			var result = ((IxTMessageAttributeProvider)interchange).GetMessageAttrDictionary();

			AssertNotNull(messageAttrDictionary);
			AssertNotNull(messageAttrDictionary[MessageSignatureProperty.SignatureType]);
			AssertEquals(string.Empty, messageAttrDictionary[MessageSignatureProperty.SignatureType]);
		}

		public void TestGetMessageAttrDictionary_ReturnsAttributes_WhenCredentialIsValid()
		{
			var factory = Factory;
			var interchange = factory.New<ILEDIInterchange>();
			var credential = factory.New<GlbILStaffExternalPassword>();
			credential.GP_UserID = "OK";
			credential.GP_CertificateAuthority = "auth1";
			credential.CurrentDecryptedPassword = "pass1";

			interchange.EI_GP = credential.PK;
			var messageAttrDictionary = ((IxTMessageAttributeProvider)interchange).GetMessageAttrDictionary();

			var result = ((IxTMessageAttributeProvider)interchange).GetMessageAttrDictionary();
			AssertNotNull(messageAttrDictionary);
			AssertNotNull(messageAttrDictionary[MessageSignatureProperty.SignatureType]);

			AssertEquals("auth1", result[MessageSignatureProperty.SignatureType]);
			AssertEquals("OK", result[MessageSignatureProperty.SignatureUser]);
			AssertNotEquals("pass1", result[MessageSignatureProperty.SignaturePIN]);
		}
	}
}
