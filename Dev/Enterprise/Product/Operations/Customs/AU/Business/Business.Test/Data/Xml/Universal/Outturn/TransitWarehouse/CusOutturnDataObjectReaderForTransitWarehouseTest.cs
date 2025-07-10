using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusOutturnDataObjectReaderForTransitWarehouseTest : DataTransfer.Universal.Testing.DataObjectReaderTestHelper
	{
		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_NilDiscrepancy() => TestReadIntoBusinessObjectCore(5, 5, 0, 0);

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_ShortLanded_Damaged() => TestReadIntoBusinessObjectCore(5, 3, 2, 0);

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_SurplusPackages_Pilliged() => TestReadIntoBusinessObjectCore(5, 5, 0, 2);

		public void TestReadIntoBusinessObject_MultiPackLines()
		{
			var (hawb, underbond) = CreateTestUnderbond();
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 5, 5, 10.0m, "M3", 10.0m, "KG", 0, 0, "Test GoodDescription");
			shipment.PackingLineCollection.Clear();

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);

			AssertExceptionThrown<DataObjectReadFailureException>("Should throw error",
	"Can not update Cargo line because UXML received doesn't contain any packline information.", () => reader.ReadIntoBusinessObject());
		}

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_MatchOneOutturn_UpdateOutturn()
		{
			var (hawb, underbond) = CreateTestUnderbond();

			var existingOutturn = underbond.Outturns.AddNew();
			existingOutturn.C5_MasterBill = "MB001";
			existingOutturn.C5_HouseBill = "HB001";
			existingOutturn.C5_ParentID = hawb.PK;
			existingOutturn.C5_ParentTableCode = "CS";

			var originReceiptDate = DateTime.Now.AddDays(-5);

			existingOutturn.C5_OuterPacks = 0;
			existingOutturn.C5_PackagesOutturned = 0;
			existingOutturn.C5_DamageIndicator = false;
			existingOutturn.C5_PillageIndicator = false;
			existingOutturn.C5_VolumeOutturnedUQ = "L";
			existingOutturn.C5_WeightOutturnedUQ = "G";
			existingOutturn.C5_CargoReceiptDate = originReceiptDate;
			existingOutturn.C5_CargoUnpackDate = originReceiptDate.AddDays(-2);

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 5, 5, 10.0m, "M3", 10.0m, "KG", 1, 0, "Test GoodDescription", setAddInfo: true);

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);
			var updatedOutturn = reader.ReadIntoBusinessObject();

			AssertContains("Should have log", "Matching Cargo Line found for Master Bill: MB001 and House Bill: HB001.", logger.Logs);

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);

			AssertEquals("Should not create new outturn", 1, outturns.Length);
			AssertEquals("Should update existing outturn", updatedOutturn.PK, existingOutturn.PK);
			AssertOutturn(outturns[0], hawb, "MB001", "HB001", unpackDate, originReceiptDate, 5, 5, 10.0m, "CU", 10.0m, "KG", true, false, "NIL", "Test GoodDescription");
		}

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_MatchTwoOutturns()
		{
			var (hawb, underbond) = CreateTestUnderbond();

			var existingOutturn = underbond.Outturns.AddNew();
			existingOutturn.C5_MasterBill = "MB001";
			existingOutturn.C5_HouseBill = "HB001";
			existingOutturn.C5_ParentID = hawb.PK;
			existingOutturn.C5_ParentTableCode = "CS";

			var existingOutturn2 = underbond.Outturns.AddNew();
			existingOutturn2.C5_MasterBill = "MB001";
			existingOutturn2.C5_HouseBill = "HB001";
			existingOutturn2.C5_ParentID = hawb.PK;
			existingOutturn2.C5_ParentTableCode = "CS";

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 5, 5, 10.0m, "M3", 10.0m, "KG", 0, 0, "Test GoodDescription");

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);

			AssertExceptionThrown<DataObjectReadFailureException>("Should throw error",
	"Multiple matching Cargo Lines found for Master Bill: MB001 and House Bill: HB001.", () => reader.ReadIntoBusinessObject());
		}

		[TestDate(2022, 5, 11)]
		public void TestReadIntoBusinessObject_OuterPacksShouldNotBeUpdated_WhenHasValue()
		{
			var (hawb, underbond) = CreateTestUnderbond();

			var existingOutturn = underbond.Outturns.AddNew();
			existingOutturn.C5_MasterBill = "MB001";
			existingOutturn.C5_HouseBill = "HB001";
			existingOutturn.C5_ParentID = hawb.PK;
			existingOutturn.C5_ParentTableCode = "CS";

			var originReceiptDate = DateTime.Now.AddDays(-5);

			existingOutturn.C5_OuterPacks = 5;
			existingOutturn.C5_PackagesOutturned = 3;
			existingOutturn.C5_CargoReceiptDate = originReceiptDate;
			existingOutturn.C5_CargoUnpackDate = originReceiptDate.AddDays(-2);

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 10, 5, 10.0m, "M3", 10.0m, "KG", 0, 0, "Test GoodDescription", setAddInfo: true);

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);
			var updatedOutturn = reader.ReadIntoBusinessObject();

			AssertContains("Should have log", "Matching Cargo Line found for Master Bill: MB001 and House Bill: HB001.", logger.Logs);

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);

			AssertOutturn(outturns[0], hawb, "MB001", "HB001", unpackDate, originReceiptDate, 5, 5, 10.0m, "CU", 10.0m, "KG", false, false, "NIL", "Test GoodDescription");
		}

		public void TestReadIntoBusinessObject_NoHouseBill()
		{
			var (hawb, underbond) = CreateTestUnderbond();
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 5, 5, 10.0m, "M3", 10.0m, "KG", 0, 0, "Test GoodDescription");

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 3,
					PackQty = 3,
					OutturnPillagedQty = 0,
					OutturnDamagedQty = 0,
					OutturnedVolume = 15,
					VolumeUnit = new UnitOfVolume()
					{
						Code = "M3"
					},
					OutturnedWeight = 30,
					WeightUnit = new UnitOfWeight()
					{
						Code = "KG"
					},
					GoodsDescription = "Test1"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = 2,
					PackQty = 2,
					OutturnPillagedQty = 0,
					OutturnDamagedQty = 0,
					OutturnedVolume = 15,
					VolumeUnit = new UnitOfVolume()
					{
						Code = "M3"
					},
					OutturnedWeight = 30,
					WeightUnit = new UnitOfWeight()
					{
						Code = "KG"
					},
					GoodsDescription = "Test2"
				}
			});

			foreach (var packline in shipment.PackingLineCollection)
			{
				packline.SetAddInfoCollection(() => new List<UAddInfo>
					{
						new UAddInfo
						{
							Key = AddInfoKeyTypes.Types.IsManifestedPackage,
							Value = true.ToString()
						}
					});
			}

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);
			reader.ReadIntoBusinessObject();

			AssertContains("Should have log", "No matching Cargo Lines found for Master Bill: MB001 and House Bill: HB001.", logger.Logs);

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);

			AssertEquals("Should create 1 outturn", 1, outturns.Length);
			AssertOutturn(outturns[0], hawb, "MB001", "HB001", unpackDate, unpackDate, 5, 5, 30.0m, "CU", 60.0m, "KG", false, false, "NIL", "Test1, Test2");
		}

		public void TestReadIntoBusinessObject_NoPackLine()
		{
			var (hawb, underbond) = CreateTestUnderbond();
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 5, 5, 10.0m, "M3", 10.0m, "KG", 0, 0, "Test GoodDescription");
			shipment.PackingLineCollection.Clear();

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);

			AssertExceptionThrown<DataObjectReadFailureException>("Should throw error",
	"Can not update Cargo line because UXML received doesn't contain any packline information.", () => reader.ReadIntoBusinessObject());
		}

		void TestReadIntoBusinessObjectCore(int packQty, int outturnQty, int damagedQty, int pillagedQty)
		{
			var (hawb, underbond) = CreateTestUnderbond();
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, packQty, outturnQty, 10.0m, "M3", 10.0m, "KG", damagedQty, pillagedQty, "Test GoodDescription", setAddInfo: true);

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, hawb);
			reader.ReadIntoBusinessObject();

			AssertContains("Should have log", "No matching Cargo Lines found for Master Bill: MB001 and House Bill: HB001.", logger.Logs);

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);

			var expectedResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			if (outturnQty < packQty)
			{
				expectedResultType = CMROutturnResultType.Codes.ShortLanded;
			}
			else if (outturnQty > packQty)
			{
				expectedResultType = CMROutturnResultType.Codes.SurplusPackages;
			}
			var expectedPillagedFlag = pillagedQty > 0;
			var expectedDamagedFlag = damagedQty > 0;

			AssertEquals("Should create 1 outturn", 1, outturns.Length);
			AssertOutturn(outturns[0], hawb, "MB001", "HB001", unpackDate, unpackDate, packQty, outturnQty, 10.0m, "CU", 10.0m, "KG", expectedDamagedFlag, expectedPillagedFlag, expectedResultType, "Test GoodDescription");
		}

		public void TestReadIntoBusinessObject_WhenCusHAWBIsNull()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "MB001";

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.UnderBond, null);
			var unpackDate = DateTime.Now.AddDays(-2);
			var shipment = CreateTestTransitShipment(dataContext, "MB001", "HB001", unpackDate, 5, 5, 10.0m, "M3", 10.0m, "KG", 1, 0, "Test GoodDescription");

			var reader = new CusOutturnDataObjectReaderForTransitWarehouse(shipment, logger, Factory, underbond, null);
			var updatedOutturn = reader.ReadIntoBusinessObject();

			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			var outturns = Factory.Load<CusOutturn>(query);

			AssertEquals("Should create new outturn", 1, outturns.Length);
			var outturn = outturns[0];
			AssertEquals("Should have master bill", "MB001", outturn.C5_MasterBill);
			AssertEquals("C5_ParentTableCode should be empty", "", outturn.C5_ParentTableCode);
			AssertEquals("C5_ParentID should be empty", Guid.Empty, outturn.C5_ParentID);
		}

		(CusHAWB hawb, CusUnderbond underbond) CreateTestUnderbond()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB001";
			mawb.CM_ApplicationCode = "CMR";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB001";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = "CM";
			return (hawb, underbond);
		}

		static Shipment CreateTestTransitShipment(IDataContextDataObject dataContext, string masterBill, string houseBill, DateTime unpackDate, long packQty, int outturnQty, decimal volume, string volumeUQ, decimal weight, string weightUQ, int damagedQty, int pillagedQty, string goodsDescription, bool setAddInfo = false)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = houseBill
			};

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new UniversalDataBuss.DataObjects.Universal.EntryType
						{
							Code = AdditionalReferenceTypes.Codes.MasterBill,
							Description = AdditionalReferenceTypes.Descriptions.MasterBill
						},
						ReferenceNumber = masterBill
					}
				});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OutturnQty = outturnQty,
					PackQty = packQty,
					OutturnPillagedQty = pillagedQty,
					OutturnDamagedQty = damagedQty,
					OutturnedVolume = volume,
					VolumeUnit = new UnitOfVolume()
					{
						Code = volumeUQ
					},
					OutturnedWeight = weight,
					WeightUnit = new UnitOfWeight()
					{
						Code = weightUQ
					},
					GoodsDescription = goodsDescription
				}
			});

			if (setAddInfo)
			{
				foreach (var packline in shipment.PackingLineCollection)
				{
					packline.SetAddInfoCollection(() => new List<UAddInfo>
					{
						new UAddInfo
						{
							Key = AddInfoKeyTypes.Types.IsManifestedPackage,
							Value = true.ToString()
						}
					});
				}
			}

			var rtuSubshipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = masterBill
			};

			rtuSubshipment.SetDateCollection(() => new List<Date>
			{
				Date.New(DateType.Unpack, true ,unpackDate)
			});

			shipment.SetRelatedShipmentCollection(() => new List<Shipment>()
			{
				rtuSubshipment
			});

			return shipment;
		}

		static void AssertOutturn(CusOutturn outturn, CusHAWB cusHAWB, string masterBill, string houseBill, DateTime unpackDate, DateTime receiptDate, int packQty, int outturnQty, decimal volume, string volumeUQ, decimal weight, string weightUQ, bool damaged, bool pillaged, string resultType, string goodsDescription)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Should have master bill", masterBill, outturn.C5_MasterBill);
				AssertEquals("Should have house bill", houseBill, outturn.C5_HouseBill);
				AssertEquals("Should have unpack date", unpackDate, outturn.C5_CargoUnpackDate);
				AssertEquals("Should have receipt date", receiptDate, outturn.C5_CargoReceiptDate);
				AssertEquals("Should have pack qty", packQty, outturn.C5_OuterPacks);
				AssertEquals("Should have outturn qty", outturnQty, outturn.C5_PackagesOutturned);
				AssertEquals("Should have volume", volume, outturn.C5_VolumeOutturned);
				AssertEquals("Should have volume unit", volumeUQ, outturn.C5_VolumeOutturnedUQ);
				AssertEquals("Should have weight ", weight, outturn.C5_WeightOutturned);
				AssertEquals("Should have weight unit", weightUQ, outturn.C5_WeightOutturnedUQ);
				AssertEquals("Should have damaged flag", damaged, outturn.C5_DamageIndicator);
				AssertEquals("Should have pillaged flag", pillaged, outturn.C5_PillageIndicator);
				AssertEquals("Should have parent table code", "CS", outturn.C5_ParentTableCode);
				AssertEquals("Should have parent ID", cusHAWB.PK, outturn.C5_ParentID);
				AssertEquals("Should have goods description", goodsDescription, outturn.C5_GoodsDescription);
			});
		}
	}
}
