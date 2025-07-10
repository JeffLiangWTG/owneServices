using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocForwardingShipment))]
	sealed class DocForwardingShipmentTest : DocShipmentTest
	{
		#region Coload Shipments Currency

		public void TestGoodsValueOfColoadShipmentsGroupedByCurrency()
		{
			CommonShipment coloadShipment = Shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_GoodsValue = 11.01M;
			coloadShipment.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.UnitedKingdom;

			coloadShipment = Shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_GoodsValue = 22.02M;
			coloadShipment.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.UnitedStates;

			coloadShipment = Shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_GoodsValue = 33m;
			coloadShipment.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.UnitedStates;

			coloadShipment = Shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_GoodsValue = 44m;
			coloadShipment.JS_RX_NKGoodsValueCurr = ZString.Empty;

			DocForwardingShipment wrapper = DocForwardingShipment.New(Shipment, Factory);
			Assert("Should contain", wrapper.GoodsValueOfColoadShipmentsGroupedByCurrency.Contains("11.01 GBP"));
			Assert("Should contain", wrapper.GoodsValueOfColoadShipmentsGroupedByCurrency.Contains("55.02 USD"));
			Assert("Should contain", wrapper.GoodsValueOfColoadShipmentsGroupedByCurrency.Contains("44.00 "));
		}

		#endregion

		#region TestVATNumbers

		public void TestRequiredVATNumbersDocRorwardingShipment()
		{
			var suffix = 0;
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode, "UYMLZ", "", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode, "UYMLZ", "BDSPD", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Uruguay, OrgCusCode.BangladeshCodeTypes.AIN, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "BDRAU", "UYMLZ", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Argentina, OrgCusCode.BangladeshCodeTypes.AIN, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "BDSPD", "ARPLO", suffix++);

			AssertRequiredVATNumbers(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Argentina, "", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "USUCE", "ARPLO", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Uruguay, "", UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "USUCE", "UYMLZ", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Paraguay, "", OrgCusCode.ParaguayCodeTypes.RUC, "USUCE", "PYAGT", suffix++);

			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Bangladesh, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode, "BRFRG", "BDSPD", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Peru, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, "BRFRG", "PESUL", suffix++);
			AssertRequiredVATNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Turkey, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode, "BRFRG", "TRBDG", suffix++);
		}

		void AssertRequiredVATNumbers(ZString expCountry, ZString impCountry, ZString expCodeType, ZString impCodeType, ZString orgPortName, ZString destPortName, int suffix)
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
			consignee.OH_Code = expCodeType + impCodeType + suffix.ToString();
			OrgCusCode taxCode1 = consignee.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = impCodeType;
			taxCode1.OK_RN_NKCodeCountry = impCountry;
			taxCode1.OK_CustomsRegNo = impRegNo;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignor.OH_Code = orgPortName + destPortName + suffix.ToString();
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

			var wrapper = DocForwardingShipment.New(shipment, Factory);
			var shortLabel = impRegNo;
			if (impCountry == Constants.CountryCodes.Bangladesh && impCodeType == OrgCusCode.CodeTypes.VATCode)
			{
				shortLabel = OrgCusCode.BangladeshCodeTypes.BIN;
			}
			var actual = shipment.JS_RL_NKDestination.IsEmpty ? ZString.Empty : (ZString)(shortLabel + ": " + impRegNo);
			var actual2 = ZString.Empty;
			var delta = ZString.Empty;

			AssertEquals("Return Consignee VAT Numbers", actual, wrapper.ConsigneeRequiredTaxNumber);

			delta = wrapper.ConsignorRequiredTaxNumber;
			if (Core.Constants.CountryCodes.UnitedStates.Equals(expCountry) && (Core.Constants.CountryCodes.Uruguay.Equals(impCountry) || Core.Constants.CountryCodes.Paraguay.Equals(impCountry)))
			{
				actual2 = !delta.IsEmpty ? (ZString)("EIN: " + expRegNo) : ZString.Empty;
				AssertEquals("Return Consignor VAT Numbers.", actual2, delta);
			}
			else
			{
				actual2 = !delta.IsEmpty ? (ZString)(expRegNo + ": " + expRegNo) : ZString.Empty;
				AssertEquals("Return Consignor VAT Numbers.", actual2, delta);
			}

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Guid.Empty;
			AssertEquals("Return NotifyParty VAT Numbers", actual, wrapper.NotifyPartyRequiredTaxNumber);

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

		public void TestConsigneeRequiredTaxNumberForIndia()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNSZX";
			shipment.JS_RL_NKDestination = "INABH";
			shipment.JS_TransportMode = "AIR";

			var consignee = Factory.NewWithValidTestData(typeof(OrgHeader), TestBusinessObjectKind.MinimumRequiredToSave) as OrgHeader;
			consignee.OH_Code = "One";
			consignee.MainAddress.OA_RN_NKCountryCode = "IN";
			var taxCode1 = consignee.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			taxCode1.OK_RN_NKCodeCountry = "IN";
			taxCode1.OK_CustomsRegNo = "GST001";
			var taxCode2 = consignee.CustomsCodes.AddNew();
			taxCode2.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
			taxCode2.OK_RN_NKCodeCountry = "IN";
			taxCode2.OK_CustomsRegNo = "IEC001";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			var wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("GST: GST001\r\nIEC: IEC001", wrapper.ConsigneeRequiredTaxNumber);
		}

		public void TestConsigneeRequiredTaxNumberForBrazil()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNSZX";
			shipment.JS_RL_NKDestination = "BRAAG";
			shipment.JS_TransportMode = "AIR";

			var consignee = Factory.NewWithValidTestData(typeof(OrgHeader), TestBusinessObjectKind.MinimumRequiredToSave) as OrgHeader;
			consignee.OH_Code = "One";
			consignee.MainAddress.OA_RN_NKCountryCode = "BR";

			var taxCode1 = consignee.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			taxCode1.OK_RN_NKCodeCountry = "BR";
			taxCode1.OK_CustomsRegNo = "CPF001";

			var taxCode2 = consignee.CustomsCodes.AddNew();
			taxCode2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			taxCode2.OK_RN_NKCodeCountry = "BR";
			taxCode2.OK_CustomsRegNo = "CNPJ001";

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			var wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("CNPJ: CNPJ001", wrapper.ConsigneeRequiredTaxNumber);

			consignee.CustomsCodes.RemoveAndDelete(taxCode2);
			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("CPF: CPF001", wrapper.ConsigneeRequiredTaxNumber);
		}

		public void TestNotifyPartyRequiredTaxNumberForBrazil()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNSZX";
			shipment.JS_RL_NKDestination = "BRAAG";
			shipment.JS_TransportMode = "AIR";

			var notifyParty = Factory.NewWithValidTestData(typeof(OrgHeader), TestBusinessObjectKind.MinimumRequiredToSave) as OrgHeader;
			notifyParty.OH_Code = "One";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "BR";

			var taxCode1 = notifyParty.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			taxCode1.OK_RN_NKCodeCountry = "BR";
			taxCode1.OK_CustomsRegNo = "CPF001";

			var taxCode2 = notifyParty.CustomsCodes.AddNew();
			taxCode2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			taxCode2.OK_RN_NKCodeCountry = "BR";
			taxCode2.OK_CustomsRegNo = "CNPJ001";

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;

			var wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("CNPJ: CNPJ001", wrapper.NotifyPartyRequiredTaxNumber);

			notifyParty.CustomsCodes.RemoveAndDelete(taxCode2);
			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("CPF: CPF001", wrapper.NotifyPartyRequiredTaxNumber);
		}
		#endregion

		#region Inbond Transit Number (US)

		public void TestITNumber()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			DocForwardingShipment docShipment = DocForwardingShipment.New(shipment, Factory);

			CusEntryNumber num = shipment.Numbers.AddNew();
			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			num.CE_EntryNum = "1234";
			num.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			num.CE_EntryLineReference = "USLAX";
			num.CE_IssueDate = new ZDateTime(2001, 4, 12);
			AssertEquals("1234", docShipment.InbondTransitNumber);
			AssertEquals("USLAX", docShipment.InbondTransitIssuePlace);
			AssertEquals(new ZDateTime(2001, 4, 12), docShipment.InbondTransitIssueDate);

			num.Delete();
			AssertEquals("", docShipment.InbondTransitNumber);
			AssertEquals("", docShipment.InbondTransitIssuePlace);
			AssertEquals(ZDateTime.Empty, docShipment.InbondTransitIssueDate);

			CusEntryNumber num2 = consol.Numbers.AddNew();
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			num2.CE_EntryNum = "666";
			num2.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			num2.CE_EntryLineReference = "USMEM";
			num2.CE_IssueDate = new ZDateTime(2005, 4, 12);
			AssertEquals("666", docShipment.InbondTransitNumber);
			AssertEquals("USMEM", docShipment.InbondTransitIssuePlace);
			AssertEquals(new ZDateTime(2005, 4, 12), docShipment.InbondTransitIssueDate);

			num2.CE_EntryType = "XYZ";
			AssertEquals("", docShipment.InbondTransitNumber);
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			num2.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			AssertEquals("", docShipment.InbondTransitNumber);
		}

		#endregion

		#region Menu Filter Tests

		#region Country Specific Tests
		#region China
		public void TestIsChina()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "GR";
			AssertEquals("IsChina should be false", ZBool.False, ShipmentWrapper.IsChina);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			AssertEquals("IsChina should be trie", ZBool.True, ShipmentWrapper.IsChina);
		}

		public void TestIsChinaAndIsAir()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ShipmentWrapper = (DocForwardingShipment)GetNewShipmentWrapper();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "GR";
			AssertEquals("IsChinaAndIsAir should return false", ZBool.False, ShipmentWrapper.IsChinaAndIsAir);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			AssertEquals("IsChinaAndIsAir should return false", ZBool.False, ShipmentWrapper.IsChinaAndIsAir);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ShipmentWrapper = (DocForwardingShipment)GetNewShipmentWrapper();
			AssertEquals("IsChinaAndIsAir should return true", ZBool.True, ShipmentWrapper.IsChinaAndIsAir);
		}

		public void TestIsChinaAndIsSea()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ShipmentWrapper = (DocForwardingShipment)GetNewShipmentWrapper();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "GR";
			AssertEquals("IsChinaAndIsSea should return false", ZBool.False, ShipmentWrapper.IsChinaAndIsSea);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			AssertEquals("IsChinaAndIsSea should return false", ZBool.False, ShipmentWrapper.IsChinaAndIsSea);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ShipmentWrapper = (DocForwardingShipment)GetNewShipmentWrapper();
			AssertEquals("IsChinaAndIsSea should return true", ZBool.True, ShipmentWrapper.IsChinaAndIsSea);
		}
		#endregion
		#endregion

		#endregion

		#region Approved Known Shipper

		public void TestIsApprovedKnownShipper()
		{
			Shipment.JS_InspectionTypeCode = "TST";
			AssertEquals(ZBool.False, ShipmentWrapper.IsApprovedKnownShipper);

			Shipment.JS_InspectionTypeCode = "APP";
			AssertEquals(ZBool.True, ShipmentWrapper.IsApprovedKnownShipper);

			Shipment.JS_InspectionTypeCode = "NON";
			AssertEquals(ZBool.False, ShipmentWrapper.IsApprovedKnownShipper);
		}

		#endregion

		#region TestPreAlertReference

		public override void TestPreAlertReference()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			AssertEquals("ORDER NUMBERS / REFERENCE", ShipmentWrapper.PreAlertReferenceHeading);

			AssertEquals("", ShipmentWrapper.PreAlertReference);

			Shipment.AttachedOrders.AddNew().JD_OrderNumber = "order1";
			Shipment.AttachedOrders.AddNew().JD_OrderNumber = "order2";
			AssertEquals("order1,order2", ShipmentWrapper.PreAlertReference);

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec1 = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec1.JE_JS = Shipment.PK;
			dec1.JE_GB = GlbBranch.CurrentBranch.PK;
			dec1.JE_OwnerRef = "OWNERS REFERENCE";

			AssertEquals("order1,order2 OWNERS REFERENCE", ShipmentWrapper.PreAlertReference);

			dec1.JE_OwnerRef = "order1";
			AssertEquals("order1,order2", ShipmentWrapper.PreAlertReference);

			dec1.JE_OwnerRef = "order2";
			AssertEquals("order1,order2", ShipmentWrapper.PreAlertReference);

			dec1.JE_OwnerRef = "order3";
			AssertEquals("order1,order2 order3", ShipmentWrapper.PreAlertReference);
		}

		#endregion

		#region TestHBHeadingForCoLoad

		public void TestHBHeadingForCoLoad()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("CoL HBL:", ShipmentWrapper.HBManifestHeading);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("HBL:", ShipmentWrapper.HBManifestHeading);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals("CoL HBL:", ShipmentWrapper.HBManifestHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("CoL HAWB:", ShipmentWrapper.HBManifestHeading);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("HAWB:", ShipmentWrapper.HBManifestHeading);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals("CoL HAWB:", ShipmentWrapper.HBManifestHeading);
		}

		#endregion

		#region TestCurrentConsol

		public void TestCurrentConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			DocForwardingShipment shipmentWrapper = DocForwardingShipment.New(shipment, Factory);

			AssertEquals("No Consols", null, shipmentWrapper.CurrentConsol);

			ForwardingConsol consol = (ForwardingConsol)GetNewConsol();
			consol.JK_UniqueConsignRef = "1";
			shipment.CurrentConsolForDocuments = consol;

			AssertEquals("CurrentConsol should be Consol 1", consol.JK_UniqueConsignRef, shipmentWrapper.CurrentConsol.ConsolNumber);

			ForwardingConsol consol2 = (ForwardingConsol)GetNewConsol();
			consol2.JK_UniqueConsignRef = "2";
			shipmentWrapper.CurrentConsol = DocForwardingConsol.New(consol2, Factory);
			AssertEquals("CurrentConsol should be Consol 2", consol2.JK_UniqueConsignRef, shipmentWrapper.CurrentConsol.ConsolNumber);
		}

		#endregion

		#region TestImageNamesToRemove

		public void TestImageNamesToRemove()
		{
			var docDataProvider = (IBODocDataProvider)ShipmentWrapper;
			AssertEquals("Should be empty collection", 0, docDataProvider.ImageNamesToRemove.Length);

			var documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.Shipment, true);
			ShipmentWrapper = DocForwardingShipment.New(documentShipment, Factory);
			Shipment.JS_HouseBillOfLadingType = "FIP";
			docDataProvider = ShipmentWrapper;

			AssertEquals("Should have 1 element", 1, docDataProvider.ImageNamesToRemove.Length);
			AssertEquals("Should have 'BillOfLading.FaceImage'", "BillOfLading.FaceImage", docDataProvider.ImageNamesToRemove[0]);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			docDataProvider = ShipmentWrapper;

			docDataProvider.SetDocWrapperContext(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Other Menu Items" } });
			AssertEquals("Should be empty collection", 0, docDataProvider.ImageNamesToRemove.Length);

			docDataProvider.SetDocWrapperContext(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Shi Lian Dan" } });
			AssertEquals("Should have 1 element", 1, docDataProvider.ImageNamesToRemove.Length);
			AssertEquals("Should have 'DockReceipt.FaceImage'", "DockReceipt.FaceImage", docDataProvider.ImageNamesToRemove[0]);
		}

		#endregion

		#region TestNote

		public void TestNote()
		{
			var organisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "Order number 1";
			order1.JD_InvoiceNumber = "123";
			order1.SupplierPK = organisation1.PK;

			var organisation2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order2 = Shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "Order number 2";
			order2.JD_InvoiceNumber = "321";
			order2.SupplierPK = organisation2.PK;

			AssertEquals("Order Numbers: " + ShipmentWrapper.OrderNumbers, ShipmentWrapper.Note);
		}

		#endregion

		#region Orders Tests

		public void TestFirstOrderNumber()
		{
			Factory.Save();

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var order1 = Shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "Order Number 1";
			order1.JD_InvoiceNumber = "123";
			order1.SupplierPK = consignor.PK;

			var order2 = Shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "Order Number 2";
			order2.JD_InvoiceNumber = "456";
			order2.SupplierPK = consignor.PK;

			AssertEquals("First Order number", "Order Number 1", ShipmentWrapper.FirstOrderNumber);
		}

		public void TestOrderNumbers()
		{
			Shipment.DocsAndCartage.JP_OrderItemsAsString = "Order ref. 111";
			Factory.Save();

			AssertEquals("Order numbers", "Order,ref.,111", ShipmentWrapper.OrderNumbers);
			AssertEquals("No Invoice numbers", "", ShipmentWrapper.InvoiceNumbers);
			Factory.Save();

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var order1 = Shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "Order number 1";
			order1.JD_InvoiceNumber = "123";
			order1.SupplierPK = consignor.PK;

			var order2 = Shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "Order number 2";
			order2.JD_InvoiceNumber = "321";
			order2.SupplierPK = consignor.PK;

			AssertEquals("Order numbers", "Order number 1,Order number 2", ShipmentWrapper.OrderNumbers);
			AssertEquals("Invoice numbers", "123, 321", ShipmentWrapper.InvoiceNumbers);
		}

		public void TestOwnerRefAndOrderRef()
		{
			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode = australia.Code;

			Shipment.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			AssertEquals("Precondition: No declaration for current branch", null, ShipmentWrapper.DeclarationForCurrentBranch);
			AssertEquals("Test with no declaration", "Orders", ShipmentWrapper.OwnerRefAndOrderRef);

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = Shipment.PK;
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_OwnerRef = "Owner's Ref 123";

			Shipment.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			AssertEquals("Test with order items as string", "Owner's Ref 123 Orders", ShipmentWrapper.OwnerRefAndOrderRef);

			var shipmentWithOrders = Shipment;
			Order order = shipmentWithOrders.AttachedOrders.AddNew();
			order.JD_OrderNumber = "456";
			AssertEquals("Test with order items as objects", "Owner's Ref 123 456", ShipmentWrapper.OwnerRefAndOrderRef);

			dec.JE_OwnerRef = "456";
			AssertEquals("Test with order items as objects", "456", ShipmentWrapper.OwnerRefAndOrderRef);
		}

		public void TestOrderNumbersForInvoice()
		{
			Shipment.DocsAndCartage.JP_OrderItemsAsString = "OrderNumbers";
			AssertEquals("Order Numbers For Invoice", ShipmentWrapper.OrderNumbers, ShipmentWrapper.OrderNumbersForInvoice);
		}

		public void TestOrderReference()
		{
			Shipment.DocsAndCartage.JP_OrderItemsAsString = "OrderReference";
			AssertEquals("Order Reference", "OrderReference", ShipmentWrapper.OrderReference);
		}

		public void TestClientOwnerOrderReferenceIncludingRelatedShipments()
		{
			Shipment.JS_UniqueConsignRef = "Lead Shipment";

			var coload1 = Shipment.CoLoadShipments.AddNew();
			coload1.JS_UniqueConsignRef = "CoLoad Shipment 1";

			var coload2 = Shipment.CoLoadShipments.AddNew();
			coload2.JS_UniqueConsignRef = "CoLoad Shipment 2";

			Order order1 = coload1.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "CoLoad1 Order1";

			Order order2 = coload2.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "CoLoad2 Order1";

			Order order3 = coload2.AttachedOrders.AddNew();
			order3.JD_OrderNumber = "CoLoad2 Order2";

			ShipmentWrapper = (DocForwardingShipment)GetNewShipmentWrapper();
			AssertEquals("Order numbers contains orders from related shipments", "Lead Shipment / CoLoad1 Order1,CoLoad2 Order1,CoLoad2 Order2", ShipmentWrapper.ClientOwnerOrderReferenceIncludingRelatedShipments);
		}

		#endregion

		public void TestClientBrandingUsesLocalClient()
		{
			var company = GlbCompany.CurrentCompany;
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var assembly = Assembly.Load("Enterprise.Rating.Business");

			var tariff1 = Factory.New(assembly.GetType("Enterprise.Rating.Business.CompanyTariff"));
			tariff1["TH_GC"] = company.PK;
			tariff1["TH_RateType"] = "GLB";

			var tariff2 = Factory.New(assembly.GetType("Enterprise.Rating.Business.CompanyTariff"));

			var levels = new ClientTariffAndLevelCollection();
			tariff1["TH_GC"] = company.PK;
			tariff1["TH_RateType"] = "GLB";

			Factory.Save();

			var level1 = levels.AddNew();
			level1.Code = ((ICompanyTariff)tariff1).TH_GlobalRateLevel.ToString();
			level1.BrandName = "Google";
			level1.BrandEmailAddress = "google@cargowise.com";
			level1.Image = new Bitmap(1, 1);

			var level2 = levels.AddNew();
			level2.Code = ((ICompanyTariff)tariff2).TH_GlobalRateLevel.ToString();
			level2.BrandName = "Microsoft";
			level2.BrandEmailAddress = "microsoft@cargowise.com";
			level2.Image = new Bitmap(1, 1);

			DocumentsDataRegistry.Instance.ClientTariffAndLevels.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, levels);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Google";
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = localClient.PK;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "123";
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = localClient.PK;

			Factory.Save();

			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			Factory.Save();

			var wrapper = DocForwardingShipment.New(shipment, Factory);
			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, consignee.PK);
			wrapper.SetTemplateConstants(constants);

			AssertEquals("wrapper.BrandName", "MICROSOFT", wrapper.BrandName);
		}

		#region Bill Of Lading

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHBLLogoWithAgentBranding()
		{
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.GetStream("DocumentWrappers.Freight.AWBMasterTitle.png");
				Image testImage = Image.FromStream(testFile);
				Env.Registry.HouseBillOfLadingLogo = testImage;
				AssertEquals("Default is Env.Registry.HouseBillOfLadingLogo", testImage.Size, ShipmentWrapper.HBLLogo.Size);

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
				AssertEquals("No Agent for shipment - fallback to default image", testImage.Size, ShipmentWrapper.HBLLogo.Size);

				DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.AgentBranded);

				CommonConsol consol = Shipment.Consols.AddNew();
				OrgHeader receivingForwarder = OrgHeader.New(Factory);
				receivingForwarder.MiscServ.OM_FWAgentCategory = "STD";

				consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				BrandingTestHelperClass.SetHybridBrandRegistryImage(DocumentsDataRegistry.Instance.HBLAgentBrandingImage);

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				AssertEquals("Brand image from registry", new Size(5, 5), ShipmentWrapper.HBLLogo.Size);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHBLLogoWithClientBranding()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.GetStream("DocumentWrappers.Freight.AWBMasterTitle.png");
				Image testImage = Image.FromStream(testFile);
				Env.Registry.HouseBillOfLadingLogo = testImage;
				DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.ClientBranded);
				DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
				FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				AssertEquals("Default is Env.Registry.HouseBillOfLadingLogo", testImage.Size, ShipmentWrapper.HBLLogo.Size);

				BrandingTestHelperClass.SetHybridBrandRegistryImage(DocumentsDataRegistry.Instance.HBLAgentBrandingImage);

				OrgHeader cnor = OrgHeader.New(Factory);
				cnor.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
				OrgHeader client = OrgHeader.New(Factory);
				client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 55);

				Shipment.ConsignorPK = cnor.PK;
				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				AssertEquals("Should return brand image at cnor's tariff level", new Size(9, 9), ShipmentWrapper.HBLLogo.Size);

				var header = Factory.NewJobForTesting<JobHeader>();
				header.JH_ParentID = Shipment.PK;
				header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				header.JH_GC = GlbCompany.CurrentCompany.PK;
				header.JH_GB = GlbBranch.CurrentBranch.PK;
				header.LocalChargesPK = client.PK;

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				AssertEquals("Should return brand image at client's tariff level", new Size(8, 8), ShipmentWrapper.HBLLogo.Size);
			}
		}

		public void TestPlaceOfReceiptForBOL()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Shipment.JS_RL_NKOrigin = "";
				AssertEquals("PlaceOfReceiptForBOL", "", ShipmentWrapper.PlaceOfReceiptForBOL);

				var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHAM"));
				Shipment.JS_RL_NKOrigin = unloco.RL_Code;
				AssertEquals("PlaceOfReceiptForBOL is in English", "HAMBURG, GERMANY", ShipmentWrapper.PlaceOfReceiptForBOL);
			}
		}

		public void TestPlaceOfDeliveryForBOL()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Shipment.JS_RL_NKDestination = "";
				AssertEquals("PlaceOfDeliveryForBOL", "", ShipmentWrapper.PlaceOfDeliveryForBOL);

				var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL"));
				Shipment.JS_RL_NKDestination = unloco.RL_Code;
				AssertEquals("PlaceOfDeliveryForBOL is in English", "AUCKLAND, NEW ZEALAND", ShipmentWrapper.PlaceOfDeliveryForBOL);
			}
		}

		public void TestFreightPayableAtForBOL()
		{
			Shipment.JS_RL_NKOrigin = "";
			Shipment.JS_RL_NKDestination = "";
			Shipment.JS_INCO = "";
			AssertEquals("FreightPayableAtForBOL", "", ShipmentWrapper.FreightPayableAtForBOL);

			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			AssertEquals("FreightPayableAtForBOL", "", ShipmentWrapper.FreightPayableAtForBOL);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			AssertEquals("FreightPayableAtForBOL", "", ShipmentWrapper.FreightPayableAtForBOL);

			Shipment.JS_RL_NKOrigin = "AUBNE";
			Shipment.JS_RL_NKDestination = "USLAX";

			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			AssertEquals("FreightPayableAtForBOL", "BRISBANE, AUSTRALIA", ShipmentWrapper.FreightPayableAtForBOL);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			AssertEquals("FreightPayableAtForBOL", "LOS ANGELES, UNITED STATES", ShipmentWrapper.FreightPayableAtForBOL);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(new Dictionary<string, object>
				{
					{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Legacy Manufacturer Bill Of Lading" }
				});

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.MainAddress.OA_RL_NKRelatedPortCode = "CRAPO";
			Shipment.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;

			AssertEquals("FreightPayableAtForBOL", "LOS ANGELES, UNITED STATES", ShipmentWrapper.FreightPayableAtForBOL);
		}

		[ExpectNoExceptions]
		public void TestFreightPayableAtForBOLWhenManufacturerBillOfLading()
		{
			Shipment.JS_RL_NKOrigin = "";
			Shipment.JS_RL_NKDestination = "";
			Shipment.JS_INCO = "";
			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(new Dictionary<string, object>
			{
				{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Legacy Manufacturer Bill Of Lading" }
			});
			AssertEquals("Should be empty", string.Empty, ShipmentWrapper.FreightPayableAtForBOL);

			Shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals("Should not be empty", "LOS ANGELES, UNITED STATES", ShipmentWrapper.FreightPayableAtForBOL);
		}

		public void TestChooseCorrectBillOfLadingConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol1.JK_RL_NKLoadPort = "AUBNE";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.JK_MasterBillNum = "FIRSTCONSOL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USCHI";
			consol2.JK_MasterBillNum = "SECONDCONSOL";

			var billOfLadingQuery = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			billOfLadingQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);
			var billOfLadingMenuItem = Factory.LoadTop1<StmMenuItem>(billOfLadingQuery);
			var consolAgentPackMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Consol Agent Pack (Sea)"));

			var contextManager = Factory.GetDocWrapperContextManager();
			contextManager.SetupDocWrapperContextFromDocumentPack(consolAgentPackMenu.SU_DocumentDirection
				, consolAgentPackMenu.PK
				, consolAgentPackMenu.SU_MenuName
				, consolAgentPackMenu.SU_ContactType
				, null
				, null);

			var wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, billOfLadingMenuItem);
			AssertEquals(1, wrappers.Length);

			var wrapper = wrappers[0] as DocForwardingShipment;
			AssertEquals(consol2.PK, wrapper.Consol.Consol.PK);
		}

		public void TestBillOfLadingLoadsPacklineContainerForAssemblyMasterShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			var subShipment = shipment.CoLoadShipments.AddNew();

			var packLine = subShipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;

			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "IRSU3344553";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedShipment = anotherFactory.Load<CommonShipment>(shipment.PK);

			var wrapper = (DocForwardingShipment)DocForwardingShipment.New(loadedShipment, anotherFactory);
			var documentSupporter = wrapper.BillOfLading as IFormedPagesSupporter;
			var pack = documentSupporter.PackLines[0];
			AssertNotNull(pack.Container);
			AssertEquals(container.JC_ContainerNum, pack.ContainerCode);
		}

		#endregion

		#region Transhipment

		public void TestExportTranshipmentPackDepotAddress()
		{
			AssertEquals("Preconditions: No Consol expected on the Shipment", 0, Shipment.Consols.Count);

			OrgAddress unpackDepotAddress = Factory.New<OrgAddress>();
			OrgAddress packDepotAddress = Factory.New<OrgAddress>();
			OrgAddress releaseDepotAddress = Factory.New<OrgAddress>();

			Shipment.JS_RL_NKOrigin = "NZAKA";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_OA_ImportReleaseDepot = releaseDepotAddress.PK;
			AssertNull("No Consol - No Wrapper", ShipmentWrapper.ExportTranshipmentConsol);
			AssertNull("No Consol - No ReleaseDepotAddress", ShipmentWrapper.ExportTranshipmentPackDepotAddress);

			CommonConsol lastLegConsol = Shipment.Consols.AddNew();
			lastLegConsol.JK_OA_UnpackDepotAddress = unpackDepotAddress.PK;
			lastLegConsol.JK_OA_PackDepotAddress = packDepotAddress.PK;
			lastLegConsol.JK_RL_NKLoadPort = "NZAKL";
			lastLegConsol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("One Consol - Shipment Delivery Depot Address would be picked up", releaseDepotAddress.PK, ((JobDocAddress)ShipmentWrapper.ExportTranshipmentPackDepotAddress.WrappedObject).E2_OA_Address);

			CommonConsol firstLegConsol = Shipment.Consols.AddNew();
			firstLegConsol.JK_RL_NKLoadPort = "NZAKA";
			firstLegConsol.JK_RL_NKDischargePort = "NZAKL";
			AssertSame("Two Consols - ExportTranshipment Consol would be the last Consol", lastLegConsol, ShipmentWrapper.ExportTranshipmentConsol.WrappedObject);
			AssertEquals("Two Consols - UnpackDepot Address on last Consol would be picked up", packDepotAddress.PK, ((JobDocAddress)ShipmentWrapper.ExportTranshipmentPackDepotAddress.WrappedObject).E2_OA_Address);
		}

		#endregion

		#region Container Liability Statement

		public void TestContainerLiabilityOpeningText()
		{
			string value = "Test Warning Text";
			DocumentsDataRegistry.Instance.ContainerLiabilityStatementLiabilityWarningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals("Opening Text", "Test Warning Text", ShipmentWrapper.ContainerLiabiltyStatementWarningText);
		}

		#endregion

		#region Test Request for Profit Share

		public void TestRequestForProfitShareOpeningText()
		{
			string value = "Opening Text Request for Profit Share";
			DocumentsDataRegistry.Instance.RequestForProfitShareOpeningText_Shipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals("Opening Text", "Opening Text Request for Profit Share", ShipmentWrapper.RequestForProfitShareOpeningText);
		}

		public void TestRequestForProfitShareDocumentHeader()
		{
			AssertEquals("Request for Shipment Profit Share Credit Note", ShipmentWrapper.RequestForProfitShareDocumentHeader);
		}

		public void TestAWBSecurtyDeclarationOpeningText()
		{
			DocumentOpenCloseText text = new DocumentOpenCloseText("AWBSD Opening Text", "");
			Env.Registry.AWBSecurityDeclaration = text;
			AssertEquals("Opening Text should be returned", "AWBSD Opening Text", ShipmentWrapper.AWBSecurityDeclarationOpeningText);
		}

		#endregion

		#region ZDateTime Fields

		public void TestMasterBillIssueDate()
		{
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.MasterBillIssueDate);

			CommonConsol consol = CreateExportConsol(Shipment);
			consol.JK_MasterBillIssueDate = ZDateTime.Today;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Don't show for air", ZDateTime.Empty, ShipmentWrapper.MasterBillIssueDate);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("JK_MasterBillIssueDate should be today", ZDateTime.Today, ShipmentWrapper.MasterBillIssueDate);
		}

		public void TestMasterBillAndIssueDate()
		{
			AssertEquals("", ShipmentWrapper.MasterBillAndIssueDate);

			CommonConsol consol = CreateExportConsol(Shipment);
			consol.JK_MasterBillIssueDate = ZDateTime.Today;
			consol.JK_MasterBillNum = "1234";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Don't show issue date for air", "1234", ShipmentWrapper.MasterBillAndIssueDate);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "1234";
			AssertEquals("JK_MasterBillIssueDate should be today", "123-4 / " + ZDateTime.Today.ToShortDateString(), ShipmentWrapper.MasterBillAndIssueDate);
		}

		#endregion

		#region ZBool Fields

		public void TestShowChargesOnBookingConfirmation()
		{
			Env.Registry.ShowChargesOnForwardingBookingConfirmation = true;
			AssertEquals("ShowChargesOnBookingConfirmation", true, ShipmentWrapper.ShowChargesOnBookingConfirmation);

			Env.Registry.ShowChargesOnForwardingBookingConfirmation = false;
			AssertEquals("ShowChargesOnBookingConfirmation", false, ShipmentWrapper.ShowChargesOnBookingConfirmation);
		}

		#endregion

		#region TestBOLPortAndCountryNames

		public void TestBOLPortAndCountryNames()
		{
			ForwardingShipment shipmentA = Factory.New<ForwardingShipment>();
			shipmentA.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentA.JS_INCO = "FOB";
			shipmentA.JS_RL_NKOrigin = "SGSIN";
			shipmentA.JS_RL_NKDestination = "HKHKG";

			ForwardingConsol consolA = Factory.New<ForwardingConsol>();
			consolA.JK_RL_NKLoadPort = "SMSAI";
			consolA.JK_RL_NKDischargePort = "MCMON";

			shipmentA.Consols.Add(consolA);

			DocForwardingShipment shipmentWrapperA = DocForwardingShipment.New(shipmentA, Factory);

			AssertEquals("SINGAPORE", shipmentWrapperA.PlaceOfReceiptForBOL);
			AssertEquals("HONG KONG", shipmentWrapperA.PlaceOfDeliveryForBOL);
			AssertEquals("HONG KONG", shipmentWrapperA.FreightPayableAtForBOL);
			AssertEquals("HONG KONG", shipmentWrapperA.DestinationLoco.PortNameAndCountryName.ToUpper());
			AssertEquals("SAN MARINO", shipmentWrapperA.BillOfLading.PortOfLoadingDefault);
			AssertEquals("MONACO", shipmentWrapperA.BillOfLading.PortOfDischargeDefault);

			ForwardingShipment shipmentB = Factory.New<ForwardingShipment>();
			shipmentB.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			shipmentB.JS_INCO = "FOB";
			shipmentB.JS_RL_NKOrigin = "ZAAAM";
			shipmentB.JS_RL_NKDestination = "FRCRQ";

			ForwardingConsol consolB = Factory.New<ForwardingConsol>();
			consolB.JK_RL_NKLoadPort = "GBXNU";
			consolB.JK_RL_NKDischargePort = "BEAES";

			shipmentB.Consols.Add(consolB);

			DocForwardingShipment shipmentWrapperB = DocForwardingShipment.New(shipmentB, Factory);

			AssertEquals("MALA MALA, SOUTH AFRICA", shipmentWrapperB.PlaceOfReceiptForBOL);
			AssertEquals("CRAPONNE, FRANCE", shipmentWrapperB.PlaceOfDeliveryForBOL);
			AssertEquals("CRAPONNE, FRANCE", shipmentWrapperB.FreightPayableAtForBOL);
			AssertEquals("CRAPONNE, FRANCE", shipmentWrapperB.DestinationLoco.PortNameAndCountryName.ToUpper());
			AssertEquals("NUTFIELD, UNITED KINGDOM", shipmentWrapperB.BillOfLading.PortOfLoadingDefault);
			AssertEquals("ASSE, BELGIUM", shipmentWrapperB.BillOfLading.PortOfDischargeDefault);
		}

		#endregion

		#region Test First/Last Consol

		public void TestFirstLastConsols()
		{
			ZQuery localPortsFilter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2));
			localPortsFilter.MaximumRows = 3;
			ZQuery nonLocalPortsFilter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2));
			nonLocalPortsFilter.MaximumRows = 4;

			RefUNLOCO[] localPorts = Factory.Load<RefUNLOCO>(localPortsFilter);
			RefUNLOCO[] nonLocalPorts = Factory.Load<RefUNLOCO>(nonLocalPortsFilter);

			ZDateTime now = ZDateTime.Now;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			DocForwardingShipment shipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("First Export: should be null, as there are no consols", null, shipmentWrapper.FirstExportConsol);
			AssertEquals("Last Import: should be null, as there are no consols", null, shipmentWrapper.LastImportConsol);
			AssertEquals("Last: should be null, as there are no consols", null, shipmentWrapper.LastConsol);

			ForwardingConsol randomConsol = shipment.Consols.AddNew();

			shipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("First Export: dont fall back to random consols", null, shipmentWrapper.FirstExportConsol);
			AssertEquals("Last Import: dont fall back to random consols", null, shipmentWrapper.LastImportConsol);
			AssertEquals("Last:", randomConsol, shipmentWrapper.LastConsol.WrappedObject);

			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = localPorts[1].RL_Code;
			consol1.JK_RL_NKDischargePort = nonLocalPorts[2].RL_Code;
			consol1.Transports[0].JW_ETD = now.AddDays(10);

			ForwardingConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = nonLocalPorts[0].RL_Code;
			consol2.JK_RL_NKDischargePort = localPorts[1].RL_Code;
			consol2.Transports[0].JW_ETD = now.AddDays(5);

			ForwardingConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = nonLocalPorts[2].RL_Code;
			consol3.JK_RL_NKDischargePort = localPorts[2].RL_Code;
			consol3.Transports[0].JW_ETD = now.AddDays(15);

			ForwardingConsol consol4 = shipment.Consols.AddNew();
			consol4.JK_RL_NKLoadPort = localPorts[0].RL_Code;
			consol4.JK_RL_NKDischargePort = nonLocalPorts[0].RL_Code;
			consol4.Transports[0].JW_ETD = now.AddDays(1);

			shipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("First Export:", consol4, shipmentWrapper.FirstExportConsol.WrappedObject);
			AssertEquals("Last Import:", consol3, shipmentWrapper.LastImportConsol.WrappedObject);
			AssertEquals("Last:", randomConsol, shipmentWrapper.LastConsol.WrappedObject);
		}

		#endregion

		#region Iceland specific
		public void TestCustomsHouse()
		{
			RefCountry oldCountry = GlbCompany.CurrentCompany.Country;

			try
			{
				RefCountry icelandCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iceland);
				if (icelandCountry == null)
				{
					icelandCountry = Factory.NewWithValidTestData<RefCountry>();
					icelandCountry.RN_Code = Core.Constants.CountryCodes.Iceland;
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);

				OrgHeader co = Factory.NewWithValidTestData<OrgHeader>();
				co.OH_FullName = "asdf";
				co.MainAddress.OA_Fax = "444444";
				OrgCusCode cusCode = co.CustomsCodes.AddNew(OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, "ASDFG", icelandCountry);

				RefUNLOCO iSZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
				iSZZZ.RL_Code = "ISZZZ";
				RefLocoMap map = iSZZZ.RefLocoMaps.AddNew();
				map.RY_RN = icelandCountry.PK;
				map.RY_SystemUsage = ISLocoMapSystemUsageList.Codes.CustomsOfficeCode;
				map.RY_LocalPortCode = "ASDFG";

				ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
				ForwardingShipment shipment = consol.Shipments.AddNew();
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);

				AssertNull("CustomsHouse", ShipmentWrapper.CustomsHouse);

				consol.JK_RL_NKLoadPort = "ISZZZ";
				shipment.JS_RL_NKOrigin = "ISREY";
				shipment.JS_RL_NKDestination = "AUSYD";
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertNotNull("CustomsHouse", ShipmentWrapper.CustomsHouse);
				AssertEquals(co.MainAddress.OA_Fax, ShipmentWrapper.CustomsHouse.Fax);
				AssertEquals(co.OH_FullName, ShipmentWrapper.CustomsHouse.CompanyName);

				OrgAddress cocAddress = co.Addresses.AddNew();
				cocAddress.OA_CompanyNameOverride = "vfrt";
				cocAddress.OA_Fax = "7777777";
				cusCode.OK_OA_PremisesAddress = cocAddress.PK;
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertNotNull("CustomsHouse", ShipmentWrapper.CustomsHouse);
				AssertEquals(cocAddress.OA_Fax_Formatted, ShipmentWrapper.CustomsHouse.Fax);
				AssertEquals(cocAddress.OA_CompanyNameOverride, ShipmentWrapper.CustomsHouse.CompanyName);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}
		}

		public void TestCustomsHouseCode()
		{
			RefCountry oldCountry = GlbCompany.CurrentCompany.Country;

			try
			{
				RefCountry icelandCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iceland);
				if (icelandCountry == null)
				{
					icelandCountry = Factory.NewWithValidTestData<RefCountry>();
					icelandCountry.RN_Code = Core.Constants.CountryCodes.Iceland;
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);

				RefUNLOCO iSZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
				iSZZZ.RL_Code = "ISZZZ";
				RefLocoMap map = iSZZZ.RefLocoMaps.AddNew();
				map.RY_RN = icelandCountry.PK;
				map.RY_SystemUsage = ISLocoMapSystemUsageList.Codes.CustomsOfficeCode;
				map.RY_LocalPortCode = "ASDFG";

				ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
				ForwardingShipment shipment = consol.Shipments.AddNew();
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertEquals("", ShipmentWrapper.CustomsHouseCode);

				consol.JK_RL_NKLoadPort = "ISZZZ";
				shipment.JS_RL_NKOrigin = "ISREY";
				shipment.JS_RL_NKDestination = "AUSYD";
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertEquals("ASDFG", ShipmentWrapper.CustomsHouseCode);

				ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, icelandCountry.Code);
				CusEntryNumber[] numbers = (CusEntryNumber[])shipment.Numbers.Find(query);
				CusEntryNumber number = numbers.Length > 0 ? numbers[0] : null;
				AssertNotNull("COC", number);
				number.CE_EntryNum = "aass";
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertEquals("aass", ShipmentWrapper.CustomsHouseCode);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}
		}

		public void TestPreviousCustomsHouseCodeAndPreviousCustomsHouse()
		{
			RefCountry lastCountry = GlbBranch.CurrentBranch.Country;

			try
			{
				GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.Iceland);
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);

				RefCountry icelandCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iceland);

				OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_FullName = "Org1";
				org1.MainAddress.OA_Fax = "111111";
				org1.CustomsCodes.AddNew(OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, "1111", icelandCountry);

				OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_FullName = "Org2";
				org2.MainAddress.OA_Fax = "222222";
				org2.CustomsCodes.AddNew(OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, "2222", icelandCountry);

				Shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				CusEntryNumber cusEntryNumber = Shipment.Numbers.AddNew();
				AssertEquals(ShipmentWrapper.PreviousCustomsHouseCode, Shipment.PreviousCOC);

				CusEntryNumber[] numbers = (CusEntryNumber[])Shipment.Numbers.Find(new ZQuery(CusEntryNumSchema.CE_EntryType, "COC"));
				cusEntryNumber = numbers[0];
				cusEntryNumber.CE_EntryNum = "1111";
				Factory.Save();

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				AssertEquals(ShipmentWrapper.PreviousCustomsHouseCode, Shipment.PreviousCOC);
				AssertEquals("Previous bloody COC number", "", ShipmentWrapper.PreviousCustomsHouseCode);
				AssertNull("PreviousCustomsHouse", ShipmentWrapper.PreviousCustomsHouse);

				numbers = (CusEntryNumber[])Shipment.Numbers.Find(new ZQuery(CusEntryNumSchema.CE_EntryType, "COC"));
				cusEntryNumber = numbers[0];
				cusEntryNumber.CE_EntryNum = "2222";
				Factory.Save();

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				AssertEquals(ShipmentWrapper.PreviousCustomsHouseCode, Shipment.PreviousCOC);
				AssertEquals("Previous bloody COC number", "1111", ShipmentWrapper.PreviousCustomsHouseCode);
				AssertNotNull("PreviousCustomsHouse", ShipmentWrapper.PreviousCustomsHouse);
				AssertEquals(org1.MainAddress.OA_Fax, ShipmentWrapper.PreviousCustomsHouse.Fax);
				AssertEquals(org1.OH_FullName, ShipmentWrapper.PreviousCustomsHouse.CompanyName);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(lastCountry.RN_Code);
				GlbCompany.CurrentCompany.SetCountry(lastCountry.RN_Code);
			}
		}

		public void TestPreviousConsignorConsignee()
		{
			RefCountry lastCountry = GlbBranch.CurrentBranch.Country;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);
				GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.Iceland);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				consignor1.OH_FullName = "~ZZZ111";
				OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
				consignee1.OH_FullName = "~YYY111";
				Factory.Save();

				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertNull(ShipmentWrapper.PreviousConsignorConsignee);
				AssertEquals(ZString.Empty, ShipmentWrapper.PreviousConsignorConsigneeName);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee1.PK;
				Factory.Save();
				OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				consignor2.OH_FullName = "~ZZZ112";
				OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
				consignee2.OH_FullName = "~YYY112";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee2.PK;
				Factory.Save();

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "ISREY";
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertNotNull(ShipmentWrapper.PreviousConsignorConsignee);
				AssertEquals(consignee1.OH_FullName, ShipmentWrapper.PreviousConsignorConsignee.Name);
				AssertEquals(consignee1.OH_FullName, ShipmentWrapper.PreviousConsignorConsigneeName);

				shipment.JS_RL_NKOrigin = "ISREY";
				shipment.JS_RL_NKDestination = "AUSYD";
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
				AssertNotNull(ShipmentWrapper.PreviousConsignorConsignee);
				AssertEquals(consignor1.OH_FullName, ShipmentWrapper.PreviousConsignorConsignee.Name);
				AssertEquals(consignor1.OH_FullName, ShipmentWrapper.PreviousConsignorConsigneeName);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(lastCountry.RN_Code);
				GlbCompany.CurrentCompany.SetCountry(lastCountry.RN_Code);
			}
		}

		public void TestIcelandCRN()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.CustomsEntryNumber = "S-D61-8735-8-DF-RTL-9U59-1";
			ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("S-D61-8735-8-DF-RTL-9U59-1", ShipmentWrapper.CRNWOCheckDigit);
			AssertEquals("S-D61-8735-8-DF-RTL-9U59-1", ShipmentWrapper.CRNWithSpaces);
			AssertEquals("", ShipmentWrapper.CRNCarrierNumer);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.CustomsEntryNumber = "S-D61-8735-8-DF-RTL-9U59-1";
			ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("S-D61-8735-8-DF-RTL-9U59", ShipmentWrapper.CRNWOCheckDigit);
			AssertEquals("S D61 8735 8 DF RTL 9U59", ShipmentWrapper.CRNWithSpaces);
			AssertEquals("9U59", ShipmentWrapper.CRNCarrierNumer);
		}
		#endregion

		#region China Specific

		public void TestShippingOrderNumber()
		{
			TestShippingOrderNumber(Core.Constants.CountryCodes.China);
			TestShippingOrderNumber(Core.Constants.CountryCodes.HongKong);
		}

		void TestShippingOrderNumber(string countryCode)
		{
			RefCountry oldCountry = GlbCompany.CurrentCompany.Country;
			ForwardingShipment shipment;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);

				shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);

				AssertEquals("", ShipmentWrapper.ShippingOrderNumber);

				var cusEntryNumber = shipment.Numbers.AddNew();
				cusEntryNumber.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
				cusEntryNumber.CE_EntryNum = "Some Number";
				AssertEquals("Some Number", ShipmentWrapper.ShippingOrderNumber);
				Factory.Save();
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			ShipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("", ShipmentWrapper.ShippingOrderNumber);
		}

		#endregion

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocForwardingShipment.New(Shipment, Factory),
				DocForwardingShipment.New(Factory, Shipment.PK)
			};
		}

		new ForwardingShipment Shipment
		{
			get
			{
				return (ForwardingShipment)base.Shipment;
			}
			set
			{
				base.Shipment = value;
			}
		}

		new DocForwardingShipment ShipmentWrapper
		{
			get
			{
				return (DocForwardingShipment)base.ShipmentWrapper;
			}
			set
			{
				base.ShipmentWrapper = value;
			}
		}

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		protected override CommonShipment GetNewShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		protected override DocShipment GetNewShipmentWrapper()
		{
			return DocForwardingShipment.New(Shipment, Factory);
		}

		#endregion
	}
}
