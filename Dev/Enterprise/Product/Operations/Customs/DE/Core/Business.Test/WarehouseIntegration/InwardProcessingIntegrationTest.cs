using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.DE;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class InwardProcessingIntegrationTest : TestCaseWithFactory
	{
		public void TestPublishShipmentForWHSOutward() => CombineAssertions(() =>
		{
			var helper = WhsDataTestHelper.New(Factory);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "TX6", true, iCancel: false);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDecl = helper.GetNewDeclarationWithInstructionForInwardProcessing(Factory, "IMP", "B0000", "ENT0001", 10000, true);
				inwardDecl.InvoiceLines[0].JI_BondedWhsQuantity = 100m;
				inwardDecl.InvoiceLines[0].CusEntryLine.ZG_CustomsStatus = "TX6";

				inwardDecl.CustomsEntryHeaders[0].PublishShipmentForWHSInward(false);
				inwardDecl.CustomsEntryHeaders[0].PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				// ^^^ this gives us 100 in quantity

				Factory.Save();

				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0001", 1, 100m);

				var declaration = helper.GetNewDeclarationWithInstructionForInwardProcessing(Factory, DEJobMessageTypeList.Codes.Import, "B0001", "ENT0002", 1000, false, previousEntryNumber: "ENT0001-1");
				declaration.CustomsEntryHeaders[0].MergedLines.DeleteAll();
				declaration.CustomsEntryHeaders.DeleteAll();
				declaration.InvoiceLines[0].JI_CEI = declaration.CustomsEntryInstructions[0].PK;
				declaration.InvoiceLines[0].JI_BondedWhsQuantity = 10m;
				declaration.InvoiceLines[0].JI_InvoiceQuantity = 10m;
				declaration.InvoiceLines[0].JI_BondedWhsUnitQty = "KG";
				declaration.InvoiceLines[0].JI_InvoiceUQ = "KG";
				declaration.InvoiceLines[0].JI_BondedWHSOrderLineNumber = 1;
				declaration.InvoiceLines[0].JI_PreviousEntryLineNumber = 0;

				Factory.Save();

				AssertEquals("order can be fulfilled", ZString.Empty, BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration));
				var links = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, declaration.PK));
				AssertEquals("docket link for this declaration created", 1, links.Length);

				declaration.InvoiceLines[0].JI_BondedWhsQuantity = 110m;
				Factory.Save();

				var expectedError = "Error - Cannot Import Order\r\nOrder could not be created for Customs Job B0001 because there are errors:\r\nYou do not have enough stock to fulfill shortfalls on this order\r\nENT0001-1 Product ~~1/~~1 DESC can not be ordered due to lack of stock. 110 was ordered, but 100 is available";
				AssertEquals("order cannot be fulfilled", expectedError, (string)BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration));
			}
		});
	}
}
