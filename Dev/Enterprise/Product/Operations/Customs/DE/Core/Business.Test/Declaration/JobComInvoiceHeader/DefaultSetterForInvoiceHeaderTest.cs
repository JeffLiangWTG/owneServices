using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestDefaultForNewElementCore_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var orgHeader = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				var invoice1 = declaration.Invoices.AddNew();
				AssertEquals("invoice1.ConsigneeOrgPK", ZGuid.Empty, invoice1.ConsigneeOrgPK);
				AssertEquals("invoice1.JZ_OA_ConsigneeAddress", ZGuid.Empty, invoice1.JZ_OA_ConsigneeAddress);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				var invoice2 = declaration.Invoices.AddNew();
				AssertEquals("invoice2.ConsigneeOrgPK", orgHeader.PK, invoice2.ConsigneeOrgPK);
				AssertEquals("invoice2.JZ_OA_ConsigneeAddress", orgHeader.MainAddress.PK, invoice2.JZ_OA_ConsigneeAddress);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Invalid;
				var invoice3 = declaration.Invoices.AddNew();
				AssertEquals("invoice3.ConsigneeOrgPK", ZGuid.Empty, invoice3.ConsigneeOrgPK);
				AssertEquals("invoice3.JZ_OA_ConsigneeAddress", ZGuid.Empty, invoice3.JZ_OA_ConsigneeAddress);
			});
		}

		public void TestDefaultForNewElementCore_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var orgHeader = Factory.New<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("ConsigneeOrgPK", ZGuid.Empty, invoice.ConsigneeOrgPK);
				AssertEquals("JZ_OA_ConsigneeAddress", ZGuid.Empty, invoice.JZ_OA_ConsigneeAddress);
			});
		}
	}
}
