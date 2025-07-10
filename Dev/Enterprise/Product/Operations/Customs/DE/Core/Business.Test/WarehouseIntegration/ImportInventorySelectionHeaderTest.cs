using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ImportInventorySelectionHeader))]
	sealed class ImportInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFillSupportingDocuments_InvoiceLine()
		{
			// Arrange
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "", PreviousEntryNumber, 1);

			var warehouseCustomsAddInfo = Factory.New<IWarehouseCustomsAddInfo>();
			warehouseCustomsAddInfo.B7_ParentTableCode = "WB";
			warehouseCustomsAddInfo.B7_ParentID = receiveLineCustomsData.PK;
			warehouseCustomsAddInfo.B7_Type = "SDL";
			warehouseCustomsAddInfo.B7_AddInfoData =
				"*Type=N380*Reference=ref*DateOfIssue=2023-08-31 00:00:00.000*Available=N*Quantity=12*UnitofMeasure=019";
			var warehouseCustomsAddInfo2 = Factory.New<IWarehouseCustomsAddInfo>();
			warehouseCustomsAddInfo2.B7_ParentTableCode = "WB";
			warehouseCustomsAddInfo2.B7_ParentID = receiveLineCustomsData.PK;
			warehouseCustomsAddInfo2.B7_Type = "SDL";
			warehouseCustomsAddInfo2.B7_AddInfoData =
				"*Type=N380*Reference=ref*DateOfIssue=2023-08-30 00:00:00.000*Available=Y*Quantity=13*UnitofMeasure=018";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });

			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;

			//Act
			header.ImportInventories();

			// Assert
			var csi = declaration.Invoices[0].InvoiceLines[0].SupportingDocuments.Cast<SupportingDocument>().Single();

			AssertEquals("N380", csi.CSI_Code);
			AssertEquals("ref", csi.CSI_ReferenceNumber);
			AssertEquals(new ZDateTime(2023, 8, 31), csi.CSI_DateOfIssue);
			AssertEquals("N", csi.CSI_Status);
			AssertEquals(50m, csi.CSI_Quantity);
			AssertEquals("019", csi.CSI_UnitOfQuantity);
			AssertEquals("N380ref", csi.KeyToDeterimeUniqueness);
		}

		public void TestFillSupportingDocuments_InvoiceHeader()
		{
			// Arrange
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);

			var warehouseCustomsAddInfo = Factory.New<IWarehouseCustomsAddInfo>();
			warehouseCustomsAddInfo.B7_ParentTableCode = "WB";
			warehouseCustomsAddInfo.B7_ParentID = receiveLineCustomsData.PK;
			warehouseCustomsAddInfo.B7_Type = "SDH";
			warehouseCustomsAddInfo.B7_AddInfoData =
				"*Type=N380*Reference=ref*DateOfIssue=2023-08-31 00:00:00.000*Available=N*Quantity=12*UnitofMeasure=019";

			var warehouseCustomsAddInfo2 = Factory.New<IWarehouseCustomsAddInfo>();
			warehouseCustomsAddInfo2.B7_ParentTableCode = "WB";
			warehouseCustomsAddInfo2.B7_ParentID = receiveLineCustomsData.PK;
			warehouseCustomsAddInfo2.B7_Type = "SDH";
			warehouseCustomsAddInfo2.B7_AddInfoData =
				"*Type=N380*Reference=ref*DateOfIssue=2023-08-31 00:00:00.000*Available=Y*Quantity=10*UnitofMeasure=018";
			Factory.Save();

			//Act
			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			// Assert
			var csi = invoiceLine.InvoiceHeader.SupportingDocuments.Cast<SupportingDocument>().Single();

			AssertEquals("N380", csi.CSI_Code);
			AssertEquals("ref", csi.CSI_ReferenceNumber);
			AssertEquals(new ZDateTime(2023, 8, 31), csi.CSI_DateOfIssue);
			AssertEquals("", csi.CSI_Status);
			AssertEquals(0m, csi.CSI_Quantity);
			AssertEquals("", csi.CSI_UnitOfQuantity);
		}

		public void TestFillInventoryDetails_Charges()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 2m;
					AddWarehouseCustomsAttributeAddInfo(customsData, "CCT", "*Amount=240*ChargeType=ADD*Currency=CNY*IsDutiable=Y*IsGSTApplicable=Y*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y");
					AddWarehouseCustomsAttributeAddInfo(customsData, "CCT", "*Amount=320*ChargeType=DED*Currency=USD*IsDutiable=*IsGSTApplicable=*IsIncludedInITOT=*IsStatisticalValueApplicable=Y");
				},
				assertion: invoiceLine =>
				{
					var add = invoiceLine.Charges.Cast<JobComInvCharge>().Single(x => x.J7_ChargeType == "ADD");
					AssertEquals(240m / 5, add.J7_Amount);
					AssertEquals("CNY", add.J7_RX_NKCurrency);
					AssertEquals(expected: true, add.J7_IsDutiable);
					AssertEquals(expected: true, add.J7_IsGSTApplicable);
					AssertEquals(expected: true, add.J7_IsIncludedInITOT);
					AssertEquals(expected: true, add.J7_IsStatisticalValueApplicable);

					var ded = invoiceLine.Charges.Cast<JobComInvCharge>().Single(x => x.J7_ChargeType == "DED");
					AssertEquals(320m / 5, ded.J7_Amount);
					AssertEquals("USD", ded.J7_RX_NKCurrency);
					AssertEquals(expected: false, ded.J7_IsDutiable);
					AssertEquals(expected: false, ded.J7_IsGSTApplicable);
					AssertEquals(expected: false, ded.J7_IsIncludedInITOT);
					AssertEquals(expected: true, ded.J7_IsStatisticalValueApplicable);
				});
		}

		public void TestJI_FormattedProcedure()
		{
			// Arrange
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);

			receiveLineCustomsData.WB_InwardProcedure = "1234";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "56";
			invoiceLine.JI_CEI = entryInstruction.PK;
			Factory.Save();

			//Act
			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			// Assert
			AssertEquals("5612", invoiceLine.JI_FormattedProcedure);
		}

		public void TestInvoiceGrouping()
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);

			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo1 = "LinePrice=500.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo1, PreviousEntryNumber, 1);

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var addInfo2 = "LinePrice=1000.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);

			var part3 = helper.CreateProduct(helper.Importer.PK, "~~3");
			var receiveLine3 = helper.GetNewWhsReceiveLine(receive.PK, part3.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-3", ZDateTime.Today.AddMonths(-1));
			var addInfo3 = "LinePrice=3000.0000*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo3, PreviousEntryNumber, 3);

			var part4 = helper.CreateProduct(helper.Importer.PK, "~~4");
			var receiveLine4 = helper.GetNewWhsReceiveLine(receive.PK, part4.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-4", ZDateTime.Today.AddMonths(-1));
			var addInfo4 = "LinePrice=4000.0000*LinePriceCurrency=USD*IncotermCode=XXX*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine4.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo4, PreviousEntryNumber, 4);

			var part5 = helper.CreateProduct(helper.Importer.PK, "~~5");
			var receiveLine5 = helper.GetNewWhsReceiveLine(receive.PK, part5.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-5", ZDateTime.Today.AddMonths(-1));
			var addInfo5 = "LinePrice=5000.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Mainz*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine5.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo5, PreviousEntryNumber, 5);

			var part6 = helper.CreateProduct(helper.Importer.PK, "~~6");
			var receiveLine6 = helper.GetNewWhsReceiveLine(receive.PK, part6.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-6", ZDateTime.Today.AddMonths(-1));
			var addInfo6 = "LinePrice=6000.0000*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=22";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine6.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo6, PreviousEntryNumber, 6);
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine3.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine4.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine5.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine6.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory, receiveLine3.Inventory, receiveLine4.Inventory, receiveLine5.Inventory, receiveLine6.Inventory });
			header.SelectionLines[0].US_ProductQtyToDraw = 100m;
			header.SelectionLines[1].US_ProductQtyToDraw = 100m;
			header.SelectionLines[2].US_ProductQtyToDraw = 100m;
			header.SelectionLines[3].US_ProductQtyToDraw = 100m;
			header.SelectionLines[4].US_ProductQtyToDraw = 100m;
			header.SelectionLines[5].US_ProductQtyToDraw = 100m;
			header.ImportInventories();

			CombineAssertions(() =>
			{
				AssertEquals("Invoices: count", 5, declaration.Invoices.Count);

				var invoice = GetInvoice(declaration, "USD", "FOB", "Frankfurt", "21");
				AssertNotNull("Invoice exists", invoice);
				AssertEquals("Invoice: InvoiceAmount", (ZDecimal)1500, invoice.JZ_InvoiceAmount);

				var invoiceEUR = GetInvoice(declaration, "EUR", "FOB", "Frankfurt", "21");
				AssertNotNull("InvoiceEUR exists", invoiceEUR);
				AssertEquals("InvoiceEUR: InvoiceAmount", (ZDecimal)3000, invoiceEUR.JZ_InvoiceAmount);

				var invoiceXXX = GetInvoice(declaration, "USD", "XXX", "Frankfurt", "21");
				AssertNotNull("InvoiceXXX exists", invoiceXXX);
				AssertEquals("InvoiceXXX: InvoiceAmount", (ZDecimal)4000, invoiceXXX.JZ_InvoiceAmount);

				var invoiceMainz = GetInvoice(declaration, "USD", "FOB", "Mainz", "21");
				AssertNotNull("InvoiceMainz exists", invoiceMainz);
				AssertEquals("InvoiceMainz: InvoiceAmount", (ZDecimal)5000, invoiceMainz.JZ_InvoiceAmount);

				var invoice22 = GetInvoice(declaration, "USD", "FOB", "Frankfurt", "22");
				AssertNotNull("Invoice22 exists", invoice22);
				AssertEquals("Invoice22: InvoiceAmount", (ZDecimal)6000, invoice22.JZ_InvoiceAmount);
			});
		}

		public void TestPopulateSecondQuantityFromInventory()
		{
			var tariffCode = "08091998";
			ImportInventoryForPartWithTariff(Factory, tariffCode, Core.Constants.CountryCodes.Germany, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Core.Constants.Weight.Milligrams, RefPackTypeStandardUnits.Codes.Milligrams);

			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			CombineAssertions(() =>
			{
				AssertEquals(tariffCode, invoiceLine.JI_Tariff);
				AssertEquals("JI_CustomsSecondUnitQty should be set from tariff additional Unit of Measure",Core.Constants.Weight.Milligrams, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("JI_CustomsSecondQuantity should be populated from JI_BondedWhsQuantity", 50.0m, invoiceLine.JI_CustomsSecondQuantity);
			});
		}

		public void TestPopulateSecondQuantityFromInventory_SecondQuantityNotSet()
		{
			var tariffCode = "08091998";
			ImportInventoryForPartWithTariff(Factory, tariffCode, Core.Constants.CountryCodes.Germany, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Core.Constants.Weight.Milligrams, RefPackTypeStandardUnits.Codes.Number);

			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			AssertEquals(tariffCode, invoiceLine.JI_Tariff);
			AssertEquals("JI_CustomsSecondUnitQty should be set from tariff additional Unit of Measure", Core.Constants.Weight.Milligrams, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsSecondQuantity should not be populated as JI_CustomsSecondUnitQty != JI_BondedWhsUnitQty", 0.0m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestPopulateSecondQuantityFromInventory_NoCU2UnitOfMeasure()
		{
			var tariffCode = "08091998";
			ImportInventoryForPartWithTariff(Factory, tariffCode, Core.Constants.CountryCodes.Germany, Universal.Constants.UnitOfMeasureTypes.ClassificationUOMType, Core.Constants.Weight.Milligrams, RefPackTypeStandardUnits.Codes.Number);

			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			AssertEquals(tariffCode, invoiceLine.JI_Tariff);
			AssertEquals("JI_CustomsSecondUnitQty should not be populated there is no UOM where ZZ8_Type == 'CU2'", ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsSecondQuantity should not be populated there is no UOM where ZZ8_Type == 'CU2'", 0.0m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestPopulateSecondQuantityFromInventory_NonDETariff()
		{
			var tariffCode = "08091998";
			ImportInventoryForPartWithTariff(Factory, tariffCode, Core.Constants.CountryCodes.France, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Core.Constants.Weight.Milligrams, RefPackTypeStandardUnits.Codes.Number);

			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];

			AssertEquals(tariffCode, invoiceLine.JI_Tariff);
			AssertEquals("JI_CustomsSecondUnitQty should not be populated from a non-DE tariff", ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsSecondQuantity should not be populated from a non-DE tariff", 0.0m, invoiceLine.JI_CustomsSecondQuantity);
		}

		void ImportInventoryForPartWithTariff(BusinessObjectFactory factory, string tariffCode, string tariffCountryCode, string tariffUOMType, string tariffUOM, string receivePackType)
		{
			var tariffHelper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = tariffHelper.CreateNewOrGetExistingTariffType(tariffCountryCode, Universal.Constants.TariffTypes.Export);
			factory.Save();

			var tariff = tariffHelper.CreateTariff(tariffCountryCode, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			tariffHelper.CreateTariffUOM(tariff, tariffUOMType, tariffUOM);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, receivePackType, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "MG", "", PreviousEntryNumber, 1);

			helper.Part.PivotsForBinding[0].CI_TariffNum = tariffCode;

			factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });

			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;

			header.ImportInventories();
		}

		protected override BusinessObject GetNewBusinessObject() => new ImportInventorySelectionHeader(Factory.New<JobDeclaration>());

		protected override void SetUp()
		{
			base.SetUp();
			helper = new WhsDataTestHelper(Factory);
			declaration = CreateImportJobDeclaration();
			header = new ImportInventorySelectionHeader(declaration);
		}
		JobDeclaration declaration;
		ImportInventorySelectionHeader header;
		WhsDataTestHelper helper;
		const string PreviousEntryNumber = "ENT1234";
		const string BondedEntryKey = "ENT1234-1";

		JobDeclaration CreateImportJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
			return declaration;
		}

		JobComInvoiceLine CreateInvoiceLine(JobDeclaration declaration, ZString previousEntryNumber)
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			return invoiceLine;
		}

		BaseJobComInvoiceHeader GetInvoice(JobDeclaration declaration, ZString currencyCode, ZString incotermCode, ZString incotermPlace, ZString valuationCode)
			=> declaration.Invoices.Cast<JobComInvoiceHeader>().SingleOrDefault(x =>
				x.JZ_RX_NKInvoice_Currency == currencyCode
				&& x.IncoTerm == incotermCode
				&& x.JZ_IncoTermPlace == incotermPlace
				&& x.JZ_ValuationCode == valuationCode);

		void AssertUpdateOutwardLinesWithInventoryDetails(Action<IWhsReceiveLine, IWhsBondedWarehouseAttribute, JobComInvoiceLine> mocker, Action<JobComInvoiceLine> assertion)
		{
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "56";
			invoiceLine.JI_CEI = entryInstruction.PK;
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);
			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);

			var inventory = receiveLine.Inventory;
			mocker?.Invoke(receiveLine, receiveLineCustomsData, invoiceLine);
			Factory.Save();

			var invoiceLines = new List<JobComInvoiceLine> { invoiceLine };
			header.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			CombineAssertions(() => assertion?.Invoke(invoiceLine));
		}

		void AddWarehouseCustomsAttributeAddInfo(IWhsBondedWarehouseAttribute customsData, string type, string addInfoData)
		{
			var addInfo = Factory.New<WarehouseCustomsAttributeAddInfo>();
			addInfo.B7_ParentTableCode = "WB";
			addInfo.B7_ParentID = customsData.PK;
			addInfo.B7_Type = type;
			addInfo.B7_AddInfoData = addInfoData;
		}
	}
}
