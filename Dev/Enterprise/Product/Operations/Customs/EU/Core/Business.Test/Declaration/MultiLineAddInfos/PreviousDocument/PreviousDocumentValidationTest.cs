using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		protected readonly string cSI_SubTypeEmpty = "Please enter a Class.";
		public virtual void TestClass()
		{
			PreviousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			AssertNoMessageError(PreviousDocument.CSI_SubTypeInfo, cSI_SubTypeEmpty);

			PreviousDocument.CSI_SubType = ZString.Empty;
			AssertHasMessageError(PreviousDocument.CSI_SubTypeInfo, cSI_SubTypeEmpty);

			PreviousDocument.CSI_SubType = "A";
			AssertHasMessageErrorContaining(PreviousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestTypeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previousDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
			helper.CreateNewOrGetExistingCusCodeType(previousDocumentType, "Test Export");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, previousDocumentType, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			Factory.Save();

			PreviousDocument.CSI_Code = ZString.Empty;
			AssertHasNotifications("You have not entered a Type.", PreviousDocument.CSI_CodeInfo);
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = "AAA";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = "ABC";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestDateOfIssue()
		{
			PreviousDocument.CSI_Code = "CLE";
			AssertHasMessageErrorContaining(PreviousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			PreviousDocument.CSI_Code = "10";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			PreviousDocument.CSI_Code = "CLE";
			PreviousDocument.CSI_DateOfIssue = ZDateTime.Now;
			AssertNoMessageErrorContaining(PreviousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation
		PreviousDocument PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					previousDocument = Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.AddNew();
				}
				return previousDocument;
			}
		}
		PreviousDocument previousDocument;
		#endregion
	}
}
