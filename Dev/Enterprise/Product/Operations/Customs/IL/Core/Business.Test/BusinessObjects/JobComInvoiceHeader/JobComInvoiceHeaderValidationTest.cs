using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestBaseClass()
		{
			AssertEquals(typeof(AutoILJobComInvoiceHeaderValidation), typeof(JobComInvoiceHeaderValidation).BaseType);
		}

		public void TestCheckJZ_InvoiceType()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceType = "1";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceTypeInfo, "not in the list");

			invoiceHeader.JZ_InvoiceType = "325";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceTypeInfo, "not in the list");
		}

		public void TestCheckJZ_PreferenceDocumentType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateTradeGroup(Core.Constants.CountryCodes.Israel, "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_PreferenceDocumentType = "1";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_PreferenceDocumentTypeInfo, "not in the list");

			invoiceHeader.JZ_PreferenceDocumentType = "CN";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_PreferenceDocumentTypeInfo, "not in the list");
		}

		public void TestCheckJZ_IncoTermPlace()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_IncoTermPlace = "1";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, "not in the list");

			invoiceHeader.JZ_IncoTermPlace = "CN";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_IncoTermPlaceInfo, "not in the list");
		}

		public void TestCheckJZ_PaymentTerms()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_PaymentTerms = "0";
			AssertHasMessageErrorContaining(invoiceHeader.JZ_PaymentTermsInfo, "not in the list");

			invoiceHeader.JZ_PaymentTerms = "1";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_PaymentTermsInfo, "not in the list");
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceAmount = 0;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceAmountInfo, "cannot be zero");

			invoiceHeader.JZ_InvoiceAmount = 1;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceAmountInfo, "cannot be zero");
		}

		public void TestCheckJZ_OH_Supplier_SupplierCustomsNumberIsMissing()
		{
			const string errorMessage = "Supplier Customs Number (CSC) is missing, please review Organization Config Tab";
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceType = InvoiceTypeList.Codes._380;
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressShippingAgent = supplier.MainAddress;
			supplier.OH_Code = "SUP";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining("When Import declaration and InvoiceType is in (325,380) But Supplier is not valid", invoiceHeader.JZ_OH_SupplierInfo, errorMessage);

			invoiceHeader.JZ_InvoiceType = InvoiceTypeList.Codes.I02;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrorContaining("When Import declaration and InvoiceType is in not (325,380) and Supplier is not valid", invoiceHeader.JZ_OH_SupplierInfo, errorMessage);

			invoiceHeader.JZ_InvoiceType = InvoiceTypeList.Codes._380;
			supplier.CustomsCodes.AddNew("CSC", "123", "IL");
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrorContaining("When Import declaration and InvoiceType is in (325,380) and Supplier is valid", invoiceHeader.JZ_OH_SupplierInfo, errorMessage);

			invoiceHeader.JZ_OH_Supplier = Guid.Empty;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining("When Import declaration and InvoiceType is in (325,380) and Supplier is empty", invoiceHeader.JZ_OH_SupplierInfo, errorMessage);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			supplier.CustomsCodes.RemoveAndDeleteAll();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrorContaining("When Export declaration and InvoiceType is in (325,380) and Supplier is empty", invoiceHeader.JZ_OH_SupplierInfo, errorMessage);
		}

		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);
	}
}

