using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAHouseForwardingShipmentCustomsStatusProviderTestCase : TestCaseWithFactory
	{
		public void TestStatusesWithEManifestACI()
		{
			var currentCompanyCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "CC", Core.Constants.CountryCodes.Canada);
				Factory.Save();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				var shipment = consol.Shipments.AddNew();
				Assert("IsSea needs to return true for this to work", shipment.IsSea);
				shipment.JS_RL_NKDestination = "CATOR";
				var master = Factory.New<CusCAeMHMaster>();
				master.BP_ParentID = consol.PK;
				master.BP_ParentTableCode = consol.TablePrefix;
				var house = master.HouseBills.AddNew();
				house.BW_ParentID = shipment.PK;
				Factory.Save();
				house.BW_MessageStatus = "WTO";
				house.BW_CustomsStatus = "HLD";

				var customsStatusProvider = new ForwardingShipmentCustomsStatusProvider(shipment);

				AssertEquals("WTO", customsStatusProvider.CustomsMessageStatus());
				AssertEquals("HLD", customsStatusProvider.CustomsCargoStatus());
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCompanyCountry;
				OrgCusCode orgCusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada);
				if (orgCusCode != null)
				{
					GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Remove(orgCusCode);
				}
			}
		}

		public void TestStatusesWithLegacyACI()
		{
			ZString currentCompanyCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "CC", Core.Constants.CountryCodes.Canada);
				Factory.Save();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				var shipment = consol.Shipments.AddNew();
				Assert("IsSea needs to return true for this to work", shipment.IsSea);
				shipment.JS_RL_NKDestination = "CATOR";
				var master = Factory.New<CusSCAOceanBill>();
				master.CB_ParentId = consol.PK;
				master.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
				var house = master.HouseBills.AddNew();
				house.CA_JS = shipment.PK;
				Factory.Save();
				house.CA_MessageStatus = "WTO";
				house.CA_ShipmentStatus = "HLD";

				var customsStatusProvider = new ForwardingShipmentCustomsStatusProvider(shipment);

				AssertEquals("WTO", customsStatusProvider.CustomsMessageStatus());
				AssertEquals("HLD", customsStatusProvider.CustomsCargoStatus());
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCompanyCountry;
				OrgCusCode orgCusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada);
				if (orgCusCode != null)
				{
					GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Remove(orgCusCode);
				}
			}
		}

		public void TestReleaseStatusWrapperMessageContentRejected()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var messageContentRejectedText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");
			AddEDIMessage(shipment, EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, messageContentRejectedText, new ZDateTime(2010, 1, 1));

			var statusProvider = new ForwardingShipmentCustomsStatusProvider(shipment);

			AssertEquals("14 - Error in last message, please fix and re-submit", statusProvider.GetReleaseStatusWrapper().ProcessingIndicatorDescription);
		}

		public void TestReleaseStatusWrapperMessageGoodsReleased()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var messageContentRejectedText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");

			var errorText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+14'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");

			var goodsReleasedText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+4'
RFF+XC:37132536987'
UNT+7+1'".Replace("\r\n", "");

			AddEDIMessage(shipment, EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, messageContentRejectedText, new ZDateTime(2010, 1, 3));
			AddEDIMessage(shipment, EDIReleaseImportEntryStatusList.Codes.Error, errorText, new ZDateTime(2010, 1, 2));
			AddEDIMessage(shipment, EDIReleaseImportEntryStatusList.Codes.GoodsReleased, goodsReleasedText, new ZDateTime(2010, 1, 1));

			var statusProvider = new ForwardingShipmentCustomsStatusProvider(shipment);
			AssertEquals("4 - Goods Released", statusProvider.GetReleaseStatusWrapper().ProcessingIndicatorDescription);
		}

		void AddEDIMessage(ForwardingShipment shipment, string messageSubType, string messageText, ZDateTime systemCreateTime)
		{
			var message = shipment.Messages.AddNew(typeof(EDIReleaseMessage));
			message.EM_SystemCreateTimeUtc = systemCreateTime;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = messageText;
		}
	}
}
