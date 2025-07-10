using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;
using UniversalConstants = Enterprise.Customs.DataTransfer.Universal.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class AirManifestDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusHAWBMappings_ICSRelease()
		{
			var mawb = CusMAWBDataObjectWriterTest.SetupCusMAWB(Factory.BOFactory, Factory.New<CusMAWB>(), "MB324242", "CL32423", AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, AirLocalPort2.RL_Code);
			var hawb1 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243", ZBool.True, ZString.Empty);
			hawb1.CS_VendorIdentifier = "vendor";
			Factory.SaveForTesting();
			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawbData = mawbData.SubShipmentCollection[0];
			AssertEquals("vendor", hawbData.VendorIdentifier);
		}

		public void TestAUSpecificCusHAWBDataMerge()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "HOUSE1232",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					ShipmentSubType = new CodeDescriptionPair() { Code = "SST" },
					MessageSubType = new CodeDescriptionPair() { Code = "MST" },
					PaymentMethod = new CodeDescriptionPair() { Code = "PAY" },
					ShipmentType = new CodeDescriptionPair() { Code = "SHP" },
					IsPersonalEffects = ZBool.True,
					WarehouseLocation = "WHLC",
					Folio = "F12",
					ActualChargeable = 132m,
					ConsolidatedCargoStatus = new CodeDescriptionPair() { Code = "CCS" },
					ServiceLevel = new ServiceLevel() { Code = "SVL" }
				};

				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = "MB4532";
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_HAWB = "HB3243";
				hawb.CS_IsSpecialReporter = true;
				hawb.CS_IsSelfAssessedClearance = false;
				hawb.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.ServiceFreightNoCharge;
				hawb.CS_ShipmentType = "STD";
				hawb.CS_IsPersonalEffects = false;
				hawb.CS_WarehouseLocation = "A324";
				hawb.CS_FolioReference = "SD32";
				hawb.CS_ChargableWeight = 532m;
				hawb.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
				hawb.CS_RS_NK_ServiceLevel = ServiceLevel1.RS_Code;
				Factory.SaveForTesting();

				IMergeDataObjectWriter writer = new CusHAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, hawb)), new AirManifestDataObjectWriterHelper(hawb));
				writer.MergeData(dataObject, hawb);
				AssertNull("Should not have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertNull("Should not have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "HOUSE1232", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertNotNull("ShipmentSubType", dataObject.ShipmentSubType);
				AssertEquals("ShipmentSubType.Code", "SST", dataObject.ShipmentSubType.Code);
				AssertNotNull("MessageSubType", dataObject.MessageSubType);
				AssertEquals("MessageSubType.Code", "MST", dataObject.MessageSubType.Code);
				AssertNotNull("PaymentMethod", dataObject.PaymentMethod);
				AssertEquals("PaymentMethod.Code", "PAY", dataObject.PaymentMethod.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", "SHP", dataObject.ShipmentType.Code);
				AssertEquals("IsPersonalEffects", ZBool.True, dataObject.IsPersonalEffects);
				AssertEquals("WarehouseLocation", "WHLC", dataObject.WarehouseLocation);
				AssertEquals("Folio", "F12", dataObject.Folio);
				AssertEquals("ActualChargeable", 132m, dataObject.ActualChargeable);
				AssertNotNull("ConsolidatedCargoStatus", dataObject.ConsolidatedCargoStatus);
				AssertEquals("ConsolidatedCargoStatus.Code", "CCS", dataObject.ConsolidatedCargoStatus.Code);
				AssertNotNull("ServiceLevel", dataObject.ServiceLevel);
				AssertEquals("ServiceLevel.Code", "SVL", dataObject.ServiceLevel.Code);

				writer = new CusHAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AAD, hawb)), new AirManifestDataObjectWriterHelper(hawb));
				writer.MergeData(dataObject, hawb);
				AssertNull("Should not have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertNull("Should not have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "HB3243", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertNotNull("ShipmentSubType", dataObject.ShipmentSubType);
				AssertEquals("ShipmentSubType.Code", Business.AirCargo.Constants.ShipmentSubTypes.IsSpecialReporter, dataObject.ShipmentSubType.Code);
				AssertNotNull("MessageSubType", dataObject.MessageSubType);
				AssertEquals("MessageSubType.Code", JobDeclaration.MessageSubType.FormalEntry, dataObject.MessageSubType.Code);
				AssertNotNull("PaymentMethod", dataObject.PaymentMethod);
				AssertEquals("PaymentMethod.Code", CMRMethodsOfPayment.Codes.ServiceFreightNoCharge, dataObject.PaymentMethod.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", "STD", dataObject.ShipmentType.Code);
				AssertEquals("IsPersonalEffects", ZBool.False, dataObject.IsPersonalEffects);
				AssertEquals("WarehouseLocation", "A324", dataObject.WarehouseLocation);
				AssertEquals("Folio", "SD32", dataObject.Folio);
				AssertEquals("ActualChargeable", 532m, dataObject.ActualChargeable);
				AssertNotNull("ConsolidatedCargoStatus", dataObject.ConsolidatedCargoStatus);
				AssertEquals("ConsolidatedCargoStatus.Code", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, dataObject.ConsolidatedCargoStatus.Code);
				AssertNotNull("ServiceLevel", dataObject.ServiceLevel);
				AssertEquals("ServiceLevel.Code", ServiceLevel1.RS_Code, dataObject.ServiceLevel.Code);

				dataObject = (Shipment)writer.GetDataObject(hawb);
				AssertNotNull("Should have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertNotNull("Should have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "MB4532", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.Master, dataObject.WayBillType.Code);
				AssertEquals("dataObject.SubShipmentCollection.Count", 1, dataObject.SubShipmentCollection.Count);
				dataObject = dataObject.SubShipmentCollection[0];
				AssertEquals("WayBillNumber", "HB3243", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertNotNull("ShipmentSubType", dataObject.ShipmentSubType);
				AssertEquals("ShipmentSubType.Code", Business.AirCargo.Constants.ShipmentSubTypes.IsSpecialReporter, dataObject.ShipmentSubType.Code);
				AssertNotNull("MessageSubType", dataObject.MessageSubType);
				AssertEquals("MessageSubType.Code", JobDeclaration.MessageSubType.FormalEntry, dataObject.MessageSubType.Code);
				AssertNotNull("PaymentMethod", dataObject.PaymentMethod);
				AssertEquals("PaymentMethod.Code", CMRMethodsOfPayment.Codes.ServiceFreightNoCharge, dataObject.PaymentMethod.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", "STD", dataObject.ShipmentType.Code);
				AssertEquals("IsPersonalEffects", ZBool.False, dataObject.IsPersonalEffects);
				AssertEquals("WarehouseLocation", "A324", dataObject.WarehouseLocation);
				AssertEquals("Folio", "SD32", dataObject.Folio);
				AssertEquals("ActualChargeable", 532m, dataObject.ActualChargeable);
				AssertNotNull("ConsolidatedCargoStatus", dataObject.ConsolidatedCargoStatus);
				AssertEquals("ConsolidatedCargoStatus.Code", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, dataObject.ConsolidatedCargoStatus.Code);
				AssertNotNull("ServiceLevel", dataObject.ServiceLevel);
				AssertEquals("ServiceLevel.Code", ServiceLevel1.RS_Code, dataObject.ServiceLevel.Code);
			}
		}

		public void TestCusHAWBMappings()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var org3 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var mawb = CusMAWBDataObjectWriterTest.SetupCusMAWB(Factory.BOFactory, Factory.New<CusMAWB>(), "MB324242", "CL32423", AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, AirLocalPort2.RL_Code);
			var hawb1 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243", ZBool.True, ZString.Empty);
			hawb1.CS_ConsignorName = org3.OH_FullName;
			hawb1.CS_ConsignorStreet = org3.MainAddress.OA_Address1;
			hawb1.CS_ConsignorStreet2 = org3.MainAddress.OA_Address2;
			hawb1.CS_ConsignorCity = org3.MainAddress.OA_City;
			hawb1.CS_ConsignorState = org3.MainAddress.OA_State;
			hawb1.CS_ConsignorPostcode = org3.MainAddress.OA_PostCode;
			hawb1.CS_RN_NKConsignorCountry = org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2);
			hawb1.CS_ConsignorContactName = org3.Contacts[0].OC_ContactName;
			hawb1.CS_ConsignorPhone = org3.MainAddress.OA_Phone;
			hawb1.CS_OA_ConsigneeAddress = org1.MainAddress.PK;
			hawb1.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			var hawb2 = SetupCusHAWB2(mawb.ChildBills.AddNew(), "HB8956", ZBool.False, "HB3243");
			hawb2.CS_OA_ConsignorAddress = org1.MainAddress.PK;
			hawb2.CS_ConsigneeName = org2.OH_FullName;
			hawb2.CS_ConsigneeStreet = org2.MainAddress.OA_Address1;
			hawb2.CS_ConsigneeStreet2 = org2.MainAddress.OA_Address2;
			hawb2.CS_ConsigneeCity = org2.MainAddress.OA_City;
			hawb2.CS_ConsigneeState = org2.MainAddress.OA_State;
			hawb2.CS_ConsigneePostcode = org2.MainAddress.OA_PostCode;
			hawb2.CS_RN_NKConsigneeCountry = org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2);
			hawb2.CS_ConsigneeContactName = org2.Contacts[0].OC_ContactName;
			hawb2.CS_ConsigneePhone = org2.MainAddress.OA_Phone;
			hawb2.CS_CS_MasterHouseBill = hawb1.PK;
			hawb2.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			hawb2.CS_ConsigneeBusinessNumber = "ConsigneeABN";
			hawb2.CS_ConsignorIdentifier = "ConsignorId";
			hawb2.CS_ConsigneeIdentifier = "ConsigneeId";
			SetHACCustomFieldValue(hawb1, "HACTESTSTRING", "HAC Field Value");
			Factory.SaveForTesting();

			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawbData = mawbData.SubShipmentCollection[0];
			AssertHVLVAirShipmentContents(hawbData, "HB3243", ZBool.True, null);
			AssertCodeDescription(hawbData.ConsolidatedCargoStatus, new CodeDescriptionPair() { Code = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, Description = CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased });
			AssertEquals("hawbData.OrganizationAddressCollection.Count", 3, hawbData.OrganizationAddressCollection.Count);
			var hawbDataSendingForwarderAddress = GetOrganizationAddressByType(hawbData, DocAddressType.SendingForwarderAddress);
			AssertAddress("SendingForwarderAddress", hawbDataSendingForwarderAddress, nameof(DocAddressType.SendingForwarderAddress), null, org3.OH_FullName, null,
				org3.MainAddress.OA_Address1, org3.MainAddress.OA_Address2, org3.MainAddress.OA_City, org3.MainAddress.OA_State, org3.MainAddress.OA_PostCode, org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2),
				org3.Contacts[0].OC_ContactName, null, null, null, org3.MainAddress.OA_Phone);
			var hawbDataReceivingForwarderAddress = GetOrganizationAddressByType(hawbData, DocAddressType.ReceivingForwarderAddress);
			AssertOrganizationBO_WUFSHIJNB("ReceivingForwarderAddress", hawbDataReceivingForwarderAddress, nameof(DocAddressType.ReceivingForwarderAddress));
			AssertEquals("hawbData.SubShipmentCollection.Count", 1, hawbData.SubShipmentCollection.Count);
			AssertCustomFieldValue(hawbData, "HACTESTSTRING", "HAC Field Value");

			var hawbSubData = hawbData.SubShipmentCollection[0];
			AssertHVLVAirShipmentContents2(hawbSubData, "HB8956", ZBool.False, "HB3243");
			AssertCodeDescription(hawbSubData.ConsolidatedCargoStatus, new CodeDescriptionPair() { Code = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, Description = CMRConsolidatedCargoStatuses.Descriptions.HeldCargoIsHeldUnderCustomsControl });
			var hawbSubDataConsignorDocumentaryAddress = GetOrganizationAddressByType(hawbSubData, DocAddressType.ConsignorDocumentaryAddress);
			AssertOrganizationBO_WUFSHIJNB("ConsignorDocumentaryAddress", hawbSubDataConsignorDocumentaryAddress, nameof(DocAddressType.ConsignorDocumentaryAddress));
			var hawbSubDataConsigneeDocumentaryAddress = GetOrganizationAddressByType(hawbSubData, DocAddressType.ConsigneeDocumentaryAddress);
			AssertAddress("ConsigneeDocumentaryAddress", hawbSubDataConsigneeDocumentaryAddress, nameof(DocAddressType.ConsigneeDocumentaryAddress), null, org2.OH_FullName, null,
				org2.MainAddress.OA_Address1, org2.MainAddress.OA_Address2, org2.MainAddress.OA_City, org2.MainAddress.OA_State, org2.MainAddress.OA_PostCode, org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2),
				org2.Contacts[0].OC_ContactName, null, null, null, org2.MainAddress.OA_Phone);
			AssertEquals("ConsigneeABN", "ConsigneeABN", hawbSubDataConsigneeDocumentaryAddress.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber)?.Value);
			AssertEquals("ConsignorId", "ConsignorId", hawbSubDataConsignorDocumentaryAddress.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.CodeTypes.CustomsClientID)?.Value);
			AssertEquals("ConsigneeId", "ConsigneeId", hawbSubDataConsigneeDocumentaryAddress.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.CodeTypes.CustomsClientID)?.Value);
			AssertNull("hawbSubData.SubShipmentCollection", hawbSubData.SubShipmentCollection);
		}

		public void TestConsignorMappings()
		{
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Code = "CRAHOLSYD";
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "AU";

			var mawb = CusMAWBDataObjectWriterTest.SetupCusMAWB(Factory.BOFactory, Factory.New<CusMAWB>(), "MB324242", "CL32423", AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, AirLocalPort2.RL_Code);
			var hawb = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243", ZBool.False, ZString.Empty);
			hawb.CS_OA_ConsignorAddress = orgAddress.PK;
			Factory.SaveForTesting();

			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawbData = mawbData.SubShipmentCollection[0];
			var hawbDataConsignorDocumentaryAddress = GetOrganizationAddressByType(hawbData, DocAddressType.ConsignorDocumentaryAddress);
			AssertAddress("ConsignorDocumentaryAddress", hawbDataConsignorDocumentaryAddress, nameof(DocAddressType.ConsignorDocumentaryAddress), shippingAgent1.OH_Code, shippingAgent1.OH_FullName, false, orgAddress.OA_Address1, "", orgAddress.OA_City, orgAddress.OA_State, orgAddress.OA_PostCode, orgAddress.OA_RN_NKCountryCode, null, "", "", null, orgAddress.OA_Phone);
		}

		public void TestConsigneeMappings()
		{
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Code = "CRAHOLSYD";
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "AU";

			var mawb = CusMAWBDataObjectWriterTest.SetupCusMAWB(Factory.BOFactory, Factory.New<CusMAWB>(), "MB324242", "CL32423", AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, AirLocalPort2.RL_Code);
			var hawb = SetupCusHAWB2(mawb.ChildBills.AddNew(), "HB8956", ZBool.False, "HB3243");
			hawb.CS_OA_ConsigneeAddress = orgAddress.PK;
			Factory.SaveForTesting();

			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawbData = mawbData.SubShipmentCollection[0];
			var hawbDataConsigneeDocumentaryAddress = GetOrganizationAddressByType(hawbData, DocAddressType.ConsigneeDocumentaryAddress);
			AssertAddress("ConsigneeDocumentaryAddress", hawbDataConsigneeDocumentaryAddress, nameof(DocAddressType.ConsigneeDocumentaryAddress), shippingAgent1.OH_Code, shippingAgent1.OH_FullName, false, orgAddress.OA_Address1, "", orgAddress.OA_City, orgAddress.OA_State, orgAddress.OA_PostCode, orgAddress.OA_RN_NKCountryCode, null, "", "", null, orgAddress.OA_Phone);
		}

		RefUNLOCO AirLocalPort1
		{
			get
			{
				if (airLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort1;
			}
		}
		RefUNLOCO airLocalPort1;

		RefUNLOCO AirLocalPort2
		{
			get
			{
				if (airLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort2;
			}
		}
		RefUNLOCO airLocalPort2;

		RefUNLOCO airForeignPort1;
		RefUNLOCO AirForeignPort1
		{
			get
			{
				if (airForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort1;
			}
		}

		RefUNLOCO airForeignPort2;
		RefUNLOCO AirForeignPort2
		{
			get
			{
				if (airForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort2;
			}
		}

		void AssertCodeDescription(ICodeDescriptionDataObject actual, ICodeDescriptionDataObject expected)
		{
			if (expected == null)
			{
				AssertNull(actual);
			}
			else
			{
				AssertEquals("Code", expected.Code, actual.Code);
				AssertEquals("Description", expected.Description, actual.Description);
			}
		}

		void AssertHVLVAirShipmentContents(Shipment hawbData, ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse)
		{
			AssertHVLVAirShipmentContents(hawbData, wayBillNumber, isMasterHouse, CodeDescriptionPairForTesting.New(AirForeignPort2.RL_Code, AirForeignPort2.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort2.RL_Code, AirLocalPort2.RL_PortName), 1500.60m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Kilograms, "Kilograms"), 350, "GOODS FOR TESTING", 1304.50m, CodeDescriptionPairForTesting.New(AUD.RX_Code, AUD.RX_Desc),
				CodeDescriptionPairForTesting.New(Business.AirCargo.Constants.ShipmentSubTypes.IsSpecialReporter, "Is Special Reporter"), CodeDescriptionPairForTesting.New(JobDeclaration.MessageSubType.FormalEntry, "Is Formal Entry"), CodeDescriptionPairForTesting.New(CMRMethodsOfPayment.Codes.Collect, CMRMethodsOfPayment.Descriptions.Collect), CodeDescriptionPairForTesting.New("DOC", "Documents Only"), ZBool.False, "WR324", "FL34", 2000.40m, CodeDescriptionPairForTesting.New(ServiceLevel1.RS_Code, ServiceLevel1.RS_Description));
			AssertEquals("hawbData.AdditionalReferenceCollection.Count", 1, hawbData.AdditionalReferenceCollection.Count);
			CusMAWBDataObjectWriterTest.AssertContents(hawbData.AdditionalReferenceCollection[0], "RPI9865463", CodeDescriptionPairForTesting.New(UniversalConstants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, UniversalConstants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));
			AssertHVLVAirShipmentMasterHouse(hawbData, masterHouse);
		}

		void AssertHVLVAirShipmentContents2(Shipment hawbData, ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse)
		{
			AssertHVLVAirShipmentContents(hawbData, wayBillNumber, isMasterHouse, CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort1.RL_Code, AirLocalPort1.RL_PortName), 1000.60m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Pounds, "Pounds"), 860, "GOODS FOR TESTING 2", 150.50m, CodeDescriptionPairForTesting.New(AUD.RX_Code, AUD.RX_Desc),
				CodeDescriptionPairForTesting.New(Business.AirCargo.Constants.ShipmentSubTypes.IsNotSpecialReporter, "Is Not Special Reporter"), CodeDescriptionPairForTesting.New(JobDeclaration.MessageSubType.SelfAssessedClearance, "Is Self Assessed Clearance"), CodeDescriptionPairForTesting.New(CMRMethodsOfPayment.Codes.ServiceFreightNoCharge, CMRMethodsOfPayment.Descriptions.ServiceFreightNoCharge), CodeDescriptionPairForTesting.New("STD", "Standard Shipment"), ZBool.True, "WR965", "FL98", 560.85m, CodeDescriptionPairForTesting.New(ServiceLevel1.RS_Code, ServiceLevel1.RS_Description));
			AssertNull("hawbData.AdditionalReferenceCollection", hawbData.AdditionalReferenceCollection);
			AssertHVLVAirShipmentMasterHouse(hawbData, masterHouse);
		}

		void AssertHVLVAirShipmentContents(Shipment hawbData, ZString? wayBillNumber, ZBool isMasterHouse, ICodeDescription portOfOrigin, ICodeDescription portOfDestination, ZDecimal? weight, ICodeDescription weightUQ, ZInt? piecesManifested, ZString? goodsDescription, ZDecimal? goodsValue, ICodeDescription goodsValueCurrency,
			ICodeDescription shipmentSubType, ICodeDescription messageSubType, ICodeDescription freightPrepaidCollect, ICodeDescription shipmentType, ZBool? isPersonalEffects, ZString? warehouseLocation, ZString? folioReference, ZDecimal? chargableWeight, ICodeDescription serviceLevel)
		{
			AssertNotNull("Precondition: mawbData", hawbData);

			CombineAssertions(delegate
			{
				AssertEquals("hawbData.WayBillNumber", wayBillNumber, hawbData.WayBillNumber);
				AssertNotNull("hawbData.WayBillType", hawbData.WayBillType);
				if (isMasterHouse)
				{
					AssertEquals("hawbData.WayBillType.Code", WayBillTypeList.Codes.MasterHouse, hawbData.WayBillType.Code);
					AssertEquals("hawbData.WayBillType.Description", WayBillTypeList.Descriptions.MasterHouse, hawbData.WayBillType.Description);
				}
				else
				{
					AssertEquals("hawbData.WayBillType.Code", WayBillTypeList.Codes.House, hawbData.WayBillType.Code);
					AssertEquals("hawbData.WayBillType.Description", WayBillTypeList.Descriptions.House, hawbData.WayBillType.Description);
				}

				AssertNotNull("hawbData.PortOfOrigin", hawbData.PortOfOrigin);
				AssertEquals("hawbData.PortOfOrigin.Code", portOfOrigin.Code, hawbData.PortOfOrigin.Code);
				AssertEquals("hawbData.PortOfOrigin.Name", portOfOrigin.Description, hawbData.PortOfOrigin.Name);
				AssertNotNull("hawbData.PortOfDestination", hawbData.PortOfDestination);
				AssertEquals("hawbData.PortOfDestination.Code", portOfDestination.Code, hawbData.PortOfDestination.Code);
				AssertEquals("hawbData.PortOfDestination.Name", portOfDestination.Description, hawbData.PortOfDestination.Name);
				AssertEquals("hawbData.TotalWeight", weight, hawbData.TotalWeight);
				AssertNotNull("hawbData.TotalWeightUnit", hawbData.TotalWeightUnit);
				AssertEquals("hawbData.TotalWeightUnit.Code", weightUQ.Code, hawbData.TotalWeightUnit.Code);
				AssertEquals("hawbData.TotalWeightUnit.Description", weightUQ.Description, hawbData.TotalWeightUnit.Description);
				AssertEquals("hawbData.TotalNoOfPieces", piecesManifested, hawbData.TotalNoOfPieces);
				AssertEquals("hawbData.GoodsDescription", goodsDescription, hawbData.GoodsDescription);
				AssertEquals("hawbData.GoodsValue", goodsValue, hawbData.GoodsValue);
				AssertNotNull("hawbData.GoodsValueCurrency", hawbData.TotalWeightUnit);
				AssertEquals("hawbData.GoodsValueCurrency.Code", goodsValueCurrency.Code, hawbData.GoodsValueCurrency.Code);
				AssertEquals("hawbData.GoodsValueCurrency.Description", goodsValueCurrency.Description, hawbData.GoodsValueCurrency.Description);
				AssertNotNull("hawbData.ShipmentSubType", hawbData.ShipmentSubType);
				AssertEquals("hawbData.ShipmentSubType.Code", shipmentSubType.Code, hawbData.ShipmentSubType.Code);
				AssertEquals("hawbData.ShipmentSubType.Description", shipmentSubType.Description, hawbData.ShipmentSubType.Description);
				if (messageSubType == null)
				{
					AssertNull("hawbData.MessageSubType", hawbData.MessageSubType);
				}
				else
				{
					AssertNotNull("hawbData.MessageSubType", hawbData.MessageSubType);
					AssertEquals("hawbData.MessageSubType.Code", messageSubType.Code, hawbData.MessageSubType.Code);
					AssertEquals("hawbData.MessageSubType.Description", messageSubType.Description, hawbData.MessageSubType.Description);
				}
				AssertNotNull("hawbData.PaymentMethod", hawbData.PaymentMethod);
				AssertEquals("hawbData.PaymentMethod.Code", freightPrepaidCollect.Code, hawbData.PaymentMethod.Code);
				AssertEquals("hawbData.PaymentMethod.Description", freightPrepaidCollect.Description, hawbData.PaymentMethod.Description);
				AssertNotNull("hawbData.ShipmentType", hawbData.ShipmentType);
				AssertEquals("hawbData.ShipmentType.Code", shipmentType.Code, hawbData.ShipmentType.Code);
				AssertEquals("hawbData.ShipmentType.Description", shipmentType.Description, hawbData.ShipmentType.Description);
				AssertEquals("hawbData.IsPersonalEffects", isPersonalEffects, hawbData.IsPersonalEffects);
				AssertEquals("hawbData.WarehouseLocation", warehouseLocation, hawbData.WarehouseLocation);
				AssertEquals("hawbData.Folio", folioReference, hawbData.Folio);
				AssertEquals("hawbData.ActualChargeable", chargableWeight, hawbData.ActualChargeable);
				AssertNotNull("hawbData.ServiceLevel", hawbData.ServiceLevel);
				AssertEquals("hawbData.ServiceLevel.Code", serviceLevel.Code, hawbData.ServiceLevel.Code);
				AssertEquals("hawbData.ServiceLevel.Description", serviceLevel.Description, hawbData.ServiceLevel.Description);
			});
		}

		void AssertHVLVAirShipmentMasterHouse(Shipment hawbData, ZString? masterHouse)
		{
			var additionalBillData = hawbData.AdditionalBillCollection.GetAdditionalBill(hawbData.WayBillNumber.GetValueOrDefault(), hawbData.WayBillType.GetCodeAsUpperCase());
			if (masterHouse.HasValue)
			{
				AssertEquals("additionalBillData.ParentBillNumber", masterHouse, additionalBillData.ParentBillNumber);
			}
			else
			{
				AssertNull("additionalBillData", additionalBillData);
			}
		}

		void AssertCustomFieldValue(Shipment hawbDataObject, ZString fieldName, ZString fieldValue)
		{
			AssertContains("Expected Custom Field", fieldName, string.Join(", ", hawbDataObject.CustomizedFieldCollection.Select(x => x.Key.Value)));
			var customField = hawbDataObject.CustomizedFieldCollection.FirstOrDefault(x => x.Key.Value == fieldName);
			AssertEquals("Expected value on Custom Field named " + fieldName, fieldValue, customField.Value.Value);
		}

		void SetHACCustomFieldValue(CusHAWB hawb, ZString fieldName, ZString fieldValue)
		{
			var hacTemplate1 = Factory.New<ProcessTaskTemplate>();
			hacTemplate1.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			hacTemplate1.P0_Name = "TEST HAC 1";
			hacTemplate1.P0_Description = "TEST HAC 1 DESC";
			var hacCustomField1 = hacTemplate1.GenCustomColumnDefinitions.AddNew();
			hacCustomField1.XC_Name = fieldName;
			hacCustomField1.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.SaveForTesting();

			ICustomPropertyContainer customBusinessObject = (hawb as ICustomFieldProvider).GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Identifier.Contains(fieldName));
			customProperty.TrySetValue(hawb, fieldValue);
		}

		CusHAWB SetupCusHAWB(CusHAWB hawb, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse)
		{
			return SetupCusHAWB(hawb, wayBillNumber, isMasterHouse, masterHouse, AirForeignPort2.RL_Code, AirLocalPort2.RL_Code, 1500.60m, Core.Constants.Weight.Kilograms, 350, "GOODS FOR TESTING", 1304.50m, AUD.RX_Code, "RPI9865463",
				ZBool.True, ZBool.False, CMRMethodsOfPayment.Codes.Collect, "DOC", ZBool.False, "WR324", "FL34", 2000.40m, ServiceLevel1.RS_Code);
		}

		CusHAWB SetupCusHAWB2(CusHAWB hawb, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse)
		{
			return SetupCusHAWB(hawb, wayBillNumber, isMasterHouse, masterHouse, AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, 1000.60m, Core.Constants.Weight.Pounds, 860, "GOODS FOR TESTING 2", 150.50m, AUD.RX_Code, ZString.Empty,
				ZBool.False, ZBool.True, CMRMethodsOfPayment.Codes.ServiceFreightNoCharge, "STD", ZBool.True, "WR965", "FL98", 560.85m, ServiceLevel1.RS_Code);
		}

		CusHAWB SetupCusHAWB(CusHAWB hawb, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency, ZString responsiblePartyID,
			ZBool isSpecialReporter, ZBool isSelfAssessedClearance, ZString freightPrepaidCollect, ZString shipmentType, ZBool isPersonalEffects, ZString warehouseLocation, ZString folioReference, ZDecimal chargableWeight, ZString serviceLevel)
		{
			hawb.CS_HAWB = wayBillNumber;
			hawb.CS_IsMasterHouse = isMasterHouse;
			hawb.CS_MasterHouseBill = masterHouse;
			hawb.CS_RL_NKOrigin = origin;
			hawb.CS_RL_NKDestination = destination;
			hawb.CS_Weight = weight;
			hawb.CS_WeightUQ = weightUQ;
			hawb.CS_PiecesManifested = piecesManifested;
			hawb.CS_GoodsDescription = goodsDescription;
			hawb.CS_GoodsValue = goodsValue;
			hawb.CS_RX_NKGoodsCurrency = goodsCurrency;
			hawb.CS_ResponsiblePartyID = responsiblePartyID;
			hawb.CS_IsSpecialReporter = isSpecialReporter;
			hawb.CS_IsSelfAssessedClearance = isSelfAssessedClearance;
			hawb.CS_FreightPrepaidCollect = freightPrepaidCollect;
			hawb.CS_ShipmentType = shipmentType;
			hawb.CS_IsPersonalEffects = isPersonalEffects;
			hawb.CS_WarehouseLocation = warehouseLocation;
			hawb.CS_FolioReference = folioReference;
			hawb.CS_ChargableWeight = chargableWeight;
			hawb.CS_RS_NK_ServiceLevel = serviceLevel;
			return hawb;
		}

		#region Implementation

		RefServiceLevel ServiceLevel1
		{
			get
			{
				if (serviceLevel1 == null)
				{
					serviceLevel1 = Factory.New<RefServiceLevel>();
					serviceLevel1.RS_Code = "U3!";
					serviceLevel1.RS_Description = "US SERVICE LEVEL 1";
				}
				return serviceLevel1;
			}
		}
		RefServiceLevel serviceLevel1;

		RefCurrency AUD
		{
			get { return aud ?? (aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia)); }
		}
		RefCurrency aud;

		OrganizationAddress GetOrganizationAddressByType(Shipment shipment, DocAddressType type)
		{
			return shipment.OrganizationAddressCollection.Single(org => org != null && org.AddressType.HasValue && org.AddressType.Value == type.ToString());
		}

		#endregion
	}
}
