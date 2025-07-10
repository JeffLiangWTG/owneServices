using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class CompletionCustomsOfficeValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data_MandatoryValidation()
		{
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			office.Validation.ValidateCY_Data();
			NUnit.Framework.Assert.Multiple(() =>
			{
				AssertHasMessageErrorContaining("Not entered", office.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

				office.CY_Data = "XXX";
				AssertNoMessageErrorContaining("Entered", office.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCY_Data_ListValidation()
		{
			office.CY_Data = "XXX";
			NUnit.Framework.Assert.Multiple(() =>
			{
				AssertHasMessageError(office.CY_DataInfo, ListValidation.InvalidCodeMessageError);

				office.CY_Data = "DE000001";
				AssertNoMessageError(office.CY_DataInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "DE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			instruction = Factory.CreateInwardProcessingInstruction();
			office = instruction.CompletionCustomsOffices.AddNew();
		}
		CusEntryInstruction instruction;
		CompletionCustomsOffice office;
	}
}
