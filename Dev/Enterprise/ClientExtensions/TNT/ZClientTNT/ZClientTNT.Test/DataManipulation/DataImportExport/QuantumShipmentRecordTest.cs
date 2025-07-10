using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TNT.NZ;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.Testing
{
	public class QuantumShipmentRecordTest : ConsignmentRecordTest
	{
#region TestFieldProperty
		public override void TestFieldProperty()
		{
			QuantumShipmentRecord record = QuantumShipmentRecord.New(SydBranch.GB_Code, MBagNo, DataString);
			AssertEquals("Branch Code", SydBranch.GB_Code, record.BranchCode);
			AssertEquals("MBag No", MBagNo, record.MBagNo);
			AssertEquals("HouseBill", "940432180", record.HouseBill);
			record.HouseBill = " Consigment No ";
			AssertEquals("HouseBill", "Consigment No", record.HouseBill);
			AssertEquals("Origin", "ADL", record.Origin);
			record.Origin = " SY D ";
			AssertEquals("Origin", "SY D", record.Origin);
			AssertEquals("Destination", "USO", record.Destination);
			record.Destination = " ND Z ";
			AssertEquals("Destination", "ND Z", record.Destination);
			AssertEquals("Consignor Legacy Code", "20908767", record.ConsignorLegacyCode);
			record.ConsignorLegacyCode = LongString;
			AssertEquals("Consignor Legacy Code", LongString.KeepChars("1234567890").Left(OrgCusCodeSchema.OK_CustomsRegNo.MaxLength), record.ConsignorLegacyCode);
			AssertEquals("Consignor Company Name", "COVANCE P/L", record.ConsignorName);
			record.ConsignorName = LongString;
			AssertEquals("Consignor Company Name", GetTrimLongStringToMaxLength(OrgHeaderSchema.OH_FullName.MaxLength), record.ConsignorName);
			AssertEquals("Consignor Address 1", "FL 3 4 RESEARCH PARK DR", record.ConsignorAddress1);
			record.ConsignorAddress1 = LongString;
			AssertEquals("Consignor Address 1", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address1.MaxLength), record.ConsignorAddress1);
			AssertEquals("Consignor Address 2", "MACQUARIE UNI", record.ConsignorAddress2);
			record.ConsignorAddress2 = LongString;
			AssertEquals("Consignor Address 2", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address2.MaxLength), record.ConsignorAddress2);
			AssertEquals("Consignor City", "NORTH RYDE", record.ConsignorCity);
			record.ConsignorCity = LongString;
			AssertEquals("Consignor City", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_City.MaxLength), record.ConsignorCity);
			AssertEquals("Consignor State", "NEW SOUTH WALES", record.ConsignorState);
			record.ConsignorState = LongString;
			AssertEquals("Consignor State", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_State.MaxLength), record.ConsignorState);
			AssertEquals("Consignor Country Code", "AU", record.ConsignorCountry);
			record.ConsignorCountry = " Consignor Country Code ";
			AssertEquals("Consignor Country Code", "Consignor Country Code", record.ConsignorCountry);
			AssertEquals("Consignor Postcode", "2113", record.ConsignorPostCode);
			record.ConsignorPostCode = LongString;
			AssertEquals("Consignor Postcode", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_PostCode.MaxLength), record.ConsignorPostCode);
			AssertEquals("Consignor Company Phone", "0288792000", record.ConsignorPhone);
			record.ConsignorPhone = LongString;
			AssertEquals("Consignor Company Phone", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Phone.MaxLength), record.ConsignorPhone);
			AssertEquals("Consignor Contact Phone", "0288792000", record.ConsignorContactPhone);
			record.ConsignorContactPhone = LongString;
			AssertEquals("Consignor Contact Phone", GetTrimLongStringToMaxLength(OrgContactSchema.OC_Phone.MaxLength), record.ConsignorContactPhone);
			AssertEquals("Consignor Contact Name", "BOB SMITH", record.ConsignorContactName);
			record.ConsignorContactName = LongString;
			AssertEquals("Consignor Contact Name", GetTrimLongStringToMaxLength(OrgContactSchema.OC_ContactName.MaxLength), record.ConsignorContactName);
			AssertEquals("Pickup Company Name", "SLEEP LAB LEVEL 6 MCEWIN BLDG", record.PickupName);
			record.PickupName = LongString;
			AssertEquals("Pickup Company Name", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_CompanyNameOverride.MaxLength), record.PickupName);
			AssertEquals("Pickup Address 1", "ROYAL ADELAIDE HOSPITAL", record.PickupAddress1);
			record.PickupAddress1 = LongString;
			AssertEquals("Pickup Address 1", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address1.MaxLength), record.PickupAddress1);
			AssertEquals("Pickup Address 2", "NORTH TERRACE", record.PickupAddress2);
			record.PickupAddress2 = LongString;
			AssertEquals("Pickup Address 2", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address1.MaxLength), record.PickupAddress2);
			AssertEquals("Pickup City", "ADELAIDE", record.PickupCity);
			record.PickupCity = LongString;
			AssertEquals("Pickup City", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_City.MaxLength), record.PickupCity);
			AssertEquals("Pickup State", "SOUTH AUSTRALIA", record.PickupState);
			record.PickupState = LongString;
			AssertEquals("Pickup State", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_State.MaxLength), record.PickupState);
			AssertEquals("Pickup Country Code", "AU", record.PickupCountry);
			record.PickupCountry = " Pickup Country Code ";
			AssertEquals("Pickup Country Code", "Pickup Country Code", record.PickupCountry);
			AssertEquals("Pickup Postcode", "5000", record.PickupPostCode);
			record.PickupPostCode = LongString;
			AssertEquals("Pickup Postcode", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_PostCode.MaxLength), record.PickupPostCode);
			AssertEquals("Pickup Company Phone", "08485648144", record.PickupPhone);
			record.PickupPhone = LongString;
			AssertEquals("Pickup Company Phone", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Phone.MaxLength), record.PickupPhone);
			AssertEquals("Pickup Contact Phone", "08468451155", record.PickupContactPhone);
			record.PickupContactPhone = LongString;
			AssertEquals("Pickup Contact Phone", GetTrimLongStringToMaxLength(OrgContactSchema.OC_Phone.MaxLength), record.PickupContactPhone);
			AssertEquals("Pickup Contact Name", "GAIL KOSHOREK", record.PickupContactName);
			record.PickupContactName = LongString;
			AssertEquals("Pickup Contact Name", GetTrimLongStringToMaxLength(OrgContactSchema.OC_ContactName.MaxLength), record.PickupContactName);
			AssertEquals("Consignee Company Name", "HENRY FORD HOSPITAL", record.ConsigneeName);
			record.ConsigneeName = LongString;
			AssertEquals("Consignee Company Name", GetTrimLongStringToMaxLength(OrgHeaderSchema.OH_FullName.MaxLength), record.ConsigneeName);
			AssertEquals("Consignee Address 1", "SLEEP DISORDERS RESEARCH CENT", record.ConsigneeAddress1);
			record.ConsigneeAddress1 = LongString;
			AssertEquals("Consignee Address 1", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address1.MaxLength), record.ConsigneeAddress1);
			AssertEquals("Consignee Address 2", "2799 W GRAND BLVD CFP 3NIK", record.ConsigneeAddress2);
			record.ConsigneeAddress2 = LongString;
			AssertEquals("Consignee Address 2", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address2.MaxLength), record.ConsigneeAddress2);
			AssertEquals("Consignee City", "DETROIT", record.ConsigneeCity);
			record.ConsigneeCity = LongString;
			AssertEquals("Consignee City", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_City.MaxLength), record.ConsigneeCity);
			AssertEquals("Consignee State", "MI", record.ConsigneeState);
			record.ConsigneeState = LongString;
			AssertEquals("Consignee State", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_State.MaxLength), record.ConsigneeState);
			AssertEquals("Consignee Country Code", "US", record.ConsigneeCountry);
			record.ConsigneeCountry = " Consignee Country Code ";
			AssertEquals("Consignee Country Code", "Consignee Country Code", record.ConsigneeCountry);
			AssertEquals("Consignee Postcode", "48202", record.ConsigneePostCode);
			record.ConsigneePostCode = LongString;
			AssertEquals("Consignee Postcode", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_PostCode.MaxLength), record.ConsigneePostCode);
			AssertEquals("Consignee Company Phone", "68454578982", record.ConsigneePhone);
			record.ConsigneePhone = LongString;
			AssertEquals("Consignee Company Phone", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Phone.MaxLength), record.ConsigneePhone);
			AssertEquals("Consignee Contact Phone", "12345678901", record.ConsigneeContactPhone);
			record.ConsigneeContactPhone = LongString;
			AssertEquals("Consignee Contact Phone", GetTrimLongStringToMaxLength(OrgContactSchema.OC_Phone.MaxLength), record.ConsigneeContactPhone);
			AssertEquals("Consignee Contact Name", "GAIL KOSHOREK", record.ConsigneeContactName);
			record.ConsigneeContactName = LongString;
			AssertEquals("Consignee Contact Name", GetTrimLongStringToMaxLength(OrgContactSchema.OC_ContactName.MaxLength), record.ConsigneeContactName);
			AssertEquals("Delivery Company Name", "DELIVERY COMPANY NAME", record.DeliveryName);
			record.DeliveryName = LongString;
			AssertEquals("Delivery Company Name", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_CompanyNameOverride.MaxLength), record.DeliveryName);
			AssertEquals("Delivery Address 1", "DELIVERY ADDRESS 1", record.DeliveryAddress1);
			record.DeliveryAddress1 = LongString;
			AssertEquals("Delivery Address 1", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address1.MaxLength), record.DeliveryAddress1);
			AssertEquals("Delivery Address 2", "DELIVERY ADDRESS 2", record.DeliveryAddress2);
			record.DeliveryAddress2 = LongString;
			AssertEquals("Delivery Address 2", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Address1.MaxLength), record.DeliveryAddress2);
			AssertEquals("Delivery City", "DELIVERY CITY", record.DeliveryCity);
			record.DeliveryCity = LongString;
			AssertEquals("Delivery City", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_City.MaxLength), record.DeliveryCity);
			AssertEquals("Delivery State", "DELIVERY STATE", record.DeliveryState);
			record.DeliveryState = LongString;
			AssertEquals("Delivery State", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_State.MaxLength), record.DeliveryState);
			AssertEquals("Delivery Country Code", "CAN", record.DeliveryCountry);
			record.DeliveryCountry = "Delivery Country Code ";
			AssertEquals("Delivery Country Code", "Delivery Country Code", record.DeliveryCountry);
			AssertEquals("Delivery Postcode", "123456789", record.DeliveryPostCode);
			record.DeliveryPostCode = LongString;
			AssertEquals("Delivery Postcode", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_PostCode.MaxLength), record.DeliveryPostCode);
			AssertEquals("Delivery Company Phone", "012345678901", record.DeliveryPhone);
			record.DeliveryPhone = LongString;
			AssertEquals("Delivery Company Phone", GetTrimLongStringToMaxLength(OrgAddressSchema.OA_Phone.MaxLength), record.DeliveryPhone);
			AssertEquals("Delivery Contact Phone", "234567890123", record.DeliveryContactPhone);
			record.DeliveryContactPhone = LongString;
			AssertEquals("Delivery Contact Phone", GetTrimLongStringToMaxLength(OrgContactSchema.OC_Phone.MaxLength), record.DeliveryContactPhone);
			AssertEquals("Delivery Contact Name", "DELIVERY CONTACT NAME", record.DeliveryContactName);
			record.DeliveryContactName = LongString;
			AssertEquals("Delivery Contact Name", GetTrimLongStringToMaxLength(OrgContactSchema.OC_ContactName.MaxLength), record.DeliveryContactName);
			AssertEquals("IsSubHousebill", false, record.IsSubHousebill);
			record.DocumentIndicator = " d ";
			AssertEquals("IsSubHousebill", true, record.IsSubHousebill);
			AssertEquals("T-Doc Number", "T-DOC1234", record.TDoc);
			AssertEquals("ECN", "T-DOC1234", record.ECN);
			record.TDoc = " T-Doc Number ";
			AssertEquals("T-Doc Number", "T-Doc Number", record.TDoc);
			AssertEquals("ECN", "T-Doc Number", record.ECN);
		}

