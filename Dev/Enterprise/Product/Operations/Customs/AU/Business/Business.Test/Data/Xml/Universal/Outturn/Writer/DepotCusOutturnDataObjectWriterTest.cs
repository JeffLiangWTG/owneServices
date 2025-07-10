using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DepotCusOutturnDataObjectWriterTest : DataTransfer.Universal.Outturn.Testing.OutturnDataObjectWriterTestHelper
	{
		public void TestDepotCusOutturnMappings()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("AA33N", "67094168242");
			using (FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList))
			{
				var outturnHeader = Factory.New<CusOutturnHeader>();
				var destinationAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				destinationAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "AA33N", Core.Constants.CountryCodes.Australia);
				outturnHeader.C6_OA_OutturningPremise = destinationAddress.MainAddress.PK;
				outturnHeader.C6_OutturningPremiseID = "AA33N";

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_FullName = "RESPONSIBLE PARTY";
				var cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				cusCode.OK_CustomsRegNo = "41065894724";
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

				var outturn = outturnHeader.Outturns.AddNew();
				outturn.C5_HouseBill = "HBL002";
				outturn.C5_MasterBill = "OBLDPT001";
				outturn.C5_ContainerNumber = "OCLU8911239";
				var seiMessage = (CMRSEIMessage)outturnHeader.Messages.AddNew(typeof(CMRSEIMessage));
				seiMessage.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
				seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
				seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
				AssertEquals("InlandMovementMode", "ROA", outturn.InlandMovementMode);
				AssertEquals("UBMResponsibleID", "41065894724", outturn.UBMResponsibleID);
				AssertEquals("UBMResponsibleIDName", "RESPONSIBLE PARTY", outturn.UBMResponsibleIDName);
				AssertEquals("RecipientSiteID", "AAA374M", outturn.RecipientSiteID);
				AssertEquals("FreightForwarderIndicator", "N", outturn.FreightForwarderIndicator);
				AssertEquals("ConsigneeName", "CONSIGNEE 2", outturn.ConsigneeName);
				AssertEquals("NetWeight", "7001 KG", outturn.NetWeight);
				AssertEquals("GrossWeight", "7000 KG", outturn.GrossWeight);
				AssertEquals("Volume", "7 CM", outturn.Volume);

				var uBMMessage = (CMRUBMREQRMessage)outturn.Messages.AddNew(typeof(CMRUBMREQRMessage));
				uBMMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+1111A::95'
LOC+4+2222O::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
RFF+MB:OB5000'
RFF+BH:HB4000'
UNT+17+000001'".Replace("\r\n", "");
				uBMMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
				uBMMessage.EM_MessageType = CMRMessage.CMRMessageTypes.UBMREQR;
				AssertEquals("UBMRequestReason", "MOV", outturn.UBMRequestReason);
				AssertEquals("UBMOriginID", "1111A", outturn.UBMOriginID);
				AssertEquals("UBMDestinationID", "2222O", outturn.UBMDestinationID);

				var writer = new CusOutturnHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, outturnHeader)));
				var outturnHeaderData = writer.GetDataObject(outturnHeader);
				var outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();

				CombineAssertions(delegate
				{
					AssertEquals("SubShipment Count", 1, outturnHeaderData.SubShipmentCollection.Count);
					AssertEquals("SubShipment AddInfo Count", 15, outturnData.AddInfoCollection.Count);
					AssertAddInfo("Consignee", outturnData, Outturn.Constants.AddInfoType.Consignee, "CONSIGNEE 2");
					AssertAddInfo("FFInd", outturnData, Outturn.Constants.AddInfoType.FFInd, "N");
					AssertAddInfo("GrossWt", outturnData, Outturn.Constants.AddInfoType.GrossWt, "7000 KG");
					AssertAddInfo("Mode", outturnData, Outturn.Constants.AddInfoType.Mode, "ROA");
					AssertAddInfo("NetWt", outturnData, Outturn.Constants.AddInfoType.NetWt, "7001 KG");
					AssertAddInfo("Site", outturnData, Outturn.Constants.AddInfoType.Site, "AAA374M");
					AssertAddInfo("UBMDest", outturnData, Outturn.Constants.AddInfoType.UBMDest, "2222O");
					AssertAddInfo("UBMOrg", outturnData, Outturn.Constants.AddInfoType.UBMOrg, "1111A");
					AssertAddInfo("UBMReason", outturnData, Outturn.Constants.AddInfoType.UBMReason, "MOV");
					AssertAddInfo("UBMPartyID", outturnData, Outturn.Constants.AddInfoType.UBMPartyID, "41065894724");
					AssertAddInfo("UBMPartyName", outturnData, Outturn.Constants.AddInfoType.UBMPartyName, "RESPONSIBLE PARTY");
					AssertAddInfo("LoadList", outturnData, Outturn.Constants.AddInfoType.LoadList, "");
					AssertAddInfo("ShipmentOrContainerNumber", outturnData, Outturn.Constants.AddInfoType.ShipmentOrContainerNumber, "");
				});

				var creator = new CFSShipmentCreator(outturn, Factory.BOFactory);
				var shipment = creator.Shipment;
				shipment.JS_UniqueConsignRef = "JS123";

				outturnHeaderData = writer.GetDataObject(outturnHeader);
				outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();
				AssertAddInfo("LoadList", outturnData, Outturn.Constants.AddInfoType.LoadList, "");
				AssertAddInfo("ShipmentOrContainerNumber", outturnData, Outturn.Constants.AddInfoType.ShipmentOrContainerNumber, "JS123");

				var consol = outturn.LoadList;
				consol.JK_UniqueConsignRef = "JK2343423424";
				consol.JK_TransportMode = "SEA";

				outturnHeaderData = writer.GetDataObject(outturnHeader);
				outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();
				AssertAddInfo("LoadList", outturnData, Outturn.Constants.AddInfoType.LoadList, "JK2343423424");

				AssertNull(outturnData.ConsolidatedCargoStatus);

				writer = new CusOutturnHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ATW, outturnHeader)));
				outturn.C5_CustomsStatus = "CLR";
				outturnHeaderData = writer.GetDataObject(outturnHeader);
				outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();
				AssertEquals("CLR", outturnData.ConsolidatedCargoStatus.Code);

				outturn.C5_CustomsStatus = "";
				outturnHeaderData = writer.GetDataObject(outturnHeader);
				outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();
				AssertNull(outturnData.ConsolidatedCargoStatus);

				outturn.C5_CustomsStatus = "CCL";
				outturnHeaderData = writer.GetDataObject(outturnHeader);
				outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();
				AssertEquals("HLD", outturnData.ConsolidatedCargoStatus.Code);

				outturn.C5_CustomsStatus = "SUB";
				outturnHeaderData = writer.GetDataObject(outturnHeader);
				outturnData = outturnHeaderData.SubShipmentCollection.FirstOrDefault();
				AssertNull(outturnData.ConsolidatedCargoStatus);
			}
		}
	}
}
