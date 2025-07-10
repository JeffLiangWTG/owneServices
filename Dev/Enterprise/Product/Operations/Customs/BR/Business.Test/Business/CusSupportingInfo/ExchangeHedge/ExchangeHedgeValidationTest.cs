using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ExchangeHedgeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_Code()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(exchangeHedge.CSI_CodeInfo, "8", "1");

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			exchangeHedge.CSI_Code = ZString.Empty;
			AssertNoMessageErrorContaining(exchangeHedge.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			exchangeHedge.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining(exchangeHedge.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
			AssertNoMessageErrorContaining(exchangeHedge.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			exchangeHedge.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining(exchangeHedge.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCSI_AdditionalDescription()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._4;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(exchangeHedge.CSI_AdditionalDescriptionInfo, "X", "99");

			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
			AssertNoMessageErrorContaining(exchangeHedge.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCSI_IssuerType()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._3;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(exchangeHedge.CSI_IssuerTypeInfo, "X", "99");

			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
			AssertNoMessageErrorContaining(exchangeHedge.CSI_IssuerTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_SubType()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(exchangeHedge.CSI_SubTypeInfo, "X", "99");

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertNoMessageErrorContaining(exchangeHedge.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_Value()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._3;
			ValidationTestHelper.AssertErrorIfValueIsNegative(exchangeHedge.CSI_ValueInfo);
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(exchangeHedge.CSI_ValueInfo);

			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._2;
			exchangeHedge.CSI_Value = 0m;
			AssertNoErrorContaining(exchangeHedge.CSI_ValueInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(exchangeHedge.CSI_ValueInfo, MandatoryValidation.ValueCannotBeZero);

			exchangeHedge.CSI_Value = 12345678908654.12m;
			AssertHasErrorContaining(exchangeHedge.CSI_ValueInfo, "too large");

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._3;
			exchangeHedge.CSI_Value = 0m;
			AssertNoErrorContaining(exchangeHedge.CSI_ValueInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(exchangeHedge.CSI_ValueInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions("ImportSiscomex", () =>
			{
				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
				ValidationTestHelper.AssertFieldIsNotMandatory(exchangeHedge.CSI_ReferenceNumberInfo);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._2;
				ValidationTestHelper.AssertFieldIsNotMandatory(exchangeHedge.CSI_ReferenceNumberInfo);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._3;
				ValidationTestHelper.AssertWarningIfNotEntered(exchangeHedge.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._4;
				ValidationTestHelper.AssertWarningIfNotEntered(exchangeHedge.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			CombineAssertions("Import", () =>
			{
				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
				ValidationTestHelper.AssertFieldIsNotMandatory(exchangeHedge.CSI_ReferenceNumberInfo);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._2;
				ValidationTestHelper.AssertFieldIsNotMandatory(exchangeHedge.CSI_ReferenceNumberInfo);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._3;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exchangeHedge.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._4;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(exchangeHedge.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});

			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			CombineAssertions("ImportLicense", () =>
			{
				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._3;
				ValidationTestHelper.AssertFieldIsNotMandatory(exchangeHedge.CSI_ReferenceNumberInfo);

				exchangeHedge.CSI_Code = ExchangeHedgeList.Codes._4;
				ValidationTestHelper.AssertFieldIsNotMandatory(exchangeHedge.CSI_ReferenceNumberInfo);
			});
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceHeader invoice;
		ExchangeHedge exchangeHedge;

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoice = jobDeclaration.Invoices.AddNew();
			exchangeHedge = invoice.ExchangeHedge;
			ReferenceTestDataHelper.CreateRefCusCodeListMISCC_BNKTestData(Factory);
		}
	}
}
