using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public sealed class AttributeCusCodeDataValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var attribute = invoiceLine.Attributes.AddNew();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();

			var tariffCharacteristic = helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "AA");
			tariffCharacteristic.ZB1_MaxLength = 10;
			tariffCharacteristic.ZB1_IsMandatory = true;
			Factory.Save();

			attribute.TariffProfileQuestion = TariffProfileQuestion.New(tariffCharacteristic);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(attribute.CY_DataInfo);

			tariffCharacteristic.ZB1_IsMandatory = false;
			attribute.TariffProfileQuestion = TariffProfileQuestion.New(tariffCharacteristic);
			ValidationTestHelper.AssertFieldIsNotMandatory(attribute.CY_DataInfo);

			tariffCharacteristic.ZB1_IsMandatory = true;
			tariffCharacteristic.ZB1_Style = Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound;
			attribute.TariffProfileQuestion = TariffProfileQuestion.New(tariffCharacteristic);
			ValidationTestHelper.AssertFieldIsNotMandatory(attribute.CY_DataInfo);

			tariffCharacteristic.ZB1_Style = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			attribute.TariffProfileQuestion = TariffProfileQuestion.New(tariffCharacteristic);
			attribute.CY_Data = "1234567890A";
			AssertHasMessageError(attribute.CY_DataInfo, "Content exceeded the max size " + attribute.MaxSize);
			attribute.CY_Data = "1234567890";
			AssertNoMessageError(attribute.CY_DataInfo, "Content exceeded the max size " + attribute.MaxSize);
			AssertNoWarning(attribute.CY_DataInfo, "This attribute has its effective date set to Start Date as determined by Customs and will remain unsent in messages until its effective date.");

			tariffCharacteristic.ZB1_StartDate = DateTime.Now.AddDays(10);
			attribute.TariffProfileQuestion = TariffProfileQuestion.New(tariffCharacteristic);
			attribute.Validation.ValidateCY_Data();
			AssertHasWarning(attribute.CY_DataInfo, "This attribute has its effective date set to Start Date as determined by Customs and will remain unsent in messages until its effective date.");
		}
	}
}
