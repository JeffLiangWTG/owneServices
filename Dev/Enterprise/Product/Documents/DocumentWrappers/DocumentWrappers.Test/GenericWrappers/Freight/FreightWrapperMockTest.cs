using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class FreightWrapperMockTest : TestCaseWithFactory
	{
		#region TestVATNumbers

		void PopulateRefData()
		{
			var cnpj = Factory.New<RefDocOrgCusCode>();
			cnpj.DOC_DocumentType = "HBL";
			cnpj.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cnpj.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Brazil;
			cnpj.DOC_Notes = "Tax Id";
			cnpj.DOC_CodeType = "CJN";
			cnpj.DOC_ShortLabel = "CJN";
			cnpj.DOC_Priority = 1;

			var com = Factory.New<RefDocOrgCusCode>();
			com.DOC_DocumentType = "HBL";
			com.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			com.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Brazil;
			com.DOC_Notes = "Tax Id";
			com.DOC_CodeType = "COM";
			com.DOC_ShortLabel = "COM";
			com.DOC_Priority = 2;

			var nit = Factory.New<RefDocOrgCusCode>();
			nit.DOC_DocumentType = "HBL";
			nit.DOC_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			nit.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Colombia;
			nit.DOC_Notes = "TEST";
			nit.DOC_CodeType = "NIT";
			nit.DOC_ShortLabel = "NIT";
			nit.DOC_Priority = 1;

			var gcr = Factory.New<RefDocOrgCusCode>();
			gcr.DOC_DocumentType = "HBL";
			gcr.DOC_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			gcr.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Colombia;
			gcr.DOC_Notes = "TEST";
			gcr.DOC_CodeType = "GCR";
			gcr.DOC_ShortLabel = "GCR";
			gcr.DOC_Priority = 2;

			Factory.Save();
		}

		public void TestFreightWrapperRequiredVATNumbers_Consignee_Consignor()
		{
			PopulateRefData();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "COMDE";
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "COMDE";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER";
			shipper.OH_RL_NKClosestPort = "BRASO";
			shipper.MainAddress.Address1 = "Unit 15";
			shipper.MainAddress.Address2 = "1 E Lane";
			shipper.MainAddress.City = "São Paulo";
			shipper.MainAddress.Postcode = "14780";
			shipper.MainAddress.OA_RN_NKCountryCode = "BR";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "COMDE";
			consignee.MainAddress.Address1 = "Unit 15";
			consignee.MainAddress.Address2 = "1 E Lane";
			consignee.MainAddress.City = "Medellín";
			consignee.MainAddress.Postcode = "050022";
			consignee.MainAddress.OA_RN_NKCountryCode = "CO";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var brTaxNum1 = shipper.CustomsCodes.AddNew();
			brTaxNum1.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			brTaxNum1.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			brTaxNum1.OK_CustomsRegNo = "TAXBR001";

			var coTaxNum1 = consignee.CustomsCodes.AddNew();
			coTaxNum1.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			coTaxNum1.OK_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			coTaxNum1.OK_CustomsRegNo = "TAXCO002";

			var wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("COM: TAXBR001", wrapper.ConsignorRequiredTaxNumber);
			AssertEquals("GCR: TAXCO002", wrapper.ConsigneeRequiredTaxNumber);

			var brTaxNum2 = shipper.CustomsCodes.AddNew();
			brTaxNum2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			brTaxNum2.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			brTaxNum2.OK_CustomsRegNo = "TAXBR001";

			var coTaxNum2 = consignee.CustomsCodes.AddNew();
			coTaxNum2.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			coTaxNum2.OK_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			coTaxNum2.OK_CustomsRegNo = "TAXCO002";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("CNPJ: TAXBR001", wrapper.ConsignorRequiredTaxNumber);
			AssertEquals("NIT: TAXCO002", wrapper.ConsigneeRequiredTaxNumber);
		}

		void PopulateRefDataIndia()
		{
			var indiaGST = Factory.New<RefDocOrgCusCode>();
			indiaGST.DOC_DocumentType = "HBL";
			indiaGST.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			indiaGST.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			indiaGST.DOC_Notes = "Tax Id";
			indiaGST.DOC_CodeType = "GST";
			indiaGST.DOC_ShortLabel = "GST";
			indiaGST.DOC_Priority = 1;

			var indiaIEC = Factory.New<RefDocOrgCusCode>();
			indiaIEC.DOC_DocumentType = "HBL";
			indiaIEC.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			indiaIEC.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			indiaIEC.DOC_Notes = "TEST";
			indiaIEC.DOC_CodeType = "IEC";
			indiaIEC.DOC_ShortLabel = "IEC";
			indiaIEC.DOC_Priority = 1;

			Factory.Save();
		}

		public void TestFreightWrapperRequiredVATNumbers_India_Consignee_Consignor()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER";
			shipper.OH_RL_NKClosestPort = "INABH";
			shipper.MainAddress.Address1 = "Unit 15";
			shipper.MainAddress.Address2 = "1 E Lane";
			shipper.MainAddress.City = "São Paulo";
			shipper.MainAddress.Postcode = "14780";
			shipper.MainAddress.OA_RN_NKCountryCode = "IN";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "INABH";
			consignee.MainAddress.Address1 = "Unit 15";
			consignee.MainAddress.Address2 = "1 E Lane";
			consignee.MainAddress.City = "IndiaCity";
			consignee.MainAddress.Postcode = "050022";
			consignee.MainAddress.OA_RN_NKCountryCode = "IN";

			var gstTaxNumShipper = shipper.CustomsCodes.AddNew();
			gstTaxNumShipper.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			gstTaxNumShipper.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			gstTaxNumShipper.OK_CustomsRegNo = "GST0001";

			var iecTaxNumShipiper = shipper.CustomsCodes.AddNew();
			iecTaxNumShipiper.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
			iecTaxNumShipiper.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			iecTaxNumShipiper.OK_CustomsRegNo = "IEC0001";

			var gstTaxNumConsignee = consignee.CustomsCodes.AddNew();
			gstTaxNumConsignee.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			gstTaxNumConsignee.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			gstTaxNumConsignee.OK_CustomsRegNo = "GST0001";

			var iecTaxNumConsignee = consignee.CustomsCodes.AddNew();
			iecTaxNumConsignee.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
			iecTaxNumConsignee.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			iecTaxNumConsignee.OK_CustomsRegNo = "IEC0001";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("GST: GST0001", wrapper.ConsignorRequiredTaxNumber);
			AssertEquals("TAX:GST0001, IEC0001", wrapper.ConsigneeRequiredTaxNumber);
		}

		void PopulateRefDataEgypt()
		{
			var egyptCOM = Factory.New<RefDocOrgCusCode>();
			egyptCOM.DOC_DocumentType = "HBL";
			egyptCOM.DOC_RN_NKCodeCountry = Constants.CountryCodes.Egypt;
			egyptCOM.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptCOM.DOC_Notes = "Tax Id";
			egyptCOM.DOC_CodeType = "COM";
			egyptCOM.DOC_ShortLabel = "COM";
			egyptCOM.DOC_Priority = 1;

			var egyptGCR = Factory.New<RefDocOrgCusCode>();
			egyptGCR.DOC_DocumentType = "HBL";
			egyptGCR.DOC_RN_NKCodeCountry = Constants.CountryCodes.Egypt;
			egyptGCR.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptGCR.DOC_Notes = "TEST";
			egyptGCR.DOC_CodeType = "GCR";
			egyptGCR.DOC_ShortLabel = "GCR";
			egyptGCR.DOC_Priority = 2;

			var australiaCOM = Factory.New<RefDocOrgCusCode>();
			australiaCOM.DOC_DocumentType = "HBL";
			australiaCOM.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			australiaCOM.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			australiaCOM.DOC_Notes = "Tax Id";
			australiaCOM.DOC_CodeType = "ABN";
			australiaCOM.DOC_ShortLabel = "ABN";
			australiaCOM.DOC_Priority = 1;

			var australiaGCR = Factory.New<RefDocOrgCusCode>();
			australiaGCR.DOC_DocumentType = "HBL";
			australiaGCR.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			australiaGCR.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			australiaGCR.DOC_Notes = "TEST";
			australiaGCR.DOC_CodeType = "GCR";
			australiaGCR.DOC_ShortLabel = "GCR";
			australiaGCR.DOC_Priority = 2;

			Factory.Save();
		}

		public void TestFreightWrapperRequiredVATNumbers_Egypt_Consignee()
		{
			PopulateRefDataEgypt();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var consigneeEG = Factory.New<OrgHeader>();
			consigneeEG.OH_FullName = "DUMMY";
			consigneeEG.OH_RL_NKClosestPort = "EGCAI";
			consigneeEG.MainAddress.Address1 = "Unit 1";
			consigneeEG.MainAddress.Address2 = "4 What Lane";
			consigneeEG.MainAddress.City = "Auckland";
			consigneeEG.MainAddress.Postcode = "5022";
			consigneeEG.MainAddress.OA_RN_NKCountryCode = "EG";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeEG.MainAddress.PK;

			var wrapper = FreightWrapper.New(shipment, Factory)[0];

			AssertEquals("Consginee Tax Number should be empty", string.Empty, wrapper.ConsigneeRequiredTaxNumber);

			var consingeeGCR = shipment.Consignee.CustomsCodes.AddNew();
			consingeeGCR.OK_RN_NKCodeCountry = "EG";
			consingeeGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consingeeGCR.OK_CustomsRegNo = "";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consginee Tax Number should be empty", string.Empty, wrapper.ConsigneeRequiredTaxNumber);

			shipment.Consignee.CustomsCodes.RemoveAll();
			consingeeGCR = shipment.Consignee.CustomsCodes.AddNew();
			consingeeGCR.OK_RN_NKCodeCountry = "EG";
			consingeeGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consingeeGCR.OK_CustomsRegNo = "123456";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consginee Tax Number should be filled in", "GCR: 123456", wrapper.ConsigneeRequiredTaxNumber);

			var consingeeCOM = shipment.Consignee.CustomsCodes.AddNew();
			consingeeCOM.OK_RN_NKCodeCountry = "EG";
			consingeeCOM.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			consingeeCOM.OK_CustomsRegNo = "7890";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consginee Tax Number should be filled in", "COM: 7890", wrapper.ConsigneeRequiredTaxNumber);
		}

		public void TestFreightWrapperRequiredVATNumbers_Egypt_Consignor()
		{
			PopulateRefDataEgypt();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var shipperAU = Factory.New<OrgHeader>();
			shipperAU.OH_FullName = "MAERSK";
			shipperAU.OH_RL_NKClosestPort = "AUSYD";
			shipperAU.MainAddress.Address1 = "Unit 13";
			shipperAU.MainAddress.Address2 = "4 Lost Lane";
			shipperAU.MainAddress.City = "Sydney";
			shipperAU.MainAddress.Postcode = "2000";
			shipperAU.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAU.MainAddress.PK;

			var wrapper = FreightWrapper.New(shipment, Factory)[0];

			AssertEquals("Consignor Tax Number should be empty", string.Empty, wrapper.ConsignorRequiredTaxNumber);

			var consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "AU";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consignor Tax Number should be empty", string.Empty, wrapper.ConsignorRequiredTaxNumber);

			shipment.Consignor.CustomsCodes.RemoveAll();
			consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "AU";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "123456";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consignor Tax Number should be filled in", "GCR: AU-01-123456", wrapper.ConsignorRequiredTaxNumber);

			var consignorABN = shipment.Consignor.CustomsCodes.AddNew();
			consignorABN.OK_RN_NKCodeCountry = "AU";
			consignorABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			consignorABN.OK_CustomsRegNo = "123456";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consignor Tax Number should be filled in", "ABN: AU-02-123456", wrapper.ConsignorRequiredTaxNumber);

			var shipperEG = Factory.New<OrgHeader>();
			shipperEG.OH_FullName = "MAERSK";
			shipperEG.OH_RL_NKClosestPort = "EGCAI";
			shipperEG.MainAddress.Address1 = "Unit 13";
			shipperEG.MainAddress.Address2 = "4 Lost Lane";
			shipperEG.MainAddress.City = "Sydney";
			shipperEG.MainAddress.Postcode = "2000";
			shipperEG.MainAddress.OA_RN_NKCountryCode = "EG";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperEG.MainAddress.PK;

			wrapper = FreightWrapper.New(shipment, Factory)[0];

			AssertEquals("Consignor Tax Number should be empty", string.Empty, wrapper.ConsignorRequiredTaxNumber);

			consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "EG";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consignor Tax Number should be empty", string.Empty, wrapper.ConsignorRequiredTaxNumber);

			shipment.Consignor.CustomsCodes.RemoveAll();
			consignorABN = shipment.Consignor.CustomsCodes.AddNew();
			consignorABN.OK_RN_NKCodeCountry = "EG";
			consignorABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			consignorABN.OK_CustomsRegNo = "123456";

			consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "EG";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "123456";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consignor Tax Number should be filled in", "GCR: EG-01-123456", wrapper.ConsignorRequiredTaxNumber);

			var consignorCOM = shipment.Consignor.CustomsCodes.AddNew();
			consignorCOM.OK_RN_NKCodeCountry = "EG";
			consignorCOM.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			consignorCOM.OK_CustomsRegNo = "7890";

			wrapper = FreightWrapper.New(shipment, Factory)[0];
			AssertEquals("Consignor Tax Number should be filled in", "COM: EG-02-7890", wrapper.ConsignorRequiredTaxNumber);
		}

		public void TestFreightWrapperRequiredVATNumbers_Notify()
		{
			AssertRequiredVATNumbers(Constants.CountryCodes.Uruguay, Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode, "UYMLZ", "");
			AssertRequiredVATNumbers(Constants.CountryCodes.Uruguay, Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode, "UYMLZ", "BDSPD");
			AssertRequiredVATNumbers(Constants.CountryCodes.Bangladesh, Constants.CountryCodes.Uruguay, OrgCusCode.BangladeshCodeTypes.AIN, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "BDRAU", "UYMLZ");
			AssertRequiredVATNumbers(Constants.CountryCodes.Bangladesh, Constants.CountryCodes.Argentina, OrgCusCode.BangladeshCodeTypes.AIN, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "BDSPD", "ARPLO");

			AssertRequiredVATNumbers(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.Argentina, "", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "USUCE", "ARPLO");
			AssertRequiredVATNumbers(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.Uruguay, "", UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "USUCE", "UYMLZ");
			AssertRequiredVATNumbers(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.Paraguay, "", OrgCusCode.ParaguayCodeTypes.RUC, "USUCE", "PYAGT");

			AssertRequiredVATNumbers(Constants.CountryCodes.Brazil, Constants.CountryCodes.Bangladesh, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode, "BRFRG", "BDSPD");
			AssertRequiredVATNumbers(Constants.CountryCodes.Brazil, Constants.CountryCodes.Peru, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, "BRFRG", "PESUL");
			AssertRequiredVATNumbers(Constants.CountryCodes.Brazil, Constants.CountryCodes.Turkey, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode, "BRFRG", "TRBDG");
		}

		void AssertRequiredVATNumbers(ZString expCountry, ZString impCountry, ZString expCodeType, ZString impCodeType, ZString orgPortName, ZString destPortName)
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var shipment = consol.Shipments.AddNew();

			var impRegNo = GetArgentinaAndBrazilCodeForTest(impCodeType);
			var expRegNo = GetArgentinaAndBrazilCodeForTest(expCodeType);

			OrgHeader consignee = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee.OH_Code = expCodeType + impCodeType + new Random(DateTime.Now.Millisecond).Next(10000);
			OrgCusCode taxCode1 = consignee.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = impCodeType;
			taxCode1.OK_RN_NKCodeCountry = impCountry;
			taxCode1.OK_CustomsRegNo = impRegNo;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignor.OH_Code = orgPortName + destPortName + new Random(DateTime.Now.Millisecond).Next(100);
			OrgCusCode taxCode2 = consignor.CustomsCodes.AddNew();
			if (expCountry.Equals(Core.Constants.CountryCodes.Bangladesh))
			{
				taxCode2.OK_CodeType = "BRN";
			}
			else
			{
				taxCode2.OK_CodeType = expCodeType;
			}
			taxCode2.OK_RN_NKCodeCountry = expCountry;
			taxCode2.OK_CustomsRegNo = expRegNo;

			orgFactory.Save();

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = orgPortName;
			shipment.JS_RL_NKDestination = destPortName;
			shipment.JS_TransportMode = "AIR";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Guid.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Guid.Empty;

			FreightWrapper wrapper = FreightWrapper.New(shipment, orgFactory)[0];

			var shortLabel = impRegNo;
			if (impCountry == Constants.CountryCodes.Bangladesh && impCodeType == OrgCusCode.CodeTypes.VATCode)
			{
				shortLabel = OrgCusCode.BangladeshCodeTypes.BIN;
			}
			AssertEquals("Return NotifyParty VAT Numbers.", shipment.JS_RL_NKDestination.IsEmpty ? ZString.Empty : (ZString)(shortLabel + ": " + impRegNo), wrapper.NotifyPartyRequiredTaxNumber);

			if (!string.IsNullOrEmpty(destPortName))
			{
				AssertEquals("NotifyParty VAT Numbers should not empty.", false, string.IsNullOrEmpty(wrapper.NotifyPartyRequiredTaxNumber));
			}
		}

		ZString GetArgentinaAndBrazilCodeForTest(ZString defaultNo)
		{
			ZString typeCode = defaultNo;
			switch (defaultNo)
			{
				case ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT:
					typeCode = "CUIT";
					break;
				case BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ:
					typeCode = "CNPJ";
					break;
			}

			return typeCode;
		}

		#endregion

		#region Test VAT numbers for import and export agents

		public void TestFreightWrapperAgentsRequiredVATNumbers()
		{
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode, "UYMLZ", "");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode, "UYMLZ", "BDSPD");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Uruguay, OrgCusCode.BangladeshCodeTypes.AIN, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "BDRAU", "UYMLZ");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Argentina, OrgCusCode.BangladeshCodeTypes.AIN, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "BDSPD", "ARPLO");

			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Argentina, "", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "USUCE", "ARPLO");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Uruguay, "", UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "USUCE", "UYMLZ");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Paraguay, "", OrgCusCode.ParaguayCodeTypes.RUC, "USUCE", "PYAGT");

			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Bangladesh, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode, "BRFRG", "BDSPD");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Peru, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, "BRFRG", "PESUL");
			AssertAgentsRequiredVATNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Turkey, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode, "BRFRG", "TRBDG");
		}

		void AssertAgentsRequiredVATNumbers(ZString expCountry, ZString impCountry, ZString expCodeType, ZString impCodeType, ZString orgPortName, ZString destPortName)
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var shipment = consol.Shipments.AddNew();

			var impRegNo = GetArgentinaAndBrazilCodeForTest(impCodeType);
			var expRegNo = GetArgentinaAndBrazilCodeForTest(expCodeType);

			OrgHeader recevingAgent = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			recevingAgent.OH_Code = expCodeType + impCodeType + new Random(DateTime.Now.Millisecond).Next(10000);
			OrgCusCode taxCode1 = recevingAgent.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = impCodeType;
			taxCode1.OK_RN_NKCodeCountry = impCountry;
			taxCode1.OK_CustomsRegNo = impRegNo;

			recevingAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_ReceivingForwarderAddress = recevingAgent.MainAddress.PK;

			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			sendingAgent.OH_Code = orgPortName + destPortName + new Random(DateTime.Now.Millisecond).Next(100);
			OrgCusCode taxCode2 = sendingAgent.CustomsCodes.AddNew();
			if (expCountry.Equals(Core.Constants.CountryCodes.Bangladesh))
			{
				taxCode2.OK_CodeType = "BRN";
			}
			else
			{
				taxCode2.OK_CodeType = expCodeType;
			}
			taxCode2.OK_RN_NKCodeCountry = expCountry;
			taxCode2.OK_CustomsRegNo = expRegNo;

			sendingAgent.OH_IsForwarder = ZBool.True;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			orgFactory.Save();

			shipment.JS_RL_NKOrigin = orgPortName;
			shipment.JS_RL_NKDestination = destPortName;
			shipment.JS_TransportMode = "AIR";

			FreightWrapper wrapper = FreightWrapper.New(shipment, orgFactory)[0];
			var shortLabel = impRegNo;
			if (impCountry == Constants.CountryCodes.Bangladesh && impCodeType == OrgCusCode.CodeTypes.VATCode)
			{
				shortLabel = OrgCusCode.BangladeshCodeTypes.BIN;
			}
			var actual = shipment.JS_RL_NKDestination.IsEmpty ? ZString.Empty : (ZString)(shortLabel + ": " + impRegNo);
			var actual2 = ZString.Empty;
			var delta = ZString.Empty;

			AssertEquals("Return Import Agent VAT Numbers.", actual, wrapper.ImportAgentRequiredTaxNumber);

			delta = wrapper.ExportAgentRequiredTaxNumber;
			if (Core.Constants.CountryCodes.UnitedStates.Equals(expCountry) && (Core.Constants.CountryCodes.Uruguay.Equals(impCountry) || Core.Constants.CountryCodes.Paraguay.Equals(impCountry)))
			{
				actual2 = !delta.IsEmpty ? (ZString)("EIN: " + expRegNo) : ZString.Empty;
				AssertEquals("Return Export Agent VAT Numbers.", actual2, delta);
			}
			else
			{
				actual2 = !delta.IsEmpty ? (ZString)(expRegNo + ": " + expRegNo) : ZString.Empty;
				AssertEquals("Return Export Agent VAT Numbers.", actual2, delta);
			}
		}

		#endregion

		#region TestDefaultValue

		public void TestDefaultValue()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var mock = new Mock<FreightWrapper>(dummy, Factory) { CallBase = true };
			mock.Protected().Setup<ZString>("GetJobNumber").Returns((ZString)"My Job Number");
			AssertEquals("My Job Number", mock.Object.ToString());
			mock.VerifyAll();
		}

		#endregion
	}
}
