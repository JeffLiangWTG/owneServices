using System;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class PreviousDocumentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new PreviousDocumentWrapper(null));
	}

	public void TestLineNo()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.LineNo), previousDocumentWrapper.LineNo);

		previousDocument.CSI_LineNo = 5;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.LineNo), 5, previousDocumentWrapper.LineNo);
	}

	public void TestNumberOfPackages()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.NumberOfPackages), previousDocumentWrapper.NumberOfPackages);

		previousDocument.CSI_PackQty = 10;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), 10, previousDocumentWrapper.NumberOfPackages);
	}

	public void TestPackageType()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.PackageType), "", previousDocumentWrapper.PackageType);

		previousDocument.CSI_PackType = "CT";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.NumberOfPackages), "CT", previousDocumentWrapper.PackageType);
	}

	public void TestQuantityForPreviousProcedureCodes()
	{
		previousDocument.CSI_Quantity3 = 1000m;
		previousDocument.CSI_UnitOfQuantity3 = "G";

		previousDocument.CSI_Quantity = 2120m;
		previousDocument.CSI_UnitOfQuantity = "G";

		previousDocument.CSI_Procedure = "1";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 2.12m, previousDocumentWrapper.Quantity);
	}

	public void TestQuantityForProcedureCodeMR1()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "0100", shipmentType: "IMP", (procedure) => procedure.ZZ6_Group = "H2");
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "0200", shipmentType: "IMP", (procedure) => procedure.ZZ6_Group = "H3");
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "0201", shipmentType: "IMP");
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "0300", shipmentType: "IMP");

		previousDocument.CSI_Quantity3 = 1000m;
		previousDocument.CSI_UnitOfQuantity3 = "G";

		previousDocument.CSI_Quantity = 2120m;
		previousDocument.CSI_UnitOfQuantity = "G";

		previousDocument.CSI_Procedure = "MR1";

		invoiceLine.JI_Procedure = "0001";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 1m, previousDocumentWrapper.Quantity);

		invoiceLine.JI_Procedure = "0002";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 1m, previousDocumentWrapper.Quantity);

		invoiceLine.JI_Procedure = "0201";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 1m, previousDocumentWrapper.Quantity);

		invoiceLine.JI_Procedure = "0300";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 2.12m, previousDocumentWrapper.Quantity);

		invoiceLine.JI_Procedure = "";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 2.12m, previousDocumentWrapper.Quantity);
	}

	public void TestQuantity()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.Quantity), previousDocumentWrapper.Quantity);

		previousDocument.CSI_Quantity3 = 1000m;
		previousDocument.CSI_UnitOfQuantity3 = "G";

		previousDocument.CSI_Quantity = 2120m;
		previousDocument.CSI_UnitOfQuantity = "G";

		previousDocument.CSI_Procedure = "A3";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 1m, previousDocumentWrapper.Quantity);
	}

	public void TestReferenceNumberForProcedureMRN()
	{
		previousDocument.CSI_Procedure = "MRN";
		previousDocument.CSI_ReferenceNumber2 = "22IT12345678901234";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "MRN-22IT12345678901234", previousDocumentWrapper.ReferenceNumber);
	}

	public void TestReferenceNumberForProcedureNUM()
	{
		previousDocument.CSI_Procedure = "NUM";
		previousDocument.CSI_ReferenceNumber = "2022FGACOH10010000169";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "2022FGACOH10010000169", previousDocumentWrapper.ReferenceNumber);
	}

	public void TestReferenceNumberForProcedureMR1()
	{
		previousDocument.CSI_Procedure = "MR1";
		previousDocument.CSI_ReferenceNumber2 = "22IT12345678901234";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "22IT12345678901234", previousDocumentWrapper.ReferenceNumber);
	}

	public void TestReferenceNumberForProcedureTC()
	{
		previousDocument.CSI_Procedure = "TC";
		previousDocument.CSI_ReferenceNumber2 = "22IT12345678901234";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "22IT12345678901234", previousDocumentWrapper.ReferenceNumber);
	}

	public void TestReferenceNumberForProcedureA3()
	{
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_ReferenceNumber2 = "25ITQX3300051541U2";
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "25ITQX3300051541U2", previousDocumentWrapper.ReferenceNumber);
	}

	public void TestReferenceNumberDefaultBehavior()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "", previousDocumentWrapper.ReferenceNumber);

		previousDocument.CSI_Procedure = "PF";
		previousDocument.CSI_ReferenceNumber = "123456";
		previousDocument.CSI_DateOfIssue = new ZDateTime(2022, 06, 23);
		previousDocument.CSI_CustomsOffice = "IT137100";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.ReferenceNumber), "PF-123456-2022-137100", previousDocumentWrapper.ReferenceNumber);
	}

	public void TestDocumentType()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.DocumentType), "", previousDocumentWrapper.DocumentType);

		previousDocument.CSI_Code = "T";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.DocumentType), "T", previousDocumentWrapper.DocumentType);
	}

	public void TestUnitOfQuantity()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.UnitOfQuantity), "", previousDocumentWrapper.UnitOfQuantity);

		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_Quantity3 = 10;
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.UnitOfQuantity), "KGM", previousDocumentWrapper.UnitOfQuantity);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		previousDocument = invoiceLine.PreviousDocuments.AddNew();
	}

	JobComInvoiceLine invoiceLine;
	PreviousDocument previousDocument;

	IPreviousDocument GetNewPreviousDocumentWrapper() => new PreviousDocumentWrapper(previousDocument);
}