#endregion
#region TestCreateShipment
		public void TestCreateShipment()
		{
			ZDateTime startDate = ZDateTime.Today;
			UpdateLOCODEMapping();
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestType03Record);
			ForwardingShipment testShipment = testRecord.CreateShipment(Factory, false, dummyNotifier);
			AssertNotNull("Shipment should have been created", testShipment);
			AssertEquals("Shipment should be on test factory", Factory, testShipment.Factory);
			AssertEquals("HouseBill", testRecord.HouseBill, testShipment.JS_HouseBill);
			AssertEquals("CartageWaybill", testRecord.OriginDestinationAndBranch_ToCartageWaybill, testShipment.JS_CartageWaybill);
			AssertEquals("Origin", "AUSYD", testShipment.JS_RL_NKOrigin);
			AssertEquals("Destination", testRecord.Destination, testShipment.JS_RL_NKDestination);
			AssertNotNull("Consignor should have been created if not found", testShipment.Consignor);
			AssertEquals("Consignor is marked as temporary account", true, testShipment.Consignor.OH_IsTempAccount);
			ZQuery legacyFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			legacyFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, testRecord.ConsignorLegacyCode.Trim());
			AssertEquals("Temporary Consignor should not have legacy code", true, testShipment.Consignor.CustomsCodes.Find(legacyFilter).Length >= 0);
			AssertEquals("ConsignorName", testRecord.ConsignorName, testShipment.Consignor.OH_FullName);
			AssertEquals("ShipmentConsignorContact", testRecord.ConsignorContactName, testShipment.ConsignorDocumentaryAddress.E2_Contact);
			AssertEquals("ConsignorAddress1", testRecord.ConsignorAddress1, testShipment.Consignor.MainAddress.OA_Address1);
			AssertNotNull("Consignee should have been created if not found", testShipment.Consignee);
			AssertEquals("Consignee is marked as temporary account", true, testShipment.Consignee.OH_IsTempAccount);
			AssertEquals("ConsigneeName", testRecord.ConsigneeName, testShipment.Consignee.OH_FullName);
			AssertEquals("ShipmentConsigneeContact", testRecord.ConsigneeContactName, testShipment.ConsigneeDocumentaryAddress.E2_Contact);
			AssertEquals("ConsigneeAddress1", testRecord.ConsigneeAddress1, testShipment.Consignee.MainAddress.OA_Address1);
			AssertEquals("GoodsValue", testRecord.GoodsValue, testShipment.JS_GoodsValue);
			RefCurrency jpyCurrency = Factory.Load<RefCurrency>(new Guid("30130E3D-4EEA-4A61-871E-151B8D75EC7F"));
			AssertEquals("GoodsCurrency - JPY GUID", jpyCurrency.RX_Code, testShipment.JS_RX_NKGoodsValueCurr);
			AssertEquals("PackageCount", Convert.ToInt32(testRecord.PackageCount), testShipment.JS_OuterPacks);
			AssertEquals("Weight", Convert.ToDecimal(testRecord.Weight), testShipment.JS_ActualWeight);
			AssertEquals("IncoTerm", testRecord.IncoTerm, testShipment.JS_INCO);
			AssertEquals("CustomsEntryNumberType", testRecord.TDoc, testShipment.CustomsEntryNumberType);
			AssertEquals("TransportMode", Core.Constants.TransportModes.Air, testShipment.JS_TransportMode);
			AssertEquals("PackingMode", Core.Constants.ContainerModes.Loose, testShipment.JS_PackingMode);
			if (ZDateTime.Today == startDate)
			{
				AssertEquals("ETD", startDate, testShipment.JS_E_DEP);
			}

			AssertEquals("OuterPacksPackType", Core.Constants.PkgUnit.Piece, testShipment.JS_F3_NKPackType);
			AssertEquals("UnitOfWeight", Core.Constants.Weight.Kilograms, testShipment.JS_UnitOfWeight);
			AssertNotNull("JobDocsAndCartage should not be null", testShipment.DocsAndCartage);
			AssertEquals("Consignor Pickup Address will default to Consignor Address", testRecord.ConsignorAddress1, testShipment.ConsignorPickupAddress.E2_Address1);
			// Declaration
			AssertEquals("NO Declaration should have been created", 0, testShipment.Declarations.Length);
		}

		public void TestCreateShipmentWithDeclaration()
		{
			ZDateTime startDate = ZDateTime.Today;
			UpdateLOCODEMapping();
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestType03RecordNoEntryNum);
			ForwardingShipment testShipment = testRecord.CreateShipment(Factory, true, dummyNotifier);
			// Declaration
			JobDeclaration testDeclaration = (JobDeclaration)testShipment.Declarations[0];
			AssertNotNull("Declaration should have been created", testDeclaration);
			AssertEquals("Declaration - Branch", SydBranch.GB_Code, testDeclaration.Branch.GB_Code);
			Assert("Declaration - DateAtOrigin", testDeclaration.JE_DateAtOrigin.IsValid);
			AssertEquals("Declaration - ExportDate", testDeclaration.JE_DateAtOrigin, testDeclaration.JE_ExportDate);
			AssertEquals("Declaration - Origin", "AUSYD", testDeclaration.JE_RL_NKOrigin);
			AssertEquals("Declaration - FinalDestination", testRecord.Destination, testDeclaration.JE_RL_NKFinalDestination);
			AssertEquals("Declaration - PortOfLoading", testDeclaration.JE_RL_NKOrigin, testDeclaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration - PortOfArrival", testDeclaration.JE_RL_NKFinalDestination, testDeclaration.JE_RL_NKPortOfArrival);
			AssertEquals("Declaration - PortOfFirstArrival", ZString.Empty, testDeclaration.JE_RL_NKPortOfFirstArrival);
			// Invoice
			Assert("Default Invoice should have been created", testDeclaration.Invoices.Count > 0);
			JobComInvoiceHeader testInvHeader = testDeclaration.Invoices[0];
			AssertEquals("Invoice - Supplier", testShipment.ConsignorPK, testInvHeader.JZ_OH_Supplier);
			AssertEquals("Invoice - Value", testShipment.JS_GoodsValue, testInvHeader.JZ_InvoiceAmount);
			AssertEquals("Invoice - Currency", testShipment.GoodsValueCurr.RX_Code, testInvHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals("Invoice - Inco term", testShipment.JS_INCO, testInvHeader.JZ_IncoTerm);
			AssertEquals("Invoice - Weight", testShipment.JS_ActualWeight, testInvHeader.JZ_Weight);
			AssertEquals("Invoice - Weight UQ", testShipment.JS_UnitOfWeight, testInvHeader.JZ_WeightUQ);
			// Invoice Line
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobComInvoiceLineSchema.JI_JZ, testInvHeader.PK);
			JobComInvoiceLine testInvLine = Factory.LoadTop1<JobComInvoiceLine>(filter);
			AssertNotNull("Default Invoice Line should have been created", testInvLine);
			AssertEquals("Invoice Line - Quantity", Convert.ToDecimal(testShipment.JS_OuterPacks), testInvLine.JI_InvoiceQuantity);
			AssertEquals("Invoice Line - Unit of Quantity", testShipment.JS_F3_NKPackType, testInvLine.JI_InvoiceUQ);
			AssertEquals("Invoice Line - Price", testShipment.JS_GoodsValue, testInvLine.JI_LinePrice);
			AssertEquals("Invoice Line - Weight", testShipment.JS_ActualWeight, testInvLine.JI_Weight);
			AssertEquals("Invoice Line - Weight UQ", testShipment.JS_UnitOfWeight, testInvLine.JI_WeightUQ);
		}

		public void TestCreateShipmentsWithDuplicatedConsignorsThatMatchTheLegacySystemNumber()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord1 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestType03Record);
			ForwardingShipment testShipment1 = testRecord1.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignor1 = testShipment1.Consignor;
			AssertNotNull("Consignor1 should have been created", consignor1);
			AssertEquals("Consignor is marked as temporary account", true, consignor1.OH_IsTempAccount);
			AssertEquals("Consignor Name 1", testRecord1.ConsignorName, consignor1.OH_FullName);
			AssertEquals("Consignor Contacts 1", 1, consignor1.Contacts.Count);
			AssertEquals("Consignor Contact1 Name", testRecord1.ConsignorContactName, consignor1.Contacts[0].OC_ContactName);
			AssertEquals("Contact notification should be PRN", Core.Constants.ContactNotifyModes.Print, consignor1.Contacts[0].OC_NotifyMode);
			ZQuery legacyFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			legacyFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, testRecord1.ConsignorLegacyCode.Trim());
			AssertEquals("Temporary Consignor should not have legacy code", true, consignor1.CustomsCodes.Find(legacyFilter).Length >= 0);
			OrgCusCode cusCode = consignor1.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			cusCode.OK_CustomsRegNo = testRecord1.ConsignorLegacyCode.Trim();
			// Consignor with same Legacy Code but different Name as TestRecord1.
			// No new Organisation should be created (as Legacy Code is the same). 
			// TestRecord2's consignor should be added as a Contact of the TestRecord1's one.
			string testType03RecDiffCnorName = TestType03Record.Replace("SYDNEY BOUTIQUE WINE", "SYDNEY BOUTIQUE DIFF");
			QuantumShipmentRecord testRecord2 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", testType03RecDiffCnorName);
			ForwardingShipment testShipment2 = testRecord2.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignor2 = testShipment2.Consignor;
			AssertEquals("Consignor2 should be the same as Consignor1.", consignor1.PK, consignor2.PK);
			AssertEquals("A new contact should have been added", 2, consignor1.Contacts.Count);
			AssertEquals("Contact notification should be PRN", Core.Constants.ContactNotifyModes.Print, consignor1.Contacts[1].OC_NotifyMode);
			AssertEquals("Consignor Contact2 Name", testRecord2.ConsignorName, consignor1.Contacts[1].OC_ContactName);
			// Consignor with same Legacy Code and Name as TestRecord2 (but Name has a different case).
			// No new Organisation should be created (as Legacy Code is the same). 
			// No other contact should be added to TestRecord1's Consignor as it would be a duplicated (same name).
			string testType03RecLCaseCnorName = testType03RecDiffCnorName.Replace("SYDNEY BOUTIQUE DIFF", "Sydney Boutique Diff");
			QuantumShipmentRecord testRecord3 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", testType03RecLCaseCnorName);
			ForwardingShipment testShipment3 = testRecord3.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignor3 = testShipment3.Consignor;
			AssertEquals("Consignor3 should be the same as Consignor2.", consignor2.PK, consignor3.PK);
			AssertEquals("No other contact should have been added", 2, consignor1.Contacts.Count);
		}

