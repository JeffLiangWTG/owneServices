using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ValuationDeclarationJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestCheckValuationQuestion5A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion5AInfo, validation.ValidateValuationQuestion5A);
		}

		public void TestCheckValuationQuestion5B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion5Series(invoice, invoice.ValuationQuestion5BInfo, validation.ValidateValuationQuestion5B, SpecialRelationshipCodeList.Codes._01);
		}

		public void TestCheckValuationQuestion5C()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion5Series(invoice, invoice.ValuationQuestion5CInfo, validation.ValidateValuationQuestion5C, YesNoList.Codes.Yes);
		}

		public void TestCheckValuationQuestion5D()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion5Series(invoice, invoice.ValuationQuestion5DInfo, validation.ValidateValuationQuestion5D, YesNoList.Codes.Yes);
		}

		public void TestCheckValuationQuestion5EA()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion5Series(invoice, invoice.ValuationQuestion5EAInfo, validation.ValidateValuationQuestion5EA, PricingCodeList.Codes._01);
		}

		public void TestCheckValuationQuestion5EB()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			validation.ValidateValuationQuestion5EB();
			AssertNoNotifications(invoice.ValuationQuestion5EBInfo);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			validation.ValidateValuationQuestion5EB();
			AssertNoNotifications(invoice.ValuationQuestion5EBInfo);

			invoice.ValuationQuestion5A = YesNoList.Codes.Yes;
			validation.ValidateValuationQuestion5EB();
			AssertNoNotifications(invoice.ValuationQuestion5EBInfo);

			invoice.ValuationQuestion5EA = PricingCodeList.Codes._01;
			validation.ValidateValuationQuestion5EB();
			AssertNoNotifications(invoice.ValuationQuestion5EBInfo);

			invoice.ValuationQuestion5EA = PricingCodeList.Codes._99;
			validation.ValidateValuationQuestion5EB();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion5EBInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ValuationQuestion5EB = "OTHER";
			validation.ValidateValuationQuestion5EB();
			AssertNoNotifications(invoice.ValuationQuestion5EBInfo);

			invoice.ValuationQuestion5EB = "";
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			validation.ValidateValuationQuestion5EB();
			AssertNoNotifications(invoice.ValuationQuestion5EBInfo);
		}

		public void TestCheckValuationQuestion6A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion6AInfo, validation.ValidateValuationQuestion6A);
		}

		public void TestCheckValuationQuestion6B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion6BInfo, validation.ValidateValuationQuestion6B);
		}

		public void TestCheckValuationQuestion7A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion7A_5SMInfo, validation.ValidateValuationQuestion7A_5SM);
		}

		public void TestCheckValuationQuestion7B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion7B_5SMInfo, validation.ValidateValuationQuestion7B_5SM);
		}

		public void TestCheckValuationQuestion8A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion8AInfo, validation.ValidateValuationQuestion8A);
		}

		public void TestCheckValuationQuestion8B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion8BInfo, validation.ValidateValuationQuestion8B);
		}

		public void TestCheckValuationQuestion8C()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion8CInfo, validation.ValidateValuationQuestion8C);
		}

		public void TestCheckValuationQuestion8D()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion8DInfo, validation.ValidateValuationQuestion8D);
		}

		public void TestCheckValuationQuestion9A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion9AInfo, validation.ValidateValuationQuestion9A);
		}

		public void TestCheckValuationQuestion9B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion9BInfo, validation.ValidateValuationQuestion9B);
		}

		public void TestCheckValuationQuestion10A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion10AInfo, validation.ValidateValuationQuestion10A);
		}

		public void TestCheckValuationQuestion10B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion10BInfo, validation.ValidateValuationQuestion10B);
		}

		public void TestCheckValuationQuestion10C()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion10CInfo, validation.ValidateValuationQuestion10C);
		}

		public void TestCheckValuationQuestion10D()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion10DInfo, validation.ValidateValuationQuestion10D);
		}

		public void TestCheckValuationQuestion11A()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion11AInfo, validation.ValidateValuationQuestion11A);
		}

		public void TestCheckValuationQuestion11B()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion11BInfo, validation.ValidateValuationQuestion11B);
		}

		public void TestCheckValuationQuestion11C()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion11CInfo, validation.ValidateValuationQuestion11C);
		}

		public void TestCheckValuationQuestion11D()
		{
			var invoice = GetInvoiceHeaderForValuationQuestionTest();
			var validation = invoice.Validation as ValuationDeclarationJobComInvoiceHeaderValidation;
			AssertValuationQuestion(invoice, invoice.ValuationQuestion11DInfo, validation.ValidateValuationQuestion11D);
		}

		JobComInvoiceHeader GetInvoiceHeaderForValuationQuestionTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			return declaration.Invoices.AddNew();
		}

		void AssertValuationQuestion(JobComInvoiceHeader invoice, ZPropertyInfo info, Action validationAction)
		{
			validationAction.Invoke();
			AssertNoNotifications(info);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			validationAction.Invoke();
			AssertNoNotifications(info);

			info.SetValueFromString(YesNoList.Codes.Yes);
			validationAction.Invoke();
			AssertNoNotifications(info);

			info.SetValueFromString("");
			validationAction.Invoke();
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			info.SetValueFromString("A");
			validationAction.Invoke();
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			validationAction.Invoke();
			AssertNoNotifications(info);
		}

		void AssertValuationQuestion5Series(JobComInvoiceHeader invoice, ZPropertyInfo info, Action validationAction, ZString correctValue)
		{
			validationAction.Invoke();
			AssertNoNotifications(info);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			validationAction.Invoke();
			AssertNoNotifications(info);

			invoice.ValuationQuestion5A = YesNoList.Codes.Yes;
			info.SetValueFromString("");
			validationAction.Invoke();
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			info.SetValueFromString(correctValue);
			validationAction.Invoke();
			AssertNoNotifications(info);

			info.SetValueFromString("A");
			validationAction.Invoke();
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			invoice.ValuationQuestion5A = YesNoList.Codes.No;
			validationAction.Invoke();
			AssertNoNotifications(info);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			validationAction.Invoke();
			AssertNoNotifications(info);
		}
	}
}
