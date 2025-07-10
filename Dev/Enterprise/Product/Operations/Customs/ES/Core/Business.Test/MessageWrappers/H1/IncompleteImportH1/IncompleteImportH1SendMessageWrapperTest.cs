using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class IncompleteImportH1SendMessageWrapperTest : WrapperHelperTest<IncompleteImportH1SendMessageWrapper>
{
	public void TestImportOperation()
	{
		var importOperation = wrapper.ImportOperation;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ImportOperation", importOperation);
			AssertSame("Cached ImportOperation", wrapper.ImportOperation, importOperation);
		});
	}

	public void TestCountryOfDispatch()
	{
		declaration.JE_GoodsOrigin = "ES";
		AssertEquals("Expected filled CountryOfExport", "ES", wrapper.CountryOfDispatch);
	}

	public void TestTransportEquipments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TransportEquipment", 0, wrapper.TransportEquipments.Count);

			declaration.JE_ContainerMode = "ULD";
			var package1 = declaration.Packages.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			invoiceLine.PackagesPivot.AddPivotFor(package1);
			wrapper = GetWrapper(entryHeader);
			var transportEquipments = wrapper.TransportEquipments;
			AssertEquals("Expected filled TransportEquipment", 1, transportEquipments.Count);
			AssertSame("Cached TransportEquipment", wrapper.TransportEquipments, transportEquipments);
		});
	}

	public void TestGoodsShipmentItems()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.GoodsShipmentItems.Count);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];
			wrapper = GetWrapper(entryHeader);

			var goodsShipmentItems = wrapper.GoodsShipmentItems;

			AssertEquals("Expected 3 Lines", 3, goodsShipmentItems.Count);
			AssertSame("Cached Lines", wrapper.GoodsShipmentItems, goodsShipmentItems);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryHeader entryHeader;
	IncompleteImportH1SendMessageWrapper wrapper;

	IncompleteImportH1SendMessageWrapper GetWrapper(CusEntryHeader entryheader) => new IncompleteImportH1SendMessageWrapper(entryheader, Certificate);

	protected override IncompleteImportH1SendMessageWrapper GetProvider() => wrapper;
}
