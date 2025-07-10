using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(QuantityPerUnitInfoValidation))]
	public class QuantityPerUnitInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_UnitOfQuantity()
		{
			var refPackType = Factory.New<RefPackType>();
			refPackType.F3_Code = "XX";
			refPackType.F3_Description = "Test";

			Factory.Save();

			var refLanguageText = Factory.New<RefLanguageText>();
			refLanguageText.RLT_ParentId = refPackType.PK;
			refLanguageText.RLT_Text = "Test";
			refLanguageText.RLT_ColumnName = JobComInvoiceLine.Schema.JI_InvoiceUQ;
			refLanguageText.RLT_ParentTableCode = "RL";
			refLanguageText.RLT_Language = "PT-BR";
			refLanguageText.RLT_IsSystem = true;

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.QuantityPerUnitInfos.AddNew(Constants.RateCodes.Antidumping);
			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			specialCaseTax.UnitOfMeasure = "XX";
			var multilingualDescription = RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory).GetMultilingualDescriptionFromCode(specialCaseTax.UnitOfMeasure);
			var ptBRDescription = multilingualDescription?.ToString("PT-BR");
			AssertEquals("Test", ptBRDescription);
			AssertHasMessageError(specialCaseTax.UnitOfMeasureInfo, "Unity Of Measure- UQ Portuguese description is the same as English Description");

			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			specialCaseTax.UnitOfMeasure = "XX";
			AssertNoMessageErrors(specialCaseTax.UnitOfMeasureInfo);
		}
	}
}
