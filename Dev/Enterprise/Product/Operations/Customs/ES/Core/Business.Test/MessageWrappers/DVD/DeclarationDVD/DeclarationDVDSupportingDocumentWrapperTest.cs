using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDSupportingDocumentWrapperTest : WrapperHelperTest<DeclarationDVDSupportingDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new DeclarationDVDSupportingDocumentWrapper(null));
		}

		public void TestEUCode()
		{
			CombineAssertions(() =>
			{
				document.CSI_Code = "A123";
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled EUCode when first char is a letter", "A123", wrapper.EUCode);

				document.CSI_Code = "1ABC";
				wrapper = GetWrapper(document);
				AssertEquals("Expected empty EUCode when first char is a number", ZString.Empty, wrapper.EUCode);
			});
		}

		public void TestNationalCode()
		{
			CombineAssertions(() =>
			{
				document.CSI_Code = "1ABC";
				wrapper = GetWrapper(document);
				AssertEquals("Expected filled NationalCode when first char is a number", "1ABC", wrapper.NationalCode);

				document.CSI_Code = "A123";
				wrapper = GetWrapper(document);
				AssertEquals("Expected empty NationalCode when first char is a letter", ZString.Empty, wrapper.NationalCode);
			});
		}

		public void TestNumber()
		{
			document.CSI_ReferenceNumber = "reference";
			AssertEquals("Expected filled Number", "reference", wrapper.Number);
		}

		public void TestDocumentDate()
		{
			var issueDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
			var expiryDate = new ZDateTime(2022, 10, 20, 15, 50, 00);

			CombineAssertions(() =>
			{
				document.CSI_DateOfIssue = ZDateTime.Empty;
				AssertEquals("Expected empty DocumentDate when both Issue and Expiry dates are empty", ZDateTime.Empty, wrapper.DocumentDate);

				document.CSI_DateOfIssue = issueDate;
				document.CSI_DateOfExpiry = ZDateTime.Empty;
				AssertEquals("Expected filled DocumentDate with Issue date when Expiry date is empty", issueDate, wrapper.DocumentDate);

				document.CSI_DateOfExpiry = expiryDate;
				AssertEquals("Expected filled DocumentDate with Expiry date when it's not empty", expiryDate, wrapper.DocumentDate);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<SupportingDocument>();
			wrapper = GetWrapper(document);
		}

		SupportingDocument document;
		DeclarationDVDSupportingDocumentWrapper wrapper;

		DeclarationDVDSupportingDocumentWrapper GetWrapper(SupportingDocument doc) => new DeclarationDVDSupportingDocumentWrapper(doc);

		protected override DeclarationDVDSupportingDocumentWrapper GetProvider() => wrapper;
	}
}
