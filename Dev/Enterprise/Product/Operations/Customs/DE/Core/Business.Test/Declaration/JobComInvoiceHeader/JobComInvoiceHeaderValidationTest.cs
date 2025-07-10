using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class JobComInvoiceHeaderValidationTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_ValuationCode_MandatoryValidation()
		{
			invoiceHeader.JZ_ValuationCode = "";
			AssertNoMessageErrors("IsJZ_ValuationCodeMandatory is false and has additional implementation for Export", invoiceHeader.JZ_ValuationCodeInfo);
		}

		public void TestCheckJZ_Weight()
		{
			invoiceHeader.JZ_Weight = -1;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_WeightInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceHeader.JZ_Weight = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJZ_WeightUQ()
		{
			AssertNoMessageErrors(invoiceHeader.JZ_WeightUQInfo);
			invoiceHeader.JZ_WeightUQ = "$";
			AssertHasMessageError(invoiceHeader.JZ_WeightUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageError(invoiceHeader.JZ_WeightUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_Weight = 123.456m;
			invoiceHeader.JZ_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Ounces;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJZ_NetWeight()
		{
			string warning = "The Net Weight should not be greater than the Gross Weight.";
			invoiceHeader.JZ_NetWeight = -1;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceHeader.JZ_NetWeight = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceHeader.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;

			invoiceHeader.JZ_Weight = 10m;
			invoiceHeader.JZ_NetWeight = 11m;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 11m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceHeader.Validation.ValidateJZ_NetWeight();

			AssertHasWarningContaining(invoiceHeader.JZ_NetWeightInfo, warning);

			invoiceHeader.JZ_NetWeight = 9m;
			AssertNoMessageErrors(invoiceHeader.JZ_NetWeightInfo);
		}

		public void TestJZ_NetWeightUQ()
		{
			AssertNoMessageErrors(invoiceHeader.JZ_NetWeightUQInfo);
			invoiceHeader.JZ_NetWeightUQ = "$";
			AssertHasMessageError(invoiceHeader.JZ_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageError(invoiceHeader.JZ_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_NetWeight = 99.99m;
			invoiceHeader.JZ_NetWeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_NetWeightUQ = Core.Constants.Weight.LongTons;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestRequireJZ_IncoTermPlaceMandatory()
		{
			if (JZ_IncoTermPlaceIsMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_IncoTermPlaceInfo);
			}
			else
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_IncoTermPlaceInfo);
			}
		}

		protected virtual ZBool JZ_IncoTermPlaceIsMandatory => ZBool.False;

		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		protected override void SetUp()
		{
			base.SetUp();
			base.declaration.JE_MessageType = MessageType;
		}

		protected virtual string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;
		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;
		protected new JobComInvoiceHeader invoiceHeader => (JobComInvoiceHeader)base.invoiceHeader;
	}
}
