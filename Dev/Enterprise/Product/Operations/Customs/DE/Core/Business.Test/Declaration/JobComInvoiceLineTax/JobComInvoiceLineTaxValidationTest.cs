using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class JobComInvoiceLineTaxValidationTest : TestCaseWithFactory
	{
		public void TestNoMethodOfPaymentValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invLine = declaration.InvoiceLines.AddNew();

			var taxLine1 = invLine.Taxes.AddNew();
			taxLine1.JLT_Type = SpecialCaseGroupList.Codes._01;
			AssertNoMessageErrors(taxLine1.JLT_MethodOfPaymentInfo);

			var taxLine2 = invLine.Taxes.AddNew();
			taxLine2.JLT_Type = SpecialCaseGroupList.Codes._01;
			taxLine1.Validation.ValidateAll();
			AssertNoMessageErrors(taxLine1.JLT_MethodOfPaymentInfo);
			AssertNoMessageErrors(taxLine2.JLT_MethodOfPaymentInfo);
		}

		public void TestCheckJLT_Type()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invLine = declaration.InvoiceLines.AddNew();
			var taxLine1 = invLine.Taxes.AddNew();
			var info1 = taxLine1.JLT_TypeInfo;
			taxLine1.JLT_Type = ZString.Empty;
			AssertHasMessageErrorContaining(info1, MandatoryValidation.YouHaveNotEntered);

			taxLine1.JLT_Type = SpecialCaseGroupList.Codes._01;
			AssertNoMessageErrorContaining(info1, MandatoryValidation.YouHaveNotEntered);

			var taxLine2 = invLine.Taxes.AddNew();
			var info2 = taxLine2.JLT_TypeInfo;
			taxLine2.JLT_Type = SpecialCaseGroupList.Codes._01;

			taxLine1.Validation.ValidateAll();
			AssertHasMessageError(info1, "A row with this Group already exists");
			AssertHasMessageError(info2, "A row with this Group already exists");
		}

		public void TestJLT_MethodOfCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invLine = header.InvoiceLines.AddNew();
			var taxLine = invLine.Taxes.AddNew();
			var info = taxLine.JLT_MethodOfCalculationInfo;

			taxLine.JLT_MethodOfCalculation = ZString.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._01;
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			CombineAssertions("Testing 06 Permitted", () =>
				{
					taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
					invLine.JI_Procedure = "XX31";
					taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._06;
					taxLine.Validation.ValidateJLT_MethodOfCalculation();
					AssertNoMessageErrors(info);

					//Invalid Group (JLT_Type)
					taxLine.JLT_Type = SpecialCaseGroupList.Codes._37;
					invLine.JI_Procedure = "XX31";
					taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._06;
					taxLine.Validation.ValidateJLT_MethodOfCalculation();
					AssertHasMessageErrorContaining(info, "This Previous Procedure and Group combination is not permitted for Type");

					//Invalid Procedure
					taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
					invLine.JI_Procedure = "XX25";
					taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._06;
					taxLine.Validation.ValidateJLT_MethodOfCalculation();
					AssertHasMessageErrorContaining(info, "This Previous Procedure and Group combination is not permitted for Type");
				});

			CombineAssertions("Testing 07 Permitted", () =>
			{
				//Valid for 07
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
				invLine.JI_Procedure = "XX51";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._07;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertNoMessageErrors(info);

				//Invalid Group (JLT_Type)
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._37;
				invLine.JI_Procedure = "XX51";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._07;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertHasMessageErrorContaining(info, "This Previous Procedure and Group combination is not permitted for Type");

				//Invalid Procedure
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
				invLine.JI_Procedure = "XX31";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._07;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertHasMessageErrorContaining(info, "This Previous Procedure and Group combination is not permitted for Type");
			});

			CombineAssertions("Testing 06 or 07 Mandatory", () =>
			{
				//Valid - Type 6
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
				invLine.JI_Procedure = "4054";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._06;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertNoMessageErrors(info);

				//Valid - Type 7
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
				invLine.JI_Procedure = "4054";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._07;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertNoMessageErrors(info);

				//Invalid - Not 6 or 7
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
				invLine.JI_Procedure = "4054";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._05;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertHasMessageError(info, "Type '06' or '07' is required for this Previous Procedure and Group combination.");

				//Valid - Has a procedure that doesnt require Type (MethodOfCalculation)
				taxLine.JLT_Type = SpecialCaseGroupList.Codes._01;
				invLine.JI_Procedure = "2154";
				taxLine.JLT_MethodOfCalculation = SpecialCaseTypeList.Codes._05;
				taxLine.Validation.ValidateJLT_MethodOfCalculation();
				AssertNoMessageErrors(info);
			});
		}
	}
}
