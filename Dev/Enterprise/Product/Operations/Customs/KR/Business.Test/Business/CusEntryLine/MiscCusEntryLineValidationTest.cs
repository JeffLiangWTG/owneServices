using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MiscCusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCL_CustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			declaration.JE_ExportGoodsType = YesNoList.Codes.Yes;

			var invoice = declaration.Invoices[0];
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(true, declaration.CustomsEntryHeaders[0].IsMisc);

			var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];

			entryLine.CL_CustomsValue = -1;
			AssertNoMessageErrors(entryLine.CL_CustomsValueInfo);

			entryLine.CL_CustomsValue = 0;
			AssertNoMessageErrors(entryLine.CL_CustomsValueInfo);
		}
	}
}
