using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	public sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFillInventoryDetails_SecondCustomsQuantity()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 1m;
					customsData.WB_CustomsSecondQuantity = 22m;
					customsData.WB_CustomsSecondUnitQty = "TON";
				},
				assertion: invoiceLine =>
				{
					AssertEquals(22m / 10, invoiceLine.JI_CustomsSecondQuantity);
					AssertEquals("TON", invoiceLine.JI_CustomsSecondUnitQty);
				});
		}

		public void TestFillInventoryDetails_ThirdCustomsQuantity()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 1m;
					customsData.WB_CustomsThirdQuantity = 33m;
					customsData.WB_CustomsThirdUnitQty = "TON";
				},
				assertion: invoiceLine =>
				{
					AssertEquals(33m / 10, invoiceLine.JI_CustomsThirdQuantity);
					AssertEquals("TON", invoiceLine.JI_CustomsThirdUnitQty);
				});
		}

		[TestDate(2021, 02, 23)]
		public void TestFillInventoryDetails_LinePrice()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 5m;
					customsData.WB_ValueForDuty = 1000m;
					customsData.WB_AddInfo = "*LinePrice=100*LinePriceCurrency=USD";
				},
				assertion: invoiceLine => AssertEquals(35.97m, invoiceLine.JI_LinePrice));  // 5 / 10 * 100 * exchange rate (USD => EUR) = 35.97
		}

		public void TestFillInventoryDetails_LinePriceEmptyAtExport()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				isImport: false,
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 5m;
					invoiceLine.JI_LinePrice = 50m;
					customsData.WB_ValueForDuty = 1000m;
					customsData.WB_AddInfo = "*LinePrice=100*LinePriceCurrency=USD";
				},
				assertion: invoiceLine => AssertEquals("JI_LinePrice should not be defaulted at export.", 0m, invoiceLine.JI_LinePrice));
		}

		public void TestFillInventoryDetails_CountryOfSupply()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) => customsData.WB_AddInfo = "*CountryOfSupply=NZ",
				assertion: invoiceLine => AssertEquals("NZ", invoiceLine.ZG_CountryOfSupply)
			);
		}

		public void TestFillInventoryDetails_ValuationCode()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) => customsData.WB_AddInfo = "*ValuationCode=3",
				assertion: invoiceLine => AssertEquals("3", invoiceLine.JI_ValuationCode)
			);
		}

		public void TestFillInventoryDetails_SupplementaryCodes()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					AddWarehouseCustomsAttributeAddInfo(customsData, "SUP", "*Code=SUP1*Order=1");
					AddWarehouseCustomsAttributeAddInfo(customsData, "SUP", "*Code=SUP2*Order=2");
					AddWarehouseCustomsAttributeAddInfo(customsData, "SUP", "*Code=SUP3*Order=3");
					AddWarehouseCustomsAttributeAddInfo(customsData, "SUP", "*Code=SUP4*Order=4");
				},
				assertion: invoiceLine =>
				{
					AssertEquals("SUP1", invoiceLine.JI_SupplementaryCode1);
					AssertEquals("SUP2", invoiceLine.JI_SupplementaryCode2);
					AssertContainsExactElementsInAnyOrder(new[] { "SUP3", "SUP4" }, invoiceLine.AdditionalSupplementaryCodes.Cast<CusCodeData>().Select(x => x.CY_Code));
				});
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
					AssertEquals(true, add.J7_IsDutiable);
					AssertEquals(true, add.J7_IsGSTApplicable);
					AssertEquals(true, add.J7_IsIncludedInITOT);
					AssertEquals(true, add.J7_IsStatisticalValueApplicable);

					var ded = invoiceLine.Charges.Cast<JobComInvCharge>().Single(x => x.J7_ChargeType == "DED");
					AssertEquals(320m / 5, ded.J7_Amount);
					AssertEquals("USD", ded.J7_RX_NKCurrency);
					AssertEquals(false, ded.J7_IsDutiable);
					AssertEquals(false, ded.J7_IsGSTApplicable);
					AssertEquals(false, ded.J7_IsIncludedInITOT);
					AssertEquals(true, ded.J7_IsStatisticalValueApplicable);
				});
		}

		public void TestFillInventoryDetails_ChargesEmptyAtExport()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				isImport: false,
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 2m;
					invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					invoiceLine.Charges.AddNew();
					AddWarehouseCustomsAttributeAddInfo(customsData, "CCT", "*Amount=240*ChargeType=ADD*Currency=CNY*IsDutiable=Y*IsGSTApplicable=Y*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y");
					AddWarehouseCustomsAttributeAddInfo(customsData, "CCT", "*Amount=320*ChargeType=DED*Currency=USD*IsDutiable=*IsGSTApplicable=*IsIncludedInITOT=*IsStatisticalValueApplicable=Y");
				},
				assertion: invoiceLine =>
				{
					var charges = invoiceLine.Charges.Cast<JobComInvCharge>();
					AssertEquals("No charge should be created at export.", 0, charges.Count());
				});
		}

		public void TestFillInventoryDetails_Procedure_Import()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Helper.WhsHelper.EnableWarehouseForBond(Helper.WhsWarehouse, true);
				Helper.WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = Helper.WhsHelper.CreateWhsArea(Helper.WhsWarehouse.PK, "IPR", "IPR");
				var row = Helper.WhsHelper.CreateRowAndGenerateLocations(Helper.WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = Helper.Importer.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;

				var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				receive.WD_IsInwardsProcessingJob = true;
				var receiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(receive.PK, Helper.Part.PK, vfd: 50000m, qty: 500m, location.PK, 3, "EN00123");
				var inventory = receiveLine.Inventory;
				Factory.Save();

				var inventorySelectionHeader = new InventorySelectionHeader(declaration);
				var wrapper = new WhsInventoryWrapper(inventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Single();

				AssertEquals("Procedure set on Select Inventory", "4051000", invoiceLine.JI_Procedure);
			}
		}

		public void TestFillInventoryDetails_Procedure_Export()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Helper.WhsHelper.EnableWarehouseForBond(Helper.WhsWarehouse, true);
				Helper.WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = Helper.WhsHelper.CreateWhsArea(Helper.WhsWarehouse.PK, "IPR", "IPR");
				var row = Helper.WhsHelper.CreateRowAndGenerateLocations(Helper.WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Supplier = Helper.Importer.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;

				var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				receive.WD_IsInwardsProcessingJob = true;
				var receiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(receive.PK, Helper.Part.PK, vfd: 50000m, qty: 500m, location.PK, 3, "EN00123");
				var inventory = receiveLine.Inventory;
				Factory.Save();

				var inventorySelectionHeader = new InventorySelectionHeader(declaration);
				var wrapper = new WhsInventoryWrapper(inventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Single();

				AssertEquals("Procedure set on Select Inventory", "3151000", invoiceLine.JI_Procedure);
			}
		}

		public void TestFillInventoryDetails_PreviousDocuments()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Helper.WhsHelper.EnableWarehouseForBond(Helper.WhsWarehouse, true);
				Helper.WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = Helper.WhsHelper.CreateWhsArea(Helper.WhsWarehouse.PK, "IPR", "IPR");
				var row = Helper.WhsHelper.CreateRowAndGenerateLocations(Helper.WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = Helper.Importer.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;

				var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				receive.WD_IsInwardsProcessingJob = true;
				var receiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(receive.PK, Helper.Part.PK, vfd: 50000m, qty: 500m, location.PK, 3, "EN00123");
				var inventory = receiveLine.Inventory;
				Factory.Save();

				var inventorySelectionHeader = new InventorySelectionHeaderForTest(declaration);
				var wrapper = new WhsInventoryWrapper(inventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single();

				CombineAssertions(() =>
				{
					AssertEquals("Previous Document count after Select Inventory", 1, invoiceLine.PreviousDocuments.Count);
					AssertEquals("No Errors on Update", ZString.Empty, inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>()));
					AssertEquals("Previous Document count after Synchronise with Inventory", 2, invoiceLine.PreviousDocuments.Count);
				});
			}
		}

		public void TestFillInventoryDetails_FillLineGrossWeightAndUnitIfFoundInAddInfo()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 5m;
					customsData.WB_AddInfo = "LineGrossWeight=100.528*LineGrossWeightUnit=G";
				},
				assertion: invoiceLine =>
				{
					AssertEquals("JI_Weight", 50.264m, invoiceLine.JI_Weight);
					AssertEquals("JI_WeightUQ", "G", invoiceLine.JI_WeightUQ);
				});
		}

		public void TestFillInventoryDetails_DoNotFillLineGrossWeightAndUnitIfNotFoundInAddInfo()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					invoiceLine.JI_WeightUQ = "";
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 5m;
					customsData.WB_AddInfo = "";
				},
				assertion: invoiceLine =>
				{
					AssertEquals("JI_Weight", 0m, invoiceLine.JI_Weight);
					AssertEquals("JI_WeightUQ", "", invoiceLine.JI_WeightUQ);
				});
		}

		public void TestFillInventoryDetails_FillNetWeightAndUnitIfCustomsQuantityIsKGM()
		{
			ZArchitecture.Environment.DataRegistry.Instance.PackageWeightUnit = "G";

			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					customsData.WB_CustomsQty = 100m;
					customsData.WB_CustomsUnitOfQty = "KGM";
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 5m;
				},
				assertion: invoiceLine =>
				{
					AssertEquals("JI_NetWeight", 50m, invoiceLine.JI_NetWeight);
					AssertEquals("JI_NetWeightUQ", "KG", invoiceLine.JI_NetWeightUQ);
				});
		}

		public void TestFillInventoryDetails_DoNotFillNetWeightAndUnitIfCustomsQuantityIsNotKGM()
		{
			AssertUpdateOutwardLinesWithInventoryDetails(
				mocker: (receiveLine, customsData, invoiceLine) =>
				{
					invoiceLine.JI_NetWeightUQ = "";
					customsData.WB_CustomsQty = 100m;
					customsData.WB_CustomsUnitOfQty = invoiceLine.JI_CustomsUnitQty = "ASV";
					receiveLine.WE_TransactionQuantity = 10m;
					invoiceLine.JI_BondedWhsQuantity = 5m;
				},
				assertion: invoiceLine =>
				{
					AssertEquals("JI_NetWeight", 0m, invoiceLine.JI_NetWeight);
					AssertEquals("JI_NetWeightUQ", "", invoiceLine.JI_NetWeightUQ);
				});
		}

		void AssertUpdateOutwardLinesWithInventoryDetails(Action<IWhsReceiveLine, IWhsBondedWarehouseAttribute, JobComInvoiceLine> mocker, Action<JobComInvoiceLine> assertion, bool isImport = true)
		{
			var declaration = Factory.New<JobDeclaration>();
			ZGuid clientPK = isImport ? Helper.Importer.PK : Helper.Supplier.PK ;
			if (isImport)
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = clientPK;
			}
			else
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Supplier = clientPK;
			}

			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = "ENT1234";
			invoiceLine.JI_PreviousEntryLineNumber = 1;

			var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, clientPK);
			var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);
			var inventory = receiveLine.Inventory;
			mocker?.Invoke(receiveLine, receiveLineCustomsData, invoiceLine);
			Factory.Save();

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<JobComInvoiceLine> { invoiceLine };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
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

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InventorySelectionHeader(Factory.New<JobDeclaration>());
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		class InventorySelectionHeaderForTest : InventorySelectionHeader
		{
			public InventorySelectionHeaderForTest(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			protected override void FillPreviousDocuments(JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine)
			{
				base.FillPreviousDocuments(invoiceLine, whsReceiveLine);
				invoiceLine.PreviousDocuments.AddNew();
			}
		}
	}
}
