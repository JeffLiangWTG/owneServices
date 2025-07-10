using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	[CodeAlive(reason: "Used by ObjectFactory")]
	public sealed class EcommerceTestDataCreator : IEcommerceTestDataCreator
	{
		public List<BusinessObject> CreateEcommerceTestData(BusinessObjectFactory factory, bool isActiveProcess = true)
		{
			var archiveablePKs = new List<BusinessObject>();

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001";

			if (!isActiveProcess)
			{
				shipment.JS_IsCancelled = true;
			}

			archiveablePKs.Add(shipment);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();

			if (!isActiveProcess)
			{
				consol.JK_IsCancelled = true;
			}

			archiveablePKs.Add(consol);

			var shipmentHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			shipmentHeader.JH_Status = JobHeaderStatus.Closed.Code;
			shipmentHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			shipmentHeader.JH_JobNum = "T0001";
			shipmentHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipmentHeader.JH_ParentID = shipment.PK;
			archiveablePKs.Add(shipmentHeader);

			var consolHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			consolHeader.JH_Status = JobHeaderStatus.Closed.Code;
			consolHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			consolHeader.JH_JobNum = "T0003";
			consolHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			consolHeader.JH_ParentID = consol.PK;
			archiveablePKs.Add(consolHeader);

			factory.Save();

			archiveablePKs.AddRange(CreateJobConsolChildren(factory, consol.PK));
			archiveablePKs.AddRange(CreateJobShipmentChildren(factory, shipment));

			return archiveablePKs;
		}

		List<BusinessObject> CreateJobConsolChildren(BusinessObjectFactory factory, ZGuid jobConsolPK)
		{
			var archiveableChildPKs = new List<BusinessObject>();

			var outerPackage = factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_JK_LoadedOnConsol = jobConsolPK;
			factory.Save();
			archiveableChildPKs.Add(outerPackage);

			return archiveableChildPKs;
		}

		List<BusinessObject> CreateJobShipmentChildren(BusinessObjectFactory factory, ForwardingShipment jobShipment)
		{
			var archiveableChildPKs = new List<BusinessObject>();

			var packLine = factory.NewWithValidTestData<ForwardingPackLine>();
			packLine.JL_JS = jobShipment.PK;
			var outerPackage = factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_JL_PackLine = packLine.PK;
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;
			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			var hvlvItem1 = consignment.Items.AddNew();
			hvlvItem1.HVI_JS_LoadedOnShipment = shipment.PK;
			hvlvItem1.HVI_HVO_OuterPackage = outerPackage.PK;
			factory.Save();
			archiveableChildPKs.Add(outerPackage);
			archiveableChildPKs.Add(hvlvItem1);
			archiveableChildPKs.Add(packLine);

			var eLoadList = factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_JS_MasterHouseShipment = jobShipment.PK;
			var supplierBookingLine = factory.NewWithValidTestData<SupplierBookingLine>();
			supplierBookingLine.DL_DO_LoadList = eLoadList.PK;
			factory.Save();
			archiveableChildPKs.Add(eLoadList);
			archiveableChildPKs.Add(supplierBookingLine);

			var shipment1 = factory.New<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader1.HCH_JS_Shipment = shipment1.PK;
			var consignment1 = factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = consignmentHeader1.PK;
			var hvlvItem2 = consignment1.Items.AddNew();
			hvlvItem2.HVI_JS_LoadedOnShipment = jobShipment.PK;
			factory.Save();
			archiveableChildPKs.Add(hvlvItem2);

			return archiveableChildPKs;
		}

		public List<BusinessObject> CreateEcommerceWithCustomsTestData(BusinessObjectFactory factory, bool isActiveProcess = true)
		{
			var archiveablePKs = new List<BusinessObject>();

			var customsShipment = factory.New<ForwardingShipment>();
			customsShipment.JS_UniqueConsignRef = "S00002";
			customsShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			if (!isActiveProcess)
			{
				customsShipment.JS_IsCancelled = true;
			}

			archiveablePKs.Add(customsShipment);

			var customsHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			customsHeader.JH_Status = JobHeaderStatus.Closed.Code;
			customsHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			customsHeader.JH_JobNum = "T0002";
			customsHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			customsHeader.JH_ParentID = customsShipment.PK;
			archiveablePKs.Add(customsHeader);

			var consignmentHeader2 = customsShipment.GetOrCreateHVLVConsignmentHeader();
			var consignment2 = factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_HCH_Header = consignmentHeader2.PK;
			var returnPivotWithFormerFK = factory.NewWithValidTestData<HVLVReturnPivot>();
			returnPivotWithFormerFK.HVP_HVC_Former = consignment2.PK;
			var returnPivotWithReturnFK = factory.NewWithValidTestData<HVLVReturnPivot>();
			returnPivotWithReturnFK.HVP_HVC_Return = consignment2.PK;
			var hvlvItem3 = consignment2.Items.AddNew();
			var hvlvItemLine = factory.NewWithValidTestData<HVLVItemLine>();
			hvlvItemLine.HVS_HVI_HVLVItem = hvlvItem3.PK;
			hvlvItemLine.HVS_Quantity = 1;
			var cusUSLVClearance = factory.NewWithValidTestData<CusUSLVClearance>();
			var cusUSLVConsignment = factory.NewWithValidTestData<CusUSLVConsignment>();
			cusUSLVConsignment.ULB_HVC_Consignment = consignment2.PK;
			cusUSLVConsignment.ULB_ULH = cusUSLVClearance.PK;
			var cusUSLVItem = cusUSLVConsignment.CusUSLVItems.AddNew();
			cusUSLVItem.ULI_ULB = cusUSLVConsignment.PK;
			var cusUSLVItemPGA = cusUSLVItem.CusUSLVItemPGAs.AddNew();
			cusUSLVItemPGA.ULP_Agency = "NMF";
			cusUSLVItemPGA.ULP_AgencyProgram = "370";
			cusUSLVItemPGA.ULP_Indicator = "C";
			cusUSLVItemPGA.ULP_DisclaimReason = "B";

			factory.Save();

			archiveablePKs.Add(consignmentHeader2);
			archiveablePKs.Add(consignment2);
			archiveablePKs.Add(returnPivotWithFormerFK);
			archiveablePKs.Add(returnPivotWithReturnFK);
			archiveablePKs.Add(hvlvItem3);
			archiveablePKs.Add(hvlvItemLine);
			archiveablePKs.Add(cusUSLVClearance);
			archiveablePKs.Add(cusUSLVConsignment);
			archiveablePKs.Add(cusUSLVItem);
			archiveablePKs.Add(cusUSLVItemPGA);

			return archiveablePKs;
		}
	}
}
