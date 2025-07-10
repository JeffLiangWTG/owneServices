using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CusEntryInstructionPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeType = helper.CreateCusCodeType("ENSTY", "IL Entry Style");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeType.ZZK_CodeType, "1", ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeType.ZZK_CodeType, "2", ZDateTime.BrettsBirthday, ZDateTime.Today);
			factory.Save();

			PreviousDocument.CSI_Code = ZString.Empty;
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = "BAD";
			AssertHasMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			PreviousDocument.CSI_Code = "1";
			AssertNoMessageErrorContaining(PreviousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		PreviousDocument PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					var jobDeclaration = Factory.New<JobDeclaration>();
					var cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
					previousDocument = cusEntryInstruction.PreviousDocuments.AddNew();
				}
				return previousDocument;
			}
		}

		PreviousDocument previousDocument;

		#endregion
	}
}