#endregion
		public void TestOrgMatchFromSimilarOrgWhenCreateShipment()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "SYDNEY BOUTIQUE WINE";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.OA_Address1 = "11B REDAN ST ";
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "VILLAGE CELLARS";
			consignee.OH_RL_NKClosestPort = "JPHMJ";
			consignee.MainAddress.OA_Address1 = "6-5 UENO , UWADA";
			Factory.Save();
			NotificationBuffer notify = new NotificationBuffer();
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestOrgMatchRecord);
			ForwardingShipment shipment = testRecord.CreateShipment(Factory, false, notify);
			AssertNotNull("Consignor should not be null", shipment.Consignor);
			AssertEquals(consignor.PK, shipment.Consignor.PK);
			AssertNotNull("Consignee should not be null", shipment.Consignee);
			AssertEquals(consignee.PK, shipment.Consignee.PK);
		}

		public void TestOnlyOneOrgIsCreatedPerFileWithSameConsignorOnMultipleShipments()
		{
			string name = "BERNARD MATTHEWS NZ LIMITED";
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_FullName, name);
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(filter);
			AssertNull(name + " should not exist", consignor);
			OrgHeader shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shippingLine.OH_IsAirLine = true;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.JK_MasterBillNum = "08622997531";
			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "NZAKL";
			Factory.Save();
			using (var testUtils = new TNTTestUtils())
			{
				var pathToFile = testUtils.CopyResourceToFile("BNE.x2.20070528.210700.ok", "Enterprise.Client.TNT.Testing.DataManipulation.DataImportExport.Testing.");
				NZImportManager importManager = new NZImportManager(Factory, pathToFile);
				importManager.Exit2Mawbs[0].LinkedConsol = consol;
				importManager.ProcessAllMatches(false);
			}
			Assert(consol.Shipments.Count > 0);
			ForwardingShipment shipment = consol.Shipments[0];
			AssertEquals("Shipment's Consignor:", name, shipment.Consignor.OH_FullName);
			OrgHeader[] consignors = Factory.Load<OrgHeader>(filter);
			AssertEquals("Only one consignor with this name should have been created: " + name, 1, consignors.Length);
		}

		public void TestNewOrgsWhenNoMatchAtAll()
		{
			NotificationBuffer notify = new NotificationBuffer();
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestOrgMatchRecord);
			ForwardingShipment shipment = testRecord.CreateShipment(Factory, false, notify);
			AssertNotNull("Consignor should not be null", shipment.Consignor);
			Assert("Consignor code:", shipment.Consignor.OH_Code.StartsWith("SYDBOU"));
			AssertNotNull("Consignee should not be null", shipment.Consignee);
			Assert("Consignee code:", shipment.Consignee.OH_Code.StartsWith("VILCEL"));
		}

		public void TestEnsureContactRecordIsNotCreatedWhenNoDetailsSupplied()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord1 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestNoContactRecord);
			ForwardingShipment testShipment1 = testRecord1.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignor1 = testShipment1.Consignor;
			AssertEquals("Consignor Contacts should not have been created", 0, consignor1.Contacts.Count);
		}

		public void TestEnsureConsigneeContactRecordIsNotCreatedWhenNoDetailsSupplied()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord1 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestWithConsigneeContact);
			ForwardingShipment testShipment1 = testRecord1.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignee1 = testShipment1.Consignee;
			AssertEquals("Consignee Contact should have been created", 1, consignee1.Contacts.Count);
			AssertEquals("GEITSY GONZALEZ", consignee1.Contacts[0].OC_ContactName);
			QuantumShipmentRecord testRecord2 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestWithNoConsigneeContact);
			ForwardingShipment testShipment2 = testRecord2.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignee2 = testShipment2.Consignee;
			AssertEquals("Consignee Contacts should not have been created", 0, consignee2.Contacts.Count);
		}

		public void TestEnsureConsignorContactRecordIsUpdatedWhenOrgFound()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord1 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestType03Record);
			ForwardingShipment testShipment1 = testRecord1.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignor1 = testShipment1.Consignor;
			AssertEquals("One Consignor Contact should have been created", 1, consignor1.Contacts.Count);
			string testType03RecNewContactName = TestType03Record.Replace("John Smith", "Andrew Johns");
			QuantumShipmentRecord testRecord2 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", testType03RecNewContactName);
			testRecord2.ConsignorLegacyCode = "032123232";
			consignor1.CustomsCodes.RemoveAndDeleteAll();
			OrgCusCode legacyCode = consignor1.CustomsCodes.AddNew();
			legacyCode.OK_CustomsRegNo = testRecord2.ConsignorLegacyCode;
			legacyCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			ForwardingShipment testShipment2 = testRecord2.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignor2 = testShipment2.Consignor;
			AssertEquals("Consignor1 and Consignor2 should be the same", consignor1.PK, consignor2.PK);
			AssertEquals("Another Consignor Contact should have been created", 2, consignor1.Contacts.Count);
			if (consignor2.Contacts[0].OC_ContactName == "John Smith")
			{
				AssertEquals("Andrew Johns", consignor2.Contacts[1].OC_ContactName);
			}
			else
			{
				AssertEquals("Andrew Johns", consignor2.Contacts[0].OC_ContactName);
				AssertEquals("John Smith", consignor2.Contacts[1].OC_ContactName);
			}
		}

		public void TestEnsureConsigneeContactRecordIsUpdatedWhenOrgFound()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord1 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestWithConsigneeContact);
			ForwardingShipment testShipment1 = testRecord1.CreateShipment(Factory, false, dummyNotifier);
			Factory.Save();
			OrgHeader consignee1 = testShipment1.Consignee;
			AssertEquals("One Consignee Contact should have been created", 1, consignee1.Contacts.Count);
			string testType03RecNewContactName = TestWithConsigneeContact.Replace("GEITSY GONZALEZ", "Will Smith");
			QuantumShipmentRecord testRecord2 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", testType03RecNewContactName);
			ForwardingShipment testShipment2 = testRecord2.CreateShipment(Factory, false, dummyNotifier);
			OrgHeader consignee2 = testShipment2.Consignee;
			AssertEquals("Another Consignee Contact should have been created", 2, consignee1.Contacts.Count);
			if (consignee2.Contacts[0].OC_ContactName == "GEITSY GONZALEZ")
			{
				AssertEquals("Will Smith", consignee2.Contacts[1].OC_ContactName);
				AssertEquals("Shipments Notify Party Set", consignee2.Contacts[1].PK, testShipment2.NotifyPartyDocumentaryAddress.ContactPK);
			}
			else
			{
				AssertEquals("Will Smith", consignee2.Contacts[0].OC_ContactName);
				AssertEquals("GEITSY GONZALEZ", consignee2.Contacts[1].OC_ContactName);
				AssertEquals("Shipments Notify Party Set", consignee2.Contacts[0], testShipment2.NotifyPartyDocumentaryAddress.ContactPK);
			}
		}

		public void TestCustomEntryNumberLengthIsInvalid()
		{
			ZDateTime startDate = ZDateTime.Today;
			UpdateLOCODEMapping();
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestCusEntryNumTypeIsLength4);
			try
			{
				ForwardingShipment testShipment = testRecord.CreateShipment(Factory, false, dummyNotifier);
			}
			catch (MaxLengthExceededException)
			{
				Fail("Max Length of CusEntryNumberType was not trimmed to 3 characters");
			}

			Assert(true);
		}

		public void TestCustomEntryTypeIsMapped()
		{
			UpdateLOCODEMapping();
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestCusEntryTypeIsEXLV);
			ForwardingShipment testShipment = testRecord.CreateShipment(Factory, false, dummyNotifier);
			AssertEquals("Entry number type stored on table as 3 character code should still indicate appropriate 4 character equivalent code value of EXLV ", "EXLV", testShipment.CustomsEntryNumberType);
			Factory.Save();
			string querySQL = "SELECT CE_EntryType FROM dbo.CusEntryNum WHERE CE_ParentID = '" + testShipment.PK + "'";
			string entryTypeOnTable = (string)Db.Connection.ExecuteScalar(querySQL);
			AssertEquals("Table value should be truncated code", "XLV", entryTypeOnTable);
		}

		public void TestSetShipmentCustomsEntryNumber()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer();
			QuantumShipmentRecord quantumShipment = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestType03Record);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "USLAX";
			quantumShipment.SetShipmentCustomsEntryNumber(shipment);
			AssertEquals("CustomsEntryNumberType", "ECN", shipment.CustomsEntryNumberType);
			AssertEquals("CustomsEntryNumber", "ECN", shipment.CustomsEntryNumber);
			quantumShipment.TDoc = "123456789";
			quantumShipment.SetShipmentCustomsEntryNumber(shipment);
			AssertEquals("CustomsEntryNumberType", CANType.CustomsAuthorityNumber.Code, shipment.CustomsEntryNumberType);
			AssertEquals("CustomsEntryNumber", "123456789", shipment.CustomsEntryNumber);
			shipment.JS_RL_NKOrigin = "AUSYD";
			quantumShipment.SetShipmentCustomsEntryNumber(shipment);
			AssertEquals("CustomsEntryNumberType", CANType.CustomsAuthorityNumber.Code, shipment.CustomsEntryNumberType);
			AssertEquals("CustomsEntryNumber", "123456789", shipment.CustomsEntryNumber);
		}

		public void TestSetShipmentCustomsEntryNumber_Transhipment()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CusHAWB houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			houseBill.Shipment.ConsigneePK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			houseBill.CS_OH_Consignee = ZGuid.Empty;
			houseBill.CS_ConsigneeName = "Different";
			houseBill.CS_TranshipmentEntryNum = "123456789";
			houseBill.CS_HAWB = "930941449 ";
			NotificationBuffer dummyNotifier = new NotificationBuffer();
			QuantumShipmentRecord quantumShipment = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestTranshipmentRecord);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "USLAX";
			Assert("Precondition - shipment is crosstrade", shipment.IsCrossTrade());
			quantumShipment.SetShipmentCustomsEntryNumber(shipment);
			AssertEquals("Shipment's CustomsEntryNumber", "123456789", shipment.CustomsEntryNumber);
			GlbCompany.CurrentCompany.SetCountry(currentCountry);
		}

		public void TestNew()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			QuantumShipmentRecord record = QuantumShipmentRecord.New("", "", "");
			AssertEquals("Should be QuantumShipmentRecord", typeof(QuantumShipmentRecord), record.GetType());
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			record = QuantumShipmentRecord.New("", "", "");
			AssertEquals("Should be NZQuantumShipmentRecord", typeof(NZQuantumShipmentRecord), record.GetType());
		}

		public void TestBlankCurrency()
		{
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord1 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestNoContactRecord);
			ForwardingShipment testShipment1 = testRecord1.CreateShipment(Factory, false, dummyNotifier);
			Customs.Business.BaseJobDeclaration baseJobDeclaration1 = Factory.NewWithValidTestData<Customs.Business.BaseJobDeclaration>();
			testShipment1.JS_RX_NKGoodsValueCurr = "";
			baseJobDeclaration1 = testRecord1.CreateDefaultInvoiceForDeclaration(testShipment1, baseJobDeclaration1);
			Assert(baseJobDeclaration1.Invoices[0].JZ_RX_NKInvoice_Currency == "AUD");
			QuantumShipmentRecord testRecord2 = QuantumShipmentRecord.New(SydBranch.GB_Code, "", TestNoContactRecord);
			ForwardingShipment testShipment2 = testRecord2.CreateShipment(Factory, false, dummyNotifier);
			Customs.Business.BaseJobDeclaration baseJobDeclaration2 = Factory.NewWithValidTestData<Customs.Business.BaseJobDeclaration>();
			testShipment2.JS_RX_NKGoodsValueCurr = "EUR";
			baseJobDeclaration2 = testRecord2.CreateDefaultInvoiceForDeclaration(testShipment2, baseJobDeclaration2);
			Assert(baseJobDeclaration2.Invoices[0].JZ_RX_NKInvoice_Currency == "EUR");
		}

