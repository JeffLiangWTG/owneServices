using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCL_CustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._100;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

			entryLine.CL_CustomsValue = -1;
			AssertHasMessageErrorContaining(entryLine.CL_CustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);

			entryLine.CL_CustomsValue = 0;
			AssertHasMessageErrorContaining(entryLine.CL_CustomsValueInfo, MandatoryValidation.ValueCannotBeZero);

			entryLine.CL_CustomsValue = 1;
			AssertNoMessageErrorContaining(entryLine.CL_CustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._82;
			entryLine.CL_CustomsValue = 0;
			AssertNoMessageErrorContaining(entryLine.CL_CustomsValueInfo, "If Transaction Type is ‘82’, then Customs Value needs to be ‘0’.");

			entryLine.CL_CustomsValue = 1;
			AssertHasMessageErrorContaining(entryLine.CL_CustomsValueInfo, "If Transaction Type is ‘82’, then Customs Value needs to be ‘0’.");

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			entryLine.CL_CustomsValue = 0;
			AssertHasMessageErrorContaining(entryLine.CL_CustomsValueInfo, MandatoryValidation.ValueCannotBeZero);

			entryLine.CL_CustomsValue = 1;
			AssertNoMessageErrorContaining(entryLine.CL_CustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);
		}
	}
}
