using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestVATNoWhenSettingABL_ConsigneeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "IL";
			org.OH_FullName = "FULL NAME";

			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "123456");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "123456978");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "IL";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var bill = Factory.New<AsycudaBill>();
			header.Bills.Add(bill);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			Factory.Save();

			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ConsigneeRegNo);
			AssertEquals(OrgCusCode.CodeTypes.VATCode, bill.ABL_ConsigneeRegNoType);

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);
		}

		public void TestConsigneeRegoNoCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var bill = Factory.New<AsycudaBill>();
			header.Bills.Add(bill);
			AssertEquals("VAT No.", bill.ConsigneeRegoNoCaption.Caption);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertEquals("Reg.No", bill.ConsigneeRegoNoCaption.Caption);
		}

		public void TestIAsycudaBill()
		{
			using (ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ILManifestRegistryOptions.Codes.IMPORT))
			{
				var bizObj = GetNewBusinessObject();
				Factory.Save();
				AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ILManifest.IAsycudaBill>(bizObj.PK).GetType());
				AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
			}
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.Packs);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Israel, bill.GetCountryCode());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(typeof(AsycudaPack), bill.GetPackType());
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var child = header.MasterBill;
			var realBill = header.Bills.AddNew();
			AssertType<AsycudaBillValidationForRegularBill>(realBill.Validation);
			AssertType<AsycudaBillValidationForMasterChild>(child.Validation);
		}

		public void TestAdditionalInfos()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaAdditionalInfoCollection>(bill.AdditionalInfos);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var bill = Factory.New<AsycudaBill>();
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes();
			AssertEquals(typeof(AsycudaBaseAdditionalInfo), actualTypes["OTH"]);
			AssertEquals(typeof(SupportingDocument), actualTypes["SUP"]);
		}

		public void TestGetFetchStrategies()
		{
			var bill = Factory.New<AsycudaBill>();
			var expectedTypes = new[]
			{
				typeof(Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy),
				typeof(Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy),
			};
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)bill).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestDefaults()
		{
			var bill = GetNewBill();
			AssertEquals("Default Gross Weight Unit should be KG", "KG", bill.ABL_GrossWeightUQ);
			AssertEquals("Default Volume Unit should be M3", "M3", bill.ABL_VolumeUQ);
		}

		public void TestLookupsType()
		{
			var bill = GetNewBill();
			AssertType<AsycudaBillLookups>("Lookups for AsycudaBill should be of type AsycudaBillLookups", bill.Lookups);
		}

		public void TestValidationType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillValidationForMasterChild>(header.MasterBill.Validation);

			var bill = header.Bills.AddNew();
			AssertType<AsycudaBillValidationForRegularBill>(bill.Validation);
		}

		public void TestParentType()
		{
			var bill = GetNewBill();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestABL_SequenceNumberAllowManualEnter()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_SequenceNumber = 10;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_SequenceNumber = 8;

			var bill3 = header.Bills.AddNew();
			bill3.ABL_SequenceNumber = 101;

			AssertEquals("Bill should allow manual setting", (short)10, header.MasterBill.ABL_SequenceNumber);
			AssertEquals("Bill should allow manual setting", (short)8, bill2.ABL_SequenceNumber);
			AssertEquals("Bill should allow manual setting", (short)101, bill3.ABL_SequenceNumber);
		}

		public void TestABL_VolumeUQ_IsReadonly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Assert("ABL_VolumeUQ is readonly", bill.ABL_VolumeUQInfo.ReadOnly);
		}

		public void TestABL_GrossWeightUQ_IsReadonly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Assert("ABL_GrossWeightUQ is readonly", bill.ABL_GrossWeightUQInfo.ReadOnly);
		}

		public void TestTransportDocumentsCollection()
		{
			var bill = GetNewBill();
			AssertType<AsycudaTransportDocumentInfoCollection>(bill.TransportDocuments);
		}

		public void TestPackedItems()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaBill>>(bill.PackedItems);
		}

		public void TestGetPackedItemTypeCore()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(typeof(AsycudaPackedItem), bill.GetPackedItemType());
		}

		public void TestCaptions()
		{
			var bill = Factory.New<AsycudaBill>();
			CombineAssertions(() =>
			{
				AssertEquals("ABL_OA_Shipper caption", "Consignor", DataBoundResourceStrings.GetDataForProperty(bill.ABL_OA_ShipperInfo).Caption);
				AssertEquals("ABL_ShipperName caption", "Consignor Name", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperNameInfo).Caption);
				AssertEquals("ABL_ShipperStreet1 caption", "Consignor Street 1", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperStreet1Info).Caption);
				AssertEquals("ABL_ShipperStreet2 caption", "Consignor Street 2", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperStreet2Info).Caption);
				AssertEquals("ABL_ShipperCity caption", "Consignor City", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperCityInfo).Caption);
				AssertEquals("ABL_ShipperState caption", "Consignor State", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperStateInfo).Caption);
				AssertEquals("ABL_ShipperPostcode caption", "Consignor Postcode", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperPostcodeInfo).Caption);
				AssertEquals("ABL_RN_NKShipperCountry caption", "Consignor Country/Region", DataBoundResourceStrings.GetDataForProperty(bill.ABL_RN_NKShipperCountryInfo).Caption);
				AssertEquals("ABL_RL_NKFinalDestination caption", "Destination", DataBoundResourceStrings.GetDataForProperty(bill.ABL_RL_NKFinalDestinationInfo).Caption);
				AssertEquals("ABL_RL_NKPortOfDischarge caption", "Discharge Port", DataBoundResourceStrings.GetDataForProperty(bill.ABL_RL_NKPortOfDischargeInfo).Caption);
				AssertEquals("ABL_Condition caption", "Condition", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ConditionInfo).Caption);
				AssertEquals("ABL_Volume caption", "Volume", DataBoundResourceStrings.GetDataForProperty(bill.ABL_VolumeInfo).Caption);
				AssertEquals("ABL_ManifestQty caption", "Quantity", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ManifestQtyInfo).Caption);
				AssertEquals("ABL_GrossWeight caption", "Weight", DataBoundResourceStrings.GetDataForProperty(bill.ABL_GrossWeightInfo).Caption);
				AssertEquals("ABL_CustomsValue caption", "Goods Value", DataBoundResourceStrings.GetDataForProperty(bill.ABL_CustomsValueInfo).Caption);
			});
		}

		public void TestPopulateAdditionalInfoPartnerVat_WhenImportConsigneeChanged()
		{
			TestPopulateAdditionalInfoPartnerVat(ShipmentTypeList.Codes.Import23, "Consignee", "520017146", (bill, orgAddress) => bill.ABL_OA_Consignee = orgAddress.PK);
		}

		public void TestPopulateAdditionalInfoPartnerVat_WhenExportConsignorChanged()
		{
			TestPopulateAdditionalInfoPartnerVat(ShipmentTypeList.Codes.Export22, "Consignor", "520017147", (bill, orgAddress) => bill.ABL_OA_Shipper = orgAddress.PK);
		}

		public void TestUpdateFDN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);

			var shipment1 = consol.Shipments.AddNew();

			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			CreateNewOrGetExistingRefUNLOCO("ILHFA");
			shipment1.JS_RL_NKDestination = "ILHFA";
			bill1.ABL_JS_Shipment = shipment1.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			CreateNewOrGetExistingRefUNLOCO("ILTLV");
			shipment2.JS_RL_NKDestination = "ILTLV";
			bill2.ABL_JS_Shipment = shipment2.PK;
			Factory.Save();

			AssertNull("No FDN number for shipment 1", shipment1.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, Core.Constants.CountryCodes.Israel));
			AssertNull("No FDN number for shipment 2", shipment2.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, Core.Constants.CountryCodes.Israel));

			var transportDocument1 = bill1.TransportDocuments.AddNew();
			transportDocument1.CSI_Code = TransportDocsTypeList.Codes.IL1;
			transportDocument1.CSI_ReferenceNumber = "I123456789";

			var transportDocument2 = bill2.TransportDocuments.AddNew();
			transportDocument2.CSI_Code = TransportDocsTypeList.Codes.IL1;
			transportDocument2.CSI_ReferenceNumber = "I223456789";

			Factory.Save();
			var fdn1 = shipment1.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, Core.Constants.CountryCodes.Israel);
			AssertNotNull("FDN 1", fdn1);
			AssertEquals("FDN 1 should be created and be equal to the expected value", "I123456789", fdn1.CE_EntryNum);
			var addLogFdn1 = fdn1.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem, "FDN: <I123456789>");
			AssertNotNull("Add Log for Shipment 1", addLogFdn1);

			var fdn2 = shipment2.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, Core.Constants.CountryCodes.Israel);
			AssertNotNull("FDN 2", fdn2);
			AssertEquals("FDN 2 should be created and be equal to the expected value", "I223456789", fdn2.CE_EntryNum);
			var addLogFdn2 = fdn2.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem, "FDN: <I223456789>");
			AssertNotNull("Add Log for Shipment 2", addLogFdn2);

			transportDocument1.CSI_ReferenceNumber = "I123456788";
			transportDocument2.CSI_ReferenceNumber = "I223456788";
			Factory.Save();

			AssertNotNull("FDN 1", fdn1);
			AssertEquals("FDN 1 should be updated to the expected value", "I123456788", fdn1.CE_EntryNum);
			var editLogFdn1 = fdn1.Logs.MostRecentLogByEventTime(Events.EditedARecord, "FDN: from <I123456789> to <I123456788>");
			AssertNotNull("Edit Log for Shipment 1", editLogFdn1);

			AssertNotNull("FDN 2", fdn2);
			AssertEquals("FDN 2 should be updated to the expected value", "I223456788", fdn2.CE_EntryNum);
			var editLogFdn2 = fdn2.Logs.MostRecentLogByEventTime(Events.EditedARecord, "FDN: from <I223456789> to <I223456788>");
			AssertNotNull("Edit Log for Shipment 2", editLogFdn2);

			fdn1.CE_EntryNum = "FDN1";
			fdn2.CE_EntryNum = "FDN2";
			Factory.Save();

			AssertNotNull("FDN 1", fdn1);
			AssertEquals("FDN 1 should be overriden with the expected value", "I123456788", fdn1.CE_EntryNum);

			AssertNotNull("FDN 2", fdn2);
			AssertEquals("FDN 2 should be overriden with the expected value", "I223456788", fdn2.CE_EntryNum);

			bill2.TransportDocuments.RemoveAndDelete(transportDocument2);
			Factory.Save();

			fdn1 = shipment1.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, Core.Constants.CountryCodes.Israel);
			AssertNotNull("FDN 1", fdn1);
			AssertEquals("FDN 1 should be overriden with the expected value", "I123456788", fdn1.CE_EntryNum);

			fdn2 = shipment2.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, Core.Constants.CountryCodes.Israel);
			AssertNull("FDN 2", fdn2);
			var deleteLogFdn2 = shipment2.Logs.GetAllLogs().Find(l => l.SL_SE_NKEvent == Events.DeletedARecordInTheSystem.Code && l.SL_EventDescription == "FDN: <I223456788>");
			AssertNotNull("Delete Log", deleteLogFdn2);
		}

		public void TestSetExporterIDTypeMatchingConsignor_WithCSC()
		{
			var (bill, address) = InitializeBillAndOrgHeader(
				"TestOrg1",
				new List<(string, string, string)>
				{
					(OrgCusCode.CodeTypes.SupplierCode, "443322", CountryCodes.Israel),
					(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "112233", CountryCodes.Spain)
				},
				CountryCodes.Israel);

			RunPreconditionAssertions(bill);

			SetShipperAndValidate(bill, address, "443322", "1");
		}

		public void TestSetExporterIDTypeMatchingConsignor_WithDUN()
		{
			var (bill, address) = InitializeBillAndOrgHeader(
				"TestOrg2",
				new List<(string, string, string)>
				{
					(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "112233", CountryCodes.Spain)
				},
				CountryCodes.Israel);

			RunPreconditionAssertions(bill);

			SetShipperAndValidate(bill, address, "112233", "3");
		}
		
		public void TestOnSave_AssignSubDealNumberToTransportDocument()
		{
			var factory = Factory;
			var bill1 = GetNewBill();
			var header = bill1.Header;
			header.AMA_TransportMode = "ROA";
			header.AMA_ManifestNumber = "123456";
			header.AMA_RN_NKCountry = "IL";

			factory.Save();
			AssertEquals("Prerequisite: No Bill Transport Documents:", 0, bill1.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));

			var orgHeader1 = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader1.MainAddress;
			orgHeader1.OH_Code = "SA";
			orgHeader1.CustomsCodes.AddNew("CCC", "123", "IL");
			header.AMA_OA_ShippingAgent = orgAddress1.PK;

			factory.Save();
			AssertEquals("Prerequisite: No Bill Transport Documents:", 0, bill1.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));

			var orgHeader2 = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = orgHeader2.MainAddress;
			orgHeader2.OH_Code = "DEC";
			orgHeader2.CustomsCodes.AddNew("CMP", "456", "IL");
			header.AMA_OA_Declarant = orgAddress2.PK;

			bill1.ABL_SequenceNumber = 1;

			factory.Save();
			AssertEquals("Bill Transport Document created only for SEA manifest", 0, bill1.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));

			header.AMA_TransportMode = "SEA";
			bill1.ABL_SequenceNumber = 2;
			factory.Save();

			AssertEquals("Bill Transport Document created:", 1, bill1.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));
			var forwarderSubDealNumber = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1).CSI_ReferenceNumber;
			AssertEquals("There should be a Transport Document record with IL1 code and reference consist of 'CCC' and 'CMP' codes value and a counter", "I123456A01", forwarderSubDealNumber);

			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			bill1.ABL_SequenceNumber = 224;
			factory.Save();
			AssertEquals("Remove the Sub Deal Number when either the declarant or shipping agent becomes invalid", 0, bill1.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));

			orgHeader2.CustomsCodes.AddNew("CMP", "456", "IL");
			bill1.ABL_SequenceNumber = 999;

			factory.Save();
			AssertEquals("If Bill Sequence Number grater than 2599, Bill Transport Documents will be created:", 1, bill1.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));
			forwarderSubDealNumber = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1).CSI_ReferenceNumber;
			AssertEquals("There should be a Transport Document record with IL1 code and reference consist of 'CCC' and 'CMP' codes value and a counter", "I123456A01", forwarderSubDealNumber);
		}

		public void TestAssignSubDealNumberToTransportDocument_RunsValidations()
		{
			var factory = Factory;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			var pdnNumber = Factory.New<CusEntryNumber>();
			pdnNumber.CE_EntryNum = "123456";
			pdnNumber.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			consol.Numbers.Add(pdnNumber);
			factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Sea;
			header.AMA_RN_NKCountry = "IL";

			header.SetParent(consol);
			header.Synchroniser.SetEnabled(true, false);
			header.Synchroniser.Synchronise();

			var bill = header.Bills[0];
			var orgHeaderShippingAgent = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressShippingAgent = orgHeaderShippingAgent.MainAddress;
			orgHeaderShippingAgent.OH_Code = "SA";
			orgHeaderShippingAgent.CustomsCodes.AddNew("CCC", "123", "IL");

			var orgHeaderDeclarant = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressDeclarant = orgHeaderDeclarant.MainAddress;
			orgHeaderDeclarant.OH_Code = "DEC";
			orgHeaderDeclarant.CustomsCodes.AddNew("CMP", "456", "IL");

			bill.RunPreSaveValidation();
			AssertEquals("Prerequisite: There is IL2 Transport Document:", 1, bill.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL2));
			AssertEquals("Prerequisite: No IL1 Transport Document:", 0, bill.TransportDocuments.Count(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1));
			var il2TransportDoc = bill.TransportDocuments.First(x => x.CSI_Code == TransportDocsTypeList.Codes.IL2);
			AssertHasMessageError(il2TransportDoc.CSI_CodeUserInterfaceInfo, "[CC_BR1_WCO_090] You must enter additional record of type \"IL1\" in case of using \"IL2\"");

			header.AMA_ManifestNumber = "123456";
			header.AMA_OA_Declarant = orgAddressDeclarant.PK;
			header.AMA_OA_ShippingAgent = orgAddressShippingAgent.PK;
			AssertNoMessageError(il2TransportDoc.CSI_CodeUserInterfaceInfo, "[CC_BR1_WCO_090] You must enter additional record of type \"IL1\" in case of using \"IL2\"");
		}

		public void TestEnsureTransportDocumentType_MasterBillAndBillNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			var masterTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704");
			var houseTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "705");

			CombineAssertions("Initial state: No Master or House Transport Documents", () =>
			{
				AssertNull(masterTransportDoc);
				AssertNull(houseTransportDoc);
			});

			header.AMA_MasterBill = "BOL123";

			masterTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704" && doc.CSI_ReferenceNumber == "BOL123");
			houseTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "705");

			CombineAssertions("Master Transport Document should appear", () =>
			{
				AssertNotNull(masterTransportDoc);
				AssertNull(houseTransportDoc);
			});

			bill.ABL_BillNumber = "HSW456";

			masterTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704" && doc.CSI_ReferenceNumber == "BOL123");
			houseTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "705" && doc.CSI_ReferenceNumber == "HSW456");

			CombineAssertions("Master and House Transport Documents should appear", () =>
			{
				AssertNotNull(masterTransportDoc);
				AssertNotNull(houseTransportDoc);
			});

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			CombineAssertions("Changing transport mode to road should not delete existing Master and House Transport Documents", () =>
			{
				AssertNotNull(masterTransportDoc);
				AssertNotNull(houseTransportDoc);
			});

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_MasterBill = ZString.Empty;

			masterTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704");
			houseTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "705" && doc.CSI_ReferenceNumber == "HSW456");

			CombineAssertions("Master Transport Document should disappear", () =>
			{
				AssertNull(masterTransportDoc);
				AssertNotNull(houseTransportDoc);
			});

			bill.ABL_BillNumber = ZString.Empty;

			masterTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704");
			houseTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "705");

			CombineAssertions("House Transport Document should disappear", () =>
			{
				AssertNull(masterTransportDoc);
				AssertNull(houseTransportDoc);
			});

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_MasterBill = "BOL123";
			bill.ABL_BillNumber = "HSW456";

			masterTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704");
			houseTransportDoc = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "705");

			CombineAssertions("When transport mode is road, Master and House Transport Documents should not be created", () =>
			{
				AssertNull(masterTransportDoc);
				AssertNull(houseTransportDoc);
			});
		}

		public void TestParentDealNumber()
		{
			var asycudaBill = GetNewBill();
			AssertNullOrEmpty("ParentDealNumber sould be empty", asycudaBill.ParentDealNumber);

			var transportDocument = asycudaBill.TransportDocuments.AddNew();
			transportDocument.CSI_Code = TransportDocsTypeList.Codes.IL2;
			transportDocument.CSI_ReferenceNumber = "TestReferenceNumber";

			AssertEquals("ParentDealNumber should be 'TestReferenceNumber'", "TestReferenceNumber", asycudaBill.ParentDealNumber);
		}

		public void TestUpdateVATNoABL_ShipperRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "IL";
			org.OH_FullName = "FULL NAME";

			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "123456");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "123456978");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "IL";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;

			var bill = Factory.New<AsycudaBill>();
			header.Bills.Add(bill);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			Factory.Save();

			bill.ABL_OA_Shipper = orgAddress.PK;
			CombineAssertions("When Export Manifest And VAT Exists, Reg No should be updated with the VAT configuration value and type should be VAT.", () =>
			{
				AssertEquals("62318879", bill.ABL_ShipperRegNo);
				AssertEquals(OrgCusCode.CodeTypes.VATCode, bill.ABL_ShipperRegNoType);
			});

			bill.ABL_OA_Shipper = orgAddress1.PK;
			CombineAssertions("When Export Manifest And VAT is Missing, Reg No and type should not be updated.", () =>
			{
				AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
			});

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			bill.ABL_OA_Shipper = orgAddress.PK;
			CombineAssertions("When Not Export Manifest And VAT Exists, Reg No and type should not be updated for non-export manifests.", () =>
			{
				AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
			});
		}

		internal AsycudaBill GetNewBill()
		{
			return (AsycudaBill)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		(AsycudaBill, OrgAddress) InitializeBillAndOrgHeader(string orgCode, List<(string codeType, string code, string country)> customsCodes, string addressCountry)
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;

			foreach (var (codeType, code, country) in customsCodes)
			{
				orgHeader.CustomsCodes.AddNew(codeType, code, country);
			}

			var address = orgHeader.Addresses.AddNew();
			address.OA_RN_NKCountryCode = addressCountry;
			address.Address1 = "Address1";
			address.OA_OH = orgHeader.PK;

			return (bill, address);
		}

		void RunPreconditionAssertions(AsycudaBill bill)
		{
			CombineAssertions("Pre-condition before pre-save", () =>
			{
				AssertEquals("No consignor reg no set", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertCollectionNotContains("No exporter type ID set", bill.AdditionalInfos, additionalInfo => additionalInfo.CSI_Code == "5");
			});
		}

		void SetShipperAndValidate(AsycudaBill bill, OrgAddress address, string expectedRegNo, string expectedReferenceNumber)
		{
			bill.ABL_OA_Shipper = address.PK;

			CombineAssertions($"Consignor has code {expectedRegNo} after pre-save", () =>
			{
				AssertEquals("Consignor's reg no equals to the expected value", expectedRegNo, bill.ABL_ShipperRegNo);
				AssertEquals("There is only one Exporter Type ID", 1, bill.AdditionalInfos.Count(additionalInfo => additionalInfo.CSI_Code == "5"));
				AssertCollectionContains($"Exporter Type ID set to {expectedReferenceNumber}", bill.AdditionalInfos, additionalInfo => additionalInfo.CSI_Code == "5" && additionalInfo.CSI_ReferenceNumber == expectedReferenceNumber);
			});

			bill.ABL_OA_Shipper = ZGuid.Empty;

			CombineAssertions("Consignor has no code", () =>
			{
				AssertEquals("Consignor's reg no is empty", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals("There is no Exporter Type ID", 0, bill.AdditionalInfos.Count(additionalInfo => additionalInfo.CSI_Code == "5"));
			});
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			return result;
		}

		void AssertSinglePartnerVatNumberRecord(AsycudaBill bill, string description)
		{
			var asycudaAdditionalInfo = bill.AdditionalInfos.Cast<AsycudaAdditionalInfo>().Single(additionalInfo => additionalInfo.CSI_Code == "2");
			AssertEquals("CSI_Description", description, asycudaAdditionalInfo.CSI_Description);
		}

		void AddNewAdditionalInfo(AsycudaBill bill, string code, string description)
		{
			var currentAsycudaAdditionalInfo = bill.AdditionalInfos.AddNew();
			currentAsycudaAdditionalInfo.CSI_Code = code;
			currentAsycudaAdditionalInfo.CSI_Description = description;
		}

		void TestPopulateAdditionalInfoPartnerVat(ZString shipmentType, string orgCode, string vatNumber, Action<AsycudaBill, OrgAddress> updateBillPartner)
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.Header.AMA_Nature = shipmentType;
			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = orgCode;
			orgAddress.OA_OH = orgHeader.PK;

			AssertEquals($"When {orgCode} is empty", 0, bill.AdditionalInfos.Count);

			updateBillPartner(bill, orgAddress);
			AssertEquals($"When {orgCode} without VAT", 0, bill.AdditionalInfos.Count);

			orgHeader.CustomsCodes.AddNew("VAT", "1", "IN");
			bill = (AsycudaBill)GetNewBusinessObject();
			bill.Header.AMA_Nature = shipmentType;
			updateBillPartner(bill, orgAddress);
			AssertEquals($"When {orgCode} with VAT but not for Israel", 0, bill.AdditionalInfos.Count);

			orgHeader.CustomsCodes.AddNew("VAT", "", "IL");
			bill = (AsycudaBill)GetNewBusinessObject();
			updateBillPartner(bill, orgAddress);
			AssertEquals($"When {orgCode} with VAT for Israel without Reg No", 0, bill.AdditionalInfos.Count);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew("VAT", vatNumber, "IL");

			CombineAssertions($"When {orgCode} with VAT for Israel with Reg No, and no {orgCode} Vat Number record ->Add new record", () =>
			{
				bill = (AsycudaBill)GetNewBusinessObject();
				bill.Header.AMA_Nature = shipmentType;
				updateBillPartner(bill, orgAddress);
				AssertEquals(1, bill.AdditionalInfos.Count);
				AssertSinglePartnerVatNumberRecord(bill, vatNumber);
			});

			CombineAssertions($"When {orgCode} with VAT for Israel with Reg No, and have 2 {orgCode} Vat Number record -> only one {orgCode} Vat Number exist", () =>
			{
				bill = (AsycudaBill)GetNewBusinessObject();
				bill.Header.AMA_Nature = shipmentType;
				AddNewAdditionalInfo(bill, "2", "111111");
				AddNewAdditionalInfo(bill, "2", "222222");

				updateBillPartner(bill, orgAddress);
				AssertEquals(1, bill.AdditionalInfos.Count);
				AssertSinglePartnerVatNumberRecord(bill, vatNumber);
			});

			CombineAssertions($"When {orgCode} with VAT for Israel with Reg No, and have empty record ->update the empty record", () =>
			{
				bill = (AsycudaBill)GetNewBusinessObject();
				bill.Header.AMA_Nature = shipmentType;
				AddNewAdditionalInfo(bill, "1", "123456");
				AddNewAdditionalInfo(bill, string.Empty, string.Empty);

				updateBillPartner(bill, orgAddress);
				AssertEquals(2, bill.AdditionalInfos.Count);
				AssertSinglePartnerVatNumberRecord(bill, vatNumber);
			});

			CombineAssertions($"When {orgCode} without VAT for Israel with Reg No-> Remove All {orgCode} Vat Number Records", () =>
			{
				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				AddNewAdditionalInfo(bill, "2", "123456");

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.Header.AMA_Nature = shipmentType;
				updateBillPartner(bill, orgAddress);
				AssertEquals($"Remove All {orgCode} Vat Number Records", 0, bill.AdditionalInfos.Count);
			});
		}

		sealed class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
		}
	}
}
