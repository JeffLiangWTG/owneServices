using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DepotCusOutturnDataObjectReaderTest : DataTransfer.Universal.Outturn.Testing.OutturnDataObjectReaderTestHelper<CusOutturnHeader, DepotCusOutturn>
	{
		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader);
			var outturn = reader.ReadIntoBusinessObject();

			AssertNotNull(outturn);
			AssertEquals("Get existed outturn C5_C6", existOutturnHeader.PK, outturn.C5_C6);
			AssertEquals("Get existed outturn PK", existOutturn.PK, outturn.PK);

			shipment.ContainerCollection[0].ContainerNumber = "OTHER123";
			Factory.SaveForTesting();

			reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader);
			outturn = reader.ReadIntoBusinessObject();

			AssertNotNull(outturn);
			AssertNotEquals("No existed outturn and create new PK", existOutturn.PK, outturn.PK);
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader);
			var outturn = reader.ReadIntoBusinessObject();

			AssertEquals("Get existed outturn C5_C6", existOutturnHeader.PK, outturn.C5_C6);
			AssertEquals("Get existed outturn PK", existOutturn.PK, outturn.PK);
			Assert("No Log error", !logger.HasErrors);

			logger.ClearLogs();
			var existOutturnHeader2 = CreateOuttrunHeader(refVessel, premise, "Q125", "O00000124");
			var existOutturn2 = CreateOuttrun("FCL", "CON123", "MB125", "HB125", existOutturnHeader2, CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived);

			shipment.OuterPacks = 6;
			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>
			{
				new AdditionalBill
				{
					BillType = new WayBillType
					{
						Code =  WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					},
					BillNumber = "HB125",
					ParentBillNumber = "MB125"
				}
			});

			reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader2);
			outturn = reader.ReadIntoBusinessObject();

			AssertEquals("Get existed outturn", existOutturn2.PK, outturn.PK);
			AssertEquals("OutturnHeader cannot be updated", 0, existOutturnHeader2.Outturns[0].C5_OuterPacks);
			Assert("Has Log error", logger.HasErrors);
			var expectedReason = "Can't update read-only Sea Cargo Outturn with cargo type 'FCL' and container 'CON123' and house bill 'HB125' and master bill 'MB125'";
			Assert("Log: Stop import when existOutturn is readonly", logger.GetErrors().Contains(expectedReason));
		}

		public void TestGetOutturnPropertiesToSuspendSetting()
		{
			var getOutturnPropertiesToSuspendSetting = typeof(DepotCusOutturnDataObjectReader).GetMethod("GetOutturnPropertiesToSuspendSetting", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, outturnHeader);
			var suspenderList = (IEnumerable<ZString>)getOutturnPropertiesToSuspendSetting.Invoke(reader, System.Array.Empty<object>());
			AssertEquals("suspend C5_OutturnResultType", false, suspenderList.Contains(CusOutturn.Schema.C5_OutturnResultType));

			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = "",
				Description = ""
			};

			reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, outturnHeader);
			suspenderList = (IEnumerable<ZString>)getOutturnPropertiesToSuspendSetting.Invoke(reader, System.Array.Empty<object>());
			AssertEquals("suspend C5_OutturnResultType", false, suspenderList.Contains(CusOutturn.Schema.C5_OutturnResultType));

			shipment.ShipmentType = null;

			reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, outturnHeader);
			suspenderList = (IEnumerable<ZString>)getOutturnPropertiesToSuspendSetting.Invoke(reader, System.Array.Empty<object>());
			AssertEquals("suspend C5_OutturnResultType", false, suspenderList.Contains(CusOutturn.Schema.C5_OutturnResultType));
		}

		public void TestImportingOutturnResultForNewOutturn()
		{
			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = "SH",
				Description = "SH"
			};

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, outturnHeader);
			var outturn = reader.ReadIntoBusinessObject();

			AssertEquals("C5_OutturnResultType:	Could not be updated by importing and set default value", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);

			var shipment2 = CreateTestOutturnShipment("123", "123", "MB2", "HB2");
			shipment2.ShipmentType = new CodeDescriptionPair
			{
				Code = "",
				Description = ""
			};

			reader = new DepotCusOutturnDataObjectReader(shipment2, logger, Factory, outturnHeader);
			outturn = reader.ReadIntoBusinessObject();

			AssertEquals("outturn.C5_OutturnResultType: Could not be updated by importing and set default value", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);

			var shipment3 = CreateTestOutturnShipment("123", "123", "MB3", "HB3");
			shipment3.ShipmentType = null;

			reader = new DepotCusOutturnDataObjectReader(shipment3, logger, Factory, outturnHeader);
			outturn = reader.ReadIntoBusinessObject();

			AssertEquals("outturn.C5_OutturnResultType: Could not be updated by importing and set default value", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
		}

		public void TestImportingOutturnResultForExistedOutturn()
		{
			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = "SH",
				Description = "SH"
			};

			var reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader);
			var outturn = reader.ReadIntoBusinessObject();

			AssertEquals("Update existed outurn", existOutturn.PK, outturn.PK);
			AssertEquals("C5_OutturnResultType: Could not be updated by importing and set default value", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);

			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = "",
				Description = ""
			};

			reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader);
			outturn = reader.ReadIntoBusinessObject();

			AssertEquals("Update existed outurn", existOutturn.PK, outturn.PK);
			AssertEquals("outturn.C5_OutturnResultType: Could not be updated by importing and set default value", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);

			shipment.ShipmentType = null;

			reader = new DepotCusOutturnDataObjectReader(shipment, logger, Factory, existOutturnHeader);
			outturn = reader.ReadIntoBusinessObject();

			AssertEquals("Update existed outurn", existOutturn.PK, outturn.PK);
			AssertEquals("outturn.C5_OutturnResultType: Could not be updated by importing and set default value", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			refVessel = CreateRefVesselForTest();
			premise = CreatePremiseAddressForTest();
			Factory.SaveForTesting();

			existOutturnHeader = CreateOuttrunHeader(refVessel, premise, "Q123", "O00000123");
			existOutturn = CreateOuttrun("FCL", "CON123", "MB123", "HB123", existOutturnHeader);
			shipment = CreateTestOutturnShipment("FCL", "CON123", "MB123", "HB123");
		}

		OrgHeader premise;
		RefVessel refVessel;
		CusOutturnHeader existOutturnHeader;
		DepotCusOutturn existOutturn;
		Shipment shipment;
	}
}
