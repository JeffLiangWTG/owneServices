using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SendsMessagesToCustomsShutterUpperer = Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer;

namespace Enterprise.Customs.DE.Business.Testing;

[TestedType(typeof(AVABRDeclarationFinalizer))]
public class AVABRDeclarationFinalizerTest : TestCaseWithFactory
{
	public void TestEntryHeaderNull()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AVABRDeclarationFinalizer(null));
	}

	public void TestEntryHeaderStatusUpdated()
	{
		AssertEquals("Precondition", ZString.Empty, entryHeader.CH_EntryStatus);

		var declarationFinalizer = new AVABRDeclarationFinalizer(entryHeader);
		declarationFinalizer.FinalizeDeclaration();

		CombineAssertions(() =>
		{
			AssertEquals("TX8", entryHeader.CH_EntryStatus);
			var log = entryHeader.Logs.Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatus.Code);
			AssertEquals("Customs Entry Status Event is logged", 1, log.Count());
		});
	}

	public void TestLinkedWarehouseOrderUpdated()
	{
		var helper = WhsDataTestHelper.New(Factory);
		WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "TX6", true, iCancel: false);

		using (helper.WhsHelper.UsePutawayEngineManagerMock())
		using (helper.WhsHelper.UseAllocationEngineMock())
		{
			var inwardDecl = helper.GetNewDeclarationWithInstructionForInwardProcessing(Factory, "IMP", "B0000", "ENT0001", 10000, true);
			inwardDecl.InvoiceLines[0].JI_BondedWhsQuantity = 100m;
			inwardDecl.InvoiceLines[0].CusEntryLine.ZG_CustomsStatus = "TX6";

			inwardDecl.CustomsEntryHeaders[0].PublishShipmentForWHSInward(false);
			inwardDecl.CustomsEntryHeaders[0].PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

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
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var header = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
			header.MovementReferenceNumberSetter("MRN11223344");

			var finalizer = new AVABRDeclarationFinalizer(header);
			finalizer.FinalizeDeclaration();

			var link = links.Single();
			var lines = Factory.Load<IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, link.WV_WD_Docket));

			AssertEquals(1, lines.Length);
			var orderLine = lines.Single();
			var whsBondedWarehouseAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, orderLine.PK));
			AssertEquals("MRN11223344", whsBondedWarehouseAttribute.WB_EntryKey);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
	}

	CusEntryHeader entryHeader;
}
