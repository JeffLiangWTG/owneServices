using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusStorageDocPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2019, 3, 6)]
		public void TestAttachmentTypeList()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string natyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(natyp, "NATYP Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY1", "1", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY2", "2", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY3", "1", date1, date2);
			const string coldt = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AUCOLSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(coldt, "COLDT Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", coldt, "CT1", "ct1 desc", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", coldt, "CT2", "ct2 desc", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", coldt, "CT3", "ct3 desc", date1, date2);
			Factory.Save();

			var pivot = Factory.New<CusStorageDocPivot>();
			AssertEquals("TY1, TY2, TY3", pivot.Lookups.AttachmentTypeList.CodesAsString);

			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			pivot.Parent = quarantineColsHeader;
			AssertEquals("CT1, CT2, CT3", pivot.Lookups.AttachmentTypeList.CodesAsString);
		}

		public void TestCOLSDocumentStatusList()
		{
			var pivot = Factory.New<CusStorageDocPivot>();
			AssertNull("COLSDocumentStatusList should be null when parent is not QuarantineColsHeader", pivot.Lookups.COLSDocumentStatusList);

			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			pivot.Parent = quarantineColsHeader;
			AssertNotNull("COLSDocumentStatusList should not be null when parent is QuarantineColsHeader", pivot.Lookups.COLSDocumentStatusList);

			var list = pivot.Lookups.COLSDocumentStatusList;
			AssertType<COLSDocumentStatusList>("List Type", list);
			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, pivot.Lookups.COLSDocumentStatusList);
		}

		public void TestAvailableEDocs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = (entry as MasterFiles.Business.IDocManagerSupport).DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment.pdf", "CIV");

			var pivot = invoiceLine.EDocPivotCollection.AddNew();
			AssertEquals(3, pivot.Lookups.AvailableEDocs.Count);
			AssertEquals(eDoc2.UniqueKey, pivot.Lookups.AvailableEDocs[0].PK);
			AssertEquals(eDoc1.UniqueKey, pivot.Lookups.AvailableEDocs[1].PK);
			AssertEquals(eDoc3.UniqueKey, pivot.Lookups.AvailableEDocs[2].PK);
		}
	}
}
