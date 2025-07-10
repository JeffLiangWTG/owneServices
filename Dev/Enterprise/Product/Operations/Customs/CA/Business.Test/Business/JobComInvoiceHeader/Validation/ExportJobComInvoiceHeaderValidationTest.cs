using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			return invoiceHeader;
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			invoiceHeader.JZ_InvoiceNumber = "1234";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceHeader.JZ_InvoiceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJZ_RW_NKOriginState()
		{
			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceHeader.Validation.ValidateJZ_RW_NKOriginState();
			AssertNoMessageErrors(invoiceHeader.JZ_RW_NKOriginStateInfo);
			invoiceHeader.JZ_RW_NKOriginState = "??";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError);
			invoiceHeader.JZ_RW_NKOriginState = CanadianProvinceList.Codes.Ontario;
			AssertNoMessageErrors(invoiceHeader.JZ_RW_NKOriginStateInfo);

			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			invoiceHeader.Validation.ValidateJZ_RW_NKOriginState();
			AssertNoMessageErrors(invoiceHeader.JZ_RW_NKOriginStateInfo);
			invoiceHeader.JZ_RW_NKOriginState = "??";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError);
			invoiceHeader.JZ_RW_NKOriginState = CanadianProvinceList.Codes.Ontario;
			AssertNoMessageErrors(invoiceHeader.JZ_RW_NKOriginStateInfo);

			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			invoiceHeader.Validation.ValidateJZ_RW_NKOriginState();
			AssertNoMessageErrors(invoiceHeader.JZ_RW_NKOriginStateInfo);
			invoiceHeader.JZ_RW_NKOriginState = "??";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_RW_NKOriginStateInfo, ListValidation.InvalidCodeMessageError);
			invoiceHeader.JZ_RW_NKOriginState = CanadianProvinceList.Codes.Ontario;
			AssertNoMessageErrors(invoiceHeader.JZ_RW_NKOriginStateInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}

		protected override Type GetTypeForTest()
		{
			return typeof(ExportJobComInvoiceHeaderValidation);
		}
	}
}
