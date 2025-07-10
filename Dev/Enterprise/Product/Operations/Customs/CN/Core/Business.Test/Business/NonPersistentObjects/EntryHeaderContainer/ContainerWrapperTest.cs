using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryHeaderContainer))]
	class ContainerWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryHeader = declaration.ActiveEntryHeaders[0];
			return new EntryHeaderContainer(entryHeader, (CusContainer)entryHeader.Containers[0]);
		}

		public void TestTareWeightInKG()
		{
			var containerRef = Factory.New<RefContainer>();
			containerRef.RC_Code = "20PP";
			containerRef.RC_CubicCapacity = 16m;
			containerRef.RC_GrossWeight = 123456.457m;
			containerRef.RC_Height = 3m;
			containerRef.RC_Length = 6m;
			containerRef.RC_Width = 2m;
			containerRef.RC_TareWeight = 123456.457m;
			containerRef.RC_TEU = 1;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var container = declaration.CusContainers.AddNew();
			var entryHeaderContainer = new EntryHeaderContainer(entryHeader, container);
			var jobContainer = Factory.NewWithValidTestData<CommonContainer>();
			container.CO_JC = jobContainer.PK;
			jobContainer.JC_RC = containerRef.PK;
			jobContainer.JC_GrossWeightUQ = "G";
			AssertEquals(123456457m, jobContainer.JC_Calc_TareWeight);
			AssertEquals(123456.457m, entryHeaderContainer.TareWeightInKG);
			containerRef.RC_TareWeight = 0m;
			AssertEquals(0m, jobContainer.JC_Calc_TareWeight);
			entryHeaderContainer = new EntryHeaderContainer(entryHeader, container);
			jobContainer.JC_TareWeight = 22222.235m;
			AssertEquals(22.222235m, entryHeaderContainer.TareWeightInKG);
		}

		public void TestIsLessContainer()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var container = declaration.CusContainers.AddNew();
			var entryHeaderContainer = new EntryHeaderContainer(entryHeader, container);
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Assert("IsLessContainer", entryHeaderContainer.IsLessContainer);
			AssertEquals("IsLessContainerDesc", "是", entryHeaderContainer.IsLessContainerDesc);
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Assert("IsLessContainer", !entryHeaderContainer.IsLessContainer);
			AssertEquals("IsLessContainerDesc", "否", entryHeaderContainer.IsLessContainerDesc);
		}

		public void TestLinkedEntryLineNosAsString()
		{
			var entryHeader = declaration.ActiveEntryHeaders[0];
			var container1 = entryHeader.Containers.First(x => x.CO_ContainerNumber == "CONTAINER1") as CusContainer;
			var container2 = entryHeader.Containers.First(x => x.CO_ContainerNumber == "CONTAINER2") as CusContainer;
			var container3 = entryHeader.Containers.First(x => x.CO_ContainerNumber == "CONTAINER3") as CusContainer;
			AssertEquals("1", new EntryHeaderContainer(entryHeader, container1).LinkedEntryLineNos.JoinAsString());
			AssertEquals("1,2,3", new EntryHeaderContainer(entryHeader, container2).LinkedEntryLineNos.JoinAsString());
			AssertEquals("1,3", new EntryHeaderContainer(entryHeader, container3).LinkedEntryLineNosAsString);
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = PrepareDataForContainerWrapperTest(Factory);
		}

		internal static JobDeclaration PrepareDataForContainerWrapperTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONTAINER1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONTAINER2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONTAINER3";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_LineNo = 1;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.ContainersPivot.AddPivotFor(container2);
			invoiceLine2.JI_LineNo = 2;
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.ContainersPivot.AddPivotFor(container2);
			invoiceLine3.ContainersPivot.AddPivotFor(container3);
			declaration.DoMerge();
			invoiceLine1.CusEntryLine.CL_LineNumber = 1;
			invoiceLine2.CusEntryLine.CL_LineNumber = 2;
			invoiceLine3.CusEntryLine.CL_LineNumber = 3;
			return declaration;
		}
	}
}
