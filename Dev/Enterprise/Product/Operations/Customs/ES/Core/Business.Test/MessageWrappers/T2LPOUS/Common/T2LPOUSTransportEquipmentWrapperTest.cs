using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business.Declaration;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSTransportEquipmentWrapperTest : WrapperHelperTest<T2LPOUSTransportEquipmentWrapper>
	{
		public void TestContainerIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled ContainerNumber for first wrapper", "CONT1", wrapper1.ContainerIdentificationNumber);

				AssertEquals("Expected filled ContainerNumber for second wrapper", "CONT2", wrapper2.ContainerIdentificationNumber);

				AssertEquals("Expected filled ContainerNumber for third wrapper", "CONT3", wrapper3.ContainerIdentificationNumber);

				AssertEquals("Expected filled ContainerNumber for fourth wrapper", "CONT4", wrapper4.ContainerIdentificationNumber);
			});
		}

		public void TestGoodsReference()
		{
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("Expected filled GoodsReference for first wrapper", new ZInt[] { 1 }, wrapper1.GoodsReference.ToArray());

				AssertArrayEqualsByElements("Expected filled GoodsReference for second wrapper", new ZInt[] { 2 }, wrapper2.GoodsReference.ToArray());

				AssertArrayEqualsByElements("Expected filled GoodsReference for third wrapper", new ZInt[] { 0 }, wrapper3.GoodsReference.ToArray());

				AssertArrayEqualsByElements("Expected filled GoodsReference for fourth wrapper", new ZInt[] { 1 }, wrapper4.GoodsReference.ToArray());
			});
		}

		public void TestGetTransportEquipmentList_NullEntryHeader()
		{
			var wrapperList = T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(null);
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestGetTransportEquipmentList_NoContainers()
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var wrapperList = T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestGetTransportEquipmentList()
		{
			var declaration = Factory.New<JobDeclaration>();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLineContainer1 = entryHeader.MergedLines.AddNew();
			entryLineContainer1.CL_LineNumber = 1;
			var entryLineContainer2 = entryHeader.MergedLines.AddNew();
			entryLineContainer2.CL_LineNumber = 2;

			var container1 = CreateContainer(declaration, "CONT1");
			var container2 = CreateContainer(declaration, "CONT2");
			var container3 = CreateContainer(declaration, "CONT3");
			var container4 = CreateContainer(declaration, "CONT4");

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			package4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;

			var invoiceLine1 = entryLineContainer1.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddNew().C2_CO = container1.PK;
			invoiceLine1.PackagesPivot.AddPivotFor(package1);

			var invoiceLine2 = entryLineContainer2.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container2.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container3.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container4.PK;
			invoiceLine2.PackagesPivot.AddPivotFor(package2);
			invoiceLine2.PackagesPivot.AddPivotFor(package3);
			invoiceLine2.PackagesPivot.AddPivotFor(package4);

			CombineAssertions(() =>
			{
				var wrapperList = T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();

				AssertEquals("First wrapper expected filled ContainerNumber", "CONT1", wrapperList[0].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for first wrapper", new ZInt[] { 1 }, wrapperList[0].GoodsReference.ToArray());

				AssertEquals("Second wrapper expected filled ContainerNumber", "CONT2", wrapperList[1].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for second wrapper", new ZInt[] { 2 }, wrapperList[1].GoodsReference.ToArray());

				AssertEquals("Third wrapper expected filled ContainerNumber", "CONT3", wrapperList[2].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for third wrapper", new ZInt[] { 2 }, wrapperList[2].GoodsReference.ToArray());

				AssertEquals("Fourth wrapper expected filled ContainerNumber", "CONT4", wrapperList[3].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for fourth wrapper", new ZInt[] { 2 }, wrapperList[3].GoodsReference.ToArray());
			});
		}

		public void TestGetTransportEquipmentList_OnlyOneEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLineContainer = entryHeader.MergedLines.AddNew();
			entryLineContainer.CL_LineNumber = 1;

			var container1 = CreateContainer(declaration, "CONT1");
			var container2 = CreateContainer(declaration, "CONT2");
			var container3 = CreateContainer(declaration, "CONT3");
			var container4 = CreateContainer(declaration, "CONT4");

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			package4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;

			var invoiceLine1 = entryLineContainer.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddNew().C2_CO = container1.PK;
			invoiceLine1.PackagesPivot.AddPivotFor(package1);

			var invoiceLine2 = entryLineContainer.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container2.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container3.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container4.PK;
			invoiceLine2.PackagesPivot.AddPivotFor(package2);
			invoiceLine2.PackagesPivot.AddPivotFor(package3);
			invoiceLine2.PackagesPivot.AddPivotFor(package4);

			CombineAssertions(() =>
			{
				var wrapperList = T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();

				AssertEquals("First wrapper expected filled ContainerNumber", "CONT1", wrapperList[0].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for first wrapper", new ZInt[] { 0 }, wrapperList[0].GoodsReference.ToArray());

				AssertEquals("Second wrapper expected filled ContainerNumber", "CONT2", wrapperList[1].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for second wrapper", new ZInt[] { 0 }, wrapperList[1].GoodsReference.ToArray());

				AssertEquals("Third wrapper expected filled ContainerNumber", "CONT3", wrapperList[2].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for third wrapper", new ZInt[] { 0 }, wrapperList[2].GoodsReference.ToArray());

				AssertEquals("Fourth wrapper expected filled ContainerNumber", "CONT4", wrapperList[3].ContainerIdentificationNumber);
				AssertArrayEqualsByElements("Expected filled GoodsReference for fourth wrapper", new ZInt[] { 0 }, wrapperList[3].GoodsReference.ToArray());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
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

				var wrapperList = T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();
				wrapper1 = wrapperList[0];
				wrapper2 = wrapperList[1];
				wrapper3 = wrapperList[2];
				wrapper4 = wrapperList[3];
			}
		}

		JobDeclaration declaration;
		T2LPOUSTransportEquipmentWrapper wrapper1;
		T2LPOUSTransportEquipmentWrapper wrapper2;
		T2LPOUSTransportEquipmentWrapper wrapper3;
		T2LPOUSTransportEquipmentWrapper wrapper4;

		CusContainer CreateContainer(JobDeclaration declaration, string containerNumber)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			return container;
		}

		protected override T2LPOUSTransportEquipmentWrapper GetProvider() => wrapper1;
	}
}
