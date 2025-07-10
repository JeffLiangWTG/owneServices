using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Description()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(document.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(document.CSI_ReferenceNumberInfo);
		}

		public void TestCSI_SubTypeCanBeBlank()
		{
			AssertNoMessageErrors(document.CSI_SubTypeInfo);
		}

		public void TestRowsAreUnique()
		{
			const string messageError = "Description and Reference combination must be unique.";
			CombineAssertions(() =>
			{
				document.CSI_Description = "ABC";
				document.CSI_ReferenceNumber = "123";
				AssertNoMessageErrorContaining("Single Record", document.CSI_DescriptionInfo, messageError);

				var document2 = declaration.Documents.AddNew();
				document2.CSI_Description = "ABC";
				document2.CSI_ReferenceNumber = "123";
				AssertHasMessageErrorContaining("Second Record duplicate", document2.CSI_ReferenceNumberInfo, messageError);

				document2.CSI_ReferenceNumber = "456";
				AssertNoMessageErrorContaining("Second Reference Different", document2.CSI_ReferenceNumberInfo, messageError);

				document.CSI_ReferenceNumber = "456";
				AssertHasMessageErrorContaining("First Record duplicate", document.CSI_ReferenceNumberInfo, messageError);

				document2.CSI_Description = "CDF";
				document2.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("Second Description Different", document2.CSI_ReferenceNumberInfo, messageError);
			});
		}

		public void TestCheckCSI_Description_AllCharactersAllowed()
		{
			document.CSI_Description = "Unicodetest-ÂÜ§$㐿㪳çËŠïÔчШĢøÅşŢǁǂ№™€";
			AssertNoErrors(document.CSI_DescriptionInfo);
		}

		public void TestCheckCheckCSI_ReferenceNumber_AllCharactersAllowed()
		{
			document.CSI_ReferenceNumber = "Unicodetest-ÂÜ§$㐿㪳çËŠïÔчШĢøÅşŢǁǂ№™€";
			AssertNoErrors(document.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_ReferenceNumberMaxLengthExceededWarning()
		{
			var warningText = "exceeds the maximum allowed. Only the first 35 characters will be transmitted to Customs.";
			CombineAssertions(() =>
			{
				document.CSI_ReferenceNumber = ZString.Replicate('x', 36);
				AssertNoWarningContaining(document.CSI_ReferenceNumberInfo, warningText);

				document.CSI_SubType = "0";
				document.Validation.ValidateCSI_ReferenceNumber();
				AssertHasWarningContaining(document.CSI_ReferenceNumberInfo, warningText);

				document.CSI_ReferenceNumber = ZString.Replicate('x', 35);
				AssertNoWarningContaining(document.CSI_ReferenceNumberInfo, warningText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			document = declaration.Documents.AddNew();
		}
		EMCSJobDeclaration declaration;
		EMCSDocument document;
	}
}
