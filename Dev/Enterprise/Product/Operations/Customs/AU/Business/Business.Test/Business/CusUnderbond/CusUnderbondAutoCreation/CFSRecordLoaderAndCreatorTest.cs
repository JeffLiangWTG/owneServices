using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CFSRecordLoaderAndCreatorTest : TestCaseWithFactory
	{
		#region Create CusSCADepot* Holder

		#region New Records

		public void TestCreateCusSCADepotHolderWithOnlyContainerNumberNew()
		{
			TestCFSRecord.ContainerNumber = HolderTestConstants.ContainerNumber;
			CheckContainerCommon();
		}

		public void TestCreateCusSCADepotHolderWithOnlyHouseBillNumberNew()
		{
			TestCFSRecord.HouseBillNum = HolderTestConstants.HouseBillNumber;
			TestCFSRecord.MessageType = CFSMessageType.StatusAdvice;
			CheckHouseCommon(true, false, HolderTestConstants.CRSStatus);

			holder = null;
			TestCFSRecord.MessageType = CFSMessageType.ExpectedArrival;
			CheckHouseCommon(true, false, HolderTestConstants.URRStatus);
		}

		public void TestCreateCusSCADepotHolderWithContainerAndHouseBillNumberNew()
		{
			TestCFSRecord.ContainerNumber = HolderTestConstants.ContainerNumber;
			TestCFSRecord.HouseBillNum = HolderTestConstants.HouseBillNumber;
			CheckHouseCommon(false, false, HolderTestConstants.URRStatus);
		}

		public void TestCreateCusSCADepotHolderWithOnlyOceanBillNumberNew()
		{
			TestCFSRecord.OceanBillNum = HolderTestConstants.OceanBillNumber;
			TestCFSRecord.MessageType = CFSMessageType.StatusAdvice;
			CheckHouseCommon(true, true, HolderTestConstants.CRSStatus);

			holder = null;
			TestCFSRecord.MessageType = CFSMessageType.ExpectedArrival;
			CheckHouseCommon(true, true, HolderTestConstants.URRStatus);
		}

		#endregion

		#region Existing Records

		public void TestCreateCusSCADepotHolderWithOnlyContainerNumberExisting()
		{
			TestCFSRecord.ContainerNumber = HolderTestConstants.ContainerNumber;
			AssertEquals("Matched container", TestContainer, Holder);
			CheckContainerCommon();
		}

		public void TestCreateCusSCADepotHolderWithOnlyHouseBillNumberExisting()
		{
			TestCFSRecord.HouseBillNum = HolderTestConstants.HouseBillNumber;
			AssertEquals("Matched house", TestHouse, Holder);
			CheckHouseCommon(false, false, HolderTestConstants.URRStatus);
		}

		public void TestCreateCusSCADepotHolderWithContainerAndHouseBillNumberExisting()
		{
			TestCFSRecord.ContainerNumber = HolderTestConstants.ContainerNumber;
			TestCFSRecord.HouseBillNum = HolderTestConstants.HouseBillNumber;
			AssertEquals("Matched house", TestHouse, Holder);
			CheckHouseCommon(false, false, HolderTestConstants.URRStatus);
		}

		public void TestCreateCusSCADepotHolderWithOnlyOceanBillNumberExisting()
		{
			TestCFSRecord.OceanBillNum = HolderTestConstants.OceanBillNumber;
			TestCFSRecord.MessageType = CFSMessageType.StatusAdvice;
			AssertEquals("Matched house", TestHouseWithOceanBillNumber, Holder);
			CheckHouseCommon(false, true, HolderTestConstants.URRStatus);
		}

		#endregion

		#region Implementation

		protected void CheckContainerCommon()
		{
			CheckContainerCommon(Holder, false);
		}

		protected void CheckContainerCommon(ICusUnderbondDependentCollectionParent parent, bool emptyContainerNumber)
		{
			AssertEquals("Our object is a container", typeof(CusSCADepotContainer), parent.GetType());
			var container = (CusSCADepotContainer)parent;

			AssertEquals("Container number", emptyContainerNumber ? ZString.Empty : (ZString)HolderTestConstants.ContainerNumber, container.CJ_ContainerNumber);
			AssertEquals("Lloyds number", "10000", container.CJ_LloydsNumber);
			AssertEquals("Voyage number", HolderTestConstants.VoyageNumber, container.CJ_Voyage);
			AssertEquals("Package count", emptyContainerNumber ? 0 : HolderTestConstants.PackageCount, container.CJ_PackageCount);
			AssertEquals("Status", emptyContainerNumber ? ZString.Empty : (ZString)HolderTestConstants.URRStatus, container.CJ_Status);
		}

		protected void CheckHouseCommon(bool emptyContainerNumber, bool onlyOceanBillNumber, ZString messageStatus)
		{
			AssertEquals("Our object is a house", typeof(CusSCADepotHouse), Holder.GetType());
			var house = (CusSCADepotHouse)Holder;

			AssertNotNull("Has a container", house.Container);
			CheckContainerCommon(house.Container, emptyContainerNumber);

			if (onlyOceanBillNumber)
			{
				AssertEquals("House bill number equals ocean bill number", HolderTestConstants.OceanBillNumber, house.CX_HouseBill);
			}
			else
			{
				AssertEquals("House bill number", HolderTestConstants.HouseBillNumber, house.CX_HouseBill);
			}

			AssertEquals(HolderTestConstants.PackageCount, house.CX_PackageCount);
			AssertEquals(messageStatus, house.CX_Status);
		}

		protected static class HolderTestConstants
		{
			public const string ContainerNumber = "CONT1111117";
			public const string HouseBillNumber = "HOUSE100";
			public const string OceanBillNumber = "OCEAN200";
			public const string VoyageNumber = "598";
			public const string LloydsNumber = "10000";
			public const int PackageCount = 7;
			public const string URRStatus = "URR";
			public const string CRSStatus = "CRS";
		}

		protected CusSCADepotHouse TestHouseWithOceanBillNumber
		{
			get
			{
				if (testHouseWithOceanBillNumber == null)
				{
					testHouseWithOceanBillNumber = TestHouse.Clone();
					testHouseWithOceanBillNumber.CX_HouseBill = HolderTestConstants.OceanBillNumber;
				}
				return testHouseWithOceanBillNumber;
			}
		}
		CusSCADepotHouse testHouseWithOceanBillNumber;

		protected CusSCADepotHouse TestHouse
		{
			get
			{
				if (testHouse == null)
				{
					testHouse = TestContainer.HouseBills.AddNew();
					testHouse.CX_HouseBill = HolderTestConstants.HouseBillNumber;
					testHouse.CX_PackageCount = HolderTestConstants.PackageCount;
					testHouse.CX_Status = HolderTestConstants.URRStatus;
				}
				return testHouse;
			}
		}
		CusSCADepotHouse testHouse;

		protected CusSCADepotContainer TestContainer
		{
			get
			{
				if (testContainer == null)
				{
					testContainer = Factory.New<CusSCADepotContainer>();
					testContainer.CJ_ContainerNumber = HolderTestConstants.ContainerNumber;
					testContainer.CJ_LloydsNumber = "10000";
					testContainer.CJ_PackageCount = HolderTestConstants.PackageCount;
					testContainer.CJ_Status = HolderTestConstants.URRStatus;
					testContainer.CJ_Voyage = HolderTestConstants.VoyageNumber;
				}
				return testContainer;
			}
		}
		CusSCADepotContainer testContainer;

		ICusUnderbondDependentCollectionParent Holder
		{
			get
			{
				if (holder == null)
				{
					holder = Creator.CreateCusSCADepotHolder(TestCFSRecord);
				}
				return holder;
			}
		}
		ICusUnderbondDependentCollectionParent holder;

		protected CFSRecord TestCFSRecord
		{
			get
			{
				if (testCFSRecord == null)
				{
					testCFSRecord = new CFSRecord();
					testCFSRecord.ContainerMode = "LCL";
					testCFSRecord.VoyageNumber = HolderTestConstants.VoyageNumber;
					testCFSRecord.LloydsNumber = HolderTestConstants.LloydsNumber;
					testCFSRecord.PackageCount = HolderTestConstants.PackageCount;
					testCFSRecord.DestinationPremise = "9144N";
					testCFSRecord.PackageType = "PK";
					testCFSRecord.OriginPremise = "9122P";
				}
				return testCFSRecord;
			}
		}
		CFSRecord testCFSRecord;

		#endregion

		#endregion

		#region TestShipmentAndContainerShouldBelongToSameConsol

		public void TestShipmentAndContainerShouldBelongToSameConsol()
		{
			const string DefaultPackageType = Core.Constants.PkgUnit.Package;
			const int DefaultPackCount = 2;
			const string HouseBillToUse = "UNUSED BILL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Bob";
			vessel.RV_LloydsNumber = "1234567";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Npaj";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUDRW";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = "MasterBill";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";

			var transport1 = consol1.Transports[0];
			transport1.JW_JX = voyage.Sailings[0].PK;

			var consol2 = Factory.New<CFSLoadListConsol>();

			var shipment = consol2.Shipments.AddNew();
			shipment.JS_HouseBill = HouseBillToUse;

			var transport2 = consol2.Transports[0];
			transport2.JW_JX = voyage.Sailings[1].PK;

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = container1.JC_ContainerNum;

			Factory.Save();

			var record = new CFSRecord();
			record.LloydsNumber = vessel.RV_LloydsNumber;
			record.VoyageNumber = voyage.JV_VoyageFlight;
			record.OceanBillNum = consol1.JK_MasterBillNum;
			record.HouseBillNum = HouseBillToUse;
			record.ContainerNumber = container2.JC_ContainerNum;
			record.ContainerMode = Core.Constants.ContainerModes.LCL;
			record.PackageCount = DefaultPackCount;
			record.PackageType = DefaultPackageType;
			record.MessageType = CFSMessageType.StatusAdvice;

			AssertEquals("Container1 shouldn't have PackLine added", 0, container1.PackLines.Count);
			AssertEquals("Container2 shouldn't have PackLine added", 0, container2.PackLines.Count);
			AssertEquals("Consol1 should not contain any shipments shipment", 0, consol1.Shipments.Count);
			AssertEquals("Consol2 should have 1 shipment", 1, consol2.Shipments.Count);

			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record);
			Factory.Save();

			AssertEquals("Container1 shouldn't have PackLine added", 0, container1.PackLines.Count);
			AssertEquals("Container2 should have PackLine added", 1, container2.PackLines.Count);
			AssertEquals("Consol1 should not contain any shipments shipment", 0, consol1.Shipments.Count);
			AssertEquals("Consol2 should have 1 shipment", 1, consol2.Shipments.Count);

			shipment.JS_HouseBill = "ANOTHER SHIPMENT1";
			Factory.Save();

			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record);
			Factory.Save();

			AssertEquals("Can't match on shipment, and containers duplicated, so should match to container1", 1, container1.PackLines.Count);
			AssertEquals("Container2 PackLines not changed", 1, container2.PackLines.Count);
			AssertEquals("Consol1 should now have a new shipment added", 1, consol1.Shipments.Count);
			AssertEquals("Consol2 should have the original shipment", 1, consol2.Shipments.Count);

			consol1.Shipments[0].JS_HouseBill = "ANOTHER SHIPMENT2";
			container1.JC_ContainerNum = "FAKE4100022";
			Factory.Save();

			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record);
			Factory.Save();

			AssertEquals("Container1 PackLines not changed", 1, container1.PackLines.Count);
			AssertEquals("Matchs on unique container2", 2, container2.PackLines.Count);
			AssertEquals("Consol1 should just have the one shipment", 1, consol1.Shipments.Count);
			AssertEquals("Consol2 should now have an additional shipment", 2, consol2.Shipments.Count);

			consol2.Shipments[0].JS_HouseBill = "ANOTHER SHIPMENT3";
			consol2.Shipments[1].JS_HouseBill = "ANOTHER SHIPMENT3";
			container2.JC_ContainerNum = "FAKE4100022";
			Factory.Save();

			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record);
			Factory.Save();

			AssertEquals("Match on Master Bill only so attached to consol1", 2, consol1.Shipments.Count);

			AUCustomsDataRegistry.Instance.CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			record.HouseBillNum = "UNUSED BILL 2";
			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record);
			Factory.Save();

			AssertEquals("Consol1 should still just have 2 shipments", 2, consol1.Shipments.Count);
		}

		public void TestFindContainer()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Bob";
			vessel.RV_LloydsNumber = "1234567";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Npaj";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUDRW";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = "MasterBill1";
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			consol1.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_MasterBillNum = "MasterBill2";
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100011";
			consol2.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var consol3 = Factory.New<CFSLoadListConsol>();
			consol3.JK_MasterBillNum = "MasterBill3";
			var container3 = consol3.Containers.AddNew();
			container3.JC_ContainerNum = "FAKE4100011";
			consol3.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var record = new CFSRecord();
			record.LloydsNumber = vessel.RV_LloydsNumber;
			record.VoyageNumber = voyage.JV_VoyageFlight;
			record.OceanBillNum = consol1.JK_MasterBillNum;
			record.ContainerNumber = "FAKE4100011";
			record.ContainerMode = Core.Constants.ContainerModes.FCL;
			record.MessageType = CFSMessageType.StatusAdvice;

			var wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSContainerWrapper;
			AssertNotNull(wrapper);
			AssertEquals(container1.PK, wrapper.Container.PK);

			record.OceanBillNum = consol2.JK_MasterBillNum;
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSContainerWrapper;
			AssertNotNull(wrapper);
			AssertEquals(container2.PK, wrapper.Container.PK);

			record.OceanBillNum = "MasterBill4";
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSContainerWrapper;
			AssertNotNull(wrapper);

			record.ContainerNumber = "FAKE4100012";
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSContainerWrapper;
			AssertNull(wrapper);

			record.ContainerNumber = "FAKE4100011";
			record.HouseBillNum = "HOUSENUM1";
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSContainerWrapper;
			AssertNotNull(wrapper);

			record.OceanBillNum = consol3.JK_MasterBillNum;
			record.HouseBillNum = "HOUSENUM2";
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSContainerWrapper;
			AssertNotNull(wrapper);
			AssertEquals(container3.PK, wrapper.Container.PK);
		}

		public void TestFindConsol()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Bob";
			vessel.RV_LloydsNumber = "1234567";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Npaj";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUDRW";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = "MasterBill1";
			var shipment1 = consol1.Shipments.AddNew();
			consol1.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_MasterBillNum = "MasterBill2";
			var shipment2 = consol2.Shipments.AddNew();
			consol2.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var consol3 = Factory.New<CFSLoadListConsol>();
			consol3.JK_MasterBillNum = "MasterBill3";
			var shipment3 = consol2.Shipments.AddNew();
			consol3.Transports[0].JW_JX = voyage.Sailings[0].PK;

			var record = new CFSRecord();
			record.LloydsNumber = vessel.RV_LloydsNumber;
			record.VoyageNumber = voyage.JV_VoyageFlight;
			record.OceanBillNum = consol1.JK_MasterBillNum;
			record.MessageType = CFSMessageType.StatusAdvice;

			var wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSShipmentWrapper;
			AssertNotNull(wrapper);
			AssertEquals(shipment1.PK, wrapper.Shipment.PK);

			record.OceanBillNum = consol2.JK_MasterBillNum;
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSShipmentWrapper;
			AssertNotNull(wrapper);
			AssertEquals(shipment2.PK, wrapper.Shipment.PK);

			record.OceanBillNum = "MasterBill4";
			wrapper = new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record) as CFSShipmentWrapper;
			AssertNull(wrapper);
		}

		public void TestContainerOnDifferentConsoleMessage()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Bob";
			vessel.RV_LloydsNumber = "1234567";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = "MasterBill";
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			var transport1 = consol1.Transports[0];
			transport1.JW_JX = voyage.Sailings[0].PK;
			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_MasterBillNum = "MASTERBILL";
			consol2.JK_UniqueConsignRef = "L00001234";
			var shipment = consol2.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSEBILL";
			shipment.JS_UniqueConsignRef = "H00001234";
			var transport2 = consol2.Transports[0];
			transport2.JW_JX = voyage.Sailings[0].PK;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100022";
			Factory.Save();

			var record = new CFSRecord();
			record.LloydsNumber = vessel.RV_LloydsNumber;
			record.VoyageNumber = voyage.JV_VoyageFlight;
			record.OceanBillNum = "MASTERBILL";
			record.HouseBillNum = "HOUSEBILL";
			record.ContainerNumber = container1.JC_ContainerNum;
			record.ContainerMode = Core.Constants.ContainerModes.LCL;
			Env.Registry.AUCustoms.CargoStatusSendErrors = Enterprise.Core.Constants.EmailTo.NominatedGroup;
			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Env.Registry.AUCustoms.CargoStatusSendErrorsToGroup = group.PK.ToGuid();

			CARSTRecord carstRecord;
			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record, out carstRecord);
			AssertNotNull(carstRecord);
			AssertEquals("Consol for house bill", consol2.PK, carstRecord.Consol.PK);
			AssertNotEquals("Container Consol is different to Shipment Consol", carstRecord.Consol.PK, carstRecord.Container.Consol.PK);
			AssertEquals("Wrong container house bill", container1.PK, carstRecord.Container.PK);
			AssertEquals("Shipment for house bill", shipment.PK, carstRecord.Shipment.PK);
			AssertNotNull(carstRecord.Line);
		}

		#endregion

		#region NoIndexOutOfRangeExceptionWhenPassingInDefaultPackDetails_W00038430

		public void TestNoIndexOutOfRangeExceptionWhenPassingInDefaultPackDetails_W00038430()
		{
			const string DefaultPackageType = "PK";
			const int DefaultPackCount = 0;
			const string HouseBillToUse = "UNUSED BILL";

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Bob";
			vessel.RV_LloydsNumber = "1234567";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "Npaj";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = "MasterBill";

			Transport transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";

			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = HouseBillToUse;

			Factory.Save();

			CFSRecord record = new CFSRecord();
			record.LloydsNumber = vessel.RV_LloydsNumber;
			record.VoyageNumber = voyage.JV_VoyageFlight;
			record.OceanBillNum = consol.JK_MasterBillNum;
			record.HouseBillNum = HouseBillToUse;
			record.ContainerNumber = container.JC_ContainerNum;
			record.ContainerMode = Core.Constants.ContainerModes.LCL;
			record.PackageCount = DefaultPackCount;
			record.PackageType = DefaultPackageType;
			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record);
			Factory.Save();

			AssertEquals("The container should now have a single PackLine", 1, container.PackLines.Count);
			AssertEquals("The only packline should belong to a shipment with the given house bill", HouseBillToUse, container.PackLines[0].Shipment.JS_HouseBill);
		}

		#endregion

		#region Implementation

		#region Test Helper

		#region enum CFSMessageType

		public enum CFSMessageType { ExpectedArrival, UnderbondApproval, StatusAdvice }

		#endregion

		#region class CFSRecord

		public class CFSRecord : ICMRDepotMessage, ICMRDepotMessageLine
		{
			public ZString LloydsNumber;
			public ZString VoyageNumber;
			public ZString OceanBillNum;
			public ZString HouseBillNum;
			public ZString ContainerNumber;
			public ZString ContainerMode;
			public ZInt PackageCount;
			public ZString PackageType;
			public ZString OriginPremise;
			public ZString DestinationPremise;
			public CFSMessageType MessageType;

			public ZString PremiseID
			{
				get
				{
					if (MessageType == CFSMessageType.ExpectedArrival)
					{
						return DestinationPremise;
					}
					else
					{
						return OriginPremise;
					}
				}
				set
				{
					DestinationPremise = value;
					OriginPremise = value;
				}
			}

			public bool IsBulkOrBreakBulk
			{
				get { return ContainerMode == CMRImportCargoTypes.Codes.BreakBulk || ContainerMode == CMRImportCargoTypes.Codes.Bulk; }
			}

			#region ICMRDepotMessage Members

			ZString ICMRDepotMessage.LloydsNumber
			{
				get { return this.LloydsNumber; }
			}

			ZString ICMRDepotMessage.VoyageNumber
			{
				get { return this.VoyageNumber; }
			}

			ZString ICMRDepotMessage.OriginPremiseID
			{
				get { return this.OriginPremise; }
			}

			ZString ICMRDepotMessage.DestinationPremiseID
			{
				get { return this.DestinationPremise; }
			}

			ZString ICMRDepotMessage.OurPremiseID
			{
				get { return ((ICMRDepotMessage)this).MessageType == CMRDepotMessageType.Approval ? ((ICMRDepotMessage)this).OriginPremiseID : ((ICMRDepotMessage)this).DestinationPremiseID; }
			}

			CMRDepotMessageType ICMRDepotMessage.MessageType
			{
				get
				{
					switch (this.MessageType)
					{
						case CFSMessageType.ExpectedArrival:
							return CMRDepotMessageType.ExpectedArrival;
						case CFSMessageType.UnderbondApproval:
							return CMRDepotMessageType.Approval;
						case CFSMessageType.StatusAdvice:
						default:
							return CMRDepotMessageType.Status;
					}
				}
			}

			ICMRDepotMessageLine[] ICMRDepotMessage.Lines
			{
				get { return new ICMRDepotMessageLine[] { this }; }
			}

			BusinessObjectFactory ICMRDepotMessage.Factory
			{
				get
				{
					if (factory == null)
					{
						factory = new BusinessObjectFactory();
					}
					return factory;
				}
			}
			BusinessObjectFactory factory;

			CMRCUSRESMessage ICMRDepotMessage.LinkOrCloneMessage(BusinessObject businessObjectToLink)
			{
				return null;
			}

			bool ICMRDepotMessage.IsSea
			{
				get { return true; }
			}

			#endregion

			void ICMRDepotMessage.AddUnmatchedContainer(CARSTRecord carstRecord)
			{
				this.AddUnmatchedContainer(carstRecord, UnmatchedContainers);
			}

			public Dictionary<string, List<CARSTRecord>> UnmatchedContainers
			{
				get { return unmatchedContainers ?? (unmatchedContainers = new Dictionary<string, List<CARSTRecord>>()); }
			}
			Dictionary<string, List<CARSTRecord>> unmatchedContainers;

			#region ICMRDepotMessageLine Members

			ZInt ICMRDepotMessageLine.NumberOfPackages
			{
				get { return this.PackageCount; }
			}

			ZString ICMRDepotMessageLine.PackageType
			{
				get { return this.PackageType; }
			}

			ZString ICMRDepotMessageLine.ContainerNumber
			{
				get { return this.ContainerNumber; }
			}

			ZString ICMRDepotMessageLine.HouseBillNumber
			{
				get { return this.HouseBillNum; }
			}

			ZString ICMRDepotMessageLine.OceanBillNumber
			{
				get { return this.OceanBillNum; }
			}

			ZString ICMRDepotMessageLine.ContainerMode
			{
				get { return this.ContainerMode; }
			}

			ZString ICMRDepotMessageLine.MarksAndNumbers
			{
				get { return "MARKS"; }
			}

			ZString ICMRDepotMessageLine.GoodsDescription
			{
				get { return "GOODS"; }
			}

			ICMRDepotMessage ICMRDepotMessageLine.Parent
			{
				get { return this; }
			}

			ZDecimal ICMRDepotMessageLine.ActualWeight
			{
				get { return ZDecimal.Zero; }
			}

			ZString ICMRDepotMessageLine.WeightUnits
			{
				get { return ZString.Empty; }
			}

			ZDecimal ICMRDepotMessageLine.ActualVolume
			{
				get { return ZDecimal.Zero; }
			}

			ZString ICMRDepotMessageLine.VolumeUnits
			{
				get { return ZString.Empty; }
			}

			#endregion
		}

		#endregion

		#endregion

		CFSRecordLoaderAndCreator fCreator;
		CFSRecordLoaderAndCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new CFSRecordLoaderAndCreator(Factory);
				}
				return fCreator;
			}
		}

		#endregion

		#region Test FindShipment

		[ExpectNoExceptions]
		public void TestFindShipmentDoesNotThrowException()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BOB";
			vessel.RV_LloydsNumber = "1234567";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();

			var forwardingShipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			forwardingShipment.JS_HouseBill = "HBLNUM";
			var forwardingConsol = forwardingShipment.Consols.AddNew();
			forwardingConsol.JK_MasterBillNum = "MBLNO";
			var forwardingContainer = forwardingConsol.Containers.AddNew();
			forwardingContainer.JC_ContainerNum = "FAKE4100011";
			var transport1 = forwardingConsol.Transports[0];
			transport1.JW_JX = voyage.Sailings[0].PK;

			var record = new CFSRecord();
			record.LloydsNumber = "1234567";
			record.VoyageNumber = "123";
			record.HouseBillNum = "HBLNUM";

			CFSContainer container = null;
			var resultShipment = new CFSRecordLoaderAndCreator(Factory).FindShipment(record, out container);
			AssertNull(resultShipment);

			var cfsShipment = Factory.New<CFSShipment>();
			cfsShipment.JS_HouseBill = "HBLNUM";
			var cfsConsol1 = cfsShipment.Consols.AddNew();
			cfsConsol1.JK_MasterBillNum = "MBLNO";
			var cfsContainer1 = cfsConsol1.Containers.AddNew();
			cfsContainer1.JC_ContainerNum = "FAKE4100011";
			var transport2 = cfsConsol1.Transports[0];
			transport2.JW_JX = voyage.Sailings[0].PK;
			var cfsConsol2 = cfsShipment.Consols.AddNew();
			cfsConsol2.JK_MasterBillNum = "MBLNO";
			var cfsContainer2 = cfsConsol2.Containers.AddNew();
			cfsContainer2.JC_ContainerNum = "FAKE4100022";
			var transport3 = cfsConsol2.Transports[0];
			transport3.JW_JX = voyage.Sailings[0].PK;

			resultShipment = new CFSRecordLoaderAndCreator(Factory).FindShipment(record, out container);
			AssertNotNull(resultShipment);
		}

		public void TestFindShipment()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BOB";
			vessel.RV_LloydsNumber = "1234567";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = "MBLNO";
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			var transport1 = consol1.Transports[0];
			transport1.JW_JX = voyage.Sailings[0].PK;
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBLNUM";
			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_MasterBillNum = "MBLNO";
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100022";
			var transport2 = consol2.Transports[0];
			transport2.JW_JX = voyage.Sailings[0].PK;
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBLNUM";
			var record = new CFSRecord();
			record.LloydsNumber = "1234567";
			record.VoyageNumber = "123";
			CFSContainer container = null;
			var shipment = new CFSRecordLoaderAndCreator(Factory).FindShipment(record, out container);
			AssertNull(shipment);
			AssertNull(container);
			record.HouseBillNum = "HBLNUM";
			shipment = new CFSRecordLoaderAndCreator(Factory).FindShipment(record, out container);
			AssertEquals(shipment1.PK, shipment.PK);
			AssertNull(container);
			record.ContainerNumber = "XXXX";
			AssertEquals(shipment1.PK, shipment.PK);
			AssertNull(container);
			record.ContainerNumber = "FAKE4100011";
			shipment = new CFSRecordLoaderAndCreator(Factory).FindShipment(record, out container);
			AssertEquals(shipment1.PK, shipment.PK);
			AssertEquals(container1.PK, container.PK);
			record.ContainerNumber = "FAKE4100022";
			shipment = new CFSRecordLoaderAndCreator(Factory).FindShipment(record, out container);
			AssertEquals(shipment2.PK, shipment.PK);
			AssertEquals(container2.PK, container.PK);
		}

		#endregion

		#region Test FindFreeStandingCusSCAHousePivot

		public void TestFindFreeStandingCusSCAHousePivot()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBLNO";
			oceanBill.CB_ApplicationCode = "CMR";
			oceanBill.CB_LloydsIMO = "1234567";
			oceanBill.CB_Voyage = "123";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CONTNO";
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "HBLNUM";
			var pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			Factory.Save();

			var record = new CFSRecord();
			record.LloydsNumber = "1234567";
			record.VoyageNumber = "123";
			record.HouseBillNum = "HBLNUM";
			record.ContainerNumber = "CONTNO";
			record.OceanBillNum = "OBLNOXXXX";

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var findPivot = new CFSRecordLoaderAndCreator(factory2).FindFreeStandingCusSCAHousePivot(record);
			AssertNotNull(findPivot);
			AssertEquals(findPivot.PK, pivot.PK);
		}

		#endregion

		public void TestOuterPackLinesOnMasterShipment()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Bob";
			vessel.RV_LloydsNumber = "1234567";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = "MASTERBILL";
			consol.JK_UniqueConsignRef = "L00001234";
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "HOUSEBILL";
			masterShipment.JS_UniqueConsignRef = "H00001234";
			var transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100022";
			var childShipment = masterShipment.CoLoadShipments.AddNew();
			childShipment.JS_HouseBill = "HOUSEBILL1";
			Factory.Save();

			var record = new CFSRecord();
			record.LloydsNumber = vessel.RV_LloydsNumber;
			record.VoyageNumber = voyage.JV_VoyageFlight;
			record.OceanBillNum = "MASTERBILL";
			record.HouseBillNum = "HOUSEBILL";
			record.ContainerNumber = container.JC_ContainerNum;
			record.ContainerMode = Core.Constants.ContainerModes.LCL;

			CARSTRecord carstRecord;
			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record, out carstRecord);
			AssertEquals(0, masterShipment.OuterPackLines.Count);

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			masterShipment.CoLoadShipments.RemoveAndDeleteAll();

			new CFSRecordLoaderAndCreator(Factory).GetOrCreateCFSRecord(record, out carstRecord);
			AssertEquals(1, masterShipment.OuterPackLines.Count);
		}
	}
}