#region Implementation
		ZString GetTrimLongStringToMaxLength(int maxLength)
		{
			return LongString.Trim().Left(maxLength);
		}

		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return QuantumShipmentRecord.New(SydBranch.GB_Code, MBagNo, rawData);
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(QuantumShipmentRecord);
			}
		}

#region SydBranch
		protected GlbBranch SydBranch
		{
			get
			{
				if (fSydBranch == null)
				{
					fSydBranch = GetSydBranch();
				}

				return fSydBranch;
			}
		}

		GlbBranch fSydBranch;
		GlbBranch GetSydBranch()
		{
			string branchCode = "SYD";
			GlbBranch result = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (result == null)
			{
				result = Factory.New<GlbBranch>();
				result.GB_Code = branchCode;
				Factory.Save();
			}

			return result;
		}

#endregion
		readonly ZString LongString = " This Is a Very Long String 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890   ";
		const string MBagNo = "MBagNo1234";
		//                                                                                                                                            1         1         1         1         1         1         1         1         1         1         2         2         2         2         2         2         2         2         2         2         3         3         3         3         3         3         3         3         3         3         4         4         4         4         4         4         4         4         4         4         5         5         5         5         5         5         5         5         5         5         6         6         6         6         6         6         6         6         6         6         7         7         7         7         7         7         7         7         7         7         8         8         8         8         8         8         8         8         8         8         9         9         9         9         9         9         9         9         9         9         
		//                        			                1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6         7         8         9         
		//                                         123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
		const string TestType03Record = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601  John Smith                                                                                                                                                                                                                                 VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000ECN                                                          T                         .";
		const string TestType03RecordNoEntryNum = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601                                                                                                                                                                                                                                             VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000ECN                                                          T                         .";
		const string TestNoContactRecord = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601                                                                                                                                                                                                                                             VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000ECN                                                          T                         .";
		const string TestWithConsigneeContact = "03922639355 BNEMIA21017331SURPLUS BEARINGS P/L           U 2 49 RANDALL ST                                             SLACKS CREEK                   QUEENSLAND                     AU 4127     0738087494  0738087494  MOORE                                                                                                                                                                                                                                      IQ ENGINEERING INC             8208 NW 30TH TERRACE                                          MIAMI                          FL                             US 33122    5924404     5924404     GEITSY GONZALEZ                                                                                                                                                                                                                            NS       450.00USD     1    13.100EX2                                                                                    .";
		const string TestWithNoConsigneeContact = "03901059863 BNEUSO20366507MICRO ELECT TECHNOLOGIES       532 SEVENTEEN MILE ROCK RD                                    SINNAMON PARK                  QUEENSLAND                     AU 4073     0733766688  0738926699  OREILLY                                                                                                                                                                                                                                    SEAN ODWYER C/O PRO CAM MACHINE18421 BOTHELL EVERETT HIGHWAY  SUITE 150                      MILL CREEK                     WA                             US 98012                                                                                                                                                                                                                                                                       NS       200.00USD     1     1.860EX2                                                                                    .";
		const string TestCusEntryNumTypeIsLength4 = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601  John Smith                                                                                                                                                                                                                                 VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000BLAH                                                         T                         .";
		const string TestCusEntryTypeIsEXLV = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601  John Smith                                                                                                                                                                                                                                 VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000EXLV                                                         T                         .";
		const string TestTranshipmentRecord = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601  John Smith                                                                                                                                                                                                                                 VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000                                                             T                         .";
		const string TestOrgMatchRecord = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601  John Smith                                                                                                                                                                                                                                 VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000ECN                                                          T                         .";
		void UpdateLOCODEMapping()
		{
			OrgProxyPortMappingTestHelper portMappingHelper = new OrgProxyPortMappingTestHelper(Factory);
			portMappingHelper.OrgProxy.PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();
			portMappingHelper.AddPortMappingToOrgProxy("SYD", portMappingHelper.AUSYDUnLoco);
			Factory.Save();
		}
#endregion
	}
}
