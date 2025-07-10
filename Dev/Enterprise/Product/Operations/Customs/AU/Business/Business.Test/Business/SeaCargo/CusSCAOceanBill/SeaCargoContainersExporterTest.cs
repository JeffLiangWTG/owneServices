using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoContainersExporter))]
	sealed class SeaCargoContainersExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "ExportOceanBill"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Sea Cargo Containers Report"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			OrgHeader depot = Factory.New<OrgHeader>();
			depot.OH_FullName = "I am a depot";
			depot.OH_Code = "DPT";
			OrgAddress depotAddress = depot.Addresses.AddNew();
			depotAddress.OA_Address1 = "A1";
			depotAddress.OA_State = "AA";
			depot.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A");

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "I am a CTO";
			cTO.OH_Code = "CTO";
			OrgAddress cTOAddress = cTO.Addresses.AddNew();
			cTOAddress.OA_Address1 = "A2";
			cTOAddress.OA_State = "BB";
			cTO.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "5678X");

			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_LloydsIMO = "121234";
			ocean.CB_Voyage = "999111";
			ocean.CB_OceanBill = "OOO111";
			ocean.CB_RL_NKPortOfDischarge = "AUBNE";
			ocean.CB_RL_NKPortOfLoading = "USLAX";
			ocean.CB_MasterHouseBill = "MasterBill";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress = depotAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = cTOAddress.PK;
			consol.JK_MasterBillNum = ocean.CB_OceanBill;
			ocean.CB_ParentId = consol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_HouseBill = ocean.CB_MasterHouseBill;

			CusSCAHouse house1 = ocean.HouseBills.AddNew();
			house1.CA_HouseBill = "Z1123";
			house1.CA_ConsignorName = "consignor1";
			house1.CA_ConsignorAddress1 = "consignor1 address1";
			house1.CA_ConsignorAddress2 = "consignor1 address2";
			house1.CA_ConsignorSuburb = "consignor1 suburb";
			house1.CA_ConsignorPostcode = "2015";
			house1.CA_ConsigneeName = "consignee1";
			house1.CA_ConsigneeAddress1 = "consignee1 address1";
			house1.CA_ConsigneeAddress2 = "consignee1 address2";
			house1.CA_ConsigneeSuburb = "consignee1 suburb";
			house1.CA_ConsigneePostcode = "2016";
			house1.CA_NotifyName = "NOTIFY ME";
			house1.CA_NotifyAddress1 = "Notify address1";
			house1.CA_NotifyAddress2 = "Notify address2";
			house1.CA_NotifySuburb = "Notify suburb";
			house1.CA_NotifyPostcode = "2222";
			house1.CA_MasterHouseBill = "SUB MASTER";
			house1.CA_BGMReference = "BGM REF";
			house1.CA_RL_NK_PortOfDestination = ZString.Empty;
			//House1.CA_JS = shipment.PK;

			CusSCAHouse house2 = ocean.HouseBills.AddNew();
			house2.CA_HouseBill = "Z2222";
			house2.CA_ConsignorName = "consignor2";
			house2.CA_ConsignorAddress1 = "consignor2 address1";
			house2.CA_ConsignorAddress2 = "consignor2 address2";
			house2.CA_ConsignorSuburb = "consignor2 suburb";
			house2.CA_ConsignorPostcode = "2115";
			house2.CA_ConsigneeName = "consignee2";
			house2.CA_ConsigneeAddress1 = "consignee2 address1";
			house2.CA_ConsigneeAddress2 = "consignee2 address2";
			house2.CA_ConsigneeSuburb = "consignee2 suburb";
			house2.CA_ConsigneePostcode = "2116";
			house2.CA_NotifyName = "NOTIFY THEM";
			house2.CA_NotifyAddress1 = "Notify2 address1";
			house2.CA_NotifyAddress2 = "Notify2 address2";
			house2.CA_NotifySuburb = "Notify2 suburb";
			house2.CA_NotifyPostcode = "2122";
			house2.CA_RL_NK_PortOfDestination = "AUXXX";

			CusSCAContainer container1 = ocean.Containers.AddNew();
			container1.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.CN_ContainerNumber = "OCLU9991110";

			CusSCAContainer container2 = ocean.Containers.AddNew();
			container2.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			container2.CN_ContainerNumber = "OCLU3333310";

			CusSCAPivot house1Pivot1 = house1.Pivot.AddNew();
			house1Pivot1.CV_CN = container1.PK;
			house1Pivot1.CV_GoodsDescription = "Goods in Container 1";
			house1Pivot1.CV_HazardousGoods = true;
			house1Pivot1.CV_NetWeight = 100m;
			house1Pivot1.CV_Weight = 300m;
			house1Pivot1.CV_WeightUQ = "KG";
			house1Pivot1.CV_Volume = 0.020m;
			house1Pivot1.CV_FumigationCert = true;
			house1Pivot1.CV_PerishableGoods = true;
			house1Pivot1.CV_PersonalEffects = true;
			house1Pivot1.CV_Timber = true;
			house1Pivot1.CV_MarksAndNumbers = "Marks 1";
			house1Pivot1.CV_IsSAC = true;
			house1Pivot1.CV_PackageCount = 8;

			CusSCAPivot house1Pivot2 = house1.Pivot.AddNew();
			house1Pivot2.CV_CN = container2.PK;
			house1Pivot2.CV_GoodsDescription = "Goods in Container 2";
			house1Pivot2.CV_HazardousGoods = false;
			house1Pivot2.CV_NetWeight = 110m;
			house1Pivot2.CV_Weight = 200m;
			house1Pivot2.CV_WeightUQ = "KG";
			house1Pivot2.CV_Volume = 0.030m;
			house1Pivot2.CV_FumigationCert = false;
			house1Pivot2.CV_PerishableGoods = false;
			house1Pivot2.CV_PersonalEffects = false;
			house1Pivot2.CV_Timber = false;
			house1Pivot2.CV_MarksAndNumbers = "Marks 2";
			house1Pivot2.CV_IsSAC = false;
			house1Pivot2.CV_PackageCount = 9;

			CusSCAPivot house2Pivot1 = house2.Pivot.AddNew();
			house2Pivot1.CV_CN = container2.PK;
			house2Pivot1.CV_GoodsDescription = "More in Container 2";
			house2Pivot1.CV_HazardousGoods = true;
			house2Pivot1.CV_NetWeight = 120m;
			house2Pivot1.CV_Weight = 400m;
			house2Pivot1.CV_WeightUQ = "KG";
			house2Pivot1.CV_Volume = 0.040m;
			house2Pivot1.CV_FumigationCert = false;
			house2Pivot1.CV_PerishableGoods = true;
			house2Pivot1.CV_PersonalEffects = true;
			house2Pivot1.CV_Timber = true;
			house2Pivot1.CV_MarksAndNumbers = "Marks 3";
			house2Pivot1.CV_IsSAC = true;
			house2Pivot1.CV_PackageCount = 10;

			return new SeaCargoContainersExporter(ocean);
		}

		public void TestShipmentNo()
		{
			OrgHeader depot = Factory.New<OrgHeader>();
			depot.OH_FullName = "I am a depot";
			depot.OH_Code = "DPT";
			OrgAddress depotAddress = depot.Addresses.AddNew();
			depotAddress.OA_Address1 = "A1";
			depotAddress.OA_State = "AA";
			depot.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A");

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "I am a CTO";
			cTO.OH_Code = "CTO";
			OrgAddress cTOAddress = cTO.Addresses.AddNew();
			cTOAddress.OA_Address1 = "A2";
			cTOAddress.OA_State = "BB";
			cTO.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "5678X");

			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_LloydsIMO = "121234";
			ocean.CB_Voyage = "999111";
			ocean.CB_OceanBill = "OOO111";
			ocean.CB_RL_NKPortOfDischarge = "AUBNE";
			ocean.CB_RL_NKPortOfLoading = "USLAX";
			ocean.CB_MasterHouseBill = "MasterBill";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress = depotAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = cTOAddress.PK;
			consol.JK_MasterBillNum = ocean.CB_OceanBill;
			ocean.CB_ParentId = consol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_HouseBill = ocean.CB_MasterHouseBill;
			Factory.Save();

			CusSCAHouse house1 = ocean.HouseBills.AddNew();
			house1.CA_HouseBill = "Z1123";
			house1.CA_ConsignorName = "consignor1";
			house1.CA_ConsignorAddress1 = "consignor1 address1";
			house1.CA_ConsignorAddress2 = "consignor1 address2";
			house1.CA_ConsignorSuburb = "consignor1 suburb";
			house1.CA_ConsignorPostcode = "2015";
			house1.CA_ConsigneeName = "consignee1";
			house1.CA_ConsigneeAddress1 = "consignee1 address1";
			house1.CA_ConsigneeAddress2 = "consignee1 address2";
			house1.CA_ConsigneeSuburb = "consignee1 suburb";
			house1.CA_ConsigneePostcode = "2016";
			house1.CA_NotifyName = "NOTIFY ME";
			house1.CA_NotifyAddress1 = "Notify address1";
			house1.CA_NotifyAddress2 = "Notify address2";
			house1.CA_NotifySuburb = "Notify suburb";
			house1.CA_NotifyPostcode = "2222";
			house1.CA_MasterHouseBill = "SUB MASTER";
			house1.CA_BGMReference = "BGM REF";
			house1.CA_RL_NK_PortOfDestination = ZString.Empty;
			//House1.CA_JS = shipment.PK;

			CusSCAHouse house2 = ocean.HouseBills.AddNew();
			house2.CA_HouseBill = "Z2222";
			house2.CA_ConsignorName = "consignor2";
			house2.CA_ConsignorAddress1 = "consignor2 address1";
			house2.CA_ConsignorAddress2 = "consignor2 address2";
			house2.CA_ConsignorSuburb = "consignor2 suburb";
			house2.CA_ConsignorPostcode = "2115";
			house2.CA_ConsigneeName = "consignee2";
			house2.CA_ConsigneeAddress1 = "consignee2 address1";
			house2.CA_ConsigneeAddress2 = "consignee2 address2";
			house2.CA_ConsigneeSuburb = "consignee2 suburb";
			house2.CA_ConsigneePostcode = "2116";
			house2.CA_NotifyName = "NOTIFY THEM";
			house2.CA_NotifyAddress1 = "Notify2 address1";
			house2.CA_NotifyAddress2 = "Notify2 address2";
			house2.CA_NotifySuburb = "Notify2 suburb";
			house2.CA_NotifyPostcode = "2122";
			house2.CA_RL_NK_PortOfDestination = "AUXXX";

			CusSCAContainer container1 = ocean.Containers.AddNew();
			container1.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.CN_ContainerNumber = "OCLU9991110";

			CusSCAContainer container2 = ocean.Containers.AddNew();
			container2.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			container2.CN_ContainerNumber = "OCLU3333310";

			CusSCAPivot house1Pivot1 = house1.Pivot.AddNew();
			house1Pivot1.CV_CN = container1.PK;
			house1Pivot1.CV_GoodsDescription = "Goods in Container 1";
			house1Pivot1.CV_HazardousGoods = true;
			house1Pivot1.CV_NetWeight = 100m;
			house1Pivot1.CV_Weight = 300m;
			house1Pivot1.CV_WeightUQ = "KG";
			house1Pivot1.CV_Volume = 0.020m;
			house1Pivot1.CV_FumigationCert = true;
			house1Pivot1.CV_PerishableGoods = true;
			house1Pivot1.CV_PersonalEffects = true;
			house1Pivot1.CV_Timber = true;
			house1Pivot1.CV_MarksAndNumbers = "Marks 1";
			house1Pivot1.CV_IsSAC = true;
			house1Pivot1.CV_PackageCount = 8;

			CusSCAPivot house1Pivot2 = house1.Pivot.AddNew();
			house1Pivot2.CV_CN = container2.PK;
			house1Pivot2.CV_GoodsDescription = "Goods in Container 2";
			house1Pivot2.CV_HazardousGoods = false;
			house1Pivot2.CV_NetWeight = 110m;
			house1Pivot2.CV_Weight = 200m;
			house1Pivot2.CV_WeightUQ = "KG";
			house1Pivot2.CV_Volume = 0.030m;
			house1Pivot2.CV_FumigationCert = false;
			house1Pivot2.CV_PerishableGoods = false;
			house1Pivot2.CV_PersonalEffects = false;
			house1Pivot2.CV_Timber = false;
			house1Pivot2.CV_MarksAndNumbers = "Marks 2";
			house1Pivot2.CV_IsSAC = false;
			house1Pivot2.CV_PackageCount = 9;

			CusSCAPivot house2Pivot1 = house2.Pivot.AddNew();
			house2Pivot1.CV_CN = container2.PK;
			house2Pivot1.CV_GoodsDescription = "More in Container 2";
			house2Pivot1.CV_HazardousGoods = true;
			house2Pivot1.CV_NetWeight = 120m;
			house2Pivot1.CV_Weight = 400m;
			house2Pivot1.CV_WeightUQ = "KG";
			house2Pivot1.CV_Volume = 0.040m;
			house2Pivot1.CV_FumigationCert = false;
			house2Pivot1.CV_PerishableGoods = true;
			house2Pivot1.CV_PersonalEffects = true;
			house2Pivot1.CV_Timber = true;
			house2Pivot1.CV_MarksAndNumbers = "Marks 3";
			house2Pivot1.CV_IsSAC = true;
			house2Pivot1.CV_PackageCount = 10;

			var exporter = new SeaCargoContainersExporter(ocean);

			AssertEquals("S1", exporter.GetShipmentNo());
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"Container Number,Seal Number,Mode,Type,Shipper Owned,Container Type,Container Size,Container Status,Packages,Package Type,Net Weight,Gross Weight,Gross Weight Unit,Volume,Hazardous Goods,Fumigation Certificate,Flammable,Personal Effects,Perishable Goods,Timber,Goods Description,Is SAC,Marks And Numbers,House Bill,Origin,Destination,Goods Origin,Payment Type,Shipment Status Code,Shipment Status Desc.,Message Status Code,Message Status Desc.,FF Ind,Shipment ID,Parent Bill,Responsible Party ID,Consignee Code,Consignee Name,Consignee Address 1,Consignee Address 2,Consignee Postcode,Consignee Suburb,Consignee Country,Consignor Code,Consignor Name,Consignor Address 1,Consignor Address 2,Consignor Postcode,Consignor Suburb,Consignor Country,Notify Code,Notify Name,Notify Address 1,Notify Address 2,Notify Postcode,Notify Suburb,Notify Country\r\n" +
"OCLU3333310,,LCL,,N,,,,10,,120,400,KG,0.040,Y,N,N,Y,Y,Y,More in Container 2,Y,MARKS 3,Z2222,USLAX,AUXXX,US,,,,NOT,Not Sent,N,,MasterBill,,,consignee2,consignee2 address1,consignee2 address2,2116,consignee2 suburb,,,consignor2,consignor2 address1,consignor2 address2,2115,consignor2 suburb,,,NOTIFY THEM,Notify2 address1,Notify2 address2,2122,Notify2 suburb,\r\n" +
"OCLU3333310,,LCL,,N,,,,9,,110,200,KG,0.030,N,N,N,N,N,N,Goods in Container 2,N,MARKS 2,Z1123,USLAX,,US,,,,NOT,Not Sent,N,,SUB MASTER,,,consignee1,consignee1 address1,consignee1 address2,2016,consignee1 suburb,,,consignor1,consignor1 address1,consignor1 address2,2015,consignor1 suburb,,,NOTIFY ME,Notify address1,Notify address2,2222,Notify suburb,\r\n" +
"OCLU9991110,,FCL,,N,,,,8,,100,300,KG,0.020,Y,Y,N,Y,Y,Y,Goods in Container 1,Y,MARKS 1,Z1123,USLAX,,US,,,,NOT,Not Sent,N,,SUB MASTER,,,consignee1,consignee1 address1,consignee1 address2,2016,consignee1 suburb,,,consignor1,consignor1 address1,consignor1 address2,2015,consignor1 suburb,,,NOTIFY ME,Notify address1,Notify address2,2222,Notify suburb,\r\n";
			}
		}
	}
}
