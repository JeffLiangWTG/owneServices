using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class CommonTransportEquipmentWrapperTest : WrapperHelperTest<CommonTransportEquipmentWrapper>
{
	public void TestSequenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled SequenceNumber for first wrapper", "1", wrapper1.SequenceNumber);

			AssertEquals("Expected filled SequenceNumber for second wrapper", "2", wrapper2.SequenceNumber);

			AssertEquals("Expected filled SequenceNumber for third wrapper", "3", wrapper3.SequenceNumber);

			AssertEquals("Expected filled SequenceNumber for fourth wrapper", "4", wrapper4.SequenceNumber);
		});
	}

	public void TestContainerNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled ContainerNumber for first wrapper", "CONT1", wrapper1.ContainerNumber);

			AssertEquals("Expected filled ContainerNumber for second wrapper", "CONT2", wrapper2.ContainerNumber);

			AssertEquals("Expected filled ContainerNumber for third wrapper", "CONT3", wrapper3.ContainerNumber);

			AssertEquals("Expected filled ContainerNumber for fourth wrapper", "CONT4", wrapper4.ContainerNumber);
		});
	}

	public void TestGoodsReference()
	{
		CombineAssertions(() =>
		{
			var goodsreference1 = wrapper1.GoodsReference.ToList();
			AssertEquals("Expected filled GoodsReference for first wrapper with count 1", 1, goodsreference1.Count);
			AssertEquals("Expected filled GoodsReference for first wrapper, SequenceNumber", "1", goodsreference1[0].SequenceNumber);
			AssertEquals("Expected filled GoodsReference for first wrapper, GoodsItemNumber", "1", goodsreference1[0].GoodsItemNumber);

			var goodsreference2 = wrapper2.GoodsReference.ToList();
			AssertEquals("Expected filled GoodsReference for second wrapper with count 1", 1, goodsreference2.Count);
			AssertEquals("Expected filled GoodsReference for second wrapper, SequenceNumber", "1", goodsreference2[0].SequenceNumber);
			AssertEquals("Expected filled GoodsReference for second wrapper, GoodsItemNumber", "2", goodsreference2[0].GoodsItemNumber);

			var goodsreference3 = wrapper3.GoodsReference.ToList();
			AssertEquals("Expected filled GoodsReference for third wrapper with count 2", 2, goodsreference3.Count);
			AssertEquals("Expected filled GoodsReference for third wrapper, first SequenceNumber", "1", goodsreference3[0].SequenceNumber);
			AssertEquals("Expected filled GoodsReference for third wrapper, first GoodsItemNumber", "1", goodsreference3[0].GoodsItemNumber);
			AssertEquals("Expected filled GoodsReference for third wrapper, second SequenceNumber", "2", goodsreference3[1].SequenceNumber);
			AssertEquals("Expected filled GoodsReference for third wrapper, second GoodsItemNumber", "2", goodsreference3[1].GoodsItemNumber);

			var goodsreference4 = wrapper4.GoodsReference.ToList();
			AssertEquals("Expected filled GoodsReference for fourth wrapper with count 1", 1, goodsreference4.Count);
			AssertEquals("Expected filled GoodsReference for fourth wrapper, SequenceNumber", "1", goodsreference4[0].SequenceNumber);
			AssertEquals("Expected filled GoodsReference for fourth wrapper, GoodsItemNumber", "1", goodsreference4[0].GoodsItemNumber);
		});
	}

	public void TestGetTransportEquipmentList_NullEntryHeader()
	{
		var wrapperList = CommonTransportEquipmentWrapper.GetTransportEquipmentList(null);
		AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
	}

	public void TestGetTransportEquipmentList_NoContainers()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var wrapperList = CommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);
		AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ContainerMode = "ULD";
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine2.CL_LineNumber = 2;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var container1 = CreateContainer(declaration, "CONT1");
			var container2 = CreateContainer(declaration, "CONT2");
			var container3 = CreateContainer(declaration, "CONT3");
			var container4 = CreateContainer(declaration, "CONT4");

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			package4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(package1);
			invoiceLine1.PackagesPivot.AddPivotFor(package3);
			invoiceLine1.PackagesPivot.AddPivotFor(package4);

			invoiceLine2.PackagesPivot.AddPivotFor(package2);
			invoiceLine2.PackagesPivot.AddPivotFor(package3);

			var wrapperList = CommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();
			wrapper1 = wrapperList[0];
			wrapper2 = wrapperList[1];
			wrapper3 = wrapperList[2];
			wrapper4 = wrapperList[3];
	}

	JobDeclaration declaration;
	CommonTransportEquipmentWrapper wrapper1;
	CommonTransportEquipmentWrapper wrapper2;
	CommonTransportEquipmentWrapper wrapper3;
	CommonTransportEquipmentWrapper wrapper4;

	EU.Business.Declaration.CusContainer CreateContainer(JobDeclaration declaration, string containerNumber)
	{
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = containerNumber;

		return container;
	}

	protected override CommonTransportEquipmentWrapper GetProvider() => wrapper1;
}
