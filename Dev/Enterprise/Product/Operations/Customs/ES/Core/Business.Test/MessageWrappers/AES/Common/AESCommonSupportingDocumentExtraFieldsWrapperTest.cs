using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers.AES.Common;

namespace Enterprise.Customs.ES.Business.Testing
{
	internal class AESCommonSupportingDocumentExtraFieldsWrapperTest : WrapperHelperTest<AESCommonSupportingDocumentExtraFieldsWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new AESCommonSupportingDocumentExtraFieldsWrapper(null));
		}
		public void TestIssuingAuthorityName()
		{
			document.CSI_AdditionalDescription = "Authority";
			AssertEquals("Expected filled IssuingAuthorityName", "Authority", wrapper.IssuingAuthorityName);
		}

		public void TestDocumentDate()
		{
			CombineAssertions(() =>
			{
				document.CSI_DateOfIssue = new ZDateTime(2022, 12, 10, 10, 40, 35);
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled DocumentDate with DateOfIssue", new DateTime(2022, 12, 10, 10, 40, 35), wrapper.DocumentDate);

				document.CSI_DateOfExpiry = new ZDateTime(2023, 04, 10, 18, 40, 35);
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled DocumentDate with DateOfExpiry (even when dateOfIssue is declared)", new DateTime(2023, 04, 10, 18, 40, 35), wrapper.DocumentDate);
			});
		}

		public void TestDocumentDateSpecified()
		{
			CombineAssertions(() =>
			{
				document.CSI_DateOfIssue = new ZDateTime(2022, 12, 10, 10, 40, 35);
				wrapper = GetWrapper(document);
				AssertEquals("Expected true DocumentDateSpecified when DocumentDate declared", true, wrapper.DocumentDateSpecified);

				document.CSI_DateOfIssue = ZDateTime.Empty;
				wrapper = GetWrapper(document);
				AssertEquals("Expected false DocumentDateSpecified when DocumentDate is not declared", false, wrapper.DocumentDateSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<SupportingDocument>();
			wrapper = GetWrapper(document);
		}

		SupportingDocument document;
		AESCommonSupportingDocumentExtraFieldsWrapper wrapper;

		AESCommonSupportingDocumentExtraFieldsWrapper GetWrapper(SupportingDocument doc) => new AESCommonSupportingDocumentExtraFieldsWrapper(doc);

		protected override AESCommonSupportingDocumentExtraFieldsWrapper GetProvider() => wrapper;
	}
}
