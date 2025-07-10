using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonWrappersHelperTest : TestCaseWithFactory
{
	public void TestWarehouseTypeListContainsCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected WarehouseTypeListContainsCode true when code is CW1", true, CommonWrappersHelper.WarehouseTypeListContainsCode("CW1"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is AAA", false, CommonWrappersHelper.WarehouseTypeListContainsCode("AAA"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is CW2", true, CommonWrappersHelper.WarehouseTypeListContainsCode("CW2"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is ACT", false, CommonWrappersHelper.WarehouseTypeListContainsCode("ACT"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is CWP", true, CommonWrappersHelper.WarehouseTypeListContainsCode("CWP"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is CVA", false, CommonWrappersHelper.WarehouseTypeListContainsCode("CVA"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is TST", true, CommonWrappersHelper.WarehouseTypeListContainsCode("TST"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is OPO", false, CommonWrappersHelper.WarehouseTypeListContainsCode("OPO"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is DDAP", true, CommonWrappersHelper.WarehouseTypeListContainsCode("DDAP"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is REM", false, CommonWrappersHelper.WarehouseTypeListContainsCode("REM"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is DDA1", true, CommonWrappersHelper.WarehouseTypeListContainsCode("DDA1"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is BOI", false, CommonWrappersHelper.WarehouseTypeListContainsCode("BOI"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is DDA2", true, CommonWrappersHelper.WarehouseTypeListContainsCode("DDA2"));

			AssertEquals("Expected WarehouseTypeListContainsCode false when code is BTI", false, CommonWrappersHelper.WarehouseTypeListContainsCode("BTI"));

			AssertEquals("Expected WarehouseTypeListContainsCode true when code is DREF", true, CommonWrappersHelper.WarehouseTypeListContainsCode("DREF"));
		});
	}

	public void TestGetDocumentSequenceNumberWrapperList()
	{
		var doc1 = Factory.New<CusSupportingInfo>();
		doc1.CSI_Code = "A";
		doc1.CSI_ReferenceNumber = "Ref1";
		doc1.CSI_Description = "Desc1";

		var doc2 = Factory.New<CusSupportingInfo>();
		doc2.CSI_Code = "B";
		doc2.CSI_ReferenceNumber = "Ref2";
		doc2.CSI_Description = "Desc2";

		var doc3 = Factory.New<CusSupportingInfo>();
		doc3.CSI_Code = "C";
		doc3.CSI_ReferenceNumber = "Ref3";
		doc3.CSI_Description = "Desc3";

		var docList = new List<CusSupportingInfo>() { doc1, doc2, doc3 };

		CombineAssertions(() =>
		{
			var wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(docList);

			AssertContainsExactElementsInAnyOrder("Expected correct Name for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is false (default)", new ZString[] { "A", "B", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder("Expected correct Number for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is false (default)", new ZString[] { "Ref1", "Ref2", "Ref3" }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInAnyOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is false (default)", new ZString[] { "1", "2", "3" }, wrapperList.Select(x => x.SequenceNumber));

			wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(docList, shouldSendReferenceNumber: false);

			AssertContainsExactElementsInAnyOrder("Expected correct Name for all documents when shouldSendReferenceNumber is false and shouldSendDescription is false (default)", new ZString[] { "A", "B", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder("Expected correct Number (empty) for all documents when shouldSendReferenceNumber is false and shouldSendDescription is false (default)", new ZString[] { ZString.Empty, ZString.Empty, ZString.Empty }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInAnyOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is false and shouldSendDescription is false (default)", new ZString[] { "1", "2", "3" }, wrapperList.Select(x => x.SequenceNumber));

			wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(docList, shouldSendReferenceNumber: false, shouldSendDescription: true);

			AssertContainsExactElementsInAnyOrder("Expected correct Name for all documents when shouldSendReferenceNumber is false and shouldSendDescription is true", new ZString[] { "A", "B", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder("Expected correct Number for all documents when shouldSendReferenceNumber is false and shouldSendDescription is true", new ZString[] { "Desc1", "Desc2", "Desc3" }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInAnyOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is false and shouldSendDescription is true", new ZString[] { "1", "2", "3" }, wrapperList.Select(x => x.SequenceNumber));

			wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperList(docList, shouldSendDescription: true);

			AssertContainsExactElementsInAnyOrder("Expected correct Name for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is true", new ZString[] { "A", "B", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInAnyOrder("Expected correct Number for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is true", new ZString[] { "Ref1", "Ref2", "Ref3" }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInAnyOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is true", new ZString[] { "1", "2", "3" }, wrapperList.Select(x => x.SequenceNumber));
		});
	}

	public void TestGetDocumentSequenceNumberWrapperListWithLineNoAsSeq()
	{
		var doc1 = Factory.New<CusSupportingInfo>();
		doc1.CSI_Code = "A";
		doc1.CSI_ReferenceNumber = "Ref1";
		doc1.CSI_Description = "Desc1";
		doc1.CSI_LineNo = 4;

		var doc2 = Factory.New<CusSupportingInfo>();
		doc2.CSI_Code = "B";
		doc2.CSI_ReferenceNumber = "Ref2";
		doc2.CSI_Description = "Desc2";
		doc2.CSI_LineNo = 2;

		var doc3 = Factory.New<CusSupportingInfo>();
		doc3.CSI_Code = "C";
		doc3.CSI_ReferenceNumber = "Ref3";
		doc3.CSI_Description = "Desc3";
		doc3.CSI_LineNo = 5;

		var docList = new List<CusSupportingInfo>() { doc1, doc2, doc3 };

		CombineAssertions(() =>
		{
			var wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(docList);

			AssertContainsExactElementsInExactOrder("Expected correct Name for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is false (default)", new ZString[] { "B", "A", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInExactOrder("Expected correct Number for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is false (default)", new ZString[] { "Ref2", "Ref1", "Ref3" }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInExactOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is false (default)", new ZString[] { "2", "4", "5" }, wrapperList.Select(x => x.SequenceNumber));

			wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(docList, shouldSendReferenceNumber: false);

			AssertContainsExactElementsInExactOrder("Expected correct Name for all documents when shouldSendReferenceNumber is false and shouldSendDescription is false (default)", new ZString[] { "B", "A", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInExactOrder("Expected correct Number (empty) for all documents when shouldSendReferenceNumber is false and shouldSendDescription is false (default)", new ZString[] { ZString.Empty, ZString.Empty, ZString.Empty }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInExactOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is false and shouldSendDescription is false (default)", new ZString[] { "2", "4", "5" }, wrapperList.Select(x => x.SequenceNumber));

			wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(docList, shouldSendReferenceNumber: false, shouldSendDescription: true);

			AssertContainsExactElementsInExactOrder("Expected correct Name for all documents when shouldSendReferenceNumber is false and shouldSendDescription is true", new ZString[] { "B", "A", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInExactOrder("Expected correct Number for all documents when shouldSendReferenceNumber is false and shouldSendDescription is true", new ZString[] { "Desc2", "Desc1", "Desc3" }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInExactOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is false and shouldSendDescription is true", new ZString[] { "2", "4", "5" }, wrapperList.Select(x => x.SequenceNumber));

			wrapperList = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(docList, shouldSendDescription: true);

			AssertContainsExactElementsInExactOrder("Expected correct Name for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is true", new ZString[] { "B", "A", "C" }, wrapperList.Select(x => x.Name));
			AssertContainsExactElementsInExactOrder("Expected correct Number for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is true", new ZString[] { "Ref2", "Ref1", "Ref3" }, wrapperList.Select(x => x.Number));
			AssertContainsExactElementsInExactOrder("Expected correct SequenceNumber for all documents when shouldSendReferenceNumber is true (default) and shouldSendDescription is true", new ZString[] { "2", "4", "5" }, wrapperList.Select(x => x.SequenceNumber));
		});
	}

	public void TestGetIncotermPlaceCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			declaration.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In Declaration, expected filled UNLCode", "ESADT", CommonWrappersHelper.GetIncotermPlaceCode(invoiceHeader, declaration));

			declaration.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In Declaration, expected empty UNLCode when length <= 2", ZString.Empty, CommonWrappersHelper.GetIncotermPlaceCode(invoiceHeader, declaration));

			invoiceHeader.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In InvoiceHeader, expected filled UNLCode", "ESADT", CommonWrappersHelper.GetIncotermPlaceCode(invoiceHeader, declaration));

			invoiceHeader.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In InvoiceHeader, expected empty UNLCode when length <= 2", ZString.Empty, CommonWrappersHelper.GetIncotermPlaceCode(invoiceHeader, declaration));
		});
	}

	public void TestGetIncotermPlaceCodeCountry()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			declaration.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In Declaration, when length > 2, expected filled DeliveryCountry", ZString.Empty, CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration));

			declaration.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In Declaration, when length = 2, expected filled DeliveryCountry", "ES", CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration));

			declaration.ZG_AgreedPlaceCode = "E";
			AssertEquals("In Declaration, when length = 1, expected filled DeliveryCountry", ZString.Empty, CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration));

			invoiceHeader.ZG_AgreedPlaceCode = "ESADT";
			AssertEquals("In InvoiceHeader, when length > 2, expected filled DeliveryCountry", ZString.Empty, CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration));

			invoiceHeader.ZG_AgreedPlaceCode = "ES";
			AssertEquals("In InvoiceHeader, when length = 2, expected filled DeliveryCountry", "ES", CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration));

			invoiceHeader.ZG_AgreedPlaceCode = "E";
			AssertEquals("In InvoiceHeader, when length = 1, expected filled DeliveryCountry", ZString.Empty, CommonWrappersHelper.GetIncotermPlaceCodeCountry(invoiceHeader, declaration));
		});
	}
}
