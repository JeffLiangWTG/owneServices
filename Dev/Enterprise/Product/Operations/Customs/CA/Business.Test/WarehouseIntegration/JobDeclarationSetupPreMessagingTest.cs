using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationSetupPreMessagingTest : TestCaseWithFactory
	{
		public void TestActionForInward()
		{
			var inwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B00000001", "00000001", B3EntryTypeList.Codes.Warehouse10, Importer, Importer);
			var invoiceLine = inwardDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = inwardDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;
			Factory.Save();

			((IWarehouseIntegrationSupporter)inwardDeclaration).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			Assert("Precondition: IsInwardBondedWarehousingEnabled = true", inwardDeclaration.IsInwardBondedWarehousingEnabled);

			var preMessagingActionResult = inwardDeclaration.PreMessagingAction(MessageSubTypes.Create);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingActionResult.PreMessagingAction);
			AssertNotNull(preMessagingActionResult.RestoreToPreMessagingState);
			AssertEquals(@"Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that is located within Canada.
An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.
An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.
An Invoice Line marked for Inventory Management must have an Entry Number and Entry Line Number; not all Invoice Lines marked for Inventory Management have a related Entry Number and Entry Line Number specified.",
				preMessagingActionResult.ErrorMessageForCheckFieldsForBondedWarehous);

			inwardDeclaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			Factory.Save();

			((IWarehouseIntegrationSupporter)inwardDeclaration).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			Assert("Precondition: IsInwardBondedWarehousingEnabled = true", inwardDeclaration.IsInwardBondedWarehousingEnabled);

			preMessagingActionResult = inwardDeclaration.PreMessagingAction(MessageSubTypes.Create);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingActionResult.PreMessagingAction);
			AssertNotNull(preMessagingActionResult.RestoreToPreMessagingState);
			AssertEquals(@"Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that is located within Canada.
An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.
An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.
An Invoice Line marked for Inventory Management must have an Entry Number and Entry Line Number; not all Invoice Lines marked for Inventory Management have a related Entry Number and Entry Line Number specified.",
				preMessagingActionResult.ErrorMessageForCheckFieldsForBondedWarehous);

			inwardDeclaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Factory.Save();

			((IWarehouseIntegrationSupporter)inwardDeclaration).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			Assert("Precondition: IsInwardBondedWarehousingEnabled = true", inwardDeclaration.IsInwardBondedWarehousingEnabled);

			preMessagingActionResult = inwardDeclaration.PreMessagingAction(MessageSubTypes.Create);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingActionResult.PreMessagingAction);
			AssertNotNull(preMessagingActionResult.RestoreToPreMessagingState);
			AssertEquals(@"Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that is located within Canada.
An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.
An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.
An Invoice Line marked for Inventory Management must have an Entry Number and Entry Line Number; not all Invoice Lines marked for Inventory Management have a related Entry Number and Entry Line Number specified.",
				preMessagingActionResult.ErrorMessageForCheckFieldsForBondedWarehous);
		}

		public void TestActionForOutward()
		{
			var outwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000002", "00000002", B3EntryTypeList.Codes.ExWarehouse20, Importer, Importer);
			var invoiceLine = outwardDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = outwardDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;
			Factory.Save();

			Assert("Precondition: IsOutwardBondedWarehousingEnabled = true", outwardDeclaration.IsOutwardBondedWarehousingEnabled);

			var preMessagingActionResult = outwardDeclaration.PreMessagingAction(MessageSubTypes.Create);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingActionResult.PreMessagingAction);
			AssertNotNull(preMessagingActionResult.RestoreToPreMessagingState);
			AssertEquals(@"Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that is located within Canada.
An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.
An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.
An Invoice Line marked for Bonded Warehousing must have Previous Tran. # and PTLN specified; not all Invoice Lines marked for Bonded Warehousing have Previous Tran. # and PTLN specified.",
				preMessagingActionResult.ErrorMessageForCheckFieldsForBondedWarehous);

			outwardDeclaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			Factory.Save();

			Assert("Precondition: IsOutwardBondedWarehousingEnabled = true", outwardDeclaration.IsOutwardBondedWarehousingEnabled);

			preMessagingActionResult = outwardDeclaration.PreMessagingAction(MessageSubTypes.Create);
			Assert("isBondedWarehouse", preMessagingActionResult.IsBondedWarehouse);
			AssertNotNull(preMessagingActionResult.PreMessagingAction);
			AssertNotNull(preMessagingActionResult.RestoreToPreMessagingState);
			AssertEquals(@"Inventory recording/Inventory Management Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that is located within Canada.
An Invoice Line marked for Inventory Management must have a valid product; not all Invoice Lines marked for Inventory Management have a valid product specified.
An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified.
An Invoice Line marked for Bonded Warehousing must have Previous Tran. # and PTLN specified; not all Invoice Lines marked for Bonded Warehousing have Previous Tran. # and PTLN specified.",
				preMessagingActionResult.ErrorMessageForCheckFieldsForBondedWarehous);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@test.com.au";
		}

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.OH_IsWarehouseClient = true;
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					BondedWarehousingHelperTest.CreateWarehouse(Factory, importer, "WHS");
				}
				return importer;
			}
		}
		OrgHeader importer;
	}
}
