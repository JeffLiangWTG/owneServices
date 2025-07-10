using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	class EMCSDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Description_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(document.CSI_DescriptionInfo);

			document.CSI_SubType = "xyz";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(document.CSI_DescriptionInfo);

			document.CSI_SubType = string.Empty;
			document.CSI_ReferenceNumber = "abc";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(document.CSI_DescriptionInfo);

			document.CSI_SubType = "xyz";
			document.CSI_ReferenceNumber = "abc";
			ValidationTestHelper.AssertFieldIsNotMandatory(document.CSI_DescriptionInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			document = (EMCSDocument)declaration.Documents.AddNew();
		}
		EMCSJobDeclaration declaration;
		EMCSDocument document;
	}
}
