using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Customs.Forwarding.GUI.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	class ForwardingShipmentModuleCustomsColumnsProviderTest : ForwardingShipmentModuleCustomsColumnsProviderAbstractTest
	{
		public void TestAddedColumns()
		{
			var shipments = new ShipmentCollection(Factory);

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(shipments, "");
				var columnsProvider = new ForwardingShipmentModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsCargoStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsMessageStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ISFBillNumber));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ISFBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ISFBillStatusDescription));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatusDescription));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ACICargoStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ACIMessageStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.EntryStatusDescription));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsEntryAuthorisationDate));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.DestinationGoodsValue));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.DestinationCurrencyCode));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.DestinationExchangeRate));
			}
		}

		public void TestUSColumnsDontShowForNonUSFilters()
		{
			var shipments = new ShipmentCollection(Factory);

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(shipments, "");
				var columnsProvider = new ForwardingShipmentModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);
				bool foundUSFields = false;

				foreach (ZGridColumnInfo column in grid.ColumnStyles)
				{
					if (column.ColumnName == "CustomsEntryType")
					{
						foundUSFields = true;
						Assert("US Column CustomsEntryType should not exist", false);
					}
					if (column.ColumnName == "ITEntryType")
					{
						foundUSFields = true;
						Assert("US Column ITEntryType should not exist", false);
					}
				}

				AssertEquals("US Customs Columns should not be visible", false, foundUSFields);
			}
		}

		public void TestISFBillData()
		{
			var foundISFBillNumber = false;
			var foundISFBillStatus = false;
			var foundISFBillStatusDescription = false;

			var shipments = new ShipmentCollection(Factory);

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(shipments, "");
				var columnsProvider = new ForwardingShipmentModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				foreach (ZGridColumnInfo column in grid.ColumnStyles)
				{
					switch (column.ColumnName)
					{
						case "ISFBillStatus":
							foundISFBillStatus = true;
							break;
						case "ISFBillStatusDescription":
							foundISFBillStatusDescription = true;
							break;
						case "ISFBillNumber":
							foundISFBillNumber = true;
							break;
					}
				}

				AssertEquals("ISF Bill Number should be visible", true, foundISFBillNumber);
				AssertEquals("ISF Bill Status should be visible", true, foundISFBillStatus);
				AssertEquals("ISF Bill Status Description should be visible", true, foundISFBillStatusDescription);
			}
		}

		public void TestBashFetchForView_CustomsCargoStatus()
		{
			// CusHAWB: 12

			BashFetchForView("CustomsCargoStatus", 12);
		}

		public void TestBashFetchForView_CustomsMessageStatus()
		{
			// CusHAWB: 12

			BashFetchForView("CustomsMessageStatus", 12);
		}

		public void TestBashFetchForView_ISFBillNumber()
		{
			BashFetchForView("ISFBillNumber", 0);
		}

		public void TestBashFetchForView_ISFBillStatus()
		{
			BashFetchForView("ISFBillStatus", 0);
		}

		public void TestBashFetchForView_ISFBillStatusDescription()
		{
			BashFetchForView("ISFBillStatusDescription", 0);
		}

		public void TestBashFetchForView_AFRBillStatus()
		{
			// JobConsol: 12
			// JobConShipLink: 1

			BashFetchForView("AFRBillStatus", 13);
		}

		public void TestBashFetchForView_AFRBillStatusDescription()
		{
			// JobConsol: 12
			// JobConShipLink: 1

			BashFetchForView("AFRBillStatusDescription", 13);
		}

		public void TestBashFetchForView_ACICargoStatus()
		{
			// CusSCAHouse: 1

			BashFetchForView("ACICargoStatus", 1);
		}

		public void TestBashFetchForView_ACIMessageStatus()
		{
			// CusSCAHouse: 1

			BashFetchForView("ACIMessageStatus", 1);
		}

		public void TestBashFetchForView_EManifestCargoStatus()
		{
			// CusCAeMHHouse: 1

			BashFetchForView("EManifestCargoStatus", 1);
		}

		public void TestBashFetchForView_EManifestMessageStatus()
		{
			// CusCAeMHHouse: 1

			BashFetchForView("EManifestMessageStatus", 1);
		}

		public void TestBashFetchForView_CustomsEntryAuthorisationDate()
		{
			// JobDeclaration: 1

			BashFetchForView("CustomsEntryAuthorisationDate", 1);
		}

		public void TestBashFetchForView_DestinationGoodsValue()
		{
			BashFetchForView("DestinationGoodsValue", 0);
		}

		public void TestBashFetchForView_DestinationCurrencyCode()
		{
			BashFetchForView("DestinationCurrencyCode", 0);
		}

		public void TestBashFetchForView_DestinationExchangeRate()
		{
			BashFetchForView("DestinationExchangeRate", 0);
		}

		public void TestBashFetchForView_EntryStatusDescription()
		{
			// JobComInvoiceHeader: 47
			// JobDocAddress: 13
			// CusContainer: 12
			// CusDecHouseBill: 12
			// CusEntryHeader: 12
			// CusEntryNum: 12
			// EDIMessage: 12
			// JobComInvHeaderCharge: 12
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobSailing: 12
			// JobVoyage: 12
			// JobVoyDestination: 12
			// JobVoyOrigin: 12
			// StmALog: 12
			// OrgAddress: 5
			// OrgContact: 2
			// OrgMiscServ: 2
			// OrgRelatedParty: 2
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1
			// OrgSupplierBuyerLink: 1

			BashFetchForView("EntryStatusDescription", 258);
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			var consignee = factory.NewWithValidTestData<OrgHeader>();
			var consignor = factory.NewWithValidTestData<OrgHeader>();
			var declarationType = ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>();

			factory.Save();

			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				for (var i = 0; i < 12; i++)
				{
					var shipment = factory.New<ForwardingShipment>();
					shipment.FillWithValidTestData();
					shipment.JS_TransportMode = "AIR";
					shipment.JS_UniqueConsignRef = "shipment" + i;

					shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

					var declaration = factory.NewWithValidTestData(declarationType);
					declaration[JobDeclarationSchema.JE_DeclarationReference] = "decl" + i;
					declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

					result.Add(shipment.PK);
				}

				factory.Save();
			}

			return result.ToArray();
		}
	}
}
