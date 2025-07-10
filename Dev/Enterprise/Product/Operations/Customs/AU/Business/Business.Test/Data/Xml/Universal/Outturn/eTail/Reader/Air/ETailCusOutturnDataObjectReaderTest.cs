using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ETailCusOutturnDataObjectReaderTest : DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		public void TestImportingData_NIL()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Type = "HVLVConsignment",
						Key = "HB1"
					}
				},
				TotalNoOfPieces = 2,
				GoodsDescription = "Goods",
			};
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 0,
						OutturnPillagedQty = 0,
						OutturnQty = 1
					},
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 0,
						OutturnPillagedQty = 0,
						OutturnQty = 1
					}
				});

			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HB1";
			var reader = new ETailCusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("underbond.C5_ParentID", cusHAWB.PK, outturn.C5_ParentID);
				AssertEquals("underbond.C5_C4_Underbond", underbond.PK, outturn.C5_C4_Underbond);
				AssertEquals("underbond.C5_OutturnResultType", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
				AssertEquals("underbond.C5_OuterPacks", 2, outturn.C5_OuterPacks);
				AssertEquals("underbond.C5_PackagesOutturned", 2, outturn.C5_PackagesOutturned);
				AssertEquals("underbond.C5_DamageIndicator", ZBool.False, outturn.C5_DamageIndicator);
				AssertEquals("underbond.C5_PillageIndicator", ZBool.False, outturn.C5_PillageIndicator);
				AssertEquals("underbond.C5_GoodsDescription", "Goods", outturn.C5_GoodsDescription);
			});
		}

		public void TestImportingData_SH()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Type = "HVLVConsignment",
						Key = "HB1"
					}
				},
				TotalNoOfPieces = 2,
				GoodsDescription = "Goods",
			};
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 0,
						OutturnPillagedQty = 0,
						OutturnQty = 0
					},
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 1,
						OutturnPillagedQty = 1,
						OutturnQty = 1
					}
				});

			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();

			cusHAWB.CS_HAWB = "HB1";
			var reader = new ETailCusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("underbond.C5_ParentID", cusHAWB.PK, outturn.C5_ParentID);
				AssertEquals("underbond.C5_C4_Underbond", underbond.PK, outturn.C5_C4_Underbond);
				AssertEquals("underbond.C5_OutturnResultType", CMROutturnResultType.Codes.ShortLanded, outturn.C5_OutturnResultType);
				AssertEquals("underbond.C5_OuterPacks", 2, outturn.C5_OuterPacks);
				AssertEquals("underbond.C5_PackagesOutturned", 1, outturn.C5_PackagesOutturned);
				AssertEquals("underbond.C5_DamageIndicator", ZBool.True, outturn.C5_DamageIndicator);
				AssertEquals("underbond.C5_PillageIndicator", ZBool.True, outturn.C5_PillageIndicator);
				AssertEquals("underbond.C5_GoodsDescription", "Goods", outturn.C5_GoodsDescription);
			});
		}

		public void TestImportingData_SP()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Type = "HVLVConsignment",
						Key = "HB1"
					}
				},
				TotalNoOfPieces = 1,
				GoodsDescription = "Goods",
			};
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnDamagedQty = 1,
					OutturnPillagedQty = 1,
					OutturnQty = 1
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnDamagedQty = 1,
					OutturnPillagedQty = 1,
					OutturnQty = 1
				}
			});

			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			cusMAWB.CM_MAWB = "MB1";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HB1";

			underbond.C4_ResponsiblePartyID = "RE123";
			var reader = new ETailCusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("underbond.C5_ParentID", cusHAWB.PK, outturn.C5_ParentID);
				AssertEquals("underbond.C5_C4_Underbond", underbond.PK, outturn.C5_C4_Underbond);
				AssertEquals("underbond.C5_OutturnResultType", CMROutturnResultType.Codes.SurplusPackages, outturn.C5_OutturnResultType);
				AssertEquals("underbond.C5_OuterPacks", 1, outturn.C5_OuterPacks);
				AssertEquals("underbond.C5_PackagesOutturned", 2, outturn.C5_PackagesOutturned);
				AssertEquals("underbond.C5_DamageIndicator", ZBool.True, outturn.C5_DamageIndicator);
				AssertEquals("underbond.C5_PillageIndicator", ZBool.True, outturn.C5_PillageIndicator);
				AssertEquals("underbond.C5_GoodsDescription", "Goods", outturn.C5_GoodsDescription);
			});
		}

		public void TestImportingData_SC()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Type = "HVLVConsignment",
						Key = "HB1"
					}
				},
				TotalNoOfPieces = 1,
				GoodsDescription = "Goods",
				GoodsValue = 100,
				TotalWeight = 25,
				TotalWeightUnit = new UnitOfWeight
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = "Kilograms"
				}
			};
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 0,
						OutturnPillagedQty = 0,
						OutturnQty = 1
					}
				});
			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HB2";
			var reader = new ETailCusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			AssertEquals("A new House Bill should have been created", 2, cusMAWB.ChildBills.Count);
			AssertEquals("Goods Description should have been set", "Goods", cusMAWB.ChildBills[1].CS_GoodsDescription);
			AssertEquals("Goods Value should have been set", 100m, cusMAWB.ChildBills[1].CS_GoodsValue);
			AssertEquals("Weight should have been set", 25m, cusMAWB.ChildBills[1].CS_Weight);
			AssertEquals("Weight unit should have been set", "KG", cusMAWB.ChildBills[1].CS_WeightUQ);

			CombineAssertions(() =>
			{
				AssertEquals("underbond.C5_HouseBill", cusMAWB.ChildBills[1].PK, outturn.C5_ParentID);
				AssertEquals("underbond.C5_C4_Underbond", underbond.PK, outturn.C5_C4_Underbond);
				AssertEquals("underbond.C5_OutturnResultType", CMROutturnResultType.Codes.SurplusConsignment, outturn.C5_OutturnResultType);
				AssertEquals("underbond.C5_OuterPacks", 1, outturn.C5_OuterPacks);
				AssertEquals("underbond.C5_PackagesOutturned", 1, outturn.C5_PackagesOutturned);
				AssertEquals("underbond.C5_DamageIndicator", ZBool.False, outturn.C5_DamageIndicator);
				AssertEquals("underbond.C5_PillageIndicator", ZBool.False, outturn.C5_PillageIndicator);
				AssertEquals("underbond.C5_GoodsDescription", "Goods", outturn.C5_GoodsDescription);
			});
		}

		public void TestImportingData_WayBillTypeHWB()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Type = "HVLVConsignment",
						Key = "HB1"
					}
				},
				WayBillType = new WayBillType { Code = "HWB" },
				WayBillNumber = "waybill",
				TotalNoOfPieces = 1,
				GoodsDescription = "Goods",
				GoodsValue = 100,
				TotalWeight = 25,
				TotalWeightUnit = new UnitOfWeight
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = "Kilograms"
				},
			};
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 0,
						OutturnPillagedQty = 0,
						OutturnQty = 1
					}
				});
			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "waybill";
			var reader = new ETailCusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			AssertEquals("If WayBillType is HWB, housebill should be set against WayBillNumber", cusHAWB.PK, outturn.C5_ParentID);
		}

		public void TestImportingData_WayBillTypeMWB()
		{
			var shipment = new UShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Type = "HVLVConsignment",
						Key = "HB1"
					}
				},
				WayBillType = new WayBillType { Code = "MWB" },
				WayBillNumber = "waybill",
				TotalNoOfPieces = 1,
				GoodsDescription = "Goods",
				GoodsValue = 100,
				TotalWeight = 25,
				TotalWeightUnit = new UnitOfWeight
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = "Kilograms"
				},
			};
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnDamagedQty = 0,
						OutturnPillagedQty = 0,
						OutturnQty = 1
					}
				});
			Factory.SaveForTesting();

			var cusMAWB = Factory.New<CusMAWB>();
			var underbond = cusMAWB.Underbonds.AddNew();
			underbond.C4_ResponsiblePartyID = "RE123";
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HB1";
			var reader = new ETailCusOutturnDataObjectReader(shipment, logger, Factory, underbond);
			var outturn = reader.ReadIntoBusinessObject();

			AssertEquals("If WayBillType is MWB, housebill should be set against the DataSource", cusHAWB.PK, outturn.C5_ParentID);
		}
	}
}
