using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.GUI.Testing
{
	class ForwardingShipmentColumnsProviderTest : GridCustomColumnsProviderAbstractTest<ForwardingShipment>
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
				var columnsProvider = new ForwardingShipmentColumnsProvider();
				columnsProvider.AddColumns(grid);
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsCargoStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsMessageStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatusDescription));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ACICargoStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ACIMessageStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.DestinationGoodsValue));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.DestinationCurrencyCode));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.DestinationExchangeRate));
			}
		}

		public void TestGBColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(shipments, "");
				var columnsProvider = new ForwardingShipmentColumnsProvider();
				columnsProvider.AddColumns(grid);
				var columnInfo = FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsCargoStatus);
				AssertEquals("GB Caption should be CCSUK", "CCS-UK Status", columnInfo.Caption);
			}
		}

		public void TestJPColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(shipments, "");
				var columnsProvider = new ForwardingShipmentColumnsProvider();
				columnsProvider.AddColumns(grid);
				CombineAssertions(() =>
				{
					var columnInfo = FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatus);
					AssertEquals("AFR Bill Status", columnInfo.Caption);
					Assert(!columnInfo.IsVisible);
					columnInfo = FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.AFRBillStatusDescription);
					AssertEquals("AFR Bill Status Desc.", columnInfo.Caption);
					Assert(!columnInfo.IsVisible);
				});
			}
		}

		ZGridColumnInfo FindColumnByName(DumyZGrid grid, string columnName)
		{
			return (from columnStyleInfo in grid.ColumnStyles.Cast<ZGridColumnInfo>()
					where columnStyleInfo.ColumnName == columnName
					select columnStyleInfo).FirstOrDefault();
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
			// CusCAeMHHouse: 1
			// CusSCAHouse: 1
			BashFetchForView("ACICargoStatus", 2);
		}

		public void TestBashFetchForView_ACIMessageStatus()
		{
			// CusCAeMHHouse: 1
			// CusSCAHouse: 1
			BashFetchForView("ACIMessageStatus", 2);
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

		protected override SchemaPKColumn PkColumn => JobShipmentSchema.PK;

		protected override GridCustomColumnsProvider GetGridCustomColumnsProvider()
		{
			return new ForwardingShipmentColumnsProvider();
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

	public class DumyZGrid : ZGrid, Integration.ZArchitecture.IGridControl
	{
		public DumyZGrid()
			: base()
		{
			var column = new ZTextBoxColumnStyleInfo { ColumnName = "FakeColumn", Caption = "Fake" };
			Columns.Add(column);
		}
	}
}
