using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Customs.Forwarding.GUI.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.UnitedStates)]
	class ForwardingShipmentModuleCustomsColumnsProviderUSTest : ForwardingShipmentModuleCustomsColumnsProviderAbstractTest
	{
		public void TestUSColumns()
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

				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CRLStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.SEBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.HLDOrEXMStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.ENSStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.EXPStatus));
				AssertNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.EntryStatusDescription));
				AssertNotNull(FindColumnByName(grid, ForwardingShipmentCustomsColumnConstants.Schema.CustomsEntryAuthorisationDate));

				foreach (ZGridColumnInfo column in grid.ColumnStyles)
				{
					if (column.ColumnName == ForwardingShipmentCustomsColumnConstants.Schema.CustomsCargoStatus)
					{
						AssertEquals("CRL Status Desc.", column.Caption);
					}
					else if (column.ColumnName == ForwardingShipmentCustomsColumnConstants.Schema.CustomsMessageStatus)
					{
						AssertEquals("ENS Status Desc.", column.Caption);
					}
					else if (column.ColumnName == ForwardingShipmentCustomsColumnConstants.Schema.SEBillStatus)
					{
						AssertEquals("SE Bill Status Desc.", column.Caption);
					}
					else if (column.ColumnName == ForwardingShipmentCustomsColumnConstants.Schema.HLDOrEXMStatus)
					{
						AssertEquals("Bill Hold/Exam", column.Caption);
					}
				}
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

		public void TestBashFetchForView_CRLStatus()
		{
			BashFetchForView("CRLStatus", 0);
		}

		public void TestBashFetchForView_SEBillStatus()
		{
			BashFetchForView("SEBillStatus", 0);
		}

		public void TestBashFetchForView_HLDOrEXMStatus()
		{
			BashFetchForView("HLDOrEXMStatus", 0);
		}

		public void TestBashFetchForView_ENSStatus()
		{
			BashFetchForView("ENSStatus", 0);
		}

		public void TestBashFetchForView_EXPStatus()
		{
			BashFetchForView("EXPStatus", 0);
		}

		public void TestBashFetchForView_CustomsEntryType()
		{
			// JobDeclaration: 1

			BashFetchForView("CustomsEntryType", 1);
		}

		public void TestBashFetchForView_ITEntryType()
		{
			// JobDeclaration: 1

			BashFetchForView("ITEntryType", 1);
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

		public void TestShipmentToDeclarationMapCache_ShouldCacheToFactory()
		{
			var columnsProvider = new ForwardingShipmentModuleCustomsColumnsProvider();

			var cacheMissExpectedDbHits = new Dictionary<string, int>
			{
				{ JobDeclarationSchema.Constants.TableName, 1 },
				{ JobShipmentSchema.Constants.TableName, 1 },
				{ StmDataSchema.Constants.TableName, 2 },
				{ StmModuleFilterSchema.Constants.TableName, 1 },
			};

			var cacheHitExpectedDbHits = new Dictionary<string, int>
			{
				{ StmDataSchema.Constants.TableName, 2 },
				{ StmModuleFilterSchema.Constants.TableName, 1 },
			};

			const string cacheKey = "ForwardingShipmentModuleCustomsColumnsProvider.ShipmentToDeclarationMap";

			AssertNull("[PreCondition]: Cache should be empty", Factory.GetCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey, () => null));
			Factory.ClearCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey);

			using var form = new ZForm();
			form.Show();

			using (AssertDbHitsForAllFactories(cacheMissExpectedDbHits))
			{
				using (var grid = GetGridWithExplicitGetterColumn(Factory, form, columnsProvider))
				{
					grid.Refresh();
					AssertNotNull(Factory.GetCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey, () => null));
				}
			}

			using (AssertDbHitsForAllFactories(cacheHitExpectedDbHits))
			{
				using (var grid = GetGridWithExplicitGetterColumn(Factory, form, columnsProvider))
				{
					grid.Refresh();
					AssertNotNull(Factory.GetCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey, () => null));
				}
			}

			var newFactory = new BusinessObjectFactory();
			AssertNull("[PreCondition]: Cache should be empty", newFactory.GetCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey, () => null));
			newFactory.ClearCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey);

			using (AssertDbHitsForAllFactories(cacheMissExpectedDbHits))
			{
				using (var grid = GetGridWithExplicitGetterColumn(newFactory, form, columnsProvider))
				{
					grid.Refresh();
					AssertNotNull(newFactory.GetCachedValue<Dictionary<ZGuid, BusinessObject>>(cacheKey, () => null));
				}
			}
		}

		DumyZGrid GetGridWithExplicitGetterColumn(BusinessObjectFactory factory, ZForm form, ForwardingShipmentModuleCustomsColumnsProvider columnsProvider)
		{
			var objs = factory.LoadTop1<ForwardingShipment>(new ZQuery());
			var shipments = new ShipmentCollection(factory);
			shipments.AddRange(objs);

			var grid = new DumyZGrid();
			form.Controls.Add(grid);

			grid.SetDataBinding(shipments, "");

			columnsProvider.AddColumns(grid);

			grid.ColumnHeadersVisible = true;
			grid.Columns[ForwardingShipmentCustomsColumnConstants.Schema.CustomsEntryType].IsVisible = true;
			grid.RefreshTableStyles();

			return grid;
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var consignee = factory.NewWithValidTestData<OrgHeader>();
			var consignor = factory.NewWithValidTestData<OrgHeader>();

			var declarationType = ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>();
			var inBondType = ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondHeader>();

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

					var inbondHeader = factory.NewWithValidTestData(inBondType);
					inbondHeader[CusInBondHeaderSchema.BH_ParentID] = shipment.PK;
					inbondHeader[CusInBondHeaderSchema.BH_ParentTableCode] = shipment.TablePrefix;
					inbondHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.InBond;
					inbondHeader[CusInBondHeaderSchema.BH_GB] = GlbBranch.CurrentBranch.PK;

					result.Add(shipment.PK);
				}

				factory.Save();
			}

			return result.ToArray();
		}
	}
}
