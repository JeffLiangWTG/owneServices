using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocJobInvoicingJob;
using FreightContainer = Enterprise.Freight.Business.CommonContainer;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocForwardingConsol))]
	sealed class DocConsolTestCase : DocBaseConsolAbstractTestClass
	{
		public void TestConsignorOrgOverride()
		{
			AssertNull("No sending forwarder", ConsolWrapper.ConsignorOrg);

			var firstOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			firstOrg.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var addr2 = firstOrg.Addresses.AddNew();
			addr2.OA_Address1 = "SF SECOND ADDRESS";
			addr2.OA_RN_NKCountryCode = ZString.Empty;

			Consol.JK_OA_SendingForwarderAddress = addr2.PK;
			AssertEquals("Sending Forwarder name", firstOrg.OH_FullName, ConsolWrapper.ConsignorOrg.Name);
			AssertEquals("Sending Forwarder address", firstOrg.OH_FullName + "\nSF SECOND ADDRESS\n" + firstOrg.Country.Description.ToUpper(), ConsolWrapper.ConsignorOrg.SelectedAddress.ToString());

			Consol.JK_TransportMode = "SEA";
			Consol.JK_AgentType = "CLD";

			((IBODocDataProvider)ConsolWrapper).SetDocWrapperContext(new Dictionary<string, object>
			{
				{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Forwarding Instruction Standard" }
			});

			AssertNull("Sending Forwarder name is null", ConsolWrapper.ConsignorOrg?.Name);

			var mbcOrg = Factory.New<OrgHeader>();
			mbcOrg.OH_Code = "MBCORG";
			mbcOrg.OH_FullName = "MBC Organisation";
			mbcOrg.MainAddress.Address1 = "MBC Address 1";
			mbcOrg.MainAddress.Address2 = "MBC Address 2";
			Consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = mbcOrg.PK;

			AssertEquals("Sending Forwarder should be Master Bill Consignor", mbcOrg, ConsolWrapper.ConsignorOrg.OrgHeader);
			AssertEquals("Sending Forwarder should be Master Bill Consignor", "MBC Organisation", ConsolWrapper.ConsignorOrg.Name);
		}

		public void TestConsigneeOrgOverride()
		{
			AssertNull("No Receiving Forwarder", ConsolWrapper.ConsigneeOrg);

			var firstOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			firstOrg.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var addr2 = firstOrg.Addresses.AddNew();
			addr2.OA_Address1 = "RF SECOND ADDRESS";
			addr2.OA_RN_NKCountryCode = ZString.Empty;

			Consol.JK_OA_ReceivingForwarderAddress = addr2.PK;
			AssertEquals("Receiving Forwarder name", firstOrg.OH_FullName, ConsolWrapper.ConsigneeOrg.Name);
			AssertEquals("Receiving Forwarder address 1", firstOrg.OH_FullName + "\nRF SECOND ADDRESS\n" + firstOrg.Country.Description.ToUpper(), ConsolWrapper.ConsigneeOrg.SelectedAddress.ToString());

			Consol.JK_TransportMode = "SEA";
			Consol.JK_AgentType = "CLD";

			((IBODocDataProvider)ConsolWrapper).SetDocWrapperContext(new Dictionary<string, object>
			{
				{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Forwarding Instruction Detailed" }
			});

			AssertNull("Receiving Forwarder name is null", ConsolWrapper.ConsigneeOrg?.Name);

			var mbcOrg = Factory.New<OrgHeader>();
			mbcOrg.OH_Code = "MBSORG";
			mbcOrg.OH_FullName = "MBS Organisation";
			mbcOrg.MainAddress.Address1 = "MBS Address 1";
			mbcOrg.MainAddress.Address2 = "MBS Address 2";
			Consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = mbcOrg.PK;

			AssertEquals("Receiving Forwarder should be Master Bill Consignee", mbcOrg, ConsolWrapper.ConsigneeOrg.OrgHeader);
			AssertEquals("Receiving Forwarder should be Master Bill Consignee", "MBS Organisation", ConsolWrapper.ConsigneeOrg.Name);
		}

		public void TestCtStatus()
		{
			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "DAN";
			ukCompany.GC_RN_NKCountryCode = "GB";
			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_RL_NKHomePort = "GBLON";
			ukBranch.GB_Code = "DAN";
			Factory.Save();

			Consol.Shipments.RemoveAndDeleteAll();
			AssertEquals("No explosion when no CT codes", "", ConsolWrapper.CtStatus);
			var ship1 = Consol.Shipments.AddNew();
			var ship2 = Consol.Shipments.AddNew();
			var ship3 = Consol.Shipments.AddNew();
			var ship4 = Consol.Shipments.AddNew();
			var euDeclarationType = ObjectFactory.GetType<Integration.Customs.EU.IJobDeclaration>();
			BaseJobDeclaration euDec1 = null;
			BaseJobDeclaration euDec2 = null;
			using (DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid()))
			{
				euDec1 = (BaseJobDeclaration)Factory.New(euDeclarationType);
				euDec2 = (BaseJobDeclaration)Factory.New(euDeclarationType);
			}
			var baseDeclarationWithoutCtStatusProperty = Factory.New<BaseJobDeclaration>();
			euDec1.JE_JS = ship1.PK;
			euDec2.JE_JS = ship2.PK;
			baseDeclarationWithoutCtStatusProperty.JE_JS = ship3.PK;
			var columnName = Enterprise.Customs.EU.Business.Declaration.JobDeclaration.Schema.ZG_CTStatusID;
			euDec1[columnName] = ExportCommunityTransitStatusList.Codes.T1;
			euDec2[columnName] = ExportCommunityTransitStatusList.Codes.TD;
			AssertEquals("Looks as the two EU declarations and ignores the AU one and gives most severe EU one", ExportCommunityTransitStatusList.Codes.TD, ConsolWrapper.CtStatus);
			euDec1[columnName] = ExportCommunityTransitStatusList.Codes.C;
			euDec2[columnName] = ExportCommunityTransitStatusList.Codes.C;
			AssertEquals(ExportCommunityTransitStatusList.Codes.C, ConsolWrapper.CtStatus);
			euDec2[columnName] = ExportCommunityTransitStatusList.Codes.X;
			AssertEquals("Mixed CT statuses give us most severe", ExportCommunityTransitStatusList.Codes.X, ConsolWrapper.CtStatus);
			ship4.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.T2;
			AssertEquals("Can pull a CTS straight from a declarationless shipment", ExportCommunityTransitStatusList.Codes.T2, ConsolWrapper.CtStatus);
		}

		#region TestTotalPrepaidAndCollectForPrintOnManifest

		public void TestTotalPrepaidAndCollectForPrintOnManifest()
		{
			GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency = "AUD";

			ZString homePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			if (homePort.IsEmpty)
			{
				homePort = "AUSYD";
			}

			ZString overseasPort = homePort.StartsWith("GB") ? "NLAMS" : "GBLON";

			Consol.JK_RL_NKLoadPort = homePort;
			Consol.JK_RL_NKDischargePort = overseasPort;

			ForwardingShipment shipment1 = AddShipment(Consol, Constants.IncoTerms.FreeOnBoard);
			Job header1 = GetNewHeader(shipment1);
			header1.LocalChargesPK = shipment1.ConsignorPK;
			header1.AgentCollectPK = shipment1.ConsigneePK;
			AddCharge(header1, "OWHARF", 1101m); // prepaid
			AddCharge(header1, "DWHARF", "AUD", 1100m).JR_OH_SellAccount = header1.AgentCollectPK; // collect
			AddCharge(header1, "FRT", "USD", 15100m); // collect

			ForwardingShipment shipment2 = AddShipment(Consol, Constants.IncoTerms.FreeOnBoard);
			Job header2 = GetNewHeader(shipment2);
			header2.LocalChargesPK = shipment2.ConsignorPK;
			header2.AgentCollectPK = shipment2.ConsigneePK;
			AddCharge(header2, "OWHARF", 1202m); // prepaid
			AddCharge(header2, "DWHARF", "USD", 1200m).JR_OH_SellAccount = header2.AgentCollectPK; // collect
			AddCharge(header2, "FRT", "USD", 15200m); // collect

			ForwardingShipment shipment3 = AddShipment(Consol, Constants.IncoTerms.FreeOnBoard);
			Job header3 = GetNewHeader(shipment3);
			header3.LocalChargesPK = shipment3.ConsignorPK;
			header3.AgentCollectPK = shipment3.ConsigneePK;
			AddCharge(header3, "OWHARF", 1404m); // prepaid
			AddCharge(header3, "DWHARF", "HKD", 1400m).JR_OH_SellAccount = header3.AgentCollectPK; // collect
			AddCharge(header3, "FRT", "USD", 15400); // collect

			AssertContainsExactElementsInAnyOrder("Collect Totals",
				new string[] { "HK$1400.00 HKD", "$46900.00 USD", "$1100.00 AUD" },
				ConsolWrapper.TotalCollectForPrintOnManifest.ToString().Split('\n'));

			AssertEquals("Prepaid Total", "$3707.00 AUD", ConsolWrapper.TotalPrepaidForPrintOnManifest);
		}

		ForwardingShipment AddShipment(ForwardingConsol consol, ZString incoTerm)
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

			ForwardingShipment result = consol.Shipments.AddNew();
			result.JS_INCO = incoTerm;
			result.ConsignorPK = consignor.PK;
			result.ConsigneePK = consignee.PK;

			return result;
		}

		JobCharge AddCharge(Job header, ZString chargeCode, ZDecimal localSellAmt)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_LocalSellAmt = localSellAmt;

			return charge;
		}

		JobCharge AddCharge(Job header, ZString chargeCode, ZString osSellCurrency, ZDecimal osSellAmt)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_RX_NKSellCurrency = osSellCurrency;
			charge.JR_OSSellAmt = osSellAmt;

			return charge;
		}

		#endregion

		public void TestDangerousGoodsStatement()
		{
			DocumentsDataRegistry.Instance.DangerousGoodsStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "111");
			AssertEquals("111", ConsolWrapper.DangerousGoodsStatement);
			DocumentsDataRegistry.Instance.DangerousGoodsStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "222");
			AssertEquals("222", ConsolWrapper.DangerousGoodsStatement);
		}

		public void TestHasDangerousGoods()
		{
			AssertEquals("Precondition: No Shipments", 0, Consol.Shipments.Count);
			AssertEquals(false, ConsolWrapper.HasDangerousGoods);

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingShipment shipment3 = Consol.Shipments.AddNew();

			shipment2.OuterPackLines.AddNew();
			shipment2.OuterPackLines.AddNew();
			shipment2.OuterPackLines.AddNew();

			AssertEquals(false, ConsolWrapper.HasDangerousGoods);

			shipment2.OuterPackLines[2].UNDGs.AddNew().DI_DG = Factory.NewWithValidTestData<UNDGSubstance>().PK;

			AssertEquals(true, ConsolWrapper.HasDangerousGoods);
		}

		public void TestDGContacts()
		{
			AssertEquals("Precondition: No Shipments", 0, Consol.Shipments.Count);
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingShipment shipment3 = Consol.Shipments.AddNew();

			OrgHeader consignor1 = Factory.New<OrgHeader>();
			OrgHeader consignor2 = Factory.New<OrgHeader>();
			OrgHeader consignor3 = Factory.New<OrgHeader>();

			shipment1.ConsignorNameOrPK = consignor1.PK.ToString();
			shipment2.ConsignorNameOrPK = consignor2.PK.ToString();
			shipment3.ConsignorNameOrPK = consignor3.PK.ToString();

			OrgContact contact1 = Factory.New<OrgContact>();
			OrgContact contact2 = Factory.New<OrgContact>();
			OrgContact contact3 = Factory.New<OrgContact>();

			consignor1.Contacts.Add(contact1);
			consignor2.Contacts.Add(contact2);
			consignor3.Contacts.Add(contact3);

			contact1.OC_ContactName = "AAA";
			contact1.OC_HomePhone = "111";
			contact1.OC_Phone = "222";

			contact2.OC_ContactName = "ABB";
			contact2.OC_HomePhone = "111";
			contact2.OC_Phone = "222";

			contact3.OC_ContactName = "AAA";
			contact3.OC_HomePhone = "111";
			contact3.OC_Phone = "222";

			consignor1.MiscServ.OM_OC_EXDefaultDGContact = contact1.PK;
			consignor2.MiscServ.OM_OC_EXDefaultDGContact = contact2.PK;
			consignor3.MiscServ.OM_OC_EXDefaultDGContact = contact3.PK;

			consignor1.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;
			consignor2.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;
			consignor3.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;

			AssertEquals("AAA 111, ABB 111", ConsolWrapper.DGContacts);

			consignor1.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;
			consignor2.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.WRK;
			consignor3.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.WRK;

			AssertEquals("AAA 111, ABB 222, AAA 222", ConsolWrapper.DGContacts);
		}

		#region Airline Security Statement

		public void TestIsApprovedKnownShipper()
		{
			AssertEquals("Precondition: No Shipments", 0, Consol.Shipments.Count);
			AssertEquals(ZBool.False, ConsolWrapper.IsApprovedKnownShipper);

			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor3 = Factory.NewWithValidTestData<OrgHeader>();

			var docOrg1 = DocOrganisation.New(Factory, consignor1.PK);
			var docOrg2 = DocOrganisation.New(Factory, consignor2.PK);
			var docOrg3 = DocOrganisation.New(Factory, consignor3.PK);

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingShipment shipment3 = Consol.Shipments.AddNew();

			shipment1.ConsignorPK = consignor1.PK;
			shipment2.ConsignorPK = consignor2.PK;
			shipment3.ConsignorPK = consignor3.PK;

			AssertEquals("Precondition: Should have three shipments", 3, Consol.Shipments.Count);

			shipment1.JS_InspectionTypeCode = "TST";
			shipment2.JS_InspectionTypeCode = "NNN";
			shipment3.JS_InspectionTypeCode = "WOW";
			AssertEquals(ZBool.False, ConsolWrapper.IsApprovedKnownShipper);
			AssertContainsExactElementsInAnyOrder(new[] { docOrg1, docOrg2, docOrg3 }, ConsolWrapper.KnownOrUnknownShippers);

			shipment1.JS_InspectionTypeCode = "TST";
			shipment2.JS_InspectionTypeCode = "APP";
			shipment3.JS_InspectionTypeCode = "WOW";
			AssertEquals(ZBool.False, ConsolWrapper.IsApprovedKnownShipper);
			AssertContainsExactElementsInAnyOrder(new[] { docOrg1, docOrg3 }, ConsolWrapper.KnownOrUnknownShippers);

			shipment1.JS_InspectionTypeCode = "APP";
			shipment2.JS_InspectionTypeCode = "APP";
			shipment3.JS_InspectionTypeCode = "WOW";
			AssertEquals(ZBool.False, ConsolWrapper.IsApprovedKnownShipper);
			AssertContainsExactElementsInAnyOrder(new[] { docOrg3 }, ConsolWrapper.KnownOrUnknownShippers);

			shipment1.JS_InspectionTypeCode = "APP";
			shipment2.JS_InspectionTypeCode = "APP";
			shipment3.JS_InspectionTypeCode = "APP";
			AssertEquals(ZBool.True, ConsolWrapper.IsApprovedKnownShipper);
			AssertContainsExactElementsInAnyOrder(new[] { docOrg1, docOrg2, docOrg3 }, ConsolWrapper.KnownOrUnknownShippers);

			shipment1.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment1.ConsignorDocumentaryAddress.E2_CompanyName = "AAAA";
			shipment3.ConsignorPK = ZGuid.Empty;

			shipment1.JS_InspectionTypeCode = "XXX";
			shipment2.JS_InspectionTypeCode = "APP";
			shipment3.JS_InspectionTypeCode = "BBB";

			docOrg1 = DocOrganisation.New(shipment1.ConsignorDocumentaryAddress, Factory);

			AssertEquals(ZBool.False, ConsolWrapper.IsApprovedKnownShipper);
			AssertEquals(true, shipment3.ConsignorDocumentaryAddress.IsEmpty);
			AssertContainsExactElementsInAnyOrder(new[] { docOrg1 }, ConsolWrapper.KnownOrUnknownShippers);
		}

		public void TestAirlineSecurityStatement()
		{
			DocumentsDataRegistry.Instance.AirlineSecurityStatementKnownShipper.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Known Shipper Statement");
			DocumentsDataRegistry.Instance.AirlineSecurityStatementUnknownShipper.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Unknown Shipper Statement");
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			AssertEquals("Precondition: Consol should have one shipment", 1, Consol.Shipments.Count);
			AssertEquals("Precondition: Airline Security Statement Known Shipper incorrect", "Known Shipper Statement", DocumentsDataRegistry.Instance.AirlineSecurityStatementKnownShipper.Value);
			AssertEquals("Precondition: Airline Security Statement Unknown Shipper incorrect", "Unknown Shipper Statement", DocumentsDataRegistry.Instance.AirlineSecurityStatementUnknownShipper.Value);

			shipment.JS_InspectionTypeCode = "APP";
			AssertEquals("Precondition: ConsolWrapper should be Approved Known", ZBool.True, ConsolWrapper.IsApprovedKnownShipper);
			AssertEquals("Known Shipper Statement", ConsolWrapper.AirlineSecurityStatement);

			shipment.JS_InspectionTypeCode = "TST";
			AssertEquals("Precondition: ConsolWrapper should not be Approved Known", ZBool.False, ConsolWrapper.IsApprovedKnownShipper);
			AssertEquals("Unknown Shipper Statement", ConsolWrapper.AirlineSecurityStatement);
		}

		#endregion

		#region Import Cargo Label

		public void TestStaticNewMethodWithDocumentImportCargoLabel()
		{
			DocForwardingConsol nullConsol = DocForwardingConsol.New((DocumentImportCargoLabel)null, Factory);
			AssertNull("Consol wrapper is null", nullConsol);

			var consol = Factory.New<ForwardingConsol>();
			DocumentImportCargoLabel documentImportCargoLabel = new DocumentImportCargoLabel(consol);
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(documentImportCargoLabel, Factory);
			AssertNotNull("Consol wrapper is not null", consolWrapper);
		}

		public void TestImportCargoLabelProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocumentImportCargoLabel documentImportCargoLabel = new DocumentImportCargoLabel(consol);
			documentImportCargoLabel.IncludeConsignee = true;
			documentImportCargoLabel.IncludeConsignor = true;
			documentImportCargoLabel.IncludeHouseBill = true;
			documentImportCargoLabel.IncludeCFSName = false;
			documentImportCargoLabel.NoOfLabelsToPrint = 14;
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(documentImportCargoLabel, Factory);
			AssertEquals("Include Consignee", true, consolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", true, consolWrapper.IncludeConsignor);
			AssertEquals("Include HouseBill", true, consolWrapper.IncludeHouseBill);
			AssertEquals("Include CFSName", false, consolWrapper.IncludeCFSName);
			AssertEquals("No of cargo labels to print", 14, consolWrapper.NoOfImportCargoLabelsToPrint);
			AssertEquals("Use Shipment for Cargo Labels", true, consolWrapper.UseShipmentForCargoLabels);

			documentImportCargoLabel = new DocumentImportCargoLabel(consol);
			documentImportCargoLabel.IncludeConsignee = false;
			documentImportCargoLabel.IncludeConsignor = false;
			documentImportCargoLabel.IncludeHouseBill = false;
			documentImportCargoLabel.IncludeCFSName = true;
			documentImportCargoLabel.NoOfLabelsToPrint = 5;
			consolWrapper = DocForwardingConsol.New(documentImportCargoLabel, Factory);
			AssertEquals("Include Consignee", false, consolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", false, consolWrapper.IncludeConsignor);
			AssertEquals("Include HouseBill", false, consolWrapper.IncludeHouseBill);
			AssertEquals("Include CFSName", true, consolWrapper.IncludeCFSName);
			AssertEquals("No of cargo labels to print", 5, consolWrapper.NoOfImportCargoLabelsToPrint);
			AssertEquals("Use Shipment for Cargo Labels", false, consolWrapper.UseShipmentForCargoLabels);
		}

		#endregion

		#region Common Consol

		public void TestStaticNewMethodWithDocumentCommonConsol()
		{
			DocForwardingConsol nullConsol = DocForwardingConsol.New((DocumentCommonConsol)null, Factory);
			AssertNull("Consol wrapper is null", nullConsol);

			var consol = Factory.New<ForwardingConsol>();
			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument);
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			AssertNotNull("Consol wrapper is not null", consolWrapper);
		}

		public void TestCommonConsolProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = true;
			documentCommonConsol.IncludeUnPacked = true;
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			AssertEquals("Include Consignee", true, consolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", true, consolWrapper.IncludeConsignor);
			AssertEquals("Include Customs Broker", true, consolWrapper.IncludeCustomsBroker);
			AssertEquals("Include All Shipments", true, consolWrapper.IncludeAllShipments);
			AssertEquals("Include Packed", true, consolWrapper.IncludePacked);
			AssertEquals("Include UnPacked", true, consolWrapper.IncludeUnPacked);

			documentCommonConsol = new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeConsignee = false;
			documentCommonConsol.IncludeConsignor = false;
			documentCommonConsol.IncludeCustomsBroker = false;
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			consolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			AssertEquals("Include Consignee", false, consolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", false, consolWrapper.IncludeConsignor);
			AssertEquals("Include Customs Broker", false, consolWrapper.IncludeCustomsBroker);
			AssertEquals("Include All Shipments", false, consolWrapper.IncludeAllShipments);
			AssertEquals("Include Packed", false, consolWrapper.IncludePacked);
			AssertEquals("Include UnPacked", false, consolWrapper.IncludeUnPacked);
		}

		public void TestPackLinesForLoadList_ContainerTotalManifestUnits()
		{
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingPackLine line1 = shipment1.OuterPackLines.AddNew();
			ForwardingPackLine unPacked1 = shipment1.OuterPackLines.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingPackLine line2 = shipment2.OuterPackLines.AddNew();

			ForwardingContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";

			JobContainerPackPivot pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line1.PK;
			pivot1.J6_JC = container1.PK;

			JobContainerPackPivot pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line2.PK;
			pivot2.J6_JC = container1.PK;

			unPacked1.JL_Calc_ContainerNumber = ZString.Empty;

			line1.JL_ActualWeightUQ = "KG";
			line2.JL_ActualWeightUQ = "LB";
			unPacked1.JL_ActualWeightUQ = "LB";

			line1.JL_ActualVolumeUQ = "M3";
			line2.JL_ActualVolumeUQ = "CF";
			unPacked1.JL_ActualVolumeUQ = "CF";

			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeAllShipments = true;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);

			DocPackLinesCollection packLines = ConsolWrapper.PackLinesForLoadList;
			AssertEquals(3, packLines.Count);

			foreach (DocPackLines docPackline in packLines)
			{
				AssertEquals("All packlines should total in Freight Weight Unit", ConsolWrapper.WeightUnit, docPackline.ContainerTotalManifestWeightUnit);
				AssertEquals("All packlines should total in Freight Weight Volume", ConsolWrapper.VolumeUnit, docPackline.ContainerTotalManifestVolumeUnit);
			}
		}

		public void TestPackLinesForLoadList()
		{
			AssertEquals(0, ConsolWrapper.PackLinesForLoadList.Count);

			var shipment1 = Consol.Shipments.AddNew();
			var line1 = shipment1.OuterPackLines.AddNew();
			var unPacked1 = shipment1.OuterPackLines.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var line2 = shipment2.OuterPackLines.AddNew();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			var exportBroker2 = Factory.NewWithValidTestData<OrgHeader>();
			var importBroker2 = Factory.NewWithValidTestData<OrgHeader>();

			consignor.OH_FullName = "Consignor";
			consignee.OH_FullName = "Consignee";
			exportBroker.OH_FullName = "ExportBroker";
			importBroker.OH_FullName = "ImportBroker";
			consignor2.OH_FullName = "Consignor2";
			consignee2.OH_FullName = "Consignee2";
			exportBroker2.OH_FullName = "ExportBroker2";
			importBroker2.OH_FullName = "ImportBroker2";

			shipment1.ConsigneePK = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.JS_OH_ExportBroker = exportBroker.PK;
			shipment1.JS_OH_ImportBroker = importBroker.PK;
			shipment2.ConsigneePK = consignee2.PK;
			shipment2.ConsignorPK = consignor2.PK;
			shipment2.JS_OH_ExportBroker = exportBroker2.PK;
			shipment2.JS_OH_ImportBroker = importBroker2.PK;

			var container1 = (FreightContainer)Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";

			var pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line1.PK;
			pivot1.J6_JC = container1.PK;

			var pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line2.PK;
			pivot2.J6_JC = container1.PK;

			unPacked1.JL_Calc_ContainerNumber = ZString.Empty;

			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			AssertEquals(0, ConsolWrapper.PackLinesForLoadList.Count);

			documentCommonConsol.IncludePacked = true;
			documentCommonConsol.IncludeUnPacked = false;
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludeConsignee = false;
			documentCommonConsol.IncludeConsignor = false;
			documentCommonConsol.IncludeCustomsBroker = false;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(2, ConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("ConsignorAndConsignee", ZString.Empty, ConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);
			AssertEquals("CustomsBroker", ZString.Empty, ConsolWrapper.PackLinesForLoadList[0].CustomsBrokerForLoadList);

			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = true;
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(1, ConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("ConsignorAndConsignee", "Consignor" + System.Environment.NewLine + "Consignee", ConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);
			AssertEquals("CustomsBroker", "Broker: ExportBroker", ConsolWrapper.PackLinesForLoadList[0].CustomsBrokerForLoadList);

			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = false;
			documentCommonConsol.IncludeCustomsBroker = true;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals(3, ConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("ConsignorAndConsignee", "Consignee2", ConsolWrapper.PackLinesForLoadList[2].ConsignorAndConsigneeForLoadList);
			AssertEquals("CustomsBroker", "Broker: ImportBroker2", ConsolWrapper.PackLinesForLoadList[2].CustomsBrokerForLoadList);

			shipment1.ConsigneePK = Guid.Empty;
			shipment1.ConsignorPK = Guid.Empty;
			shipment1.JS_OH_ExportBroker = Guid.Empty;
			shipment1.JS_OH_ImportBroker = Guid.Empty;
			shipment2.ConsigneePK = Guid.Empty;
			shipment2.ConsignorPK = Guid.Empty;
			shipment2.JS_OH_ExportBroker = Guid.Empty;
			shipment2.JS_OH_ImportBroker = Guid.Empty;

			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ConsignorAndConsignee", System.Environment.NewLine, ConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);

			ForwardingShipment masterShipment = Consol.Shipments.AddNew();
			ForwardingShipment subShipmentA = Factory.New<ForwardingShipment>();
			ForwardingShipment subShipmentB = Factory.New<ForwardingShipment>();

			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_HouseBill = "MASTER A";
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_HouseBill = "A";
			subShipmentA.JS_OuterPacks = 10;
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_HouseBill = "B";
			subShipmentB.JS_OuterPacks = 20;

			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			DocPackLinesCollection result = ConsolWrapper.PackLinesForLoadList;
			AssertEquals(4, result.Count);
			AssertEquals("Master shipment", result[3].GoodsDescription);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.CoLoadShipments.Add(subShipmentA);
			masterShipment.CoLoadShipments.Add(subShipmentB);
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			result = ConsolWrapper.PackLinesForLoadList;
			AssertEquals(4, result.Count);
		}

		public void TestPackLinesForLoadListMerging()
		{
			AssertEquals(0, ConsolWrapper.PackLinesForLoadList.Count);

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			PackLine line11 = shipment1.OuterPackLines.AddNew();
			PackLine unPacked11 = shipment1.OuterPackLines.AddNew();
			PackLine unPacked12 = shipment1.OuterPackLines.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			PackLine line21 = shipment2.OuterPackLines.AddNew();
			PackLine line22 = shipment2.OuterPackLines.AddNew();

			var container1 = (FreightContainer)Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";

			JobContainerPackPivot pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line11.PK;
			pivot1.J6_JC = container1.PK;

			JobContainerPackPivot pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line21.PK;
			pivot2.J6_JC = container1.PK;

			JobContainerPackPivot pivot3 = Factory.New<JobContainerPackPivot>();
			pivot3.J6_JL = line22.PK;
			pivot3.J6_JC = container1.PK;

			unPacked11.JL_Calc_ContainerNumber = ZString.Empty;
			unPacked12.JL_Calc_ContainerNumber = ZString.Empty;

			line11.JL_ContainerPackingOrder = 4;
			line21.JL_ContainerPackingOrder = 2;
			line22.JL_ContainerPackingOrder = 1;
			unPacked11.JL_ContainerPackingOrder = 0;
			unPacked12.JL_ContainerPackingOrder = 0;

			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = true;
			documentCommonConsol.IncludeUnPacked = true;
			ConsolWrapper = DocForwardingConsol.New(documentCommonConsol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(3, ConsolWrapper.PackLinesForLoadList.Count);

			ConsolWrapper.SetReportNameForTesting("Anything DetAiLed");
			DocPackLinesCollection result = ConsolWrapper.PackLinesForLoadList;
			AssertEquals(5, ConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals(0, result[0].ContainerPackingOrder);
			AssertEquals(0, result[1].ContainerPackingOrder);
			AssertEquals(1, result[2].ContainerPackingOrder);
			AssertEquals(2, result[3].ContainerPackingOrder);
			AssertEquals(4, result[4].ContainerPackingOrder);
		}

		#endregion

		#region Delivery Agent Test

		public void TestDeliveryAgentAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			AssertNull("DeliveryAgentAddress is Null.", consolWrapper.DeliveryAgentAddress);

			var testDeliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			testDeliveryAgent.OH_FullName = "Delivery Agent Ltd.";
			testDeliveryAgent.MainAddress.OA_Address1 = "333 Three Street";
			testDeliveryAgent.MainAddress.OA_Address2 = "Building C";
			testDeliveryAgent.MainAddress.OA_City = "CITY";
			consolWrapper.SetDeliveryAgent(testDeliveryAgent);

			AssertEquals("Delivery Agent Name", testDeliveryAgent.OH_FullName, consolWrapper.DeliveryAgentAddress.CompanyName);
			AssertEquals("Delivery Agent Address1", testDeliveryAgent.MainAddress.OA_Address1, consolWrapper.DeliveryAgentAddress.Address1);
			AssertEquals("Delivery Agent Address2", testDeliveryAgent.MainAddress.OA_Address2, consolWrapper.DeliveryAgentAddress.Address2);
			AssertEquals("Delivery Agent Address2", testDeliveryAgent.MainAddress.OA_City, consolWrapper.DeliveryAgentAddress.City);
		}

		const string ContainerNumber1 = "CONTC";
		const string ContainerNumber2 = "CONTB";
		const string ContainerNumber3 = "CONTA";

		public void TestDeliveryAgentContainers()
		{
			var shipment1 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment2 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment3 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment4 = (CommonShipment)Consol.Shipments.AddNew();

			var testDeliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			ConsolWrapper.SetDeliveryAgent(testDeliveryAgent);

			var testReceivingForwarder = Factory.New<OrgHeader>();
			Consol.SetDefaultReceivingForwarderAddress(testReceivingForwarder);

			shipment1.JS_IsForwardRegistered = ZBool.True;
			shipment2.JS_IsForwardRegistered = ZBool.True;
			shipment3.JS_IsForwardRegistered = ZBool.True;
			shipment4.JS_IsForwardRegistered = ZBool.True;

			shipment1.JS_OH_DeliveryAgent = testDeliveryAgent.PK;
			shipment2.JS_OH_DeliveryAgent = testReceivingForwarder.PK;
			shipment3.JS_OH_DeliveryAgent = testDeliveryAgent.PK;

			AssertEquals("Delivery Agent Containers is empty", 0, ConsolWrapper.DeliveryAgentContainers.Count);

			FreightContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			FreightContainer container3 = Consol.Containers.AddNew();
			container3.JC_ContainerNum = ContainerNumber3;

			PackLine line1 = shipment1.OuterPackLines.AddNew();
			PackLine line2 = shipment1.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line2.JL_PackageCount = 20;
			line1.SetContainer(Consol, container1);
			AssertEquals("Shipment 1 Container Number on Line1", ContainerNumber1, line1.GetContainer(Consol).JC_ContainerNum);
			line2.SetContainer(Consol, container3);

			AssertEquals("Shipment 1 Container Number on Line1", ContainerNumber1, line1.GetContainer(Consol).JC_ContainerNum);
			AssertEquals("Shipment 1 Container Number on Line2", ContainerNumber3, line2.GetContainer(Consol).JC_ContainerNum);

			PackLine line3 = shipment2.OuterPackLines.AddNew();
			line3.JL_PackageCount = 10;
			line3.SetContainer(Consol, container2);

			AssertEquals("Delivery Agent Containers count is two", 2, ConsolWrapper.DeliveryAgentContainers.Count);
			AssertEquals("Container number 3 should be first container in list", ContainerNumber3, ConsolWrapper.DeliveryAgentContainers[0].ContainerNumber);
			AssertEquals("Container number 1 should be second container in list", ContainerNumber1, ConsolWrapper.DeliveryAgentContainers[1].ContainerNumber);

			PackLine line4 = shipment3.OuterPackLines.AddNew();
			line4.JL_PackageCount = 10;
			line4.SetContainer(Consol, container3);

			AssertEquals("Delivery Agent Containers count is two", 2, ConsolWrapper.DeliveryAgentContainers.Count);
			AssertEquals("Container number 3 should be first container in list", ContainerNumber3, ConsolWrapper.DeliveryAgentContainers[0].ContainerNumber);
			AssertEquals("Container number 1 should be second container in list", ContainerNumber1, ConsolWrapper.DeliveryAgentContainers[1].ContainerNumber);
		}

		#endregion

		#region Export Receival Adivce Methods

		public void TestFinalDestinationForERA()
		{
			AssertEquals("Final destination for ERA is empty", ZString.Empty, ConsolWrapper.FinalDestinationForERA);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());

			Consol.JK_RL_NKDischargePort = uNLOCO.RL_Code;
			AssertEquals("Final destination for ERA", uNLOCO.RL_PortName, ConsolWrapper.FinalDestinationForERA);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Final destination for ERA", uNLOCO.RL_PortName, ConsolWrapper.FinalDestinationForERA);

			var shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = ZString.Empty;
			AssertEquals("Final destination for ERA - Shipment dest null shouldnt throw exception", uNLOCO.RL_PortName, ConsolWrapper.FinalDestinationForERA);

			Consol.JK_RL_NKDischargePort = ZString.Empty;
			shipment.JS_RL_NKDestination = uNLOCO.RL_Code;
			AssertEquals("Final destination for ERA", uNLOCO.RL_PortName, ConsolWrapper.FinalDestinationForERA);
		}

		public void TestGoodsDescriptionForERA()
		{
			var expectedDefaultResult = "Freight All Kinds";
			AssertEquals("Goods desc for ERA", expectedDefaultResult, ConsolWrapper.GoodsDescriptionForERA);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Goods desc for ERA", expectedDefaultResult, ConsolWrapper.GoodsDescriptionForERA);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("Goods desc for ERA", expectedDefaultResult, ConsolWrapper.GoodsDescriptionForERA);

			//			Shipment.JS_GoodsDescription = "Short des";
			//			AssertEquals("Goods desc for ERA", Shipment.JS_GoodsDescription, ConsolWrapper.GoodsDescriptionForERA);

			var detailedDescription = shipment.Notes.AddNew();
			detailedDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			detailedDescription.ST_ParentID = shipment.PK;
			detailedDescription.ST_Table = shipment.TableName;
			detailedDescription.ST_NoteDataAsText = "Detailed goods description\nLine Two5 123467890 1234567890 1234567980 12345";
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("Goods desc for ERA", "Detailed goods description Line Two5 123467890 1234567890 12345679", ConsolWrapper.GoodsDescriptionForERA);
		}

		public void TestExportReceivalInstructionsForERA()
		{
			AssertEquals("Export receival instructions for ERA is blank", ZString.Empty, ConsolWrapper.ExportReceivalInstructionsForERA);

			var detailedDescription = Consol.Notes.AddNew();
			detailedDescription.ST_Description = PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks.Description;
			detailedDescription.ST_ParentID = Consol.PK;
			detailedDescription.ST_Table = Consol.TableName;
			detailedDescription.ST_NoteDataAsText = "Export receival instructions";
			Factory.Save();
			AssertEquals("Export receival instructions for ERA", "Export receival instructions", ConsolWrapper.ExportReceivalInstructionsForERA);

			detailedDescription.ST_NoteDataAsText = "1234657980\n1234567890 1234567890 1234567890\n1234567890 1234567890 1234567890";
			AssertEquals("Export receival instructions for ERA", "1234657980 1234567890 1234567890 1234567890 1234567890 1234567890 ", ConsolWrapper.ExportReceivalInstructionsForERA);
		}

		public void TestHazardousShipmentForERA()
		{
			AssertEquals("!IsHazardousShipmentForERA", false, ConsolWrapper.HazardousShipmentForERA);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("!IsHazardousShipmentForERA", false, ConsolWrapper.HazardousShipmentForERA);

			var shipment = Consol.Shipments.AddNew();
			var line1 = (PackLine)shipment.OuterPackLines.AddNew();
			var line2 = (PackLine)shipment.OuterPackLines.AddNew();
			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("!IsHazardousShipmentForERA", false, ConsolWrapper.HazardousShipmentForERA);

			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("IsHazardousShipmentForERA", true, ConsolWrapper.HazardousShipmentForERA);

			Consol.JK_ConsolMode = "";
			AssertEquals("IsHazardousShipmentForERA", true, ConsolWrapper.HazardousShipmentForERA);

			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("!IsHazardousShipmentForERA", false, ConsolWrapper.HazardousShipmentForERA);
		}

		public void TestShipmentCustomsEntryNumber()
		{
			AssertEquals("Shipment ECN for ERA is empty", ZString.Empty, ConsolWrapper.ShipmentCustomsEntryNumber);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Shipment ECN for ERA is empty", ZString.Empty, ConsolWrapper.ShipmentCustomsEntryNumber);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("Shipment ECN for ERA is empty", ZString.Empty, ConsolWrapper.ShipmentCustomsEntryNumber);

			shipment.CustomsEntryNumber = "TestECN";
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Shipment ECN", "TestECN", ConsolWrapper.ShipmentCustomsEntryNumber);

			shipment.CustomsEntryNumber = "TestCAN";
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CAN;
			AssertEquals("Shipment CAN", "TestCAN", ConsolWrapper.ShipmentCustomsEntryNumber);
		}

		public void TestShipmentMarksAndNumbsForERA()
		{
			AssertEquals("Shipment marks and numbers for ERA", "", ConsolWrapper.ShipmentMarksAndNumbsForERA);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Shipment marks and numbers for ERA", "", ConsolWrapper.ShipmentMarksAndNumbsForERA);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("Shipment marks and numbers for ERA", "", ConsolWrapper.ShipmentMarksAndNumbsForERA);

			var marksAndNumberNote1 = shipment.Notes.AddNew();
			marksAndNumberNote1.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumberNote1.ST_ParentID = shipment.PK;
			marksAndNumberNote1.ST_Table = shipment.TableName;
			marksAndNumberNote1.ST_NoteDataAsText = "Marks and numbers";
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Shipment marks and numbers for ERA", "Marks and numbers", ConsolWrapper.ShipmentMarksAndNumbsForERA);

			marksAndNumberNote1.ST_NoteDataAsText = "123467890 1234657890 123467890\n1234657890 123467890 1234657890 123467890 1234657890\n123467890 1234657890 123467890 1234657890 123467890\n1234657890";
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Shipment marks and numbers for ERA", "123467890 1234657890 123467890 1234657890 123467890 1234657890 123467890 1234657890 123467890 1234657890 123467890 12346578", ConsolWrapper.ShipmentMarksAndNumbsForERA);
		}

		#endregion

		#region Customs Entry Number

		public void TestIsIceland()
		{
			RefCountry oldCountry = GlbCompany.CurrentCompany.Country;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
				Assert(ConsolWrapper.IsIceland);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				Assert(!ConsolWrapper.IsIceland);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}
		}

		public void TestECNCount()
		{
			AssertEquals("ECN Count", 0, ConsolWrapper.ECNCount);

			var shipment1 = Consol.Shipments.AddNew();
			var shipment2 = Consol.Shipments.AddNew();

			var shipment3 = Consol.Shipments.AddNew();
			shipment3.CustomsEntryNumber = "Test";
			shipment3.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;

			var shipment4 = Consol.Shipments.AddNew();

			AssertEquals("Shipments with ECN Count", 1, ConsolWrapper.ECNCount);

			shipment1.CustomsEntryNumber = "Test";
			shipment1.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Shipments with ECN Count", 2, ConsolWrapper.ECNCount);
		}

		public void TestCRNECN()
		{
			AssertEquals("CRN", ZString.Empty, ConsolWrapper.CRNECN);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ECN", ZString.Empty, ConsolWrapper.CRNECN);

			var shipment = Consol.Shipments.AddNew();
			shipment.CustomsEntryNumber = "Test";
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("ECN", "ECN: Test", ConsolWrapper.CRNECN);
		}

		public void TestCRNBasedOnAgentType()
		{
			AssertEquals("CRN Based on agent type is empty", ZString.Empty, ConsolWrapper.CRNBasedOnAgentType);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("CRN based on agent type", ZString.Empty, ConsolWrapper.CRNBasedOnAgentType);
		}

		public void TestShipmentPermitNumbers()
		{
			ZString savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

				AssertEquals("ShipmentECNs is empty", ZString.Empty, ConsolWrapper.ShipmentPermitNumbers);

				var shipment1 = Consol.Shipments.AddNew();
				var shipment2 = Consol.Shipments.AddNew();
				var shipment3 = Consol.Shipments.AddNew();
				var shipment4 = Consol.Shipments.AddNew();
				var shipment5 = Consol.Shipments.AddNew();

				shipment1.JS_RL_NKOrigin = "AUMEL";
				shipment1.JS_RL_NKDestination = "INBOM";
				shipment2.JS_RL_NKOrigin = "AUMEL";
				shipment2.JS_RL_NKDestination = "INBOM";
				shipment3.JS_RL_NKOrigin = "AUMEL";
				shipment3.JS_RL_NKDestination = "INBOM";
				shipment4.JS_RL_NKOrigin = "AUMEL";
				shipment4.JS_RL_NKDestination = "INBOM";
				shipment5.JS_RL_NKOrigin = "AUMEL";
				shipment5.JS_RL_NKDestination = "INBOM";

				shipment1.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;

				shipment2.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
				shipment2.CustomsEntryNumber = "ECN9999999";

				shipment3.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;

				shipment4.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
				shipment4.CustomsEntryNumber = "ECN2222222";

				shipment5.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
				shipment5.CustomsEntryNumber = "ECN3333333";

				Assert(!ConsolWrapper.ShipmentPermitNumbers.Contains("CRN"));
				Assert(ConsolWrapper.ShipmentPermitNumbers.Contains("ECN: ECN9999999"));
				Assert(ConsolWrapper.ShipmentPermitNumbers.Contains("EX1"));
				Assert(!ConsolWrapper.ShipmentPermitNumbers.Contains("EX1:"));
				Assert(ConsolWrapper.ShipmentPermitNumbers.Contains("ECN: ECN2222222"));
				Assert(ConsolWrapper.ShipmentPermitNumbers.Contains("ECN: ECN3333333"));
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = savedCountry;
			}
		}

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
				co.MainAddress.OA_Address1 = "11111";
				co.MainAddress.OA_Address2 = "2222";
				co.MainAddress.OA_City = "Rej";
				co.MainAddress.OA_State = "3333";
				co.OH_FullName = "asdf";
				co.MainAddress.OA_Fax = "444444";
				co.MainAddress.OA_PostCode = "55";
				OrgCusCode cusCode = co.CustomsCodes.AddNew(OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, "ASDFG", icelandCountry);

				RefUNLOCO iSZZZ = Factory.NewWithValidTestData<RefUNLOCO>();
				iSZZZ.RL_Code = "ISZZZ";
				RefLocoMap map = iSZZZ.RefLocoMaps.AddNew();
				map.RY_RN = icelandCountry.PK;
				map.RY_SystemUsage = ISLocoMapSystemUsageList.Codes.CustomsOfficeCode;
				map.RY_LocalPortCode = "ASDFG";

				ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
				ConsolWrapper = DocForwardingConsol.New(consol, Factory);
				AssertNull(ConsolWrapper.CustomsHouse);

				consol.JK_RL_NKLoadPort = "ISZZZ";
				consol.JK_RL_NKDischargePort = "AUSYD";
				ConsolWrapper = DocForwardingConsol.New(consol, Factory);
				AssertNotNull(ConsolWrapper.CustomsHouse);
				AssertEquals(co.MainAddress.OA_Fax, ConsolWrapper.CustomsHouse.Fax);
				AssertEquals(co.OH_FullName, ConsolWrapper.CustomsHouse.CompanyName);

				OrgAddress cocAddress = co.Addresses.AddNew();
				cocAddress.OA_CompanyNameOverride = "vfrt";
				cocAddress.OA_Fax = "7777777";
				cusCode.OK_OA_PremisesAddress = cocAddress.PK;
				ConsolWrapper = DocForwardingConsol.New(consol, Factory);
				AssertNotNull(ConsolWrapper.CustomsHouse);
				AssertEquals(cocAddress.OA_Fax_Formatted, ConsolWrapper.CustomsHouse.Fax);
				AssertEquals(cocAddress.OA_CompanyNameOverride, ConsolWrapper.CustomsHouse.CompanyName);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}
		}

		public void TestCRNCarrierNumberPrefix()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			FreightDataRegistry.Instance.ForwarderSendingarnumerCodeForAirfreight.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "W");
			ConsolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals(FreightDataRegistry.Instance.ForwarderSendingarnumerCodeForAirfreight.Value, ConsolWrapper.CRNCarrierNumberPrefix);
		}

		public void TestIcelandConsolCRN()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_CRN = "S-D61-8735-8-DF-RTL-9U59-1";
			ConsolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("S-D61-8735-8-DF-RTL-9U59-1", ConsolWrapper.ConsolCRNWithSpaces);
			AssertEquals("S-D61-8735-8-DF-RTL-9U59-1", ConsolWrapper.ConsolCRNWOCheckDigit);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			consol.JK_CRN = "S-D61-8735-8-DF-RTL-9U59-1";
			ConsolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("S D61 8735 8 DF RTL 9U59", ConsolWrapper.ConsolCRNWithSpaces);
			AssertEquals("S-D61-8735-8-DF-RTL-9U59", ConsolWrapper.ConsolCRNWOCheckDigit);
		}
		#endregion

		#region Headings

		public void TestHeadingTransportMode()
		{
			AssertEquals("Transport is empty", ZString.Empty, ConsolWrapper.HeadingTransportMode);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport mode SEA, should print 'Sea'", "S" + "ea".ToLower(), ConsolWrapper.HeadingTransportMode);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport mode SEA, should print 'Air'", "A" + "ir".ToLower(), ConsolWrapper.HeadingTransportMode);
		}

		public void TestFreightDepotHeading()
		{
			AssertEquals("Default freight depot heading", "FREIGHT DEPOT", ConsolWrapper.FreightDepotHeading);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Depot heading", "WHARF", ConsolWrapper.FreightDepotHeading);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Depot heading", "WHARF", ConsolWrapper.FreightDepotHeading);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Depot heading", "WHARF", ConsolWrapper.FreightDepotHeading);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Depot heading", "FREIGHT DEPOT", ConsolWrapper.FreightDepotHeading);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Other;
			AssertEquals("Depot heading", "FREIGHT DEPOT", ConsolWrapper.FreightDepotHeading);
		}

		public void TestECNHeading()
		{
			AssertEquals("ECN heading is blank", ZString.Empty, ConsolWrapper.ECNHeading);

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForTheUS();
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForCurrentCountry();

			AssertEquals("ECN Heading is blank as it is an import consol", ZString.Empty, ConsolWrapper.ECNHeading);

			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForCurrentCountry();
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForTheUS();

			AssertEquals("ECN Heading not blank as it is an export consol", "ECN: ", ConsolWrapper.ECNHeading);
		}

		public void TestOriginatingAgentHeading()
		{
			AssertEquals("Originating agent heading default", "ORIGINATING AGENT", ConsolWrapper.OriginatingAgentHeading);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Originating agent heading", "CONSIGNOR", ConsolWrapper.OriginatingAgentHeading);

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			AssertEquals("Originating agent heading", "ORIGINATING AGENT", ConsolWrapper.OriginatingAgentHeading);
		}

		public void TestDestinationAgentHeading()
		{
			AssertEquals("Originating agent heading default", "DESTINATION AGENT", ConsolWrapper.DestinationAgentHeading);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Originating agent heading", "CONSIGNEE", ConsolWrapper.DestinationAgentHeading);

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			AssertEquals("Originating agent heading", "DESTINATION AGENT", ConsolWrapper.DestinationAgentHeading);
		}

		public void TestSCACHeading()
		{
			AssertEquals("SCAC heading is empty", ZString.Empty, ConsolWrapper.SCACHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("SCAC heading is empty", ZString.Empty, ConsolWrapper.SCACHeading);

			var uSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "US"));
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;

			AssertEquals("SCAC heading", "SCAC", ConsolWrapper.SCACHeading);
		}

		public void TestContainerSealNumberHeading()
		{
			AssertEquals("Container Seal Number", ZString.Empty, ConsolWrapper.ContainerSealNumberHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Container Seal Number", "Rate Class", ConsolWrapper.ContainerSealNumberHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Container Seal Number", "Seal No.", ConsolWrapper.ContainerSealNumberHeading);
		}

		public void TestLastForeignPortHeading()
		{
			AssertEquals("Last foreign port heading is empty", "", ConsolWrapper.LastForeignPortHeading);

			var uSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "US"));

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;

			AssertEquals("Last foreign port heading is empty", "", ConsolWrapper.LastForeignPortHeading);

			Consol.JK_RL_NKLastForeignPort = uSUNLOCO.RL_Code;
			AssertEquals("Last foreign port is not empty", "LAST FOREIGN PORT", ConsolWrapper.LastForeignPortHeading);
		}

		public void TestDepartureReferenceHeading()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", ConsolWrapper.DepartureReferenceHeading);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("", ConsolWrapper.DepartureReferenceHeading);

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = sailing.PK;
			AssertEquals("", ConsolWrapper.DepartureReferenceHeading);

			origin.JA_DepartReference = "Origin Ref";
			destination.JB_ArrivalReference = "Arrival Ref";

			AssertEquals("DEPARTURE REFERENCE", ConsolWrapper.DepartureReferenceHeading);
		}

		#endregion

		#region Notes

		public void TestLoadListInstructions()
		{
			AssertEquals("Load list instructions are empty", ZString.Empty, ConsolWrapper.LoadListInstructions);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.LoadListInstructions.Description, "Load list instructions\nNext line");
			Factory.Save();
			AssertEquals("Load list instructions", "Load list instructions\nNext line", ConsolWrapper.LoadListInstructions);

			note.ST_NoteDataAsText = "Change note texts.\nNext line\n";
			AssertEquals("Load list instructions", "Change note texts.\nNext line", ConsolWrapper.LoadListInstructions);
		}

		public void TestExportReceivalInstructions()
		{
			AssertEquals("Export receival instructions is empty", ZString.Empty, ConsolWrapper.ExportReceivalInstructions);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks.Description, "Export receival advice\nNext line");
			Factory.Save();
			AssertEquals("Export receival instructions", "Export receival advice\nNext line", ConsolWrapper.ExportReceivalInstructions);

			note.ST_NoteDataAsText = "Export receival advice\nNext line\n";
			AssertEquals("Export receival instructions", "Export receival advice\nNext line", ConsolWrapper.ExportReceivalInstructions);
		}

		public void TestForwardingInstructionNotes()
		{
			AssertEquals("", ConsolWrapper.ForwardingInstructionNotes);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, "Please forward to shipping line\nNext line\n");
			Factory.Save();
			AssertEquals("Please forward to shipping line\nNext line", ConsolWrapper.ForwardingInstructionNotes);

			note.ST_NoteDataAsText = "New notes";
			AssertEquals("New notes", ConsolWrapper.ForwardingInstructionNotes);
		}

		public void TestDangerousGoodsHandlingInstruction()
		{
			AssertEquals("", ConsolWrapper.DangerousGoodsHandlingInstruction);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "contains very hazardous goods\nNext line\n");
			Factory.Save();
			AssertEquals("contains very hazardous goods\nNext line", ConsolWrapper.DangerousGoodsHandlingInstruction);
		}

		public void TestHandlingInstructions()
		{
			AssertEquals("", ConsolWrapper.HandlingInstructions);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "contains handling instruction");
			Factory.Save();
			AssertEquals("contains handling instruction", ConsolWrapper.HandlingInstructions);
		}

		public void TestCartageInstructions()
		{
			AssertEquals("", ConsolWrapper.CartageInstructions);

			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			FreightHelperClass.AddNote(Consol, pickupDesc, "Consol Pickup Instructions");
			FreightHelperClass.AddNote(Consol, deliveryDesc, "Consol Delivery Instructions");

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Consol Delivery Instructions", ConsolWrapper.CartageInstructions);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Consol Pickup Instructions", ConsolWrapper.CartageInstructions);
		}

		public void TestAgentNotes()
		{
			StmNote agentNotes = AddNotes(Consol, PredefinedNoteTypes.Instance.AgentNotes.Description, "Agent Notes Stuff\nLine Two");
			StmNote otherNotes = AddNotes(Consol, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");

			AssertEquals("Detailed goods description", "Agent Notes Stuff\nLine Two", ConsolWrapper.AgentNotes);
		}

		public void TestCarrierBookingRequestNotes()
		{
			StmNote carrierBookingRequestNotes = AddNotes(Consol, PredefinedNoteTypes.Instance.CarrierBookingRequest.Description, "Carrier Booking Request Notes Stuff\nLine Two");
			StmNote otherNotes = AddNotes(Consol, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");

			AssertEquals("Carrier Booking Request description", "Carrier Booking Request Notes Stuff\nLine Two", ConsolWrapper.CarrierBookingRequestNotes);
		}

		public void TestConsoleSpecialInstructions()
		{
			AssertEquals("", ConsolWrapper.ConsolSpecialInstructions);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Handle with care\r\nNext line\r\n");
			Factory.Save();
			AssertEquals(note.ST_NoteText, ConsolWrapper.ConsolSpecialInstructions);
		}

		public void TestAllNotes()
		{
			AssertEquals(0, ConsolWrapper.AllNotes.Length);

			StmNote note1 = AddNotes(Consol, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, "Please forward to shipping line\nNo, next line\n");
			StmNote note2 = AddNotes(Consol, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "More goods description here.\n");
			StmNote note3 = AddNotes(Consol, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "Internal notes testing\nThis is internal.\n");
			StmNote note4 = AddNotes(Consol, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Testing Marks And Numbers Notes\nSecond line in M&N Notes.\n");
			Factory.Save();

			ZString[] result = ConsolWrapper.AllNotes;
			AssertEquals(PredefinedNoteTypes.Instance.MarksAndNumbers.Description.ToUpper() + "  (" + note4.ST_NoteType_DescriptiveText + ")", result[0]);
			AssertEquals("Testing Marks And Numbers Notes", result[1]);
			AssertEquals("Second line in M&N Notes.", result[2]);
			AssertEquals("", result[3]);
			AssertEquals(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description.ToUpper() + "  (" + note2.ST_NoteType_DescriptiveText + ")", result[4]);
			AssertEquals("More goods description here.", result[5]);
			AssertEquals("", result[6]);
			AssertEquals(PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description.ToUpper() + "  (" + note1.ST_NoteType_DescriptiveText + ")", result[7]);
			AssertEquals("Please forward to shipping line", result[8]);
			AssertEquals("No, next line", result[9]);
			AssertEquals("", result[10]);
			AssertEquals(PredefinedNoteTypes.Instance.InternalWorkNotes.Description.ToUpper() + "  (" + note3.ST_NoteType_DescriptiveText + ")", result[11]);
			AssertEquals("Internal notes testing", result[12]);
			AssertEquals("This is internal.", result[13]);
		}

		public void TestDisplayPackingDetailsByContainer()
		{
			DocumentsDataRegistry.Instance.DisplayPackLinesByContainerOnConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ConsolWrapper.DisplayPackingDetailsByContainer);
			DocumentsDataRegistry.Instance.DisplayPackLinesByContainerOnConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ConsolWrapper.DisplayPackingDetailsByContainer);
		}

		#endregion

		#region Organisations
		public void TestInterimReceiptConsignee()
		{
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			Consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			AssertEquals("Should Return the Recieving Forwarder", receivingForwarder.OH_FullName, ConsolWrapper.InterimReceiptConsignee.Name);
			AssertEquals("Should return type DocOrganisation", typeof(DocOrganisation), ConsolWrapper.InterimReceiptConsignee.GetType());
		}
		public void TestInterimReceiptConsignor()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			Consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			AssertEquals("Should Return the Sending Forwarder", sendingForwarder.OH_FullName, ConsolWrapper.InterimReceiptConsignor.Name);
			AssertEquals("Should return type DocOrganisation", typeof(DocOrganisation), ConsolWrapper.InterimReceiptConsignor.GetType());
		}
		#endregion

		#region AirwayBill Fields

		public void TestConsoleETA()
		{
			AssertEquals(ZDateTime.Empty, ConsolWrapper.ConsoleETA);
			Consol.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2004, 3, 6);
			AssertEquals(new ZDateTime(2004, 3, 6), ConsolWrapper.ConsoleETA);
		}

		public void TestConsoleETD()
		{
			AssertEquals(ZDateTime.Empty, ConsolWrapper.ConsoleETD);
			Consol.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2004, 3, 6);
			AssertEquals(new ZDateTime(2004, 3, 6), ConsolWrapper.ConsoleETD);
		}

		public void TestThreeCodeMasterBillNum()
		{
			Consol.JK_MasterBillNum = "12";
			AssertEquals("ThreeCodeMasterBillNum", ZString.Empty, ConsolWrapper.ThreeCodeMasterBillNum);

			Consol.JK_MasterBillNum = "123";
			AssertEquals("ThreeCodeMasterBillNum", ZString.Empty, ConsolWrapper.ThreeCodeMasterBillNum);

			Consol.JK_MasterBillNum = "123 123123123";
			AssertEquals("ThreeCodeMasterBillNum", "123", ConsolWrapper.ThreeCodeMasterBillNum);
		}

		public void TestLastEightLetterMasterBillNum()
		{
			AssertEquals("LastEightLetterMasterBillNum", ZString.Empty, ConsolWrapper.LastEightLetterMasterBillNum);

			Consol.JK_MasterBillNum = "1 2-3 ";
			AssertEquals("LastEightLetterMasterBillNum", ZString.Empty, ConsolWrapper.LastEightLetterMasterBillNum);

			Consol.JK_MasterBillNum = "123 12345678";
			AssertEquals("LastEightLetterMasterBillNum", "12345678", ConsolWrapper.LastEightLetterMasterBillNum);

			Consol.JK_MasterBillNum = "123-12345678";
			AssertEquals("LastEightLetterMasterBillNum", "12345678", ConsolWrapper.LastEightLetterMasterBillNum);
		}

		public void TestFlight1Discharge()
		{
			AssertEquals("Flight 1 discharge is empty", ZString.Empty, ConsolWrapper.Flight1Discharge);

			var transport = Consol.Transports.AddNew();
			AssertEquals("Flight 1 discharge is empty", ZString.Empty, ConsolWrapper.Flight1Discharge);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL1";
			AssertEquals("Flight 1 discharge is empty", ZString.Empty, ConsolWrapper.Flight1Discharge);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals("Flight 1 discharge", "SYD", ConsolWrapper.Flight1Discharge);
		}

		public void TestFlight2Discharge()
		{
			AssertEquals("Flight 2 discharge is empty", ZString.Empty, ConsolWrapper.Flight2Discharge);

			var transport = Consol.Transports.AddNew();
			AssertEquals("Flight 2 discharge is empty", ZString.Empty, ConsolWrapper.Flight2Discharge);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL2";
			AssertEquals("Flight 2 discharge is empty", ZString.Empty, ConsolWrapper.Flight2Discharge);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals("Flight 2 discharge", "SYD", ConsolWrapper.Flight2Discharge);
		}

		public void TestFlight3Discharge()
		{
			AssertEquals("Flight 3 discharge is empty", ZString.Empty, ConsolWrapper.Flight3Discharge);

			var transport = Consol.Transports.AddNew();
			AssertEquals("Flight 3 discharge is empty", ZString.Empty, ConsolWrapper.Flight3Discharge);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL3";
			AssertEquals("Flight 3 discharge is empty", ZString.Empty, ConsolWrapper.Flight3Discharge);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals("Flight 3 discharge", "SYD", ConsolWrapper.Flight3Discharge);
		}

		public void TestRequestedFlightDate1()
		{
			AssertEquals("RequestedFlightDate1", ZString.Empty, ConsolWrapper.RequestedFlightDate1);

			var transport = Consol.Transports.AddNew();
			AssertEquals("RequestedFlightDate1 is empty", ZString.Empty, ConsolWrapper.RequestedFlightDate1);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL1";
			AssertEquals("RequestedFlightDate1 is empty", ZString.Empty, ConsolWrapper.RequestedFlightDate1);

			transport.JW_VoyageFlight = "FLY 333";
			AssertEquals("Requested flight date 1", "FLY 333", ConsolWrapper.RequestedFlightDate1);

			transport.JW_ETD = Env.Time.CurrentLocalDate;
			AssertEquals("Requested flight date 1", "FLY 333/" + Env.Time.CurrentLocalDate.Day, ConsolWrapper.RequestedFlightDate1);
		}

		public void TestRequestedFlightDate2()
		{
			AssertEquals("RequestedFlightDate2", ZString.Empty, ConsolWrapper.RequestedFlightDate2);

			var transport = Consol.Transports.AddNew();
			AssertEquals("RequestedFlightDate2 is empty", ZString.Empty, ConsolWrapper.RequestedFlightDate2);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL2";
			AssertEquals("RequestedFlightDate2 is empty", ZString.Empty, ConsolWrapper.RequestedFlightDate2);

			transport.JW_VoyageFlight = "FLY 333";
			AssertEquals("RequestedFlightDate2", "FLY 333", ConsolWrapper.RequestedFlightDate2);

			transport.JW_ETD = Env.Time.CurrentLocalDate;
			AssertEquals("RequestedFlightDate2", "FLY 333/" + Env.Time.CurrentLocalDate.Day, ConsolWrapper.RequestedFlightDate2);
		}

		public void TestAirlineTwoLetterPrefix()
		{
			AssertEquals("AirlineTwoLetterPrefix", ZString.Empty, ConsolWrapper.AirlineTwoLetterPrefix);

			var transport = Consol.Transports.AddNew();
			AssertEquals("AirlineTwoLetterPrefix is empty", ZString.Empty, ConsolWrapper.AirlineTwoLetterPrefix);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL2";
			AssertEquals("AirlineTwoLetterPrefix is empty", ZString.Empty, ConsolWrapper.AirlineTwoLetterPrefix);

			transport.JW_VoyageFlight = "QF123";
			AssertEquals("AirlineTwoLetterPrefix is empty", "QF", ConsolWrapper.AirlineTwoLetterPrefix);
		}

		public void TestFlight3AirlineTwoLetterPrefix()
		{
			AssertEquals("Flight3AirlineTwoLetterPrefix is empty", ZString.Empty, ConsolWrapper.Flight3AirlineTwoLetterPrefix);

			var transport = Consol.Transports.AddNew();
			AssertEquals("Flight3AirlineTwoLetterPrefix is empty", ZString.Empty, ConsolWrapper.Flight3AirlineTwoLetterPrefix);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL3";
			AssertEquals("Flight3AirlineTwoLetterPrefix is empty", ZString.Empty, ConsolWrapper.Flight3AirlineTwoLetterPrefix);

			transport.JW_VoyageFlight = "QF123";
			AssertEquals("Flight3AirlineTwoLetterPrefix is empty", "QF", ConsolWrapper.Flight3AirlineTwoLetterPrefix);
		}

		public void TestFirstCarrier()
		{
			AssertEquals("First carrier is empty", ZString.Empty, ConsolWrapper.FirstCarrier);

			var transport = Consol.Transports.AddNew();
			AssertEquals("First carrier is empty", ZString.Empty, ConsolWrapper.FirstCarrier);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL1";
			AssertEquals("First carrier is empty", ZString.Empty, ConsolWrapper.FirstCarrier);

			var airLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.NotEqual, ""));
			transport.JW_VoyageFlight = airLine.RM_TwoCharacterCode + "123";
			AssertEquals("First carrier", airLine.RM_AirlineName1.ToUpper(), ConsolWrapper.FirstCarrier);
		}

		public void TestSecondCarrier()
		{
			AssertEquals("Second carrier is empty", ZString.Empty, ConsolWrapper.SecondCarrier);

			var transport = Consol.Transports.AddNew();
			AssertEquals("Second carrier is empty", ZString.Empty, ConsolWrapper.SecondCarrier);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL2";
			AssertEquals("Second carrier is empty", ZString.Empty, ConsolWrapper.SecondCarrier);

			var airLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.NotEqual, ""));
			transport.JW_VoyageFlight = airLine.RM_TwoCharacterCode + "123";
			AssertEquals("Second carrier", airLine.RM_AirlineName1.ToUpper(), ConsolWrapper.SecondCarrier);
		}

		public void TestThirdCarrier()
		{
			AssertEquals("Third carrier is empty", ZString.Empty, ConsolWrapper.ThirdCarrier);

			var transport = Consol.Transports.AddNew();
			AssertEquals("Third carrier is empty", ZString.Empty, ConsolWrapper.ThirdCarrier);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = "FL3";
			AssertEquals("Third carrier is empty", ZString.Empty, ConsolWrapper.ThirdCarrier);

			var airLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.NotEqual, ""));
			transport.JW_VoyageFlight = airLine.RM_TwoCharacterCode + "123";
			AssertEquals("Third carrier", airLine.RM_AirlineName1.ToUpper(), ConsolWrapper.ThirdCarrier);
		}

		public void TestCHGSCode()
		{
			AssertEquals("CHGSCode is empty", ZString.Empty, ConsolWrapper.CHGSCode);

			Consol.JK_PrepaidCollect = "1";
			AssertEquals("CHGSCode is empty", ZString.Empty, ConsolWrapper.CHGSCode);

			Consol.JK_PrepaidCollect = "12";
			AssertEquals("CHGSCode is empty", "12", ConsolWrapper.CHGSCode);

			Consol.JK_PrepaidCollect = "123";
			AssertEquals("CHGSCode is empty", "12", ConsolWrapper.CHGSCode);
		}

		public void TestApprovedExporterCode()
		{
			AssertEquals("ApprovedExporterCode is empty", ZString.Empty, ConsolWrapper.ApprovedExporterCode);

			var sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			AssertEquals("ApprovedExporterCode", sendingForwarder.OH_Code.ToUpper(), ConsolWrapper.ApprovedExporterCode);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ApprovedExporterCode is empty", ZString.Empty, ConsolWrapper.ApprovedExporterCode);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("ApprovedExporterCode is empty", ZString.Empty, ConsolWrapper.ApprovedExporterCode);

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("ApprovedExporterCode is empty", consignor.OH_Code.ToUpper(), ConsolWrapper.ApprovedExporterCode);

			consignor.CountryData.OV_EXExportPermissionDetails = "Exe";
			AssertEquals("ApprovedExporterCode", "EXE", ConsolWrapper.ApprovedExporterCode);
		}

		public void TestApprovedImporterCode()
		{
			AssertEquals("ApprovedImporterCode is empty", ZString.Empty, ConsolWrapper.ApprovedImporterCode);

			var receivingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			AssertEquals("ApprovedImporterCode", receivingForwarder.OH_Code.ToUpper(), ConsolWrapper.ApprovedImporterCode);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ApprovedImporterCode is empty", ZString.Empty, ConsolWrapper.ApprovedImporterCode);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("ApprovedImporterCode is empty", ZString.Empty, ConsolWrapper.ApprovedImporterCode);

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("ApprovedImporterCode is empty", consignor.OH_Code.ToUpper(), ConsolWrapper.ApprovedImporterCode);

			consignor.CountryData.OV_EXExportPermissionDetails = "Exe";
			AssertEquals("ApprovedImporterCode", "EXE", ConsolWrapper.ApprovedImporterCode);
		}

		public void TestMAWBHeadingOrFirst3Chars()
		{
			AssertEquals("MAWBHeadingOrFirst3Chars is empty", ZString.Empty, ConsolWrapper.MAWBHeadingOrFirst3Chars);
			Consol.JK_AgentType = Constants.AgentType.Direct;
			Consol.JK_MasterBillNum = "059 12345678";
			AssertEquals("MAWBHeadingOrFirst3Chars", "059", ConsolWrapper.MAWBHeadingOrFirst3Chars);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("MAWBHeadingOrFirst3Chars", "059", ConsolWrapper.MAWBHeadingOrFirst3Chars);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("MAWBHeadingOrFirst3Chars", "MASTER HAWB:", ConsolWrapper.MAWBHeadingOrFirst3Chars);

			Consol.JK_AgentType = Constants.AgentType.Other;
			AssertEquals("MAWBHeadingOrFirst3Chars", "", ConsolWrapper.MAWBHeadingOrFirst3Chars);
		}

		public void TestMAWBConsolNoOrLast8CharsOfMB()
		{
			AssertEquals("MAWBHeadingOrFirst3Chars", "", ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_AgentType = Constants.AgentType.Direct;
			Consol.JK_MasterBillNum = "059 12345678";
			AssertEquals("MAWBHeadingOrFirst3Chars", "12345678", ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("MAWBHeadingOrFirst3Chars", "12345678", ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("MAWBHeadingOrFirst3Chars", ZString.Empty, ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_UniqueConsignRef = "ConNum";
			AssertEquals("MAWBHeadingOrFirst3Chars", "ConNum", ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_AgentType = Constants.AgentType.Other;
			AssertEquals("MAWBHeadingOrFirst3Chars", "", ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_UniqueConsignRef = ZString.Empty;
			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertEquals("MAWBHeadingOrFirst3Chars", ZString.Empty, ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);

			Consol.JK_UniqueConsignRef = "ConNum2";
			AssertEquals("MAWBHeadingOrFirst3Chars", "ConNum2", ConsolWrapper.MAWBConsolNoOrLast8CharsOfMB);
		}

		public void TestIssuingCarrierNameAndAddress()
		{
			AssertEquals("IssuingCarrierNameAndAddress is empty", ZString.Empty, ConsolWrapper.IssuingCarrierNameAndAddress);

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			var sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			DocOrganisation sendingForwarderWrapper = DocOrganisation.New(sendingForwarder, Factory);
			AssertEquals("IssuingCarrierNameAndAddress", sendingForwarderWrapper.PostalAddress, ConsolWrapper.IssuingCarrierNameAndAddress);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("IssuingCarrierNameAndAddress is empty", ZString.Empty, ConsolWrapper.IssuingCarrierNameAndAddress);

			Consol.JK_MasterBillNum = "123";
			AssertEquals("IssuingCarrierNameAndAddress is empty", ZString.Empty, ConsolWrapper.IssuingCarrierNameAndAddress);

			var airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.Equal, "059"));
			Consol.JK_MasterBillNum = airline.RM_EagleAddedAirlinePrefixOrAccountingCode + " 12312213";
			AssertEquals("IssuingCarrierNameAndAddress", airline.RM_AirlineName1.ToUpper() + "\nC/ MOLL VELL- 11A, PALMA  MALLORCA, SPAIN AND CANARY ISLANDS, 07012", ConsolWrapper.IssuingCarrierNameAndAddress);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(airline.RM_AirlineName1.ToUpper() + "\nC/ MOLL VELL- 11A, PALMA  MALLORCA, SPAIN AND CANARY ISLANDS, 07012", ConsolWrapper.IssuingCarrierNameAndAddress);
		}

		public void TestIssuingCarrierAgentName()
		{
			AssertEquals("IssuingCarrierAgentName", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName.ToUpper(), ConsolWrapper.IssuingCarrierAgentName);
		}

		public void TestIssuingCarrierAgentCity()
		{
			AssertEquals("IssuingCarrierAgentCity", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity.ToUpper(), ConsolWrapper.IssuingCarrierAgentCity);
		}

		public void TestIssuingCarrierAgentIATACode()
		{
			AssertEquals("IssuingCarrierAgentIATACode", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode.ToUpper(), ConsolWrapper.IssuingCarrierAgentIATACode);
		}

		public void TestIssuingCarrierAgentAccountNumber()
		{
			AssertEquals("IssuingCarrierAgentAccountNumber", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber.ToUpper(), ConsolWrapper.IssuingCarrierAgentAccountNumber);
		}

		public void TestShipperNameAndAddress()
		{
			AssertEquals("ShipperNameAndAddress", ZString.Empty, ConsolWrapper.ShipperNameAndAddress);

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			var sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			DocOrganisation sendingForwarderWrapper = DocOrganisation.New(sendingForwarder, Factory);
			AssertEquals("ShipperNameAndAddress", sendingForwarderWrapper.PostalAddress.ToUpper(), ConsolWrapper.ShipperNameAndAddress);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ShipperNameAndAddress", ZString.Empty, ConsolWrapper.ShipperNameAndAddress);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("ShipperNameAndAddress", ZString.Empty, ConsolWrapper.ShipperNameAndAddress);

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.NotEqual, sendingForwarder.OH_Code));
			shipment.ConsignorPK = consignor.PK;
			DocOrganisation consignorWrapper = DocOrganisation.New(consignor, Factory);
			AssertEquals("ShipperNameAndAddress", consignorWrapper.PostalAddress.ToUpper(), ConsolWrapper.ShipperNameAndAddress);
		}

		public void TestShipmentsInnerPacks()
		{
			AssertEquals("ShipmentsInnerPacks is empty", 0, ConsolWrapper.ShipmentsInnerPacks);

			var shipment1 = Consol.Shipments.AddNew();
			AssertEquals("ShipmentsInnerPacks is empty", 0, ConsolWrapper.ShipmentsInnerPacks);

			shipment1.JS_TotalPackageCount = 12;
			AssertEquals("ShipmentsInnerPacks", 12, ConsolWrapper.ShipmentsInnerPacks);

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_TotalPackageCount = 23;
			AssertEquals("ShipmentsInnerPacks", 35, ConsolWrapper.ShipmentsInnerPacks);
		}

		public void TestShipmentsWeightAndShipmentsWeightUnit()
		{
			AssertEquals("ShipmentsWeight is empty", 0M, ConsolWrapper.ShipmentsWeight);
			Assert("ShipmentsWeightUnit is not empty", ConsolWrapper.ShipmentsWeightUnit != ZString.Empty);

			var shipment1 = Consol.Shipments.AddNew();
			AssertEquals("ShipmentsWeight is empty", 0M, ConsolWrapper.ShipmentsWeight);
			Assert("ShipmentsWeightUnit is not empty", ConsolWrapper.ShipmentsWeightUnit != ZString.Empty);

			shipment1.JS_ActualWeight = 12.00M;
			AssertEquals("ShipmentsWeight", 12.00M, ConsolWrapper.ShipmentsWeight);
			Assert("ShipmentsWeightUnit is not empty", ConsolWrapper.ShipmentsWeightUnit != ZString.Empty);

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 23.00M;
			AssertEquals("ShipmentsWeight", 35.00M, ConsolWrapper.ShipmentsWeight);
			Assert("ShipmentsWeightUnit is not empty", ConsolWrapper.ShipmentsWeightUnit != ZString.Empty);
		}

		public void TestNatureAndQuantityOfGoods()
		{
			AssertEquals("NatureAndQuantityOfGoods", ZString.Empty, ConsolWrapper.NatureAndQuantityOfGoods);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("NatureAndQuantityOfGoods", ZString.Empty, ConsolWrapper.NatureAndQuantityOfGoods);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("NatureAndQuantityOfGoods", "Consolidation as per attached list".ToUpper(), ConsolWrapper.NatureAndQuantityOfGoods);

			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals("NatureAndQuantityOfGoods", ZString.Empty, ConsolWrapper.NatureAndQuantityOfGoods);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			var con1 = (FreightContainer)Consol.Containers.AddNew();
			con1.JC_ContainerNum = "Con1";
			AssertEquals("NatureAndQuantityOfGoods", "Con1".ToUpper(), ConsolWrapper.NatureAndQuantityOfGoods);

			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals("NatureAndQuantityOfGoods", "Con1".ToUpper(), ConsolWrapper.NatureAndQuantityOfGoods);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("NatureAndQuantityOfGoods", "Con1".ToUpper(), ConsolWrapper.NatureAndQuantityOfGoods);

			shipment.JS_GoodsDescription = "Desc";
			AssertEquals("NatureAndQuantityOfGoods", "Desc\nCon1".ToUpper(), ConsolWrapper.NatureAndQuantityOfGoods);
		}

		public void TestOriginPortCode3Char()
		{
			AssertEquals("OriginPortCode3Char", ZString.Empty, ConsolWrapper.OriginPortCode3Char);

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "1";
			AssertEquals("OriginPortCode3Char", ZString.Empty, ConsolWrapper.OriginPortCode3Char);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			transport.JW_RL_NKLoadPort = uNLOCO.RL_Code;
			AssertEquals("OriginPortCode3Char", uNLOCO.RL_Code.Substring(2), ConsolWrapper.OriginPortCode3Char);
		}

		public void TestSignatureOfIssuingCarrierOrItsAgent()
		{
			//ZString ExpectedResult = GlbStaffWithGroups.CurrentUser.GS_FullName + " " + GlbStaffWithGroups.CurrentUser.DangerousGoodsCertificateNumber;
			ZString expectedResult = GlbStaff.CurrentUser.GS_FullName + " " + GlbStaff.CurrentUser.DangerousGoodsCertificateNumber;
			AssertEquals("SignatureOfIssuingCarrierOrItsAgent", expectedResult, ConsolWrapper.SignatureOfIssuingCarrierOrItsAgent);
		}

		public void TestCurrentBranchCity()
		{
			AssertEquals("CurrentBranchCity", GlbBranch.CurrentBranch.GB_City, ConsolWrapper.CurrentBranchCity);
		}

		public void TestSignatureOfShipperOrHisAgent()
		{
			AssertEquals("SignatureOfShipperOrHisAgent", ConsolWrapper.IssuingCarrierAgentName, ConsolWrapper.SignatureOfShipperOrHisAgent);
		}

		public void TestShipmentsGoodsValue()
		{
			AssertEquals("ShipmentsGoodsValue", 0M, ConsolWrapper.ShipmentsGoodsValue);

			var shipment1 = Consol.Shipments.AddNew();
			AssertEquals("ShipmentsGoodsValue", 0M, ConsolWrapper.ShipmentsGoodsValue);

			shipment1.JS_GoodsValue = 123.00M;
			AssertEquals("ShipmentsGoodsValue", 123.00M, ConsolWrapper.ShipmentsGoodsValue);

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_GoodsValue = 27.00M;
			AssertEquals("ShipmentsGoodsValue", 150.00M, ConsolWrapper.ShipmentsGoodsValue);
		}

		public void TestConsolCurrency()
		{
			AssertEquals("ConsolCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ConsolWrapper.ConsolCurrency);
		}

		public void TestChargeableWeight()
		{
			AssertEquals("ChargeableWeight", 0M, ConsolWrapper.ChargeableWeight);

			var shipment = Consol.Shipments.AddNew();
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 5.3M;
			AssertEquals("ChargeableWeight", 5.3m, ConsolWrapper.ChargeableWeight);

			Consol.JK_OverrideConsolChargeable = true;
			Consol.JK_ConsolChargeable = 2.1M;
			AssertEquals("ChargeableWeight", 2.1M, ConsolWrapper.ChargeableWeight);
		}

		public void TestDestination()
		{
			AssertEquals("Destination", ZString.Empty, ConsolWrapper.Destination);

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "12";
			AssertEquals("Destination", ZString.Empty, ConsolWrapper.Destination);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			transport.JW_RL_NKDiscPort = uNLOCO.RL_Code;
			AssertEquals("Destination", uNLOCO.RL_Code.Substring(2), ConsolWrapper.Destination);
		}

		public void TestAirlineName()
		{
			AssertEquals("AirlineName", ZString.Empty, ConsolWrapper.AirlineName);

			Consol.JK_MasterBillNum = "12";
			AssertEquals("AirlineName", ZString.Empty, ConsolWrapper.AirlineName);

			var airline = RefAirline.LoadFromAirlinePrefix(Factory, "059");
			Consol.JK_MasterBillNum = airline.RM_EagleAddedAirlinePrefixOrAccountingCode + " 12312213";
			AssertEquals("AirlineName", airline.RM_AirlineName1.ToUpper(), ConsolWrapper.AirlineName);
		}

		public void TestOuterPacks()
		{
			ForwardingShipment shipment;

			shipment = Consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;
			shipment.JS_HouseBill = "123";

			shipment = Consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_HouseBill = "456";

			shipment = Consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 1;
			shipment.JS_HouseBill = "789";

			DocOuterPackCollection outerPacks = ConsolWrapper.OuterPacks;
			AssertEquals("Count", 13, outerPacks.Count);

			int i = 0;
			for (; i < 2; i++)
			{
				AssertEquals("house bill for first shipment", "123", outerPacks[i].Shipment.HouseBill);
			}

			for (; i < 12; i++)
			{
				AssertEquals("house bill for second shipment", "456", outerPacks[i].Shipment.HouseBill);
			}

			for (; i < 13; i++)
			{
				AssertEquals("house bill for third shipment", "789", outerPacks[i].Shipment.HouseBill);
			}

			for (i = 0; i < 13; i++)
			{
				AssertEquals("Pack number", i + 1, outerPacks[i].Number);
			}
		}

		public void TestCargoLabelOuterPacks()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocumentImportCargoLabel documentImportCargoLabel = new DocumentImportCargoLabel(consol);
			documentImportCargoLabel.NoOfLabelsToPrint = 13;
			DocForwardingConsol consolWrapperWithCargoLabel = DocForwardingConsol.New(documentImportCargoLabel, Factory);
			AssertNotNull("PreCondition: Valid DocForwardingConsol", consolWrapperWithCargoLabel);

			ForwardingShipment shipment;

			shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;
			shipment.JS_HouseBill = "123";

			shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_HouseBill = "456";

			shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 1;
			shipment.JS_HouseBill = "789";

			DocOuterPackCollection outerPacks = consolWrapperWithCargoLabel.CargoLabelOuterPacks;
			AssertEquals("Count", 13, outerPacks.Count);

			int i = 0;
			for (; i < 2; i++)
			{
				AssertEquals("house bill for first shipment", "123", outerPacks[i].Shipment.HouseBill);
			}

			for (; i < 12; i++)
			{
				AssertEquals("house bill for second shipment", "456", outerPacks[i].Shipment.HouseBill);
			}

			for (; i < 13; i++)
			{
				AssertEquals("house bill for third shipment", "789", outerPacks[i].Shipment.HouseBill);
			}

			for (i = 0; i < 13; i++)
			{
				AssertEquals("Pack number", i + 1, outerPacks[i].Number);
			}

			documentImportCargoLabel.NoOfLabelsToPrint = 6;
			consolWrapperWithCargoLabel = DocForwardingConsol.New(documentImportCargoLabel, Factory);
			AssertNotNull("PreCondition: Valid DocForwardingConsol", consolWrapperWithCargoLabel);
			outerPacks = consolWrapperWithCargoLabel.CargoLabelOuterPacks;
			AssertEquals("Count", 6, outerPacks.Count);
		}

		#endregion

		#region Collection Tests

		public void TestShipmentCollectionAllForwardingRegisteredWithDeliveryAgent()
		{
			var shipment1 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment2 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment3 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment4 = (CommonShipment)Consol.Shipments.AddNew();

			var testDeliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			ConsolWrapper.SetDeliveryAgent(testDeliveryAgent);

			var testReceivingForwarder = Factory.New<OrgHeader>();
			Consol.SetDefaultReceivingForwarderAddress(testReceivingForwarder);

			shipment1.JS_IsForwardRegistered = ZBool.True;
			shipment2.JS_IsForwardRegistered = ZBool.True;
			shipment3.JS_IsForwardRegistered = ZBool.True;
			shipment4.JS_IsForwardRegistered = ZBool.False;

			shipment1.JS_OH_DeliveryAgent = testDeliveryAgent.PK;
			shipment2.JS_OH_DeliveryAgent = testReceivingForwarder.PK;

			shipment1.JS_IsCFSRegistered = ZBool.False;
			shipment2.JS_IsCFSRegistered = ZBool.False;
			shipment3.JS_IsCFSRegistered = ZBool.True;
			shipment4.JS_IsCFSRegistered = ZBool.True;

			AssertEquals(1, ConsolWrapper.ForwardRegisteredShipments.Count);
			AssertEquals(1, ConsolWrapper.Shipments.Count);
			AssertEquals(1, ConsolWrapper.MasterShipments.Count);
			AssertEquals(1, ConsolWrapper.MasterAndSubShipments.Count);
		}

		public void TestShipmentCollectionAllForwardingRegisteredWithDeliveryAgent_FallbackOnConsolReceivingForwarder()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DeliveryAgentOrgHeader receivingForwarder = Factory.New<DeliveryAgentOrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();

			shipment1.JS_IsForwardRegistered = ZBool.True;
			shipment2.JS_IsForwardRegistered = ZBool.True;
			shipment3.JS_IsForwardRegistered = ZBool.True;
			shipment4.JS_IsForwardRegistered = ZBool.False;

			DeliveryAgentOrgHeader deliveryAgent1 = Factory.New<DeliveryAgentOrgHeader>();
			deliveryAgent1.OH_FullName = "Shipment 1 Delivery Agent";
			deliveryAgent1.MainAddress.OA_Address1 = "Address";

			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment2.JS_OH_DeliveryAgent = receivingForwarder.PK;

			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, GetWrappedShipments(consolWrapper.ForwardRegisteredShipments));

			consolWrapper.SetDeliveryAgent(deliveryAgent1);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, GetWrappedShipments(consolWrapper.ForwardRegisteredShipments));

			consolWrapper.SetDeliveryAgent(receivingForwarder);
			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, GetWrappedShipments(consolWrapper.ForwardRegisteredShipments));
		}

		CommonShipment[] GetWrappedShipments(DocForwardingShipmentCollection shipmentCollection)
		{
			return shipmentCollection.Cast<DocForwardingShipment>().Select((wrapper) => wrapper.CommonShipment).ToArray();
		}

		public void TestShipmentCollectionAllForwardingRegistered()
		{
			var shipment1 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment2 = (CommonShipment)Consol.Shipments.AddNew();
			var shipment3 = (CommonShipment)Consol.Shipments.AddNew();

			shipment1.JS_IsForwardRegistered = ZBool.True;
			shipment2.JS_IsForwardRegistered = ZBool.True;
			shipment3.JS_IsForwardRegistered = ZBool.False;

			shipment1.JS_IsCFSRegistered = ZBool.False;
			shipment2.JS_IsCFSRegistered = ZBool.True;
			shipment3.JS_IsCFSRegistered = ZBool.True;

			AssertEquals(2, ConsolWrapper.ForwardRegisteredShipments.Count);
			AssertEquals(2, ConsolWrapper.Shipments.Count);
			AssertEquals(2, ConsolWrapper.MasterShipments.Count);
			AssertEquals(2, ConsolWrapper.MasterAndSubShipments.Count);
		}

		public void TestPackLines()
		{
			AssertEquals(0, ConsolWrapper.PackLines.Count);
			var shipment1 = Consol.Shipments.AddNew();
			var line1 = shipment1.OuterPackLines.AddNew();
			line1.JL_PackageCount = 1;
			var shipment2 = Consol.Shipments.AddNew();
			var line2 = shipment2.OuterPackLines.AddNew();
			line2.JL_PackageCount = 1;

			var container1 = (FreightContainer)Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";

			var pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line1.PK;
			pivot1.J6_JC = container1.PK;

			var pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line2.PK;
			pivot2.J6_JC = container1.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(2, ConsolWrapper.PackLines.Count);
			AssertEquals("CurrentConsol on packline 1 is this Consol", ConsolWrapper, ConsolWrapper.PackLines[0].CurrentConsol);
			AssertEquals("CurrentConsol on packline 2 is this Consol", ConsolWrapper, ConsolWrapper.PackLines[1].CurrentConsol);

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_HouseBill = "MASTER A";
			var line3 = masterShipment.OuterPackLines.AddNew();
			line3.JL_PackageCount = 1;

			var subShipmentA = Factory.New<ForwardingShipment>();
			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_HouseBill = "A";
			var line4 = subShipmentA.OuterPackLines.AddNew();
			line4.JL_PackageCount = 1;
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_HouseBill = "B";
			var line5 = subShipmentB.OuterPackLines.AddNew();
			line5.JL_PackageCount = 1;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			var result = ConsolWrapper.PackLines;
			AssertEquals(3, result.Count);
			AssertEquals("Master shipment", result[2].GoodsDescription);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.CoLoadShipments.Add(subShipmentA);
			masterShipment.CoLoadShipments.Add(subShipmentB);
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			result = ConsolWrapper.PackLines;
			AssertEquals(3, result.Count);
		}

		public void TestLoadListTotalPackLineWeightAndVolume()
		{
			ForwardingContainer container1 = Consol.Containers.AddNew();
			ForwardingContainer container2 = Consol.Containers.AddNew();

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.SetContainer(Consol, container1);
			line1.JL_ActualWeight = 8;
			line1.JL_ActualWeightUQ = Constants.Weight.Ounces;
			line1.JL_ActualVolume = 864;
			line1.JL_ActualVolumeUQ = Constants.Volume.CubicInches;

			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.SetContainer(Consol, container2);
			line2.JL_ActualWeight = 3;
			line2.JL_ActualWeightUQ = Constants.Weight.Pounds;
			line2.JL_ActualVolume = 2;
			line2.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			DocumentCommonConsol dcc = new DocumentCommonConsol(Consol, Constants.DataContext.Consol);
			dcc.IncludeAllShipments = true;
			ConsolWrapper = DocForwardingConsol.New(dcc, Factory);

			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicFeet;
			Env.Registry.FreightWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Weight", 3.5m, ConsolWrapper.LoadListTotalPackLineWeight);
			AssertEquals("Weight Unit", Constants.Weight.Pounds, ConsolWrapper.LoadListTotalPackLineWeightUnit);
			AssertEquals("Volume", 2.5m, ConsolWrapper.LoadListTotalPackLineVolume);
			AssertEquals("Volume Unit", Constants.Volume.CubicFeet, ConsolWrapper.LoadListTotalPackLineVolumeUnit);

			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;

			AssertEquals("Weight", 1.588m, ConsolWrapper.LoadListTotalPackLineWeight.Round(3));
			AssertEquals("Weight Unit", Constants.Weight.Kilograms, ConsolWrapper.LoadListTotalPackLineWeightUnit);
			AssertEquals("Volume", 0.071m, ConsolWrapper.LoadListTotalPackLineVolume.Round(3));
			AssertEquals("Volume Unit", Constants.Volume.CubicMetres, ConsolWrapper.LoadListTotalPackLineVolumeUnit);
		}

		public void TestPackLinesUseCorrectUnits()
		{
			ForwardingContainer container = Consol.Containers.AddNew();

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.SetContainer(Consol, container);
			line1.JL_ActualWeight = 8;
			line1.JL_ActualWeightUQ = Constants.Weight.Ounces;
			line1.JL_ActualVolume = 864;
			line1.JL_ActualVolumeUQ = Constants.Volume.CubicInches;

			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.SetContainer(Consol, container);
			line2.JL_ActualWeight = 3;
			line2.JL_ActualWeightUQ = Constants.Weight.Pounds;
			line2.JL_ActualVolume = 2;
			line2.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			DocumentCommonConsol dcc = new DocumentCommonConsol(Consol, Constants.DataContext.Consol);
			dcc.IncludeAllShipments = true;
			ConsolWrapper = DocForwardingConsol.New(dcc, Factory);

			AssertEquals("should have 1 line", 1, ConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("Weight", 3.5m, ConsolWrapper.PackLines[0].ContainerManifestWeight);
			AssertEquals("Weight Unit", Constants.Weight.Pounds, ConsolWrapper.PackLinesForLoadList[0].ContainerManifestWeightUnit);
			AssertEquals("Volume", 2.5m, ConsolWrapper.PackLinesForLoadList[0].ContainerManifestVolume);
			AssertEquals("Volume Unit", Constants.Volume.CubicFeet, ConsolWrapper.PackLinesForLoadList[0].ContainerManifestVolumeUnit);
		}

		public void TestPackLinesSameShipment()
		{
			AssertEquals(0, ConsolWrapper.PackLines.Count);
			var shipment1 = Consol.Shipments.AddNew();
			var line1 = (PackLine)shipment1.OuterPackLines.AddNew();
			var line2 = (PackLine)shipment1.OuterPackLines.AddNew();

			FreightContainer container1 = Consol.Containers.AddNew();
			FreightContainer container2 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";

			var pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line1.PK;
			pivot1.J6_JC = container1.PK;

			var pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line2.PK;
			pivot2.J6_JC = container1.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(1, ConsolWrapper.PackLines.Count);

			line1.JL_ActualVolume = 10M;
			line2.JL_ActualVolume = 20M;
			line1.JL_ActualWeight = 1M;
			line2.JL_ActualWeight = 2M;
			line1.JL_PackageCount = 100;
			line2.JL_PackageCount = 200;
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(30M, ConsolWrapper.PackLines[0].ContainerManifestVolume);
			AssertEquals(3M, ConsolWrapper.PackLines[0].ContainerManifestWeight);
			AssertEquals(300, ConsolWrapper.PackLines[0].ContainerManifestPackage);
		}
		public void TestContainerManifestPackType()
		{
			var shipment1 = Consol.Shipments.AddNew();
			var line1 = (PackLine)shipment1.OuterPackLines.AddNew();
			var container1 = (FreightContainer)Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			line1.JL_F3_NKPackType = "PLT";
			var pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line1.PK;
			pivot1.J6_JC = container1.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals(1, ConsolWrapper.PackLines.Count);
			AssertEquals("Pack Type should be PLT", "PLT", ConsolWrapper.PackLines[0].ContainerManifestPackType);

			var line2 = (PackLine)shipment1.OuterPackLines.AddNew();
			line2.JL_F3_NKPackType = "PLT";
			var pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = line2.PK;
			pivot2.J6_JC = container1.PK;

			AssertEquals("Pack Type should be PLT", "PLT", ConsolWrapper.PackLines[0].ContainerManifestPackType);

			var line3 = (PackLine)shipment1.OuterPackLines.AddNew();
			line3.JL_F3_NKPackType = "DRM";
			var pivot3 = Factory.New<JobContainerPackPivot>();
			pivot3.J6_JL = line3.PK;
			pivot3.J6_JC = container1.PK;

			AssertEquals("Pack Type should be Package(s)", "Package(s)", ConsolWrapper.PackLines[0].ContainerManifestPackType);
		}
		public void TestShipmentsWithECNs()
		{
			var shipment1 = Consol.Shipments.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var shipment3 = Consol.Shipments.AddNew();

			shipment2.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			shipment2.CustomsEntryNumber = "CRN";

			shipment3.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			shipment3.CustomsEntryNumber = "ECN";

			AssertEquals("Collection is not empty", 1, ConsolWrapper.ShipmentsWithECNs.Count);
			AssertEquals("Object in collection is correct", "ECN", ConsolWrapper.ShipmentsWithECNs[0].CustomsEntryNumForECN);
		}
		public void TestMasterAndSubShipmentCollection()
		{
			AssertEquals(0, ConsolWrapper.MasterAndSubShipments.Count);
			var superMasterShipment = Consol.Shipments.AddNew();
			var masterShipment = superMasterShipment.CoLoadShipments.AddNew();
			var subShipmentA = Factory.New<ForwardingShipment>();
			var subShipmentB = Factory.New<ForwardingShipment>();

			superMasterShipment.JS_GoodsDescription = "Super Master Shipment";
			superMasterShipment.JS_HouseBill = "0";
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_HouseBill = "MASTER A";
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_HouseBill = "A";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipmentB.JS_HouseBill = "B";

			var masterShipment2 = Consol.Shipments.AddNew();
			var subShipmentC = Factory.New<ForwardingShipment>();
			var subShipmentD = Factory.New<ForwardingShipment>();

			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_HouseBill = "MASTER B";
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_HouseBill = "C";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_HouseBill = "D";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			Factory.Save();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			DocShipmentCollection result = ConsolWrapper.MasterAndSubShipments;
			AssertEquals(7, result.Count);
			AssertEquals("Super Master Shipment", result[0].GoodsDescription);
			AssertEquals("Master shipment", result[1].GoodsDescription);
			AssertEquals("shipment A", result[2].GoodsDescription);
			AssertEquals("shipment B", result[3].GoodsDescription);
			AssertEquals("Master shipment 2", result[4].GoodsDescription);
			AssertEquals("shipment C", result[5].GoodsDescription);
			AssertEquals("shipment D", result[6].GoodsDescription);
		}

		public void TestMasterShipmentCollection()
		{
			AssertEquals(0, ConsolWrapper.MasterShipments.Count);

			var superMasterShipment = Consol.Shipments.AddNew();

			var masterShipment = Consol.Shipments.AddNew();
			var masterShipment2 = superMasterShipment.CoLoadShipments.AddNew();

			var subShipmentA = Factory.New<ForwardingShipment>();
			var subShipmentB = Factory.New<ForwardingShipment>();

			var independentShipment = Consol.Shipments.AddNew();

			superMasterShipment.JS_GoodsDescription = "Super master shipment";
			superMasterShipment.JS_HouseBill = "SM";
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_HouseBill = "A";
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;
			independentShipment.JS_GoodsDescription = "independent shipment";
			independentShipment.JS_HouseBill = "I";

			var subShipmentC = Factory.New<ForwardingShipment>();
			var subShipmentD = Factory.New<ForwardingShipment>();

			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_HouseBill = "B";
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			AssertEquals(3, ConsolWrapper.MasterShipments.Count);
			AssertEquals("Master shipment", ConsolWrapper.MasterShipments[0].GoodsDescription);
			AssertEquals("independent shipment", ConsolWrapper.MasterShipments[1].GoodsDescription);
			AssertEquals("Super master shipment", ConsolWrapper.MasterShipments[2].GoodsDescription);
		}

		public void TestSubShipmentsCollectionWithNoSubs()
		{
			var master1 = Consol.Shipments.AddNew();
			var master2 = Consol.Shipments.AddNew();
			AssertEquals(0, ConsolWrapper.SubShipments.Count);
		}

		public void TestSubShipmentsCollection()
		{
			var master1 = Consol.Shipments.AddNew();
			var master2 = Consol.Shipments.AddNew();
			var subA = Factory.New<ForwardingShipment>();
			var subB = Factory.New<ForwardingShipment>();
			var subC = Factory.New<ForwardingShipment>();

			subA.JS_JS_ColoadMasterShipment = master1.PK;
			subB.JS_JS_ColoadMasterShipment = master1.PK;
			subC.JS_JS_ColoadMasterShipment = master2.PK;
			Factory.Save();

			AssertEquals(3, ConsolWrapper.SubShipments.Count);
			AssertEquals("CurrentConsol on shipment 1 is this Consol", ConsolWrapper, ConsolWrapper.SubShipments[0].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 2 is this Consol", ConsolWrapper, ConsolWrapper.SubShipments[1].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 3 is this Consol", ConsolWrapper, ConsolWrapper.SubShipments[2].CurrentConsol);
		}

		public void TestUltimateShipments()
		{
			var standAloneShipment1 = Consol.Shipments.AddNew();
			var standAloneShipment2 = Consol.Shipments.AddNew();
			var superMaster = Consol.Shipments.AddNew();
			var master1 = Consol.Shipments.AddNew();
			var master2 = superMaster.CoLoadShipments.AddNew();
			var subA = Factory.New<ForwardingShipment>();
			var subB = Factory.New<ForwardingShipment>();
			var subC = Factory.New<ForwardingShipment>();

			subA.JS_JS_ColoadMasterShipment = master1.PK;
			subB.JS_JS_ColoadMasterShipment = master1.PK;
			subC.JS_JS_ColoadMasterShipment = master2.PK;

			standAloneShipment1.JS_HouseBill = "XYZ1";
			standAloneShipment2.JS_HouseBill = "XYZ2";
			superMaster.JS_HouseBill = "SUPERMASTER";
			master1.JS_HouseBill = "MASTER1";
			master2.JS_HouseBill = "MASTER2";
			subA.JS_HouseBill = "SUB3";
			subB.JS_HouseBill = "SUB2";
			subC.JS_HouseBill = "SUB1";

			DocShipmentCollection coll = ConsolWrapper.UltimateShipments;
			AssertEquals(5, coll.Count);
			AssertEquals("SUB1", coll[0].HouseBill);
			AssertEquals("SUB2", coll[1].HouseBill);
			AssertEquals("SUB3", coll[2].HouseBill);
			AssertEquals("XYZ1", coll[3].HouseBill);
			AssertEquals("XYZ2", coll[4].HouseBill);

			AssertEquals("CurrentConsol on shipment 1 is this Consol", ConsolWrapper, coll[0].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 2 is this Consol", ConsolWrapper, coll[1].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 3 is this Consol", ConsolWrapper, coll[2].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 4 is this Consol", ConsolWrapper, coll[3].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 5 is this Consol", ConsolWrapper, coll[4].CurrentConsol);
		}

		public void TestSubShipmentsHBLSort()
		{
			AssertEquals(0, ConsolWrapper.SubShipments.Count);

			var master1 = Consol.Shipments.AddNew();
			var subA = Factory.New<ForwardingShipment>();
			var subB = Factory.New<ForwardingShipment>();

			master1.JS_HouseBill = "HBL333";
			subA.JS_HouseBill = "SHIP A";
			subA.JS_JS_ColoadMasterShipment = master1.PK;
			subB.JS_HouseBill = "SHIP B";
			subB.JS_JS_ColoadMasterShipment = master1.PK;

			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTNMBR2";
			FreightContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTNMBR1";
			FreightContainer container4 = Consol.Containers.AddNew();
			container4.JC_ContainerNum = "CONTNMBR4";
			FreightContainer container3 = Consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTNMBR3";
			FreightContainer container5 = Consol.Containers.AddNew();
			container5.JC_ContainerNum = "CONTNMBR5";
			FreightContainer container6 = Consol.Containers.AddNew();
			container6.JC_ContainerNum = "CONTNMBR6";

			ZString expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6";
			AssertEquals("Container Numbers (ContainerLine)", expectedContainerLine, ConsolWrapper.ContainerLine);

			FreightContainer container7 = Consol.Containers.AddNew();
			container7.JC_ContainerNum = "CONTNMBR7";
			expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6, CONTNMBR7";
			AssertEquals("Container Numbers (ContainerLine)", expectedContainerLine, ConsolWrapper.ContainerLine);

			FreightContainer container8 = Consol.Containers.AddNew();
			container8.JC_ContainerNum = "CONTNMBR8";
			expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6, CONTNMBR7 ...";
			AssertEquals("Container Numbers (ContainerLine)", expectedContainerLine, ConsolWrapper.ContainerLine);
		}

		public void TestSortOrderOnMasterShipments()
		{
			var masterShipment = Consol.Shipments.AddNew();
			var masterShipment2 = Consol.Shipments.AddNew();
			var masterShipment3 = Consol.Shipments.AddNew();
			var subShipmentA = Factory.New<ForwardingShipment>();
			var subShipmentC = Factory.New<ForwardingShipment>();

			masterShipment.JS_HouseBill = "";
			subShipmentA.JS_HouseBill = "shipment A";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			masterShipment2.JS_HouseBill = "Master shipment 2";
			subShipmentC.JS_HouseBill = "shipment C";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			masterShipment3.JS_HouseBill = "";

			masterShipment.JS_UniqueConsignRef = "S0001284";
			masterShipment2.JS_UniqueConsignRef = "S0000562";
			masterShipment3.JS_UniqueConsignRef = "S0000777";

			DocShipmentCollection result = ConsolWrapper.MasterShipments;
			AssertEquals(3, result.Count);
			AssertEquals("S0000777", result[0].ShipmentNumber);
			AssertEquals("S0001284", result[1].ShipmentNumber);
			AssertEquals("S0000562", result[2].ShipmentNumber);

			AssertEquals("CurrentConsol on shipment 1 is this Consol", ConsolWrapper, result[0].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 2 is this Consol", ConsolWrapper, result[1].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 3 is this Consol", ConsolWrapper, result[2].CurrentConsol);
		}

		public void TestMasterShipmentsSortOnShipmentNumber()
		{
			var shipA = Consol.Shipments.AddNew();
			var shipB = Consol.Shipments.AddNew();
			var shipC = Consol.Shipments.AddNew();
			var shipD = Consol.Shipments.AddNew();

			shipA.JS_UniqueConsignRef = "S0001";
			shipB.JS_UniqueConsignRef = "S0002";
			shipC.JS_UniqueConsignRef = "S0003";
			shipD.JS_UniqueConsignRef = "S0004";

			shipA.JS_HouseBill = "D";
			shipB.JS_HouseBill = "C";
			shipC.JS_HouseBill = "B";
			shipD.JS_HouseBill = "A";

			AssertEquals("Shipment ordered by HBL", "A", ConsolWrapper.MasterShipments[0].HouseBill);
			AssertEquals("Shipment ordered by HBL", "B", ConsolWrapper.MasterShipments[1].HouseBill);
			AssertEquals("Shipment ordered by HBL", "C", ConsolWrapper.MasterShipments[2].HouseBill);
			AssertEquals("Shipment ordered by HBL", "D", ConsolWrapper.MasterShipments[3].HouseBill);

			ConsolWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.SortShipmentsOnHBL, 0);
			AssertEquals("Shipment ordered by Ship No", "D", ConsolWrapper.MasterShipments[0].HouseBill);
			AssertEquals("Shipment ordered by Ship No", "C", ConsolWrapper.MasterShipments[1].HouseBill);
			AssertEquals("Shipment ordered by Ship No", "B", ConsolWrapper.MasterShipments[2].HouseBill);
			AssertEquals("Shipment ordered by Ship No", "A", ConsolWrapper.MasterShipments[3].HouseBill);
		}

		public void TestSortOrderOnMasterAndSubShipmentCollection()
		{
			var masterShipment = Consol.Shipments.AddNew();
			var subShipmentA = Factory.New<ForwardingShipment>();
			var subShipmentB = Factory.New<ForwardingShipment>();

			masterShipment.JS_UniqueConsignRef = "Number 2";
			subShipmentA.JS_UniqueConsignRef = "Number 2a";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipmentB.JS_UniqueConsignRef = "Number 2b";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentE = Factory.New<ForwardingShipment>();
			var subShipmentC = Factory.New<ForwardingShipment>();
			var subShipmentD = Factory.New<ForwardingShipment>();

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_UniqueConsignRef = "Number 1";
			subShipmentE.JS_UniqueConsignRef = "Number 1E";
			subShipmentE.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			subShipmentC.JS_UniqueConsignRef = "Number 1c";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			subShipmentD.JS_UniqueConsignRef = "Number 1D";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var masterShipment3 = Consol.Shipments.AddNew();
			masterShipment3.JS_UniqueConsignRef = "Number 0";
			Factory.Save();

			DocShipmentCollection result = ConsolWrapper.MasterAndSubShipments;
			AssertEquals(8, result.Count);
			AssertEquals("Number 0", result[0].ShipmentNumber);
			AssertEquals("Number 1", result[1].ShipmentNumber);
			AssertEquals("Number 1c", result[2].ShipmentNumber);
			AssertEquals("Number 1D", result[3].ShipmentNumber);
			AssertEquals("Number 1E", result[4].ShipmentNumber);
			AssertEquals("Number 2", result[5].ShipmentNumber);
			AssertEquals("Number 2a", result[6].ShipmentNumber);
			AssertEquals("Number 2b", result[7].ShipmentNumber);

			masterShipment.JS_HouseBill = "HBL 234";
			masterShipment2.JS_HouseBill = "";
			masterShipment3.JS_HouseBill = "234 HBL";

			subShipmentA.JS_HouseBill = "000999";
			subShipmentB.JS_HouseBill = "SUBHBL1";
			subShipmentC.JS_HouseBill = "ABC 999";
			subShipmentD.JS_HouseBill = "729 DEF";
			subShipmentE.JS_HouseBill = "";

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			result = ConsolWrapper.MasterAndSubShipments;
			AssertEquals(8, result.Count);
			AssertEquals("Number 1", result[0].ShipmentNumber);
			AssertEquals("Number 1E", result[1].ShipmentNumber);
			AssertEquals("Number 1D", result[2].ShipmentNumber);
			AssertEquals("Number 1c", result[3].ShipmentNumber);
			AssertEquals("Number 0", result[4].ShipmentNumber);
			AssertEquals("Number 2", result[5].ShipmentNumber);
			AssertEquals("Number 2b", result[6].ShipmentNumber);
			AssertEquals("Number 2a", result[7].ShipmentNumber);
		}

		public void TestShipmentCollectionReportName()
		{
			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S999999991";
			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S999999992";
			var shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S999999993";

			ConsolWrapper.SetReportNameForTesting("Pre-Alert");
			ConsolWrapper.SetDocumentDirectionForTesting("DEP");

			foreach (DocForwardingShipment shipmentWrapper in ConsolWrapper.Shipments)
			{
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong report name", "Pre-Alert", shipmentWrapper.ReportName);
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong document direction", "DEP", shipmentWrapper.DocumentDirection);
			}

			foreach (DocForwardingShipment shipmentWrapper in ConsolWrapper.MasterAndSubShipments)
			{
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong report name", "Pre-Alert", shipmentWrapper.ReportName);
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong document direction", "DEP", shipmentWrapper.DocumentDirection);
			}

			foreach (DocForwardingShipment shipmentWrapper in ConsolWrapper.MasterShipments)
			{
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong report name", "Pre-Alert", shipmentWrapper.ReportName);
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong document direction", "DEP", shipmentWrapper.DocumentDirection);
			}

			foreach (DocForwardingShipment shipmentWrapper in ConsolWrapper.ShipmentsWithECNs)
			{
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong report name", "Pre-Alert", shipmentWrapper.ReportName);
				AssertEquals(shipmentWrapper.ShipmentNumber + " has the wrong document direction", "DEP", shipmentWrapper.DocumentDirection);
			}
		}

		public void TestPacklinesForManifest()
		{
			var masterShipment = Consol.Shipments.AddNew();
			var subShipmentA = Factory.New<ForwardingShipment>();
			var subShipmentB = Factory.New<ForwardingShipment>();

			masterShipment.JS_UniqueConsignRef = "Number 2";
			subShipmentA.JS_UniqueConsignRef = "Number 2a";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipmentA.JS_MarksAndNumbers = "Shipment Marks";
			subShipmentB.JS_UniqueConsignRef = "Number 2b";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			FreightContainer container = Consol.Containers.AddNew();
			PackLine masterShipmentPackLine = masterShipment.OuterPackLines.AddNew();
			PackLine subShipmentAPackLine = subShipmentA.OuterPackLines.AddNew();
			PackLine subShipmentBPackLine = subShipmentB.OuterPackLines.AddNew();
			subShipmentBPackLine.JL_MarksAndNumbers = "PackLine Marks";
			PackLine ignoredPackLine = subShipmentB.OuterPackLines.AddNew();
			ignoredPackLine.JL_MarksAndNumbers = "Ignored PackLine";
			ignoredPackLine.Containers.RemoveAll();

			masterShipmentPackLine.SetContainer(Consol, container);
			subShipmentAPackLine.SetContainer(Consol, container);
			subShipmentBPackLine.SetContainer(Consol, container);

			masterShipmentPackLine.JL_PackageCount = 10;
			subShipmentAPackLine.JL_PackageCount = 20;
			subShipmentBPackLine.JL_PackageCount = 20;

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals("Only Masters: So should show 1", 1, ConsolWrapper.PackLinesForManifest.Count);
			AssertEquals("Only Masters: PackageCount should be 10", 10, ConsolWrapper.PackLinesForManifest[0].PackageCount);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals("Only SubShipments: So should show 2", 2, ConsolWrapper.PackLinesForManifest.Count);
			AssertEquals("Only SubShipments: PackageCount should be 20", 20, ConsolWrapper.PackLinesForManifest[0].PackageCount);
			AssertEquals("Only SubShipments: PackageCount should be 20", 20, ConsolWrapper.PackLinesForManifest[1].PackageCount);

			if (ConsolWrapper.PackLinesForManifest[0].ContainerManifestMarksAndNumbers.StartsWith("Shipment"))
			{
				AssertEquals("Shipment Marks", ConsolWrapper.PackLinesForManifest[0].ContainerManifestMarksAndNumbers);
				AssertEquals("PackLine Marks", ConsolWrapper.PackLinesForManifest[1].ContainerManifestMarksAndNumbers);
			}
			else
			{
				AssertEquals("Shipment Marks", ConsolWrapper.PackLinesForManifest[1].ContainerManifestMarksAndNumbers);
				AssertEquals("PackLine Marks", ConsolWrapper.PackLinesForManifest[0].ContainerManifestMarksAndNumbers);
			}

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals("All: So should show 3", 3, ConsolWrapper.PackLinesForManifest.Count);
		}

		public void TestPacklinesForManifest_NoDuplicateMarksAndNumbers()
		{
			var shipment = Consol.Shipments.AddNew();

			shipment.JS_UniqueConsignRef = "Number 1";
			shipment.JS_MarksAndNumbers = "Shipment Marks";

			FreightContainer container = Consol.Containers.AddNew();
			PackLine shipmentPackLine = shipment.OuterPackLines.AddNew();
			PackLine shipmentPackLine2 = shipment.OuterPackLines.AddNew();
			shipmentPackLine.SetContainer(Consol, container);
			shipmentPackLine2.SetContainer(Consol, container);

			AssertEquals(1, ConsolWrapper.PackLinesForManifest.Count);
			AssertEquals("Shipment Marks", ConsolWrapper.PackLinesForManifest[0].ContainerManifestMarksAndNumbers);

			shipmentPackLine2.JL_MarksAndNumbers = "PackLine Marks";
			AssertEquals(1, ConsolWrapper.PackLinesForManifest.Count);
			AssertEquals("Shipment Marks\r\nPackLine Marks", ConsolWrapper.PackLinesForManifest[0].ContainerManifestMarksAndNumbers);
		}

		public void TestTranshipments()
		{
			ConsolWrapper.SetReportNameForTesting("Manifest");
			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			var ship1 = Consol.Shipments.AddNew();
			var ship2 = Consol.Shipments.AddNew();
			ship2.CoLoadShipments.AddNew();
			AssertEquals(3, ConsolWrapper.Transhipments.Count);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(2, ConsolWrapper.Transhipments.Count);

			ConsolWrapper.SetReportNameForTesting("Forwarding Instruction");
			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(3, ConsolWrapper.Transhipments.Count);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(2, ConsolWrapper.Transhipments.Count);

			AssertEquals("CurrentConsol on shipment 1 is this Consol", ConsolWrapper, ConsolWrapper.Transhipments[0].CurrentConsol);
			AssertEquals("CurrentConsol on shipment 2 is this Consol", ConsolWrapper, ConsolWrapper.Transhipments[1].CurrentConsol);

			ZGuid initialBranchPK = GlbBranch.CurrentBranch.PK;
			GlbBranch sydneyBranch = Factory.NewWithValidTestData<GlbBranch>();
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, sydneyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				ConsolWrapper.SetReportNameForTesting("Transhipment List");
				Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
				AssertEquals(0, ConsolWrapper.Transhipments.Count);
				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "DEFRA";
				ship1.JS_RL_NKOrigin = "AUSYD";
				ship1.JS_RL_NKDestination = "ITBAG";
				ship2.JS_RL_NKOrigin = "ITBAG";
				ship2.JS_RL_NKDestination = "DEFRA";
				AssertEquals("Shipment from ITBAG to DEFRA should be in Transhipments list", ship2.PK, ((CommonShipment)ConsolWrapper.Transhipments[0].Shipment.WrappedObject).PK);
				AssertEquals("One shipment out of two should found in the collection", 1, ConsolWrapper.Transhipments.Count);
			}
		}

		public void TestConsolOuterPacks()
		{
			AssertEquals(0, ConsolWrapper.ConsolOuterPacks.Count);
			var shipment1 = Consol.Shipments.AddNew();
			var line1 = (PackLine)shipment1.OuterPackLines.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var line2 = (PackLine)shipment2.OuterPackLines.AddNew();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(2, ConsolWrapper.ConsolOuterPacks.Count);
		}

		public void TestTotalConsolPackCount()
		{
			AssertEquals(0, ConsolWrapper.TotalConsolPackCount);
			var shipment1 = Consol.Shipments.AddNew();
			var line1 = (PackLine)shipment1.OuterPackLines.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var line2 = (PackLine)shipment2.OuterPackLines.AddNew();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(0, ConsolWrapper.TotalConsolPackCount);

			line1.JL_PackageCount = 2;
			line2.JL_PackageCount = 3;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(5, ConsolWrapper.TotalConsolPackCount);
		}

		public void TestTotalActualWeight()
		{
			AssertEquals(0m, ConsolWrapper.TotalActualWeight);
			var shipment1 = Consol.Shipments.AddNew();
			var line1 = (PackLine)shipment1.OuterPackLines.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var line2 = (PackLine)shipment2.OuterPackLines.AddNew();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(0m, ConsolWrapper.TotalActualWeight);

			line1.JL_ActualWeight = 2m;
			line2.JL_ActualWeight = 3m;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(11.02m, ConsolWrapper.TotalActualWeight);
		}

		#endregion

		#region Consol Fields Method Test

		#region TestShipmentsForTranshipmentList

		public void TestShipmentsForTranshipmentListForImportConsols()
		{
			GlbBranch sydneyBranch = Factory.NewWithValidTestData<GlbBranch>();
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, sydneyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Consol.JK_RL_NKLoadPort = "DEFRA";
				Consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("Consol should be Import", true, Consol.IsImport());
				ForwardingShipment shipment1 = Consol.Shipments.AddNew();
				ForwardingShipment shipment2 = Consol.Shipments.AddNew();
				ForwardingShipment shipment3 = Consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "ITABG";
				shipment2.JS_RL_NKOrigin = "ITBAG";
				shipment2.JS_RL_NKDestination = "NZAKL";
				shipment3.JS_RL_NKOrigin = "NZAKL";
				shipment3.JS_RL_NKDestination = "AUSYD";

				AssertEquals(2, ConsolWrapper.ShipmentsForTranshipmentList.Count);

				ArrayList shipmentsListedInDocShipmentCollection = new ArrayList();
				foreach (DocForwardingShipment docShipment in ConsolWrapper.ShipmentsForTranshipmentList)
				{
					shipmentsListedInDocShipmentCollection.Add(docShipment.WrappedObject);
				}
				AssertEquals("Shipment from DEFRA to ITABG should be in Transhipments list when Shipment is from DEFRA to AUSYD", true, shipmentsListedInDocShipmentCollection.Contains(shipment1));
				AssertEquals("Shipment from ITABG to NZAKL should be in Transhipments list when Shipment is from DEFRA to AUSYD", true, shipmentsListedInDocShipmentCollection.Contains(shipment2));
				AssertEquals("Shipment from NZAKL to AUSYD should not be in Transhipments list when Shipment is from DEFRA to AUSYD", false, shipmentsListedInDocShipmentCollection.Contains(shipment3));
			}
		}

		public void TestShipmentsForTranshipmentListForExportConsols()
		{
			GlbBranch sydneyBranch = Factory.NewWithValidTestData<GlbBranch>();
			sydneyBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, sydneyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "DEFRA";
				AssertEquals("Consol should be Export", true, Consol.IsExport());
				ForwardingShipment shipment1 = Consol.Shipments.AddNew();
				ForwardingShipment shipment2 = Consol.Shipments.AddNew();
				ForwardingShipment shipment3 = Consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "AUSYD";
				shipment1.JS_RL_NKDestination = "NZAKL";
				shipment2.JS_RL_NKOrigin = "NZAKL";
				shipment2.JS_RL_NKDestination = "ITBAG";
				shipment3.JS_RL_NKOrigin = "ITABG";
				shipment3.JS_RL_NKDestination = "DEFRA";

				AssertEquals(2, ConsolWrapper.ShipmentsForTranshipmentList.Count);

				ArrayList shipmentsListedInDocShipmentCollection = new ArrayList();
				foreach (DocForwardingShipment docShipment in ConsolWrapper.ShipmentsForTranshipmentList)
				{
					shipmentsListedInDocShipmentCollection.Add(docShipment.WrappedObject);
				}
				AssertEquals("Shipment from AUSYD to NZAKL should not be in Transhipments list when Shipment is from AUSYD to DEFRA", false, shipmentsListedInDocShipmentCollection.Contains(shipment1));
				AssertEquals("Shipment from ITABG to DEFRA should be in Transhipments list when Shipment is from AUSYD to DEFRA", true, shipmentsListedInDocShipmentCollection.Contains(shipment2));
				AssertEquals("Shipment from NZAKL to ITABG should be in Transhipments list when Shipment is from AUSYD to DEFRA", true, shipmentsListedInDocShipmentCollection.Contains(shipment3));
			}
		}

		#endregion

		public void TestContext()
		{
			AssertEquals("CONSOL", ConsolWrapper.Context);
		}

		public void TestDepartureReference()
		{
			AssertEquals("", ConsolWrapper.DepartureReference);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("", ConsolWrapper.DepartureReference);

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = sailing.PK;
			AssertEquals("", ConsolWrapper.DepartureReference);

			origin.JA_DepartReference = "Origin Ref";
			destination.JB_ArrivalReference = "Arrival Ref";

			AssertEquals("Origin Ref", ConsolWrapper.DepartureReference);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", ConsolWrapper.DepartureReference);
		}

		public void TestAgentSCACForExport()
		{
			AssertEquals("Agent SCAC code for export is empty", ZString.Empty, ConsolWrapper.AgentSCACForExport);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForCurrentCountry();
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForTheUS();
			AssertEquals("Agent SCAC code for export is empty", "*MISSING*", ConsolWrapper.AgentSCACForExport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultReceivingForwarderAddress(header);
			ZString expectedResult = header.SCACCode;
			if (expectedResult.IsEmpty)
			{
				expectedResult = "*MISSING*";
			}

			AssertEquals("Agent SCAC code for export", expectedResult, ConsolWrapper.AgentSCACForExport);
		}

		public void TestLineSCACForExport()
		{
			AssertEquals("Line SCAC code for export is empty", ZString.Empty, ConsolWrapper.LineSCACForExport);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForCurrentCountry();
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForTheUS();
			AssertEquals("Line SCAC code for export is empty", "*MISSING*", ConsolWrapper.LineSCACForExport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(header);
			ZString expectedResult = header.SCACCode;
			if (expectedResult.IsEmpty)
			{
				expectedResult = "*MISSING*";
			}

			AssertEquals("Line SCAC code for export", expectedResult, ConsolWrapper.LineSCACForExport);
		}

		public void TestContainerLine()
		{
			AssertEquals("Container line is empty", "", ConsolWrapper.ContainerLine);

			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "2";
			FreightContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			FreightContainer container4 = Consol.Containers.AddNew();
			container4.JC_ContainerNum = "4";
			FreightContainer container3 = Consol.Containers.AddNew();
			container3.JC_ContainerNum = "3";
			FreightContainer container5 = Consol.Containers.AddNew();

			ZString expectedResult = "1, 2, 3, 4";
			AssertEquals("Container line", expectedResult, ConsolWrapper.ContainerLine);
		}

		public void TestMainVesselTransportInfoForSea()
		{
			AssertEquals("Pre carriage transport info is empty", ZString.Empty, ConsolWrapper.PreCarriageTransportInfo);

			ZString expectedResult = "    " + " /    " + " /    ";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Main vessel transport info", expectedResult, ConsolWrapper.MainVesselTransportInfo);

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "111";
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			expectedResult = Consol.JK_JX_JV_NKVessel + " / " + Consol.JK_JX_JV_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.MainVesselTransportInfo);

			var mainTrasport = Consol.Transports.AddNew();
			mainTrasport.JW_Vessel = vessel.RV_Code;
			mainTrasport.JW_VoyageFlight = "MainVoy";
			mainTrasport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			expectedResult = mainTrasport.JW_Vessel + " / " + mainTrasport.JW_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.MainVesselTransportInfo);

			var preCarriageTrasport = Consol.Transports.AddNew();
			preCarriageTrasport.JW_Vessel = vessel.RV_Code;
			preCarriageTrasport.JW_VoyageFlight = "PreVoy";
			preCarriageTrasport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			expectedResult = mainTrasport.JW_Vessel + " / " + mainTrasport.JW_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.MainVesselTransportInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestPreCarriageTransportInfoForAir()
		{
			AssertEquals("Pre carriage transport info is empty", ZString.Empty, ConsolWrapper.PreCarriageTransportInfo);

			ZString expectedResult = "    " + " /    " + " /    ";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);

			ZDateTime departureTime = new ZDateTime(2004, 01, 01);

			Transport primariyTransport = Consol.Transports[0];
			primariyTransport.JW_ETD = departureTime;
			primariyTransport.JW_VoyageFlight = "Voyage";
			primariyTransport.JW_RL_NKLoadPort = "NZAKL";
			primariyTransport.JW_RL_NKDiscPort = "AUSYD";
			expectedResult = Consol.JK_JX_JV_VoyageFlight + " / " + Consol.JK_JX_JB_RL_NKPortOfDischarge + " / " + departureTime.ToString("dd-MMM-yy");
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);

			var transport = Consol.Transports.AddNew();
			ZDateTime transportETD = new ZDateTime(2003, 01, 01);
			transport.JW_ETD = transportETD;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport.JW_VoyageFlight = "TransVoy";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			expectedResult += " -> " + transport.JW_VoyageFlight + " / " + transport.JW_RL_NKDiscPort + " / " + transportETD.ToString("dd-MMM-yy");
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);
		}

		public void TestPreCarriageTransportInfoForSea()
		{
			AssertEquals("Pre carriage transport info is empty", ZString.Empty, ConsolWrapper.PreCarriageTransportInfo);

			ZString expectedResult = "    " + " /    " + " /    ";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "111";
			expectedResult = Consol.JK_JX_JV_NKVessel + " / " + Consol.JK_JX_JV_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);

			var mainTrasport = Consol.Transports.AddNew();
			mainTrasport.JW_Vessel = vessel.RV_Code;
			mainTrasport.JW_VoyageFlight = "MainVoy";
			mainTrasport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			expectedResult = mainTrasport.JW_Vessel + " / " + mainTrasport.JW_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);

			var preCarriageTrasport = Consol.Transports.AddNew();
			preCarriageTrasport.JW_Vessel = vessel.RV_Code;
			preCarriageTrasport.JW_VoyageFlight = "PreVoy";
			preCarriageTrasport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			expectedResult = preCarriageTrasport.JW_Vessel + " / " + preCarriageTrasport.JW_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("Pre carriage transport info", expectedResult, ConsolWrapper.PreCarriageTransportInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestOnForwardingTransportInfoForAir()
		{
			AssertEquals("OnForwardingTransportInfo is empty", ZString.Empty, ConsolWrapper.OnForwardingTransportInfo);

			ZString expectedResult = "    " + " /    " + " /    ";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);

			ZDateTime departureTime = new ZDateTime(2004, 01, 01);

			Transport transport = Consol.Transports[0];
			transport.JW_ETD = departureTime;
			transport.JW_VoyageFlight = "Voyage";
			transport.JW_RL_NKLoadPort = "NZCHC";
			transport.JW_RL_NKDiscPort = "AUMEL";
			expectedResult = Consol.JK_JX_JV_VoyageFlight + " / " + Consol.JK_JX_JB_RL_NKPortOfDischarge + " / " + departureTime.ToString("dd-MMM-yy");
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);

			var newTransport = Consol.Transports.AddNew();
			ZDateTime transportETD = new ZDateTime(2003, 01, 01);
			newTransport.JW_ETD = transportETD;
			newTransport.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			newTransport.JW_VoyageFlight = "TransVoy";
			newTransport.JW_RL_NKLoadPort = "AUMEL";
			newTransport.JW_RL_NKDiscPort = "AUSYD";
			expectedResult += " -> " + newTransport.JW_VoyageFlight + " / " + newTransport.JW_RL_NKDiscPort + " / " + transportETD.ToString("dd-MMM-yy");
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);
		}

		public void TestOnForwardingTransportInfoForSea()
		{
			AssertEquals("OnForwardingTransportInfo is empty", ZString.Empty, ConsolWrapper.OnForwardingTransportInfo);

			ZString expectedResult = "    " + " /    " + " /    ";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "111";

			expectedResult = Consol.JK_JX_JV_NKVessel + " / " + Consol.JK_JX_JV_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);

			var mainTrasport = Consol.Transports.AddNew();
			mainTrasport.JW_Vessel = vessel.RV_Code;
			mainTrasport.JW_VoyageFlight = "MainVoy";
			mainTrasport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			expectedResult = mainTrasport.JW_Vessel + " / " + mainTrasport.JW_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);

			var preCarriageTrasport = Consol.Transports.AddNew();
			preCarriageTrasport.JW_Vessel = vessel.RV_Code;
			preCarriageTrasport.JW_VoyageFlight = "PreVoy";
			preCarriageTrasport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			expectedResult = preCarriageTrasport.JW_Vessel + " / " + preCarriageTrasport.JW_VoyageFlight + " / " + vessel.RV_LloydsNumber;
			AssertEquals("OnForwardingTransportInfo", expectedResult, ConsolWrapper.OnForwardingTransportInfo);
		}

		public void TestAgentSCAC()
		{
			AssertEquals("AgentSCAC is empty", ZString.Empty, ConsolWrapper.AgentSCAC);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForTheUS();
			AssertEquals("AgentSCAC is empty", "*MISSING*", ConsolWrapper.AgentSCAC);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultReceivingForwarderAddress(header);
			ZString expectedResult = header.SCACCode;
			if (expectedResult.IsEmpty)
			{
				expectedResult = "*MISSING*";
			}

			AssertEquals("AgentSCAC", expectedResult, ConsolWrapper.AgentSCAC);
		}

		public void TestLastForeignPort()
		{
			AssertEquals("Last Foreign port is empty", ZString.Empty, ConsolWrapper.LastForeignPort);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForTheUS();
			AssertEquals("Last Foreign port is empty", ZString.Empty, ConsolWrapper.LastForeignPort);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Consol.JK_RL_NKLastForeignPort = uNLOCO.RL_Code;
			ZString expectedResult = uNLOCO.RL_Code + " = " + uNLOCO.RL_PortName + ", " + uNLOCO.Country.RN_Desc;
			AssertEquals("Last foreign port", expectedResult, ConsolWrapper.LastForeignPort);
		}

		public void TestLineSCAC()
		{
			AssertEquals("Line SCAC is empty", ZString.Empty, ConsolWrapper.LineSCAC);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForTheUS();
			AssertEquals("Line SCAC code for export is empty", "*MISSING*", ConsolWrapper.LineSCAC);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(header);
			ZString expectedResult = header.SCACCode;
			if (expectedResult.IsEmpty)
			{
				expectedResult = "*MISSING*";
			}

			AssertEquals("Line SCAC code for export", expectedResult, ConsolWrapper.LineSCAC);
		}

		public void TestHBLNumbers()
		{
			AssertEquals("HBL Numbers is empty", ZString.Empty, ConsolWrapper.HBLNumbers);

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "b";
			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "a";
			var shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "c";
			var shipment4 = Consol.Shipments.AddNew();
			shipment4.JS_HouseBill = "d";

			ZString expectedResult = "A, B, C, D";
			AssertEquals("HBLNumbers", expectedResult, ConsolWrapper.HBLNumbers);
		}

		public void TestPaymentType()
		{
			AssertEquals("Payment type", "PREPAID", ConsolWrapper.PaymentType);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertEquals("Payment type", "COLLECT", ConsolWrapper.PaymentType);

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery());

			AssertEquals("Payment type", "COLLECT", ConsolWrapper.PaymentType);

			ApportionmentListing app = new ApportionmentListing(Factory, Consol);
			var cost = app.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = currency.RX_Code;
			cost.E6_ExchangeRate = 1m;
			cost.E6_OSCostAmount = 12.00m;

			AssertEquals("Payment type", "COLLECT: " + currency.RX_Code + " 12.00", ConsolWrapper.PaymentType);
		}

		public void TestVoyageLabel()
		{
			AssertEquals("VoyageLabel", Consol.JK_Calc_VoyageLabel, ConsolWrapper.VoyageLabel);
		}

		public void TestAgentType()
		{
			ZString agentType = new ZString("AAA");
			Consol.JK_AgentType = agentType;
			AssertEquals("AgentType", agentType, ConsolWrapper.AgentType);
		}

		public void TestConsolStatus()
		{
			ZString consolStatus = new ZString("CCC");
			Consol.JK_ConsolStatus = consolStatus;
			AssertEquals("ConsolStatus", consolStatus, ConsolWrapper.ConsolStatus);
		}

		public void TestCustomsReference()
		{
			ZString customsReference = new ZString("CustomsReference");
			Consol.JK_CustomsReference = customsReference;
			AssertEquals("CustomsReference", customsReference, ConsolWrapper.CustomsReference);
		}

		public void TestPrintOptionForColoadsOnManifest()
		{
			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals("ALL", ConsolWrapper.PrintOptionForColoadsOnManifest);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals("MAS", ConsolWrapper.PrintOptionForColoadsOnManifest);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals("SUB", ConsolWrapper.PrintOptionForColoadsOnManifest);
		}

		public void TestPrintOptionForColoadsOnOtherDocs()
		{
			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals("ALL", ConsolWrapper.PrintOptionForColoadsOnOtherDocs);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals("MAS", ConsolWrapper.PrintOptionForColoadsOnOtherDocs);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals("SUB", ConsolWrapper.PrintOptionForColoadsOnOtherDocs);
		}

		public void TestServiceLevel()
		{
			AssertEquals("Service level", "STD", ConsolWrapper.ServiceLevel);

			Consol.JK_AWBServiceLevel = "ZZZ";
			AssertEquals("Service level", "ZZZ", ConsolWrapper.ServiceLevel);
		}

		public void TestServiceLevelDescription()
		{
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			OrgCarrierServiceLevel level = carrier.MiscServ.CarrierServiceLevels.AddNew();
			level.PL_Code = "ZZZ";
			level.PL_CarrierServiceLevelDescription = "ZZZ description";
			level = carrier.MiscServ.CarrierServiceLevels.AddNew();
			level.PL_Code = "YYY";
			level.PL_CarrierServiceLevelDescription = "YYY description";
			Factory.Save();

			AssertEquals("STD Only", 1, Consol.NeutralAirWaybillServiceLevelList.Count);

			Consol.SetDefaultShippingLineAddress(carrier);
			AssertEquals("Two defined service levels + STD", 3, Consol.NeutralAirWaybillServiceLevelList.Count);

			Consol.JK_AWBServiceLevel = "ZZZ";
			AssertEquals("Service level Description", "ZZZ description", ConsolWrapper.ServiceLevelDescription);
		}

		public void TestFlight1()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNull("Flight 1", ConsolWrapper.Flight1);

			Transport transport = Consol.Transports.AddNew();
			AssertNull("Flight 1", ConsolWrapper.Flight1);
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			AssertNotNull("Flight 1", ConsolWrapper.Flight1);
			AssertEquals("Flight 1 should be of type DocConsolTransport", typeof(DocTransport), ConsolWrapper.Flight1.GetType());
		}

		public void TestFlight2()
		{
			AssertNull("Flight2", ConsolWrapper.Flight2);

			Transport transport = Consol.Transports.AddNew();
			AssertNull("Flight2", ConsolWrapper.Flight1);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertNull("Flight2", ConsolWrapper.Flight2);

			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			AssertNotNull("Flight2", ConsolWrapper.Flight2);
			AssertEquals("Flight2 should be of type DocConsolTransport", typeof(DocTransport), ConsolWrapper.Flight2.GetType());
		}

		public void TestFlight3()
		{
			AssertNull("Flight3", ConsolWrapper.Flight3);

			Transport transport = Consol.Transports.AddNew();
			AssertNull("Flight3", ConsolWrapper.Flight3);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertNull("Flight3", ConsolWrapper.Flight3);

			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
			AssertNotNull("Flight3", ConsolWrapper.Flight3);
			AssertEquals("Flight3 should be of type DocConsolTransport", typeof(DocTransport), ConsolWrapper.Flight3.GetType());
		}

		public void TestDirectShipment()
		{
			AssertNull("Direct shipment", ConsolWrapper.DirectShipment);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertNull("Direct shipment", ConsolWrapper.DirectShipment);

			var shipment = Consol.Shipments.AddNew();
			AssertNotNull("Direct shipment", ConsolWrapper.DirectShipment);
			AssertEquals("Direct shipment is of type DocForwardingShipment", typeof(DocForwardingShipment), ConsolWrapper.DirectShipment.GetType());
		}

		public void TestExportConsolRate()
		{
			AssertEquals("ExportConsolRate", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ConsolWrapper.ExportConsolRate.Code);

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery());

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_RX_NKCurrency = currency.RX_Code;

			AssertNotNull("ExportConsolRate", ConsolWrapper.ExportConsolRate);
			AssertEquals("ExportConsolRate is of type DocCurrency", typeof(DocCurrency), ConsolWrapper.ExportConsolRate.GetType());
		}

		public void TestActualDateLastForeignPort()
		{
			ZDateTime actualDateLastForeignPort = new ZDateTime(2004, 01, 01);
			Consol.JK_DateLastForeignPort = actualDateLastForeignPort;
			AssertEquals("ActualDateLastForeignPort", actualDateLastForeignPort, ConsolWrapper.ActualDateLastForeignPort);
		}

		public void TestActualDatePortOfFirstArrival()
		{
			ZDateTime actualDatePortOfFirstArrival = new ZDateTime(2004, 01, 01);
			Consol.JK_DatePortOfFirstArrival = actualDatePortOfFirstArrival;
			AssertEquals("ActualDatePortOfFirstArrival", actualDatePortOfFirstArrival, ConsolWrapper.ActualDatePortOfFirstArrival);
		}

		public void TestCutOffDateGetter_SomeCutOffDateSpecifiedOnConsol_ReturnDateFromConsol()
		{
			Consol.JK_ConsolCutOffDateLocal = new ZDateTime(2013, 11, 4);

			AssertEquals("CutOffDate", Consol.JK_ConsolCutOffDateLocal, ConsolWrapper.CutOffDate);
		}

		public void TestConsolChargeable()
		{
			ZDecimal consolChargeable = new ZDecimal(2004);
			Consol.JK_OverrideConsolChargeable = true;
			Consol.JK_ConsolChargeable = consolChargeable;
			AssertEquals("ConsolChargeable", consolChargeable, ConsolWrapper.ConsolChargeable);
		}

		public void TestConsolChargeableRate()
		{
			ZDecimal consolChargeableRate = new ZDecimal(2004);
			Consol.JK_ConsolChargeableRate = consolChargeableRate;
			AssertEquals("ConsolChargeableRate", consolChargeableRate, ConsolWrapper.ConsolChargeableRate);
		}

		public void TestTotalConsolCollectAmount()
		{
			ZDecimal totalConsolCollectAmount = new ZDecimal(2004);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OSCostAmount = totalConsolCollectAmount;

			AssertEquals("TotalConsolCollectAmount", totalConsolCollectAmount, ConsolWrapper.TotalConsolCollectAmount);
		}

		public void TestTotalGoodsValue()
		{
			AssertEquals("TotalGoodsValue", 0M, ConsolWrapper.TotalGoodsValue);
		}

		public void TestExportConsolExchangeRate()
		{
			ZDecimal exportConsolExchangeRate = new ZDecimal(2004);
			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = "USD";
			cost.E6_ExchangeRate = exportConsolExchangeRate;

			AssertEquals("ExportConsolExchangeRate", exportConsolExchangeRate, ConsolWrapper.ConsolExchangeRate);
		}

		public void TestPrintColoadOnManifest()
		{
			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(ZBool.True, ConsolWrapper.PrintColoadOnManifest);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(ZBool.False, ConsolWrapper.PrintColoadOnManifest);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(ZBool.True, ConsolWrapper.PrintColoadOnManifest);
		}

		public void TestPrintMastersOnManifest()
		{
			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(ZBool.True, ConsolWrapper.PrintMastersOnManifest);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(ZBool.True, ConsolWrapper.PrintMastersOnManifest);

			Consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(ZBool.False, ConsolWrapper.PrintMastersOnManifest);
		}

		public void TestPrintColoadOnOtherDocs()
		{
			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(ZBool.True, ConsolWrapper.PrintColoadOnOtherDocs);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(ZBool.False, ConsolWrapper.PrintColoadOnOtherDocs);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(ZBool.True, ConsolWrapper.PrintColoadOnOtherDocs);
		}

		public void TestPrintMastersOnOtherDocs()
		{
			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(ZBool.True, ConsolWrapper.PrintMastersOnOtherDocs);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(ZBool.True, ConsolWrapper.PrintMastersOnOtherDocs);

			Consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(ZBool.False, ConsolWrapper.PrintMastersOnOtherDocs);
		}

		public void TestManifestTotalShipmentWeight()
		{
			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualWeight = 10.5M;
			standAloneShipment.JS_UnitOfWeight = "KG";

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualWeight = 3.5M;
			masterShipment.JS_UnitOfWeight = "KG";

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_ActualWeight = 5.5M;
			subShipmentA.JS_UnitOfWeight = "KG";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_ActualWeight = 10M;
			subShipmentB.JS_UnitOfWeight = "G";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualWeight = 1M;
			masterShipment2.JS_UnitOfWeight = "KG";

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_ActualWeight = 2M;
			subShipmentC.JS_UnitOfWeight = "T";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_ActualWeight = 300M;
			subShipmentD.JS_UnitOfWeight = "G";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			ZDecimal result = 5.5M;
			result += Core.Constants.Weight.ConvertSafe(10M, "G", "KG");
			result += Core.Constants.Weight.ConvertSafe(2M, "T", "KG");
			result += Core.Constants.Weight.ConvertSafe(300M, "G", "KG");
			result += 10.5M;

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals(result.ToString(2) + " KG", ConsolWrapper.ManifestTotalShipmentWeight);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("15 KG", ConsolWrapper.ManifestTotalShipmentWeight);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("15 KG", ConsolWrapper.ManifestTotalShipmentWeight);

			standAloneShipment.JS_ActualWeight = 23.149M;
			standAloneShipment.JS_UnitOfWeight = "LB";

			AssertEquals("15 KG", ConsolWrapper.ManifestTotalShipmentWeight);
		}

		public void TestManifestTotalShipmentWeightImperial()
		{
			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualWeight = 10.5M;
			standAloneShipment.JS_UnitOfWeight = "LB";

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualWeight = 3.5M;
			masterShipment.JS_UnitOfWeight = "LB";

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_ActualWeight = 5.5M;
			subShipmentA.JS_UnitOfWeight = "LB";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_ActualWeight = 10M;
			subShipmentB.JS_UnitOfWeight = "OZ";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualWeight = 1M;
			masterShipment2.JS_UnitOfWeight = "LB";

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_ActualWeight = 2M;
			subShipmentC.JS_UnitOfWeight = "LT";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_ActualWeight = 300M;
			subShipmentD.JS_UnitOfWeight = "OZ";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			ZDecimal result = 5.5M;
			result += Core.Constants.Weight.ConvertSafe(10M, "OZ", "LB");
			result += Core.Constants.Weight.ConvertSafe(2M, "LT", "LB");
			result += Core.Constants.Weight.ConvertSafe(300M, "OZ", "LB");
			result += 10.5M;

			result = Utilities.Round(result, 3);

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals(result.ToString(3) + " LB", ConsolWrapper.ManifestTotalShipmentWeight);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("15 LB", ConsolWrapper.ManifestTotalShipmentWeight);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("15 LB", ConsolWrapper.ManifestTotalShipmentWeight);

			standAloneShipment.JS_ActualWeight = 4.763M;
			standAloneShipment.JS_UnitOfWeight = "KG";

			AssertEquals(Utilities.Round(Constants.Weight.ConvertSafe(15m, "LB", "KG"), 3) + " KG", ConsolWrapper.ManifestTotalShipmentWeight);
		}

		public void TestManifestTotalShipmentVolume()
		{
			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualVolume = 10.500M;
			standAloneShipment.JS_UnitOfVolume = "M3";

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualVolume = 3.500M;
			masterShipment.JS_UnitOfVolume = "M3";

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_ActualVolume = 5.500M;
			subShipmentA.JS_UnitOfVolume = "M3";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_ActualVolume = 10.00M;
			subShipmentB.JS_UnitOfVolume = "L";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualVolume = 1.00M;
			masterShipment2.JS_UnitOfVolume = "M3";

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_ActualVolume = 2.00M;
			subShipmentC.JS_UnitOfVolume = "ML";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_ActualVolume = 300.00M;
			subShipmentD.JS_UnitOfVolume = "CF";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			ZDecimal result = 5.5M;
			result += Core.Constants.Volume.ConvertSafe(10.00M, "L", "M3");
			result += Core.Constants.Volume.ConvertSafe(2.00M, "ML", "M3");
			result += Core.Constants.Volume.ConvertSafe(300.00M, "CF", "M3");
			result += 10.5M;

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals(result.ToString(3) + " M3", ConsolWrapper.ManifestTotalShipmentVolume);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("15 M3", ConsolWrapper.ManifestTotalShipmentVolume);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("15 M3", ConsolWrapper.ManifestTotalShipmentVolume);
		}

		public void TestManifestTotalShipmentVolumeImperial()
		{
			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualVolume = 10.500M;
			standAloneShipment.JS_UnitOfVolume = "CF";

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualVolume = 3.500M;
			masterShipment.JS_UnitOfVolume = "CF";

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_ActualVolume = 5.500M;
			subShipmentA.JS_UnitOfVolume = "CF";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_ActualVolume = 10.00M;
			subShipmentB.JS_UnitOfVolume = "CI";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualVolume = 1.00M;
			masterShipment2.JS_UnitOfVolume = "CF";

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_ActualVolume = 2.00M;
			subShipmentC.JS_UnitOfVolume = "CY";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_ActualVolume = 300.00M;
			subShipmentD.JS_UnitOfVolume = "CF";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			ZDecimal result = 5.5M;
			result += Core.Constants.Volume.ConvertSafe(10.00M, "CI", "CF");
			result += Core.Constants.Volume.ConvertSafe(2.00M, "CY", "CF");
			result += Core.Constants.Volume.ConvertSafe(300.00M, "CF", "CF");
			result += 10.5M;

			result = Utilities.Round(result, 3);

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals(result.ToString(3) + " CF", ConsolWrapper.ManifestTotalShipmentVolume);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("15 CF", ConsolWrapper.ManifestTotalShipmentVolume);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("15 CF", ConsolWrapper.ManifestTotalShipmentVolume);

			standAloneShipment.JS_ActualVolume = 0.2975M;
			standAloneShipment.JS_UnitOfVolume = "M3";

			AssertEquals(Utilities.Round(Constants.Volume.ConvertSafe(15m, "CF", "M3"), 3) + " M3", ConsolWrapper.ManifestTotalShipmentVolume);
		}

		public void TestManifestTotalShipmentPackages()
		{
			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_OuterPacks = 5;

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_OuterPacks = 3;
			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_OuterPacks = 5;
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;
			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_OuterPacks = 10;
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_OuterPacks = 1;
			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_OuterPacks = 2;
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;
			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_OuterPacks = 30;
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals(9, ConsolWrapper.ManifestTotalShipmentPackages);

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals(52, ConsolWrapper.ManifestTotalShipmentPackages);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals(9, ConsolWrapper.ManifestTotalShipmentPackages);
		}

		public void TestManifestTotalShipmentChargeableAirWeightVolume()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			ForwardingShipment standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualWeight = 10.500M;
			standAloneShipment.JS_UnitOfWeight = "KG";
			standAloneShipment.JS_ActualVolume = 20.500M;
			standAloneShipment.JS_UnitOfVolume = "M3";

			ForwardingShipment masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualWeight = 6.500M;
			masterShipment.JS_UnitOfWeight = "KG";
			masterShipment.JS_ActualVolume = 3.500M;
			masterShipment.JS_UnitOfVolume = "M3";

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentA.JS_ActualWeight = 5.500M;
			subShipmentA.JS_UnitOfWeight = "KG";
			subShipmentA.JS_ActualVolume = 10.500M;
			subShipmentA.JS_UnitOfVolume = "M3";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentB.JS_ActualWeight = 20.00M;
			subShipmentB.JS_UnitOfWeight = "G";
			subShipmentB.JS_ActualVolume = 10.00M;
			subShipmentB.JS_UnitOfVolume = "L";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualWeight = 1.00M;
			masterShipment2.JS_UnitOfWeight = "KG";
			masterShipment2.JS_ActualVolume = 2.00M;
			masterShipment2.JS_UnitOfVolume = "M3";

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentC.JS_ActualWeight = 4.00M;
			subShipmentC.JS_UnitOfWeight = "T";
			subShipmentC.JS_ActualVolume = 2.00M;
			subShipmentC.JS_UnitOfVolume = "ML";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentD.JS_ActualWeight = 300.00M;
			subShipmentD.JS_UnitOfWeight = "G";
			subShipmentD.JS_ActualVolume = 600.00M;
			subShipmentD.JS_UnitOfVolume = "CF";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals("341333.352 KG", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("4333.333 KG", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("4333.333 KG", ConsolWrapper.ManifestTotalShipmentChargeable);
		}

		public void TestManifestTotalShipmentChargeableSeaWeightVolume()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualWeight = 10.500M;
			standAloneShipment.JS_UnitOfWeight = "KG";
			standAloneShipment.JS_ActualVolume = 20.500M;
			standAloneShipment.JS_UnitOfVolume = "M3";

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualWeight = 6.500M;
			masterShipment.JS_UnitOfWeight = "KG";
			masterShipment.JS_ActualVolume = 3.500M;
			masterShipment.JS_UnitOfVolume = "M3";

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentA.JS_ActualWeight = 5.500M;
			subShipmentA.JS_UnitOfWeight = "KG";
			subShipmentA.JS_ActualVolume = 10.500M;
			subShipmentA.JS_UnitOfVolume = "M3";
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentB.JS_ActualWeight = 20.00M;
			subShipmentB.JS_UnitOfWeight = "G";
			subShipmentB.JS_ActualVolume = 10.00M;
			subShipmentB.JS_UnitOfVolume = "L";
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			ForwardingShipment masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualWeight = 1.00M;
			masterShipment2.JS_UnitOfWeight = "KG";
			masterShipment2.JS_ActualVolume = 2.00M;
			masterShipment2.JS_UnitOfVolume = "M3";

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentC.JS_ActualWeight = 4.00M;
			subShipmentC.JS_UnitOfWeight = "T";
			subShipmentC.JS_ActualVolume = 2.00M;
			subShipmentC.JS_UnitOfVolume = "ML";
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentD.JS_ActualWeight = 300.00M;
			subShipmentD.JS_UnitOfWeight = "G";
			subShipmentD.JS_ActualVolume = 600.00M;
			subShipmentD.JS_UnitOfVolume = "CF";
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals("2048 M3", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("26 M3", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("26 M3", ConsolWrapper.ManifestTotalShipmentChargeable);
		}

		public void TestManifestTotalShipmentChargeableAir()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualChargeable = 10.500M;

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualChargeable = 6.500M;

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentA.JS_ActualChargeable = 5.500M;
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentB.JS_ActualChargeable = 20.00M;
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualChargeable = 1.00M;

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentC.JS_ActualChargeable = 4.00M;
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_TransportMode = Core.Constants.TransportModes.Air;
			subShipmentD.JS_ActualChargeable = 300.00M;
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals("340 KG", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("18 KG", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("18 KG", ConsolWrapper.ManifestTotalShipmentChargeable);
		}

		public void TestManifestTotalShipmentChargeableSea()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var standAloneShipment = Consol.Shipments.AddNew();
			standAloneShipment.JS_GoodsDescription = "StandAlone shipment";
			standAloneShipment.JS_ActualChargeable = 10.500M;

			var masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_GoodsDescription = "Master shipment";
			masterShipment.JS_ActualChargeable = 6.500M;

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentA.JS_ActualChargeable = 5.500M;
			subShipmentA.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentB.JS_ActualChargeable = 20.00M;
			subShipmentB.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var masterShipment2 = Consol.Shipments.AddNew();
			masterShipment2.JS_GoodsDescription = "Master shipment 2";
			masterShipment2.JS_ActualChargeable = 1.00M;

			var subShipmentC = Factory.New<ForwardingShipment>();
			subShipmentC.JS_GoodsDescription = "shipment C";
			subShipmentC.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentC.JS_ActualChargeable = 4.00M;
			subShipmentC.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			var subShipmentD = Factory.New<ForwardingShipment>();
			subShipmentD.JS_GoodsDescription = "shipment D";
			subShipmentD.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipmentD.JS_ActualChargeable = 300.00M;
			subShipmentD.JS_JS_ColoadMasterShipment = masterShipment2.PK;

			Consol.JK_PrintOptionForColoadsOnManifest = "SUB";
			AssertEquals("340 M3", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "MAS";
			AssertEquals("18 M3", ConsolWrapper.ManifestTotalShipmentChargeable);

			Consol.JK_PrintOptionForColoadsOnManifest = "ALL";
			AssertEquals("18 M3", ConsolWrapper.ManifestTotalShipmentChargeable);
		}

		public void TestQuotedAmount()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.QuotedAmount);
		}

		public void TestAuthorizedSignature()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.AuthorizedSignature);
		}

		public void TestStop()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.Stop);
		}

		public void TestShipperReference()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ShipperReference);
		}

		public void TestConsigneeReference()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeReference);
		}

		public void TestTrailerNo()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.TrailerNo);
		}

		public void TestSealNo()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.SealNo);
		}

		public void TestTractorNo()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.TractorNo);
		}

		public void TestEmergencyNo()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.EmergencyNo);
		}

		public void TestDateSigned()
		{
			AssertEquals(ZDateTime.Empty, ConsolWrapper.DateSigned);
		}

		#endregion

		#region Forwarding Instruction Test

		public void TestPlaceOfReceipt()
		{
			var consolPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var shipmentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));

			AssertEquals("PlaceOfReceipt", ZString.Empty, ConsolWrapper.PlaceOfReceipt);

			Consol.JK_RL_NKLoadPort = consolPort.RL_Code;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "";

			AssertEquals("Shipments.Count", 0, Consol.Shipments.Count);
			AssertEquals("PlaceOfReceipt", consolPort.RL_PortName, ConsolWrapper.PlaceOfReceipt);

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = shipmentPort.RL_Code;
			AssertEquals("PlaceOfReceipt", shipmentPort.RL_PortName, ConsolWrapper.PlaceOfReceipt);

			shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = shipmentPort.RL_Code;
			AssertEquals("PlaceOfReceipt", shipmentPort.RL_PortName, ConsolWrapper.PlaceOfReceipt);

			shipment.JS_RL_NKOrigin = anotherPort.RL_Code;
			AssertEquals("PlaceOfReceipt", consolPort.RL_PortName, ConsolWrapper.PlaceOfReceipt);

			DocumentEngine.DocumentNote documentNote = Enterprise.DocumentEngine.DocumentNote.LoadNote(Consol);
			documentNote.SetSystemDefinedFieldValue(DocForwardingConsol.SDFields.PlaceOfReceipt, "Kherson");
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("PlaceOfReceipt", "Kherson", ConsolWrapper.PlaceOfReceipt);
		}

		public void TestPlaceOfDelivery()
		{
			var consolPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var shipmentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));

			AssertEquals("PlaceOfDelivery", ZString.Empty, ConsolWrapper.PlaceOfDelivery);

			Consol.JK_RL_NKDischargePort = consolPort.RL_Code;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "";

			AssertEquals("Shipments.Count", 0, Consol.Shipments.Count);
			AssertEquals("PlaceOfDelivery", consolPort.RL_PortName, ConsolWrapper.PlaceOfDelivery);

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = shipmentPort.RL_Code;
			AssertEquals("PlaceOfDelivery", shipmentPort.RL_PortName, ConsolWrapper.PlaceOfDelivery);

			shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = shipmentPort.RL_Code;
			AssertEquals("PlaceOfDelivery", shipmentPort.RL_PortName, ConsolWrapper.PlaceOfDelivery);

			shipment.JS_RL_NKDestination = anotherPort.RL_Code;
			AssertEquals("PlaceOfDelivery", consolPort.RL_PortName, ConsolWrapper.PlaceOfDelivery);

			DocumentEngine.DocumentNote documentNote = Enterprise.DocumentEngine.DocumentNote.LoadNote(Consol);
			documentNote.SetSystemDefinedFieldValue(DocForwardingConsol.SDFields.PlaceOfDelivery, "Kherson");
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("PlaceOfDelivery", "Kherson", ConsolWrapper.PlaceOfDelivery);
		}

		public void TestFreightPayableAt()
		{
			var loadPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var discPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL"));
			var originPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var destPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));

			Consol.JK_PrepaidCollect = "PPD";
			AssertEquals("FreightPayableAt", ZString.Empty, ConsolWrapper.FreightPayableAt);
			Consol.JK_PrepaidCollect = "";
			AssertEquals("FreightPayableAt", ZString.Empty, ConsolWrapper.FreightPayableAt);

			Consol.JK_RL_NKLoadPort = loadPort.RL_Code;
			Consol.JK_RL_NKDischargePort = discPort.RL_Code;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "";
			transport.JW_RL_NKDiscPort = "";
			Consol.JK_PrepaidCollect = "PPD";
			AssertEquals("FreightPayableAt", loadPort.RL_PortName, ConsolWrapper.FreightPayableAt);
			Consol.JK_PrepaidCollect = "CCX";
			AssertEquals("FreightPayableAt", discPort.RL_PortName, ConsolWrapper.FreightPayableAt);
			Consol.JK_PrepaidCollect = "";
			AssertEquals("FreightPayableAt", "", ConsolWrapper.FreightPayableAt);

			AssertEquals("Shipments.Count", 0, Consol.Shipments.Count);
			CommonShipment shipment1 = Consol.Shipments.AddNew();
			AssertEquals("Shipments.Count", 1, Consol.Shipments.Count);
			shipment1.JS_RL_NKOrigin = originPort.RL_Code;
			shipment1.JS_RL_NKDestination = destPort.RL_Code;
			shipment1.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //Prepaid
			AssertEquals("FreightPayableAt", "", ConsolWrapper.FreightPayableAt);
			Consol.JK_PrepaidCollect = "PPD";
			AssertEquals("FreightPayableAt", loadPort.RL_PortName, ConsolWrapper.FreightPayableAt);

			Consol.JK_AgentType = "DRT";
			AssertEquals("FreightPayableAt", originPort.RL_PortName, ConsolWrapper.FreightPayableAt);
			shipment1.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //Collect
			AssertEquals("FreightPayableAt", destPort.RL_PortName, ConsolWrapper.FreightPayableAt);
			shipment1.JS_RL_NKDestination = "";
			AssertEquals("FreightPayableAt", discPort.RL_PortName, ConsolWrapper.FreightPayableAt);
			shipment1.JS_INCO = "";
			AssertEquals("FreightPayableAt", "", ConsolWrapper.FreightPayableAt);
			shipment1.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //Prepaid
			shipment1.JS_RL_NKOrigin = "";
			AssertEquals("FreightPayableAt", loadPort.RL_PortName, ConsolWrapper.FreightPayableAt);
		}

		public void TestMakeBillOutTo()
		{
			AssertEquals("Forwarder", ConsolWrapper.MakeBillOutTo);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Shipper", ConsolWrapper.MakeBillOutTo);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("Forwarder", ConsolWrapper.MakeBillOutTo);
		}

		public void TestAlertText()
		{
			try
			{
				DocumentsDataRegistry.Instance.OrderDelayAlertText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "A test message for alert");
				AssertEquals("A test message for alert", ConsolWrapper.AlertText);
			}
			finally
			{
				((IRegistryItemInternals)DocumentsDataRegistry.Instance.OrderDelayAlertText).ClearCache();
			}
		}

		public void TestConsignor()
		{
			AssertEquals("Consignor", ZString.Empty, ConsolWrapper.Consignor);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Consignor", ZString.Empty, ConsolWrapper.Consignor);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("Consignor", ZString.Empty, ConsolWrapper.Consignor);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.ConsignorPK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Consignor", organisationWrapper.PostalAddress, ConsolWrapper.Consignor);

			Consol.JK_AgentType = "";
			Consol.SetDefaultSendingForwarderAddress(header);
			AssertEquals("Consignor", organisationWrapper.PostalAddress, ConsolWrapper.Consignor);
		}

		public void TestConsignorName()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorName);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorName);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorName);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_FullName = "XXX";
			shipment.ConsignorPK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("XXX", ConsolWrapper.ConsignorName);

			Consol.JK_AgentType = "";
			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "121";
			Consol.SetDefaultSendingForwarderAddress(header1);
			AssertEquals("121", ConsolWrapper.ConsignorName);
		}

		public void TestConsignorPhoneNumber()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorPhoneNumber);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorPhoneNumber);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorPhoneNumber);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.MainAddress.OA_Phone = "123";
			shipment.ConsignorPK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("123", ConsolWrapper.ConsignorPhoneNumber);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.MainAddress.OA_Phone = "121";
			Consol.JK_AgentType = "";
			Consol.SetDefaultSendingForwarderAddress(header1);
			AssertEquals("121", ConsolWrapper.ConsignorPhoneNumber);
		}

		public void TestConsignee()
		{
			AssertEquals("Consignee", ZString.Empty, ConsolWrapper.Consignee);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Consignee", ZString.Empty, ConsolWrapper.Consignee);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("Consignee", ZString.Empty, ConsolWrapper.Consignee);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.ConsigneePK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Consignee", organisationWrapper.PostalAddress, ConsolWrapper.Consignee);

			Consol.JK_AgentType = "";
			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.MainAddress.OA_Address1 = "121";
			Consol.SetDefaultReceivingForwarderAddress(header1);
			organisationWrapper = DocOrganisation.New(header1, Factory);
			AssertEquals("Consignee", organisationWrapper.PostalAddress, ConsolWrapper.Consignee);
		}

		public void TestConsigneeName()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeName);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeName);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeName);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_FullName = "XXX";
			shipment.ConsigneePK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("XXX", ConsolWrapper.ConsigneeName);

			Consol.JK_AgentType = "";
			Consol.SetDefaultReceivingForwarderAddress(header);
			AssertEquals("XXX", organisationWrapper.PostalAddress, ConsolWrapper.Consignee);
		}

		public void TestConsigneePhoneNumber()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneePhoneNumber);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneePhoneNumber);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneePhoneNumber);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.MainAddress.OA_Phone = "123";
			shipment.ConsigneePK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("123", ConsolWrapper.ConsigneePhoneNumber);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.MainAddress.OA_Phone = "121";
			Consol.JK_AgentType = "";
			Consol.SetDefaultReceivingForwarderAddress(header1);
			AssertEquals("121", ConsolWrapper.ConsigneePhoneNumber);
		}

		public void TestCarrier()
		{
			AssertEquals("Carrier", ZString.Empty, ConsolWrapper.Carrier);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			Consol.SetDefaultShippingLineAddress(shippingLine);
			DocOrganisation shippingLineWrapper = DocOrganisation.New(shippingLine, Factory);
			AssertEquals("Carrier", shippingLineWrapper.PostalAddress, ConsolWrapper.Carrier);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("Carrier", shippingLineWrapper.PostalAddress, ConsolWrapper.Carrier);

			ZQuery filter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, shippingLine.PK);
			var creditor = Factory.LoadTop1<OrgHeader>(filter);
			Consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			DocOrganisation creditorWrapper = DocOrganisation.New(creditor, Factory);
			AssertEquals("Carrier", creditorWrapper.PostalAddress, ConsolWrapper.Carrier);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("Carrier", creditorWrapper.PostalAddress, ConsolWrapper.Carrier);
		}

		public void TestCarrierName()
		{
			AssertEquals("Carrier Name", ZString.Empty, ConsolWrapper.CarrierName);

			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			DocOrganisation shippingLineWrapper = DocOrganisation.New(shippingLine, Factory);
			AssertEquals("Carrier Name", shippingLineWrapper.Name, ConsolWrapper.CarrierName);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("Carrier", shippingLineWrapper.Name, ConsolWrapper.CarrierName);

			ZQuery filter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, shippingLine.PK);
			var creditor = Factory.LoadTop1<OrgHeader>(filter);
			Consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			DocOrganisation creditorWrapper = DocOrganisation.New(creditor, Factory);
			AssertEquals("Carrier Name", creditorWrapper.Name, ConsolWrapper.CarrierName);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("Carrier Name", creditorWrapper.Name, ConsolWrapper.CarrierName);
		}

		public void TestCarrierPhoneNumber()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.CarrierPhoneNumber);

			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			shippingLine.MainAddress.OA_Phone = "123";
			DocOrganisation shippingLineWrapper = DocOrganisation.New(shippingLine, Factory);
			AssertEquals("123", ConsolWrapper.CarrierPhoneNumber);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("123", ConsolWrapper.CarrierPhoneNumber);

			ZQuery filter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, shippingLine.PK);
			var creditor = Factory.LoadTop1<OrgHeader>(filter);
			creditor.MainAddress.OA_Phone = "121";
			Consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			DocOrganisation creditorWrapper = DocOrganisation.New(creditor, Factory);
			AssertEquals("121", ConsolWrapper.CarrierPhoneNumber);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("121", ConsolWrapper.CarrierPhoneNumber);
		}

		public void TestDirectShipmentNotifyPartyUDF()
		{
			AssertEquals("DirectShipmentNotifyPartyUDF", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyUDF);
			AssertEquals("DirectShipmentNotifyPartyPhone", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyPhone);
			AssertEquals("DirectShipmentNotifyPartyFax", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyFax);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("DirectShipmentNotifyPartyUDF", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyUDF);
			AssertEquals("DirectShipmentNotifyPartyPhone", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyPhone);
			AssertEquals("DirectShipmentNotifyPartyFax", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyFax);

			Env.Registry.NotifyPartyDefaultText = "Registry Default Text";
			AssertEquals("DirectShipmentNotifyPartyUDF", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyUDF);
			AssertEquals("DirectShipmentNotifyPartyPhone", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyPhone);
			AssertEquals("DirectShipmentNotifyPartyFax", ZString.Empty, ConsolWrapper.DirectShipmentNotifyPartyFax);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("DirectShipmentNotifyPartyUDF", "Registry Default Text", ConsolWrapper.DirectShipmentNotifyPartyUDF);
			AssertEquals("DirectShipmentNotifyPartyPhone", string.Empty, ConsolWrapper.DirectShipmentNotifyPartyPhone);
			AssertEquals("DirectShipmentNotifyPartyFax", string.Empty, ConsolWrapper.DirectShipmentNotifyPartyFax);

			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			contact.OC_Phone = "123";
			contact.OC_Fax = "234";
			shipment.NotifyPartyDocumentaryAddress.ContactPK = contact.PK;
			DocContacts contactWrapper = DocContacts.New(contact, Factory);
			AssertEquals("DirectShipmentNotifyPartyUDF", contactWrapper.PostalAddress, ConsolWrapper.DirectShipmentNotifyPartyUDF);
			AssertEquals("DirectShipmentNotifyPartyPhone", "123", ConsolWrapper.DirectShipmentNotifyPartyPhone);
			AssertEquals("DirectShipmentNotifyPartyFax", "234", ConsolWrapper.DirectShipmentNotifyPartyFax);

			DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(shipment);
			docNote.SetFieldValue("Notify Party", "Shipment UDF");
			Factory.Save();
			AssertEquals("DirectShipmentNotifyPartyUDF", "Shipment UDF", ConsolWrapper.DirectShipmentNotifyPartyUDF);
			AssertEquals("DirectShipmentNotifyPartyPhone", string.Empty, ConsolWrapper.DirectShipmentNotifyPartyPhone);
			AssertEquals("DirectShipmentNotifyPartyFax", string.Empty, ConsolWrapper.DirectShipmentNotifyPartyFax);
		}

		public void TestReceivingForwarderNotifyParty()
		{
			AssertEquals("ReceivingForwarder", null, ConsolWrapper.ReceivingForwarder);
			AssertEquals("ReceivingForwarderNotifyParty", ZString.Empty, ConsolWrapper.ReceivingForwarderNotifyParty);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultReceivingForwarderAddress(org);
			AssertNotNull("ReceivingForwarder", ConsolWrapper.ReceivingForwarder);
			org.Contacts.RemoveAll();
			DocOrganisation orgWrapper = DocOrganisation.New(org, Factory);
			AssertEquals("ReceivingForwarderNotifyParty", orgWrapper.PostalAddress, ConsolWrapper.ReceivingForwarderNotifyParty);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TEST NAME";
			AssertEquals("ReceivingForwarderNotifyParty", orgWrapper.PostalAddress, ConsolWrapper.ReceivingForwarderNotifyParty);

			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;
			AssertEquals("ReceivingForwarderNotifyParty", "TEST NAME\n" + orgWrapper.PostalAddress, ConsolWrapper.ReceivingForwarderNotifyParty);

			org.Addresses.RemoveAll();
			OrgAddress address = org.Addresses.AddNew();
			address.OA_Address1 = "TEST ADDRESS";
			contact.OC_OA_OrgAddress = address.PK;
			AssertEquals("ReceivingForwarderNotifyParty", DocContacts.New(contact, Factory).PostalAddress, ConsolWrapper.ReceivingForwarderNotifyParty);
		}

		public void TestNotifyPartyAddress()
		{
			AssertEquals("NotifyPartyAddress", ZString.Empty, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", ZString.Empty, ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", ZString.Empty, ConsolWrapper.NotifyPartyFax);

			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultReceivingForwarderAddress(org);
			AssertNotNull("ReceivingForwarder", ConsolWrapper.ReceivingForwarder);

			org.Contacts.RemoveAll();
			DocOrganisation orgWrapper = DocOrganisation.New(org, Factory);
			AssertEquals("NotifyPartyAddress", orgWrapper.PostalAddress, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", orgWrapper.Phone, ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", orgWrapper.Fax, ConsolWrapper.NotifyPartyFax);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TEST NAME";
			contact.OC_Phone = "123";
			contact.OC_Fax = "234";

			orgWrapper = DocOrganisation.New(org, Factory);
			AssertEquals("NotifyPartyAddress", orgWrapper.PostalAddress, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", orgWrapper.Phone, ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", orgWrapper.Fax, ConsolWrapper.NotifyPartyFax);

			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;
			orgWrapper = DocOrganisation.New(org, Factory);
			AssertEquals("NotifyPartyAddress", "TEST NAME\n" + orgWrapper.PostalAddress, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", "123", ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", "234", ConsolWrapper.NotifyPartyFax);

			org.Addresses.RemoveAll();
			OrgAddress address = org.Addresses.AddNew();
			address.OA_Address1 = "TEST ADDRESS";
			contact.OC_OA_OrgAddress = address.PK;
			AssertEquals("NotifyPartyAddress", DocContacts.New(contact, Factory).PostalAddress, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", DocContacts.New(contact, Factory).Phone, ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", DocContacts.New(contact, Factory).Fax, ConsolWrapper.NotifyPartyFax);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "aaa";
			org2.OH_FullName = "bbb";
			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "fff";
			OrgDocument document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.Marketing.Code;
			Consol.NotifyPartyDocumentaryAddress.OrganisationPK = org2.PK;
			Consol.NotifyPartyDocumentaryAddress.ContactPK = contact2.PK;
			AssertEquals("NotifyPartyAddress", DocDocAddress.New(Consol.NotifyPartyDocumentaryAddress, Factory).PostalAddress, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", DocDocAddress.New(Consol.NotifyPartyDocumentaryAddress, Factory).Phone, ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", DocDocAddress.New(Consol.NotifyPartyDocumentaryAddress, Factory).Fax, ConsolWrapper.NotifyPartyFax);

			Consol.NotifyPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("NotifyPartyAddress", DocContacts.New(contact, Factory).PostalAddress, ConsolWrapper.NotifyPartyAddress);
			AssertEquals("NotifyPartyPhone", DocContacts.New(contact, Factory).Phone, ConsolWrapper.NotifyPartyPhone);
			AssertEquals("NotifyPartyFax", DocContacts.New(contact, Factory).Fax, ConsolWrapper.NotifyPartyFax);
		}

		public void TestShippersReference()
		{
			AssertEquals("ShippersReference", ZString.Empty, ConsolWrapper.ShippersReference);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ShippersReference", ZString.Empty, ConsolWrapper.ShippersReference);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("ShippersReference", ZString.Empty, ConsolWrapper.ShippersReference);

			shipment.JS_UniqueConsignRef = "S11111111";
			AssertEquals("ShippersReference", "S11111111", ConsolWrapper.ShippersReference);

			shipment.JS_BookingReference = "99999999";
			AssertEquals("ShippersReference", "99999999", ConsolWrapper.ShippersReference);
		}

		public void TestExportPermits()
		{
			AssertEquals("ExportPermits", "", ConsolWrapper.ExportPermits);

			BusinessObject cusEntryNum = Consol.CusEntryNums.AddNew();
			cusEntryNum[CusEntryNumSchema.Constants.CE_EntryNum] = "CRN 11111111";
			AssertEquals("ExportPermits", "", ConsolWrapper.ExportPermits);

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForCurrentCountry();
			AssertEquals("ExportPermits", "CRN 11111111", ConsolWrapper.ExportPermits);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ExportPermits", "CRN 11111111", ConsolWrapper.ExportPermits);

			var shipment = Consol.Shipments.AddNew();
			shipment.CustomsEntryNumberType = "ECN";
			shipment.CustomsEntryNumber = "99999999";
			AssertEquals("ExportPermits", "ECN 99999999 CRN 11111111", ConsolWrapper.ExportPermits);
		}

		public void TestFreightTerms()
		{
			AssertEquals("FreightTerms", ZString.Empty, ConsolWrapper.FreightTerms);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			ZString expected = Consol.JK_PrepaidCollect_List.GetDescriptionFromCode(Core.Constants.PaymentType.Prepaid).ToUpper();
			AssertEquals("FreightTerms", expected, ConsolWrapper.FreightTerms);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("FreightTerms", ZString.Empty, ConsolWrapper.FreightTerms);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals("FreightTerms", ZString.Empty, ConsolWrapper.FreightTerms);

			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			expected = "FREIGHT PREPAID";
			AssertEquals("FreightTerms", expected, ConsolWrapper.FreightTerms);

			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			expected = "FREIGHT COLLECT";
			AssertEquals("FreightTerms", expected, ConsolWrapper.FreightTerms);
		}

		public void TestGoodsSummary()
		{
			ZString expected = "Total Package(s): 0";
			AssertEquals("GoodsSummary", expected, ConsolWrapper.GoodsSummary);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("GoodsSummary", ZString.Empty, ConsolWrapper.GoodsSummary);

			FreightContainer container1 = Consol.Containers.AddNew();
			FreightContainer container2 = Consol.Containers.AddNew();
			FreightContainer container3 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container2.JC_ContainerNum = "2";
			container3.JC_ContainerNum = "3";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = "";

			RefContainer refContainer1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20NOR");
			RefContainer refContainer2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			RefContainer refContainer3 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20NOR");
			container1.JC_RC = refContainer1.PK;
			container2.JC_RC = refContainer2.PK;
			container3.JC_RC = refContainer3.PK;

			var shipment = Consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 50;
			PackLine line1 = shipment.OuterPackLines.AddNew();
			PackLine line2 = shipment.OuterPackLines.AddNew();
			PackLine line3 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line2.JL_PackageCount = 10;
			line3.JL_PackageCount = 0;

			container1.PackLines.RemoveAll();
			container2.PackLines.RemoveAll();
			container3.PackLines.RemoveAll();
			line1.SetContainer(Consol, container1);
			line2.SetContainer(Consol, container1);

			var subShipment1 = Consol.Shipments.AddNew();
			var subShipment2 = Consol.Shipments.AddNew();

			subShipment1.JS_JS_ColoadMasterShipment = shipment.PK;
			PackLine subLine1 = subShipment1.OuterPackLines.AddNew();
			subLine1.JL_PackageCount = 10;
			subLine1.SetContainer(Consol, container1);

			subShipment2.JS_JS_ColoadMasterShipment = shipment.PK;
			PackLine subLine2 = subShipment2.OuterPackLines.AddNew();
			subLine2.JL_PackageCount = 20;
			subLine2.SetContainer(Consol, container2);

			Factory.Save();

			expected = "1 x " + refContainer1.RC_Code + " Container STC 20 Package(s); 50 LCL Package(s)";
			AssertEquals("GoodsSummary", expected, ConsolWrapper.GoodsSummary);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			Factory.Save();
			AssertEquals("GoodsSummary", expected, ConsolWrapper.GoodsSummary);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			Factory.Save();
			AssertEquals("GoodsSummary", expected, ConsolWrapper.GoodsSummary);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();
			expected = "Total Package(s): 50";
			AssertEquals("GoodsSummary", expected, ConsolWrapper.GoodsSummary);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container3.JC_ContainerNum = Core.Constants.ContainerModes.FCL;
			line3.JL_PackageCount = 10;
			line3.SetContainer(Consol, container2);
			PackLine line4 = shipment.OuterPackLines.AddNew();
			line4.SetContainer(Consol, container3);
			Factory.Save();

			expected = "2 x " + refContainer1.RC_Code + " Container STC 20 Package(s); 1 x " + refContainer2.RC_Code + " Container STC 10 Package(s); 50 LCL Package(s)";
			AssertEquals("GoodsSummary", expected, ConsolWrapper.GoodsSummary);

			Consol.JK_RL_NKDischargePort = "USLAX";
			expected = expected.Replace("STC ", "");
			AssertEquals("GoodsSummary when Consol is bound for US should not show STC", expected, ConsolWrapper.GoodsSummary);
		}

		public void TestShowMarksAndNumbers()
		{
			Consol.JK_AgentType = "";
			Consol.JK_ConsolMode = "";
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("ShowMarksAndNumbers", true, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			AssertEquals("ShowMarksAndNumbers", true, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = "";
			AssertEquals("ShowMarksAndNumbers", true, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_AgentType = Core.Constants.AgentType.Charter;
			Consol.JK_ConsolMode = "";
			AssertEquals("ShowMarksAndNumbers", true, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("ShowMarksAndNumbers", false, ConsolWrapper.ShowMarksAndNumbers);
		}

		public void TestShowBlankTotals()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("ShowBlankTotals", true, ConsolWrapper.ShowBlankTotals);

			var container1 = (FreightContainer)Consol.Containers.AddNew();
			var container2 = (FreightContainer)Consol.Containers.AddNew();
			var container3 = (FreightContainer)Consol.Containers.AddNew();
			AssertEquals("ShowBlankTotals", true, ConsolWrapper.ShowBlankTotals);

			var shipment = Consol.Shipments.AddNew();
			var packLine = (PackLine)shipment.OuterPackLines.AddNew();
			packLine.SetContainer(Consol, container2);
			AssertEquals("ShowBlankTotals", true, ConsolWrapper.ShowBlankTotals);

			packLine.JL_ActualVolume = 10M;
			AssertEquals("ShowBlankTotals", false, ConsolWrapper.ShowBlankTotals);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("ShowBlankTotals", false, ConsolWrapper.ShowBlankTotals);

			packLine.JL_ActualVolume = 0M;
			AssertEquals("ShowBlankTotals", true, ConsolWrapper.ShowBlankTotals);
		}

		public void TestPackType()
		{
			AssertEquals("PIECE(S)", ConsolWrapper.PackType);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var shipment = Consol.Shipments.AddNew();
			AssertEquals("PALLET(S)", ConsolWrapper.PackType);

			shipment.JS_F3_NKPackType = "BOX";
			AssertEquals("BOX(S)", ConsolWrapper.PackType);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			shipment.JS_F3_NKPackType = "XXX";
			AssertEquals("", ConsolWrapper.PackType);
		}

		public void TestConsigneeOrganisation()
		{
			AssertEquals("ConsigneeOrganisation", null, ConsolWrapper.ConsigneeOrganisation);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var shipment = Consol.Shipments.AddNew();
			AssertEquals("ConsigneeOrganisation", null, ConsolWrapper.ConsigneeOrganisation);

			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = header1.PK;
			header1.OH_Code = "ABCDEFG";
			DocOrganisation organisationWrapper = DocOrganisation.New(header1, Factory);
			AssertEquals("ConsigneeOrganisation", organisationWrapper.Code, ConsolWrapper.ConsigneeOrganisation.Code);

			Consol.JK_AgentType = "";
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			Consol.SetDefaultReceivingForwarderAddress(header2);
			header2.OH_Code = "JJJKKK";
			organisationWrapper = DocOrganisation.New(header2, Factory);
			AssertEquals("ConsigneeOrganisation", organisationWrapper.Code, ConsolWrapper.ConsigneeOrganisation.Code);
		}

		public void TestRegistrationNumber()
		{
			AssertEquals("RegistrationNumber", ZString.Empty, ConsolWrapper.RegistrationNumber);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var shipment = Consol.Shipments.AddNew();
			AssertEquals("RegistrationNumber", ZString.Empty, ConsolWrapper.RegistrationNumber);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_RL_NKClosestPort = "";
			shipment.ConsigneePK = header.PK;

			RefCountry brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);

			OrgCusCode code1 = header.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			code1.OK_RN_NKCodeCountry = brazil.Code;
			code1.OK_CustomsRegNo = "REG1111";

			OrgCusCode code2 = header.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.RebateUserCode;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code2.OK_CustomsRegNo = "REG2222";

			OrgCusCode code3 = header.CustomsCodes.AddNew();
			code3.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code3.OK_RN_NKCodeCountry = brazil.Code;
			code3.OK_CustomsRegNo = "REG3333";

			AssertEquals("RegistrationNumber", ZString.Empty, ConsolWrapper.RegistrationNumber);

			header.OH_RL_NKClosestPort = "BRAAG";
			AssertEquals("RegistrationNumber", "CNPJ: REG3333", ConsolWrapper.RegistrationNumber);

			header.CustomsCodes.RemoveAndDelete(code3);
			AssertEquals("RegistrationNumber", "CPF: REG1111", ConsolWrapper.RegistrationNumber);
		}

		#endregion

		#region Consol Job Profit Document

		#region TestConstructor

		public void TestConstructor()
		{
			ConsolJobDocumentPrintItem jobPrintItem = null;
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(jobPrintItem, Factory);
			AssertNull("ConsolWrapper", consolWrapper);

			ConsolJobDocumentPrinter jobPrinter = new ConsolJobDocumentPrinter(Factory);
			jobPrintItem = new ConsolJobDocumentPrintItem(jobPrinter, null, Factory);
			consolWrapper = DocForwardingConsol.New(jobPrintItem, Factory);
			AssertNull("ConsolWrapper", consolWrapper);

			jobPrintItem = new ConsolJobDocumentPrintItem(jobPrinter, Consol, Factory);
			jobPrinter.PrintChargeSummary = true;
			jobPrinter.PrintChargeDetail = false;
			jobPrinter.PrintARInvoiceAnalysis = true;
			jobPrinter.PrintAPInvoiceAnalysis = false;
			jobPrinter.PrintJobRevenueJournalAnalysis = false;
			jobPrinter.PrintJobByJobSummary = true;
			jobPrinter.PrintContainerPackingSummary = false;
			consolWrapper = DocForwardingConsol.New(jobPrintItem, Factory);
			AssertNotNull("ConsolWrapper", consolWrapper);
			AssertEquals("PrintChargeSummary", true, consolWrapper.PrintChargeSummary);
			AssertEquals("PrintChargeDetail", false, consolWrapper.PrintChargeDetail);
			AssertEquals("PrintARInvoiceAnalysis", true, consolWrapper.PrintARInvoiceAnalysis);
			AssertEquals("PrintAPInvoiceAnalysis", false, consolWrapper.PrintAPInvoiceAnalysis);
			AssertEquals("PrintJobRevenueJournalAnalysis", false, consolWrapper.PrintJobRevenueJournalAnalysis);
			AssertEquals("PrintJobByJobSummary", true, consolWrapper.PrintJobByJobSummary);
			AssertEquals("PrintContainerPackingSummary", false, consolWrapper.PrintContainerPackingSummary);
			AssertEquals("Job Profit Document", false, consolWrapper.IsProfitLossDoc);
		}

		#endregion

		#region TestNumberOfContainersForConsolJobProfitDocument

		public void TestNumberOfContainersForConsolJobProfitDocument()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("NumberOfContainers", 0, ConsolWrapper.NumberOfContainersForConsolJobProfitDocument);

			ForwardingContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("NumberOfContainers", 0, ConsolWrapper.NumberOfContainersForConsolJobProfitDocument);

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("NumberOfContainers", 1, ConsolWrapper.NumberOfContainersForConsolJobProfitDocument);

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("NumberOfContainers", 2, ConsolWrapper.NumberOfContainersForConsolJobProfitDocument);

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals("NumberOfContainers", 3, ConsolWrapper.NumberOfContainersForConsolJobProfitDocument);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("NumberOfContainers", 0, ConsolWrapper.NumberOfContainersForConsolJobProfitDocument);
		}

		#endregion

		#region TestTEUForConsolJobProfitDocument

		public void TestTEUForConsolJobProfitDocument()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TEU", 0M, ConsolWrapper.TEUForConsolJobProfitDocument);

			RefContainer refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_TEU = 1M;

			ForwardingContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_RC = refContainer.PK;
			AssertEquals("TEU", 0M, ConsolWrapper.TEUForConsolJobProfitDocument);

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_RC = refContainer.PK;
			AssertEquals("TEU", 1M, ConsolWrapper.TEUForConsolJobProfitDocument);

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			container.JC_RC = refContainer.PK;
			AssertEquals("TEU", 2M, ConsolWrapper.TEUForConsolJobProfitDocument);

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			container.JC_RC = refContainer.PK;
			AssertEquals("TEU", 3M, ConsolWrapper.TEUForConsolJobProfitDocument);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TEU", 0M, ConsolWrapper.TEUForConsolJobProfitDocument);
		}

		#endregion

		#region Collections

		#region TestJobs

		public void TestJobs()
		{
			AssertNotNull("Jobs", ConsolWrapper.Jobs);
			AssertEquals("Count", 0, ConsolWrapper.Jobs.Count);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Count", 0, ConsolWrapper.Jobs.Count);

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			Factory.Save();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Count", 1, ConsolWrapper.Jobs.Count);
		}

		#endregion

		#region TestAllLines

		public void TestAllLines()
		{
			AssertNotNull("AllLines", ConsolWrapper.AllLines);
			AssertEquals("Count", 0, ConsolWrapper.AllLines.Count);

			var anotherFactory = new BusinessObjectFactory();

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			Func<ForwardingConsol, BusinessObjectFactory, DocForwardingConsol> createWrapper = (consol, factory) => DocForwardingConsol.New(consol, factory);
			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 0, ConsolWrapper.AllLines.Count);

			JobHeader job = anotherFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 0, ConsolWrapper.AllLines.Count);

			AccTransactionLines line = anotherFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_JH = job.PK;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 1, ConsolWrapper.AllLines.Count);
		}

		public void TestAllLinesWhenChildJobIsUsed()
		{
			AssertNotNull("AllLines", ConsolWrapper.AllLines);
			AssertEquals("Count", 0, ConsolWrapper.AllLines.Count);

			var anotherFactory = new BusinessObjectFactory();

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			Func<ForwardingConsol, BusinessObjectFactory, DocForwardingConsol> createWrapper = (consol, factory) => DocForwardingConsol.New(consol, factory);
			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 0, ConsolWrapper.AllLines.Count);

			JobHeader job = anotherFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			JobHeader childJob = anotherFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			childJob.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			childJob.JH_ParentID = ZGuid.NewZGuid();
			childJob.JH_JH_ParentJob = job.PK;

			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 0, ConsolWrapper.AllLines.Count);

			AccTransactionLines line = anotherFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_JH = childJob.PK;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 1, ConsolWrapper.AllLines.Count);
		}

		#endregion

		#region TestARExcludeJRJLines

		public void TestARExcludeJRJLines()
		{
			AssertNotNull("ARExcludeJRJLines", ConsolWrapper.ARExcludeJRJLines);
			AssertEquals("Count", 0, ConsolWrapper.ARExcludeJRJLines.Count);

			var anotherFactory = new BusinessObjectFactory();

			var shipment = Consol.Shipments.AddNew();
			Func<ForwardingConsol, BusinessObjectFactory, DocForwardingConsol> createWrapper = (consol, factory) => DocForwardingConsol.New(consol, factory);

			var job = anotherFactory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			anotherFactory.Save();

			var header = anotherFactory.NewWithValidTestData<ARInvoice>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;

			var chargeCode = anotherFactory.NewWithValidTestData<AccChargeCode>();

			var line = anotherFactory.NewWithValidTestData<ARInvoiceLine>();
			line.AL_AH = header.PK;
			line.AL_JH = job.PK;
			line.AL_AC = chargeCode.PK;
			line.AL_LineType = TransactionLineTypes.Revenue;

			var charge = anotherFactory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AL_ARLine = line.PK;

			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(ConsolWrapper.Factory, line.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(5.25m)) });

			AssertEquals("AR Exclude JRJ Lines Count", 1, ConsolWrapper.ARExcludeJRJLines.Count);
			AssertEquals("ARExcludeJRJLinesForProfitShare", 2, ConsolWrapper.ARExcludeJRJLinesForProfitShare.Count);
			AssertEquals(1, ConsolWrapper.ARExcludeJRJLinesForProfitShare.Cast<DocJobLineDetail>().Count(e => e.IsTaxExpense));

			var creator = new TestObjectCreator(anotherFactory);
			var jobRevenueJournal = creator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("AR Exclude JRJ Lines Count", 1, ConsolWrapper.ARExcludeJRJLines.Count);
		}

		#endregion

		#region TestAPExcludeJRJLines

		public void TestAPExcludeJRJLines()
		{
			AssertNotNull("APExcludeJRJLines", ConsolWrapper.APExcludeJRJLines);
			AssertEquals("APExcludeJRJLines", 0, ConsolWrapper.APExcludeJRJLines.Count);

			var anotherFactory = new BusinessObjectFactory();

			var shipment = Consol.Shipments.AddNew();
			Func<ForwardingConsol, BusinessObjectFactory, DocForwardingConsol> createWrapper = (consol, factory) => DocForwardingConsol.New(consol, factory);

			var job = anotherFactory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var header = anotherFactory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			var line = anotherFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_AH = header.PK;
			line.AL_JH = job.PK;

			var charge = anotherFactory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = anotherFactory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_AL_APLine = line.PK;

			line.AL_LineType = TransactionLineTypes.Cost;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(t => t.GetTaxExpenses(ConsolWrapper.Factory, line.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(5.25m)) });

			AssertEquals("APExcludeJRJLines", 1, ConsolWrapper.APExcludeJRJLines.Count);
			AssertEquals("APExcludeJRJLinesForProfitShare", 2, ConsolWrapper.APExcludeJRJLinesForProfitShare.Count);
			AssertEquals(1, ConsolWrapper.APExcludeJRJLinesForProfitShare.Cast<DocJobLineDetail>().Count(e => e.IsTaxExpense));

			var creator = new TestObjectCreator(anotherFactory);
			var jobRevenueJournal = creator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("APExcludeJRJLines", 1, ConsolWrapper.APExcludeJRJLines.Count);
		}

		#endregion

		#region TestJRJLines

		public void TestJRJLines()
		{
			AssertNotNull("JRJLines", ConsolWrapper.JRJLines);
			AssertEquals("Count", 0, ConsolWrapper.JRJLines.Count);

			var anotherFactory = new BusinessObjectFactory();

			var shipment = Consol.Shipments.AddNew();
			Func<ForwardingConsol, BusinessObjectFactory, DocForwardingConsol> createWrapper = (consol, factory) => DocForwardingConsol.New(consol, factory);

			var job = anotherFactory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var header = anotherFactory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			var line = anotherFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_AH = header.PK;
			line.AL_JH = job.PK;

			var charge = anotherFactory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = anotherFactory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_AL_APLine = line.PK;

			line.AL_LineType = TransactionLineTypes.Cost;
			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("JRJLines", 0, ConsolWrapper.JRJLines.Count);

			var creator = new TestObjectCreator(anotherFactory);
			var jobRevenueJournal1 = creator.CreateJobRevenueJournal(creator.CC1, job, 500M);
			jobRevenueJournal1.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal1.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;

			var jobRevenueJournal2 = creator.CreateJobRevenueJournal(creator.CC1, job, 600M);
			jobRevenueJournal2.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal2.JournalLines[1].CostRevenueType = TransactionLineTypes.Revenue;

			var jobRevenueJournal3 = creator.CreateJobRevenueJournal(creator.CC1, job, 600M);
			jobRevenueJournal3.JournalLines[0].CostRevenueType = TransactionLineTypes.Cost;
			jobRevenueJournal3.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;

			var jobRevenueJournal4 = creator.CreateJobRevenueJournal(creator.CC1, job, 600M);
			jobRevenueJournal4.JournalLines[0].CostRevenueType = TransactionLineTypes.Cost;
			jobRevenueJournal4.JournalLines[1].CostRevenueType = TransactionLineTypes.Revenue;

			anotherFactory.Save();

			ConsolWrapper = RecreateTestingDocWrapper(Consol, createWrapper);
			AssertEquals("Count", 8, ConsolWrapper.JRJLines.Count);
		}

		#endregion

		#endregion

		#region Totals
		#region TestTotalRevenue
		public void TestTotalRevenue()
		{
			AssertEquals("TotalRevenue", 0M, ConsolWrapper.TotalRevenue);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 100m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalRevenue", 100M, ConsolWrapper.TotalRevenue);
		}

		public void TestTotalRevenueWithSubShipment()
		{
			AssertEquals("TotalRevenue", 0M, ConsolWrapper.TotalRevenue);

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, 100M, TransactionLineTypes.Revenue, LedgerTypes.AccountsReceivable, "FRT");

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.Revenue, LedgerTypes.AccountsReceivable, "FRT");

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalRevenue", 200M, ConsolWrapper.TotalRevenue);
		}

		public void TestTotalRevenueWithSubShipmentWhenARBuyersConsolInvoicingStyleEqualToApportion()
		{
			AssertEquals("TotalRevenue", 0M, ConsolWrapper.TotalRevenue);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Apportion;

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobA.LocalChargesPK = org.PK;
			SetUpCharges(jobA, shipmentPK, 1000M, TransactionLineTypes.Revenue, LedgerTypes.AccountsReceivable, "FRT");

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.Revenue, LedgerTypes.AccountsReceivable, "FRT");

			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalRevenue", 1100M, ConsolWrapper.TotalRevenue);
		}

		public void TestTotalRevenueWithColoadMasterShipment()
		{
			AssertEquals("TotalRevenue", 0M, ConsolWrapper.TotalRevenue);

			ZGuid shipmentPK = PrepareCoLoadMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, 100M, TransactionLineTypes.Revenue, LedgerTypes.AccountsReceivable, "FRT");

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.Revenue, LedgerTypes.AccountsReceivable, "FRT");

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalRevenue", 200M, ConsolWrapper.TotalRevenue);
		}
		#endregion

		#region TestTotalWip
		public void TestTotalWip()
		{
			AssertEquals("TotalWip", 0M, ConsolWrapper.TotalWip);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_APLine = line.PK;
			charge.JR_LocalSellAmt = 100M;
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalWip", 100M, ConsolWrapper.TotalWip);
		}

		public void TestTotalWipWithSubShipment()
		{
			AssertEquals("TotalWip", 0M, ConsolWrapper.TotalWip);

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, 100M, TransactionLineTypes.WIP, LedgerTypes.AccountsReceivable);

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.WIP, LedgerTypes.AccountsReceivable);

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalWip", 200M, ConsolWrapper.TotalWip);
		}

		public void TestTotalWipWithSubShipmentWhenARBuyersConsolInvoicingStyleEqualToApportion()
		{
			AssertEquals("TotalWip", 0M, ConsolWrapper.TotalWip);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Apportion;

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobA.LocalChargesPK = org.PK;
			SetUpCharges(jobA, shipmentPK, 1000M, TransactionLineTypes.WIP, LedgerTypes.AccountsReceivable);

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.WIP, LedgerTypes.AccountsReceivable);

			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalWip", 1100M, ConsolWrapper.TotalWip);
		}

		public void TestTotalWipWithColoadMasterShipment()
		{
			AssertEquals("TotalWip", 0M, ConsolWrapper.TotalWip);

			ZGuid shipmentPK = PrepareCoLoadMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, 100M, TransactionLineTypes.WIP, LedgerTypes.AccountsReceivable);

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.WIP, LedgerTypes.AccountsReceivable);

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalWip", 200M, ConsolWrapper.TotalWip);
		}
		#endregion

		#region TestTotalIncome
		public void TestTotalIncome()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("TotalIncome", 0M, ConsolWrapper.TotalIncome);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 100m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 200m, "FRT");

			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalIncome", 300M, ConsolWrapper.TotalIncome);
		}
		#endregion

		#region TestTotalCost
		public void TestTotalCost()
		{
			AssertEquals("TotalCost", 0M, ConsolWrapper.TotalCost);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateCostLineAndCharge(job.PK, 100m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalCost", 100M, ConsolWrapper.TotalCost);
		}

		public void TestTotalCostWithSubShipment()
		{
			AssertEquals("TotalCost", 0M, ConsolWrapper.TotalCost);

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, -100M, TransactionLineTypes.Cost, LedgerTypes.AccountsPayable, "FRT");

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, -100M, TransactionLineTypes.Cost, LedgerTypes.AccountsPayable, "FRT");

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalCost", -200M, ConsolWrapper.TotalCost);
		}

		public void TestTotalCostWithSubShipmentWhenARBuyersConsolInvoicingStyleEqualToAPPortion()
		{
			AssertEquals("TotalCost", 0M, ConsolWrapper.TotalCost);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Apportion;

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobA.LocalChargesPK = org.PK;
			SetUpCharges(jobA, shipmentPK, -1000M, TransactionLineTypes.Cost, LedgerTypes.AccountsPayable, "FRT");

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, -100M, TransactionLineTypes.Cost, LedgerTypes.AccountsPayable, "FRT");

			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalCost", -1100M, ConsolWrapper.TotalCost);
		}

		public void TestTotalCostWithColoadMAsterShipment()
		{
			AssertEquals("TotalCost", 0M, ConsolWrapper.TotalCost);

			ZGuid shipmentPK = PrepareCoLoadMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, -100M, TransactionLineTypes.Cost, LedgerTypes.AccountsPayable, "FRT");

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, -100M, TransactionLineTypes.Cost, LedgerTypes.AccountsPayable, "FRT");

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalCost", -200M, ConsolWrapper.TotalCost);
		}
		#endregion

		#region TestTotalAccrual
		public void TestTotalAccrual()
		{
			AssertEquals("TotalAccrual", 0M, ConsolWrapper.TotalAccrual);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_APLine = line.PK;
			line.AL_JH = job.PK;
			charge.JR_LocalCostAmt = 100M;
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalAccrual", 100M, ConsolWrapper.TotalAccrual);
		}

		public void TestTotalAccrualWithSubshipment()
		{
			AssertEquals("TotalAccrual", 0M, ConsolWrapper.TotalAccrual);

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, 100M, TransactionLineTypes.Accrual, LedgerTypes.AccountsPayable);

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.Accrual, LedgerTypes.AccountsPayable);

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalAccrual", 200M, ConsolWrapper.TotalAccrual);
		}

		public void TestTotalAccrualWithSubShipmentWhenARBuyersConsolInvoicingStyleEqualToAPPortion()
		{
			AssertEquals("TotalAccrual", 0M, ConsolWrapper.TotalAccrual);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Apportion;

			ZGuid shipmentPK = PrepareMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobA.LocalChargesPK = org.PK;
			SetUpCharges(jobA, shipmentPK, 1000M, TransactionLineTypes.Accrual, LedgerTypes.AccountsPayable);

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.Accrual, LedgerTypes.AccountsPayable);

			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalAccrual", 1100M, ConsolWrapper.TotalAccrual);
		}

		public void TestTotalAccrualWithColoadMasterShipment()
		{
			AssertEquals("TotalAccrual", 0M, ConsolWrapper.TotalAccrual);

			ZGuid shipmentPK = PrepareCoLoadMasterShipment();
			JobHeader jobA = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobA, shipmentPK, 100M, TransactionLineTypes.Accrual, LedgerTypes.AccountsPayable);

			ZGuid subShipmentPK = PrepareSubShipment(shipmentPK);
			JobHeader jobB = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			SetUpCharges(jobB, subShipmentPK, 100M, TransactionLineTypes.Accrual, LedgerTypes.AccountsPayable);

			Factory.Save();
			jobA.LoadCharges_ForTestOnly();
			jobB.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			AssertEquals("TotalAccrual", 200M, ConsolWrapper.TotalAccrual);
		}
		#endregion

		#region TestTotalExpense
		public void TestTotalExpense()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("TotalExpense", 0M, ConsolWrapper.TotalExpense);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateCostLineAndCharge(job.PK, 100m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalExpense", 300M, ConsolWrapper.TotalExpense);
		}
		#endregion

		#region TestTotalRealisedAmount
		public void TestTotalRealisedAmount()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("TotalRealisedAmount", 0M, ConsolWrapper.TotalRealisedAmount);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 300m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalRealisedAmount", 100M, ConsolWrapper.TotalRealisedAmount);
		}
		#endregion

		#region TestTotalEstimatedAmount
		public void TestTotalEstimatedAmount()
		{
			AssertEquals("TotalEstimatedAmount", 0M, ConsolWrapper.TotalEstimatedAmount);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			AccTransactionLines line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			Charge charge1 = Factory.NewWithValidTestData<Charge>();
			charge1.JR_JH = job.PK;
			charge1.JR_AL_APLine = line1.PK;
			line1.AL_JH = job.PK;
			charge1.JR_LocalSellAmt = 300M;

			AccTransactionLines line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_JH = job.PK;
			charge2.JR_AL_APLine = line2.PK;
			line2.AL_JH = job.PK;
			charge2.JR_LocalCostAmt = 200M;
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalEstimatedAmount", 100M, ConsolWrapper.TotalEstimatedAmount);
		}
		#endregion

		#region TestTotalProfit
		public void TestTotalProfit()
		{
			AssertEquals("TotalProfit", 0M, ConsolWrapper.TotalProfit);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TotalProfit", 500M, ConsolWrapper.TotalProfit);
		}
		#endregion

		public void TestTotalRevenueMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			var line1 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 200m, "BAF");
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(300m, ConsolWrapper.TotalRevenueMovementsRecognized);
			AssertEquals(500m, ConsolWrapper.RevenueNotRecognized);
			AssertNotNull(ConsolWrapper.RevenueMovementsRecognized);
			AssertEquals(2, ConsolWrapper.RevenueMovementsRecognized.Count);
			AssertEquals(300m, ConsolWrapper.RevenueMovementsRecognized.Sum(x => ((DocJobInvoicingJobCharge)x).Revenue));
		}

		public void TestTotalCostMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			var line1 = ObjectCreator.CreateCostLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "BAF");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 500m, "FRT");
			Factory.Save();
			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(300m, ConsolWrapper.TotalCostMovementsRecognized);
			AssertEquals(500m, ConsolWrapper.CostNotRecognized);
			AssertNotNull(ConsolWrapper.CostMovementsRecognized);
			AssertEquals(2, ConsolWrapper.CostMovementsRecognized.Count);
			AssertEquals(300m, ConsolWrapper.CostMovementsRecognized.Sum(x => ((DocJobInvoicingJobCharge)x).Cost));
		}

		public void TestTotalWIPMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 900m, "FRT");
			Factory.Save();
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_JH = job.PK;
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalSellAmt = 400;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_JH = job.PK;
			charge3.JR_LocalCostAmt = 600;
			charge3.JR_AC = chargeCodePK;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(900m, ConsolWrapper.TotalWIPMovementsRecognized);
			AssertEquals(400m, ConsolWrapper.WIPNotRecognized);
			AssertNotNull(ConsolWrapper.WIPMovementsRecognized);
			AssertEquals(1, ConsolWrapper.WIPMovementsRecognized.Count);
			AssertEquals(-900m, ConsolWrapper.WIPMovementsRecognized.Sum(x => ((WIPACRDummyLine)x).LineAmount));
		}

		public void TestTotalACRMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 900m, "FRT");
			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = chargeCodePK;
			charge2.JR_LocalCostAmt = 400;
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_LocalSellAmt = 600;
			charge3.JR_AC = chargeCodePK;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(900m, ConsolWrapper.TotalACRMovementsRecognized);
			AssertEquals(400m, ConsolWrapper.ACRNotRecognized);
			AssertNotNull(ConsolWrapper.ACRMovementsRecognized);
			AssertEquals(1, ConsolWrapper.ACRMovementsRecognized.Count);
			AssertEquals(900m, ConsolWrapper.ACRMovementsRecognized.Sum(x => ((WIPACRDummyLine)x).LineAmount));
		}

		public void TestTotalJobProfitRecognizedInGL()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			var line1 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 300m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "BAF");
			var line3 = ObjectCreator.CreateCostLineAndCharge(job.PK, 400m, "FRT");
			var line4 = ObjectCreator.CreateCostLineAndCharge(job.PK, 600m, "BAF");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 300m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 650m, "FRT");
			Factory.Save();
			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line3.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line4.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(800m, ConsolWrapper.TotalRevenueMovementsRecognized);
			AssertEquals(1000m, ConsolWrapper.TotalCostMovementsRecognized);
			AssertEquals(300m, ConsolWrapper.TotalWIPMovementsRecognized);
			AssertEquals(650m, ConsolWrapper.TotalACRMovementsRecognized);
			AssertEquals("TotalJobProfitRecognizedInGL should be TotalRev + TotalWIP - TotalCost - TotalACR", -550M, ConsolWrapper.TotalJobProfitRecognizedInGL);
		}

		public void TestTotalJobProfitNotRecognizedInGL()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ZGuid chargeCodePK = ObjectCreator.GetChargeCodeByCode("FRT");
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 900m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 800m, "FRT");
			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = chargeCodePK;
			charge3.JR_LocalSellAmt = 600;
			charge3.JR_LocalCostAmt = 500;
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals(900m, ConsolWrapper.RevenueNotRecognized);
			AssertEquals(800m, ConsolWrapper.CostNotRecognized);
			AssertEquals(600m, ConsolWrapper.WIPNotRecognized);
			AssertEquals(500m, ConsolWrapper.ACRNotRecognized);

			AssertEquals("TotalJobProfitNotRecognizedInGL should be Rev + WIP - Cost - ACR", 200M, ConsolWrapper.TotalJobProfitNotRecognizedInGL);
		}

		#region Preparation
		ZGuid PrepareMasterShipment()
		{
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;

			return shipment.PK;
		}

		ZGuid PrepareSubShipment(ZGuid masterPK)
		{
			ForwardingShipment subShipment = Consol.Shipments.AddNew();
			subShipment.JS_TransportMode = Constants.TransportModes.Sea;
			subShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankSightDraft;
			subShipment.JS_JS_ColoadMasterShipment = masterPK;

			return subShipment.PK;
		}

		ZGuid PrepareCoLoadMasterShipment()
		{
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;

			return shipment.PK;
		}

		void SetUpCharges(JobHeader job, ZGuid shipmentPK, ZDecimal amount, string lineType, string ledgerType, string chargeCode = "")
		{
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipmentPK;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = ledgerType;
			header.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			line.AL_LineType = lineType;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			if (!chargeCode.IsNullOrEmpty())
			{
				ZQuery chargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
				ZGuid chargeCodePK = Factory.LoadTop1(typeof(AccChargeCode), chargeCodeFilter).PK;
				line.AL_AC = chargeCodePK;
			}
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_GE = Env.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = "AUD";

			Charge jobCharge = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_JH = job.PK;

			switch (lineType)
			{
				case TransactionLineTypes.Revenue:
					line.AL_JH = job.PK;
					line.AL_LineAmount = amount;
					line.AL_OSAmount = amount;
					jobCharge.JR_AL_ARLine = line.PK;
					jobCharge.JR_LocalSellAmt = amount;
					break;

				case TransactionLineTypes.WIP:
					line.AL_LineAmount = amount;
					line.AL_OSAmount = amount;
					jobCharge.JR_AL_APLine = line.PK;
					jobCharge.JR_LocalSellAmt = amount;
					break;

				case TransactionLineTypes.Accrual:
					jobCharge.JR_AL_APLine = line.PK;
					jobCharge.JR_LocalCostAmt = amount;
					break;

				case TransactionLineTypes.Cost:
					line.AL_LineAmount = line.AL_OSAmount = -amount;
					line.AL_JH = job.PK;
					jobCharge.JR_AL_APLine = line.PK;
					jobCharge.JR_LocalCostAmt = amount;
					break;

				default:
					break;
			}
		}
		#endregion
		#endregion

		#region Margins
		public void TestProfitCostMargin()
		{
			AssertEquals("TestProfitCostMargin", 0M, ConsolWrapper.ProfitCostMargin);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			Factory.Save();

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TestProfitCostMargin", 500M / 400M, ConsolWrapper.ProfitCostMargin);
		}

		public void TestProfitRevenueMargin()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("TestProfitRevenueMargin", 0M, ConsolWrapper.ProfitRevMargin);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			Factory.Save();

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("TestProfitRevenueMargin", 500M / 900M, ConsolWrapper.ProfitRevMargin);
		}
		#endregion

		public void TestAllProfitLossSummaryLines()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertNotNull(ConsolWrapper.AllProfitLossSummaryLines);
			AssertEquals(1, ConsolWrapper.AllProfitLossSummaryLines.Count);
			AssertEquals(500m, ConsolWrapper.AllProfitLossSummaryLines[0].Revenue);
			AssertEquals(400m, ConsolWrapper.AllProfitLossSummaryLines[0].WIP);
			AssertEquals(200m, ConsolWrapper.AllProfitLossSummaryLines[0].Cost);
			AssertEquals(200m, ConsolWrapper.AllProfitLossSummaryLines[0].Accrual);
			AssertEquals(900m, ConsolWrapper.AllProfitLossSummaryLines[0].Income);
			AssertEquals(-400m, ConsolWrapper.AllProfitLossSummaryLines[0].Expense);
			AssertEquals(500m, ConsolWrapper.AllProfitLossSummaryLines[0].Profit);
			AssertEquals("FRT", ConsolWrapper.AllProfitLossSummaryLines[0].ChargeCode.Code);
			AssertEquals(job.JH_JobNum, ConsolWrapper.AllProfitLossSummaryLines[0].Job.JobNum);
		}

		public void TestRevenueMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var shipment = Consol.Shipments.AddNew();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var line1 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateRevenueLineAndCharge(job.PK, 200m, "BAF");
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line1.PK)).Returns(new[] { (ZDate.Today.AddDays(1), new ZDecimal(8.24m)), (ZDate.Today, new ZDecimal(2.78m)) });
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line2.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(6.32m)), (ZDate.Today.AddDays(-1), new ZDecimal(13.35m)), (ZDate.Today, new ZDecimal(10.62m)) });

			AssertEquals(4, ConsolWrapper.RevenueMovementsRecognized.Count);
			AssertEquals(9, ConsolWrapper.RevenueMovementsRecognizedForProfitShare.Count);
			AssertEquals(5, ConsolWrapper.RevenueMovementsRecognizedForProfitShare.Cast<DocJobInvoicingJobCharge>().Count(c => c.IsTaxExpense));
			AssertEquals(300m, ConsolWrapper.TotalRevenueMovementsRecognized);
		}

		public void TestCostMovementsRecognized()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var shipment = Consol.Shipments.AddNew();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var line1 = ObjectCreator.CreateCostLineAndCharge(job.PK, 100m, "FRT");
			var line2 = ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "BAF");
			var jobRevenueJournal = ObjectCreator.CreateJobRevenueJournal(ObjectCreator.CC1, job, 500M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			line1.AL_ReverseDate = ZDateTime.BrettsBirthday;
			line2.AL_ReverseDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			job.LoadCharges_ForTestOnly();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line1.PK)).Returns(new[] { (ZDate.Today.AddDays(1), new ZDecimal(8.24m)), (ZDate.Today, new ZDecimal(2.78m)) });
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, line2.PK)).Returns(new[] { (ZDate.Today, new ZDecimal(6.32m)), (ZDate.Today.AddDays(-1), new ZDecimal(13.35m)), (ZDate.Today, new ZDecimal(10.62m)) });

			AssertEquals(2, ConsolWrapper.CostMovementsRecognized.Count);
			AssertEquals(7, ConsolWrapper.CostMovementsRecognizedForProfitShare.Count);
			AssertEquals(5, ConsolWrapper.CostMovementsRecognizedForProfitShare.Cast<DocJobInvoicingJobCharge>().Count(c => c.IsTaxExpense));
			AssertEquals(300m, ConsolWrapper.TotalCostMovementsRecognized);
		}

		public void TestAllProfitLossDetailedLines()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 300m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertNotNull(ConsolWrapper.AllProfitLossDetailedLines);
			AssertEquals(4, ConsolWrapper.AllProfitLossDetailedLines.Count);
			AssertEquals("REV", ConsolWrapper.AllProfitLossDetailedLines[0].LineType);
			AssertEquals(500m, ConsolWrapper.AllProfitLossDetailedLines[0].LineAmount);
			AssertEquals(500m, ConsolWrapper.AllProfitLossDetailedLines[0].Income);
			AssertEquals(0m, ConsolWrapper.AllProfitLossDetailedLines[0].Expense);
			AssertEquals(500m, ConsolWrapper.AllProfitLossDetailedLines[0].Profit);
			AssertEquals("FRT", ConsolWrapper.AllProfitLossDetailedLines[0].ChargeCode.Code);
			AssertEquals(job.JH_JobNum, ConsolWrapper.AllProfitLossDetailedLines[0].Job.JobNum);

			AssertEquals("WIP", ConsolWrapper.AllProfitLossDetailedLines[1].LineType);
			AssertEquals(400m, ConsolWrapper.AllProfitLossDetailedLines[1].LineAmount);
			AssertEquals(400m, ConsolWrapper.AllProfitLossDetailedLines[1].Income);
			AssertEquals(0m, ConsolWrapper.AllProfitLossDetailedLines[1].Expense);
			AssertEquals(400m, ConsolWrapper.AllProfitLossDetailedLines[1].Profit);
			AssertEquals("FRT", ConsolWrapper.AllProfitLossDetailedLines[1].ChargeCode.Code);
			AssertEquals(job.JH_JobNum, ConsolWrapper.AllProfitLossDetailedLines[1].Job.JobNum);

			AssertEquals("CST", ConsolWrapper.AllProfitLossDetailedLines[2].LineType);
			AssertEquals(-300m, ConsolWrapper.AllProfitLossDetailedLines[2].LineAmount);
			AssertEquals(0m, ConsolWrapper.AllProfitLossDetailedLines[2].Income);
			AssertEquals(300m, ConsolWrapper.AllProfitLossDetailedLines[2].Expense);
			AssertEquals(-300m, ConsolWrapper.AllProfitLossDetailedLines[2].Profit);
			AssertEquals("FRT", ConsolWrapper.AllProfitLossDetailedLines[2].ChargeCode.Code);
			AssertEquals(job.JH_JobNum, ConsolWrapper.AllProfitLossDetailedLines[2].Job.JobNum);

			AssertEquals("ACR", ConsolWrapper.AllProfitLossDetailedLines[3].LineType);
			AssertEquals(-200m, ConsolWrapper.AllProfitLossDetailedLines[3].LineAmount);
			AssertEquals(0m, ConsolWrapper.AllProfitLossDetailedLines[3].Income);
			AssertEquals(200m, ConsolWrapper.AllProfitLossDetailedLines[3].Expense);
			AssertEquals(-200m, ConsolWrapper.AllProfitLossDetailedLines[3].Profit);
			AssertEquals("FRT", ConsolWrapper.AllProfitLossDetailedLines[3].ChargeCode.Code);
			AssertEquals(job.JH_JobNum, ConsolWrapper.AllProfitLossDetailedLines[3].Job.JobNum);
		}

		#endregion

		#region IDocJobDetail Tests

		IDocJobDetail ConsolAsJobDocDetail
		{
			get { return ConsolWrapper; }
		}

		public void TestOrderNumbersForInvoice()
		{
			AssertEquals(ZString.Empty, ConsolAsJobDocDetail.OrderNumbersForInvoice);
		}

		public void TestOurReference()
		{
			Consol.JK_UniqueConsignRef = "C00001234";
			AssertEquals(ConsolWrapper.ConsolNumber, ConsolAsJobDocDetail.OurReference);
		}

		public void TestSupplierAsString()
		{
			AssertEquals(ZString.Empty, ConsolAsJobDocDetail.SupplierAsString);
		}

		public void TestVesselAndVoyage()
		{
			SetUpConsolWithMinimumRequiredToSave();
			AssertEquals(ConsolWrapper.ConsolTransportInfo, ConsolAsJobDocDetail.VesselAndVoyage);
		}

		public void TestMasterBillNumber()
		{
			Consol.JK_MasterBillNum = "MASTER123";
			AssertEquals(ConsolWrapper.MasterBillNum, ConsolAsJobDocDetail.MasterBillNumber);
		}

		public void TestETAPortName()
		{
			SetUpConsolWithValidPortCodes();
			AssertEquals(ConsolWrapper.PortOfDischarge.PortName, ConsolAsJobDocDetail.ETAPortName);
		}

		public void TestETDPortName()
		{
			SetUpConsolWithValidPortCodes();
			AssertEquals(ConsolWrapper.PortOfLoading.PortName, ConsolAsJobDocDetail.ETDPortName);
		}

		public void TestService()
		{
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(ConsolWrapper.ConsolMode, ConsolAsJobDocDetail.Service);
		}

		public void TestPackageQuantity()
		{
			AssertEquals(ConsolWrapper.OuterPacks.Count.ToString(), ConsolAsJobDocDetail.PackageQuantity);
		}

		public void TestPackageType()
		{
			AssertEquals("PACKAGE", ConsolAsJobDocDetail.PackageType);
		}

		public void TestConsolDepotPack()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForCurrentCountry().ToString();

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Pack Depot Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "111 One Street";
			address.OA_Address2 = "Building A";
			Consol.JK_OA_PackDepotAddress = address.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Consol Depot (Pack Address)", ConsolWrapper.PackDepotAddress.ToString(), ConsolAsJobDocDetail.ConsolDepot);
		}

		public void TestConsolDepotUnpack()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForCurrentCountry().ToString();

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Pack Depot Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "111 One Street";
			address.OA_Address2 = "Building A";
			Consol.JK_OA_UnpackDepotAddress = address.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Consol Depot (Unpack Address)", ConsolWrapper.UnpackDepotAddress.ToString(), ConsolAsJobDocDetail.ConsolDepot);
		}

		public void TestWeightAsString()
		{
			ZString registryValue = Env.Registry.FreightWeightUnit;
			Env.Registry.FreightWeightUnit = "KG";
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 123;
			shipment.JS_UnitOfWeight = "KG";

			AssertEquals(ConsolWrapper.TotalWeight + " " + ConsolWrapper.WeightUnit, ConsolAsJobDocDetail.WeightAsString);

			Env.Registry.FreightWeightUnit = registryValue;
		}

		public void TestVolumeAsString()
		{
			ForwardingShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 123;
			shipment.JS_UnitOfVolume = "M3";

			AssertEquals(ConsolWrapper.TotalVolume + " " + ConsolWrapper.VolumeUnit, ConsolAsJobDocDetail.VolumeAsString);
		}

		public void TestNote()
		{
			AssertEquals(ZString.Empty, ConsolAsJobDocDetail.Note);
		}

		public void TestConsignorAsString()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var shipment = Consol.Shipments.AddNew();
			shipment.ConsignorPK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Consignor", ConsolWrapper.Consignor, ConsolAsJobDocDetail.ConsignorAsString);
		}

		public void TestConsigneeAsString()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var shipment = Consol.Shipments.AddNew();
			shipment.ConsigneePK = header.PK;
			DocOrganisation organisationWrapper = DocOrganisation.New(header, Factory);
			AssertEquals("Consignee", ConsolWrapper.Consignee, ConsolAsJobDocDetail.ConsigneeAsString);
		}

		public void TestShortContainerAndSealNumbers()
		{
			FreightContainer testContainer = Consol.Containers.AddNew();
			testContainer.JC_ContainerNum = "1";
			testContainer.JC_SealNum = "1234";
			var rContainer = Factory.New<RefContainer>();
			rContainer.RC_Code = "ABC";
			testContainer.JC_RC = rContainer.PK;

			AssertEquals("1 / 1234 / ABC", ConsolAsJobDocDetail.ShortContainerAndSealNumbersForInvoice);
		}

		public void TestLongContainerAndSealNumbers()
		{
			FreightContainer testContainer1 = Consol.Containers.AddNew();
			FreightContainer testContainer2 = Consol.Containers.AddNew();

			testContainer1.JC_ContainerNum = "12345678901234567890";
			testContainer1.JC_SealNum = "12345678901234567890";
			var rContainer1 = Factory.New<RefContainer>();
			rContainer1.RC_Code = "ABCDEFGHIJ";
			testContainer1.JC_RC = rContainer1.PK;

			testContainer2.JC_ContainerNum = "98765432109876543210";
			testContainer2.JC_SealNum = "98765432109876543210";
			var rContainer2 = Factory.New<RefContainer>();
			rContainer2.RC_Code = "ABCDEFGHIJ";
			testContainer2.JC_RC = rContainer2.PK;
			ZString testResultShouldBe = "- 12345678901234567890 - 12345678901234567890 - ABCDEFGHIJ     \n- 98765432109876543210 - 98765432109876543210 - ABCDEFGHIJ     \n";
			AssertEquals(testResultShouldBe, ConsolAsJobDocDetail.LongContainerAndSealNumbersForInvoice);
		}

		public void TestNumberOfContainers()
		{
			AssertEquals("NumberOfContainers", 0, ConsolAsJobDocDetail.NumberOfContainers);
			Consol.Containers.AddNew();
			Consol.Containers.AddNew();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("NumberOfContainers", ConsolWrapper.Containers.Count, ConsolAsJobDocDetail.NumberOfContainers);
		}

		public void TestMarksAndNumbersForInvoice()
		{
			AssertEquals("Marks & Numbers", ZString.Empty, ConsolAsJobDocDetail.MarksAndNumbersForInvoice);
		}

		public void TestShortGoodsDescription()
		{
			AssertEquals("ShortGoodsDescription", ZString.Empty, ConsolAsJobDocDetail.ShortGoodsDescriptionForInvoice);
		}

		public void TestLongGoodsDescription()
		{
			AssertEquals("ShortGoodsDescription", ZString.Empty, ConsolAsJobDocDetail.LongGoodsDescriptionForInvoice);
		}

		public void TestETADate()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			AssertEquals("ETADate is not empty", ZDateTime.Today, ConsolAsJobDocDetail.ETADate);
		}

		public void TestETDDate()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			AssertEquals("ETDDate is not empty", ZDateTime.Today, ConsolAsJobDocDetail.ETDDate);
		}

		public void TransportModeIsAir()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeIsAir", true, ConsolAsJobDocDetail.TransportModeIsAir);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("TransportModeIsAir", false, ConsolAsJobDocDetail.TransportModeIsAir);
		}

		public void TransportModeIsSea()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeIsAir", true, ConsolAsJobDocDetail.TransportModeIsSea);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("TransportModeIsAir", false, ConsolAsJobDocDetail.TransportModeIsSea);
		}

		#endregion

		#region IRequestForMissingDocuments Tests

		public void TestHouseBillHeading()
		{
			AssertEquals("Should be empty", ZString.Empty, ConsolWrapper.HouseBillHeading);
		}

		public void TestHouseBill()
		{
			AssertEquals("Should be empty", ZString.Empty, ConsolWrapper.HouseBill);
		}

		public void TestDeclarationOrConsolNumber()
		{
			AssertEquals("Should be empty", ZString.Empty, ConsolWrapper.DeclarationOrConsolNumber);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("Should be Consolidated Cargo", "Consolidated Cargo", ConsolWrapper.GoodsDescription);
		}

		public void TestOwnerRefAndOrderRef()
		{
			Consol.JK_AgentsReference = "222AAA";
			AssertEquals("Should return Agents Reference", "222AAA", ConsolWrapper.OwnerRefAndOrderRef);
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			AssertEquals("Should return AGENTS REFERENCE", "AGENTS REFERENCE", ConsolWrapper.OwnerRefAndOrderRefHeading);
		}

		public void TestWeight()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			shipment.JS_ActualWeight = 500;
			AssertEquals("Should return shipments total weight", "500", consolWrapper.Weight);
		}

		public void TestVolume()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			shipment.JS_ActualVolume = 44;
			AssertEquals("Should return shipments total volume", "44", consolWrapper.Volume);
		}

		public void TestTransportInfo()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = Consol.Transports[0];
			transport.JW_VoyageFlight = "VOY123";
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / " + vessel.RV_LloydsNumber, ConsolWrapper.TransportInfo);
		}

		public void TestConsigneeOrgHeading()
		{
			AssertEquals("Should return RECEIVING AGENT", "RECEIVING AGENT", ConsolWrapper.ConsigneeOrgHeading);
		}

		public void TestConsignorOrgHeading()
		{
			AssertEquals("Should return SENDING AGENT", "SENDING AGENT", ConsolWrapper.ConsignorOrgHeading);
		}

		public void TestConsigneeOrg()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			Consol.SetDefaultReceivingForwarderAddress(header);
			AssertEquals("Should return the receiving forwarder", header.OH_FullName, ConsolWrapper.ConsigneeOrg.Name);
		}

		public void TestConsignorOrg()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			Consol.SetDefaultSendingForwarderAddress(header);
			AssertEquals("Should return the sending forwarder", header.OH_FullName, ConsolWrapper.ConsignorOrg.Name);
		}

		public void TestRequestForMissingDocumentsInstruction()
		{
			AssertEquals(Env.Registry.ShipmentRequestForMissingDocumentsClause, ConsolWrapper.RequestForMissingDocumentsInstruction);
		}

		public void TestContainerNumbers()
		{
			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "2";
			FreightContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			FreightContainer container4 = Consol.Containers.AddNew();
			container4.JC_ContainerNum = "4";
			FreightContainer container3 = Consol.Containers.AddNew();
			container3.JC_ContainerNum = "3";
			FreightContainer container5 = Consol.Containers.AddNew();

			ZString expectedResult = "1, 2, 3, 4";
			AssertEquals("Should return the container line", expectedResult, ConsolWrapper.ContainerNumbers);
		}

		public void TestMissingRequiredDocuments()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIP123";
			shipment.JS_HouseBill = "HOUSE444";
			ForwardingConsol consol = shipment.Consols.AddNew();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			AssertEquals("RequestForMissingDocumentsInstruction", ZString.Empty, consolWrapper.MissingRequiredDocuments);

			shipment.DocsAndCartage.RequiredDocuments.AddNew(Enterprise.Core.Constants.RefDocTypes.CommercialInvoice);

			ZString value = @"Shipment: SHIP123 House bill: HOUSE444
- Commercial Invoice";
			AssertEquals("Shipments missing document should be displayed", value, consolWrapper.MissingRequiredDocuments);
		}

		public void TestPackages()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			shipment.JS_OuterPacks = 1;
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("Should return totalpackagescount", "1", consolWrapper.Packages);
		}

		#endregion

		#region ITimeSlotRequest Members

		public void TestPackingMode()
		{
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Packing Mode should be Container Mode", ConsolWrapper.ConsolMode, ConsolWrapper.PackingMode);
		}

		public void TestBookingETA()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Should return ETA", ZDateTime.Today.ToShortDateString(), ConsolWrapper.BookingETA);
		}

		public void TestBookingETD()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Should return ETD", ZDateTime.Today.ToShortDateString(), ConsolWrapper.BookingETD);
		}

		public void TestEquipmentType()
		{
			AssertEquals("Should return empty string", ZString.Empty, ConsolWrapper.EquipmentType);
		}

		public void TestFullCartageInstructions()
		{
			AssertEquals("", ConsolWrapper.CartageInstructions);

			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			FreightHelperClass.AddNote(Consol, pickupDesc, "Consol Pickup Instructions");
			FreightHelperClass.AddNote(Consol, deliveryDesc, "Consol Delivery Instructions");

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Consol Delivery Instructions", ConsolWrapper.CartageInstructions);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Consol Pickup Instructions", ConsolWrapper.CartageInstructions);
		}

		public void TestFullHandlingInstructions()
		{
			AssertEquals("", ConsolWrapper.FullHandlingInstructions);

			StmNote note = AddNotes(Consol, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Contains Handling Instructions");
			Factory.Save();
			AssertEquals("Contains Handling Instructions", ConsolWrapper.FullHandlingInstructions);
		}

		public void TestTimeSlotRequestEquipmentType()
		{
			AssertEquals("Should return empty string", ZString.Empty, ConsolWrapper.EquipmentType);
		}

		public void TestCutOffOrAvailableDate()
		{
			Consol.Transports[0].JW_TerminalCutOff = ZDateTime.Today.AddDays(1);
			Consol.Transports[0].JW_TerminalAvailabilityDate = ZDateTime.Today.AddDays(2);
			Consol.JK_ConsolMode = "FCL";

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(1), ConsolWrapper.CutOffOrAvailableDate);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(2), ConsolWrapper.CutOffOrAvailableDate);

			Consol.Transports[0].JW_DepotCutOff = ZDateTime.Today.AddDays(3);
			Consol.Transports[0].JW_DepotAvailabilityDate = ZDateTime.Today.AddDays(4);
			Consol.JK_ConsolMode = "LCL";

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(3), ConsolWrapper.CutOffOrAvailableDate);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(4), ConsolWrapper.CutOffOrAvailableDate);
		}

		public void TestPickupOrStorageCommenceDate()
		{
			Consol.Transports[0].JW_TerminalReceivalCommences = ZDateTime.Today.AddDays(1);
			Consol.Transports[0].JW_TerminalStorageDate = ZDateTime.Today.AddDays(2);
			Consol.JK_ConsolMode = "FCL";

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(1), ConsolWrapper.PickupOrStorageCommenceDate);
			AssertEquals("FCL DEP Heading", "RECEIVALS START", ConsolWrapper.PickupOrStorageCommenceDateHeading);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(2), ConsolWrapper.PickupOrStorageCommenceDate);
			AssertEquals("FCL ARV Heading", "STORAGE STARTS", ConsolWrapper.PickupOrStorageCommenceDateHeading);

			Consol.JK_ConsolMode = "ULD";

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(1), ConsolWrapper.PickupOrStorageCommenceDate);
			AssertEquals("ULD DEP Heading", "RECEIVALS START", ConsolWrapper.PickupOrStorageCommenceDateHeading);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(2), ConsolWrapper.PickupOrStorageCommenceDate);
			AssertEquals("ULD ARV Heading", "STORAGE STARTS", ConsolWrapper.PickupOrStorageCommenceDateHeading);

			Consol.Transports[0].JW_DepotReceivalCommences = ZDateTime.Today.AddDays(3);
			Consol.Transports[0].JW_DepotStorageDate = ZDateTime.Today.AddDays(4);
			Consol.JK_ConsolMode = "LCL";

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(3), ConsolWrapper.PickupOrStorageCommenceDate);
			AssertEquals("LCL DEP Heading", "RECEIVALS START", ConsolWrapper.PickupOrStorageCommenceDateHeading);

			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(4), ConsolWrapper.PickupOrStorageCommenceDate);
			AssertEquals("LCL ARV Heading", "STORAGE STARTS", ConsolWrapper.PickupOrStorageCommenceDateHeading);
		}

		public void TestIDocCartageAdviceDates()
		{
			Transport transport1 = Consol.Transports[0];
			transport1.JW_ETD = new ZDateTime(2011, 2, 1);
			transport1.JW_ETA = new ZDateTime(2011, 2, 15);

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_ETD = new ZDateTime(2011, 1, 1);
			transport2.JW_ETA = new ZDateTime(2011, 1, 15);
			transport2.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			transport2.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			transport2.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			transport2.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport transport3 = Consol.Transports.AddNew();
			transport3.JW_ETD = new ZDateTime(2011, 4, 1);
			transport3.JW_ETA = new ZDateTime(2011, 4, 15);
			transport3.JW_TerminalAvailabilityDate = new ZDateTime(2011, 4, 18);
			transport3.JW_DepotAvailabilityDate = new ZDateTime(2011, 4, 20);
			transport3.JW_TerminalStorageDate = new ZDateTime(2011, 4, 22);
			transport3.JW_DepotStorageDate = new ZDateTime(2011, 4, 25);

			Transport transport4 = Consol.Transports.AddNew();
			transport4.JW_ETD = new ZDateTime(2011, 3, 1);
			transport4.JW_ETA = new ZDateTime(2011, 3, 15);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), ConsolWrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), ConsolWrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), ConsolWrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), ConsolWrapper.CartageStorageCommenceDate);

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), ConsolWrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), ConsolWrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), ConsolWrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), ConsolWrapper.CartageStorageCommenceDate);
		}

		public void TestCTOAddress()
		{
			AssertEquals(null, ConsolWrapper.CTOAddress);
		}

		public void TestNotifyParty()
		{
			AssertEquals(null, ConsolWrapper.NotifyParty);
		}

		public void TestOriginLoco()
		{
			AssertEquals(null, ConsolWrapper.OriginLoco);
		}

		public void TestDestinationLoco()
		{
			AssertEquals(null, ConsolWrapper.DestinationLoco);
		}

		public void TestSimpleContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("Consol should have no containers", 0, consolWrapper.SimpleContainers.Count);

			FreightContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "ctr1";

			AssertEquals("Consol should have one container", 1, consolWrapper.SimpleContainers.Count);
			AssertEquals("Containers should contain Container1", container1.JC_ContainerNum, consolWrapper.SimpleContainers[0].ContainerNumber);
		}

		#endregion

		#region IDoc Cartage Advice

		public void TestIDocCartagePrintTwoJourneys()
		{
			AssertEquals("Must be false for consol", false, ConsolWrapper.PrintTwoJourneys);
		}

		public void TestIDocCartageJourneyOnePickUpHeading()
		{
			SetUpConsolForDocCartageTests();
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOnePickUpHeading: LSE Export", "PICKUP FROM", ConsolWrapper.JourneyOnePickUpHeading);
		}

		public void TestIDocCartageJourneyOneDeliverToHeading()
		{
			SetUpConsolForDocCartageTests();
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliverToHeading: LSE Import", "DELIVER TO", ConsolWrapper.JourneyOneDeliverToHeading);
		}

		public void TestIDocCartageJourneyTwoHeadings()
		{
			SetUpConsolForDocCartageTests();
			AssertEquals("JourneyTwoPickUpHeading: LSE Export", "", ConsolWrapper.JourneyTwoPickUpHeading);
			AssertEquals("JourneyTwoDeliverToHeading: LSE Import", "", ConsolWrapper.JourneyTwoDeliverToHeading);
		}

		public void TestIDocCartageJourneysAddresses()
		{
			SetUpConsolForDocCartageTests();

			ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
				AssertEquals("Export Document", true, ConsolWrapper.IsExportDocument);
				AssertEquals("JourneyOnePickUpAddress", "departure CFS address", ConsolWrapper.JourneyOnePickUpAddress.Address1);
				AssertEquals("JourneyOneDeliverToAddress", "departure CTO address", ConsolWrapper.JourneyOneDeliverToAddress.Address1);
				AssertEquals("JourneyTwoPickUpAddress", null, ConsolWrapper.JourneyTwoPickUpAddress);
				AssertEquals("JourneyTwoDeliverToAddress", null, ConsolWrapper.JourneyTwoDeliverToAddress);

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);
				ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
				AssertEquals("Import Document", false, ConsolWrapper.IsExportDocument);
				AssertEquals("JourneyOnePickUpAddress", "arrival CTO address", ConsolWrapper.JourneyOnePickUpAddress.Address1);
				AssertEquals("JourneyOneDeliverToAddress", "arrival CFS address", ConsolWrapper.JourneyOneDeliverToAddress.Address1);
				AssertEquals("JourneyTwoPickUpAddress", null, ConsolWrapper.JourneyTwoPickUpAddress);
				AssertEquals("JourneyTwoDeliverToAddress", null, ConsolWrapper.JourneyTwoDeliverToAddress);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCompany);
			}
		}

		public void TestIDocCartageJourneysContacts()
		{
			SetUpConsolForDocCartageTests();

			ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				AssertEquals("Import", false, ConsolWrapper.IsExportConsol);

				AssertEquals("JourneyOnePickUpContactName", "arrival CTO manager", ConsolWrapper.JourneyOnePickUpContactName);
				AssertEquals("JourneyOnePickUpContactPhone", "888888", ConsolWrapper.JourneyOnePickUpContactPhone);

				AssertEquals("JourneyOneDeliverToContactName", "arrival CFS manager", ConsolWrapper.JourneyOneDeliverToContactName);
				AssertEquals("JourneyOneDeliverToContactPhone", "222333", ConsolWrapper.JourneyOneDeliverToContactPhone);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCompany);
			}
		}

		void SetUpConsolForDocCartageTests()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;

			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "USLAX";

			OrgHeader orgArrivalCFS = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = orgArrivalCFS.Contacts.AddNew();
			contact1.OC_ContactName = "arrival CFS manager";
			contact1.OC_Phone = "222333";
			contact1.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			orgArrivalCFS.MainAddress.OA_Address1 = "arrival CFS address";
			orgArrivalCFS.MainAddress.OA_Phone = "4815162342";

			OrgHeader orgArrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contactCTO = orgArrivalCTO.Contacts.AddNew();
			contactCTO.OC_ContactName = "arrival CTO manager";
			contactCTO.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;

			orgArrivalCTO.MainAddress.OA_Address1 = "arrival CTO address";
			orgArrivalCTO.MainAddress.OA_Phone = "888888";

			OrgHeader orgArrivalContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contactContainerYard = orgArrivalContainerYard.Contacts.AddNew();
			contactContainerYard.OC_ContactName = "arrival Container Yard manager";
			contactContainerYard.Documents.AddNew().OD_DocumentGroup = ContactType.All.Code;

			orgArrivalContainerYard.MainAddress.OA_Address1 = "arrival Container Yard address";
			orgArrivalContainerYard.MainAddress.OA_Phone = "987654321";

			Consol.JK_OA_ArrivalCTOAddress = orgArrivalCTO.MainAddress.PK;
			Consol.JK_OA_UnpackDepotAddress = orgArrivalCFS.MainAddress.PK;
			Consol.JK_OA_ContainerYardEmptyReturnAddress = orgArrivalContainerYard.MainAddress.PK;

			var departureCFS = Factory.NewWithValidTestData<OrgAddress>();
			departureCFS.OA_Address1 = "departure CFS address";
			var departureCTO = Factory.NewWithValidTestData<OrgAddress>();
			departureCTO.OA_Address1 = "departure CTO address";
			var departureContainerYard = Factory.NewWithValidTestData<OrgAddress>();
			departureContainerYard.OA_Address1 = "departure Container Yard address";

			Consol.JK_OA_DepartureCTOAddress = departureCTO.PK;
			Consol.JK_OA_PackDepotAddress = departureCFS.PK;
			Consol.JK_OA_ContainerYardEmptyPickupAddress = departureContainerYard.PK;
		}

		#endregion

		#region Implementation Method Tests

		public void TestGetCarrier()
		{
			AssertEquals("Carrier is empty", ZString.Empty, ConsolWrapper.GetCarrier(null));

			Transport transport = Consol.Transports.AddNew();
			DocTransport transportWrapper = DocTransport.New(Consol, transport, Factory);
			AssertEquals("Carrier is empty", ZString.Empty, ConsolWrapper.GetCarrier(transportWrapper));

			var airLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.NotEqual, ""));
			transport.JW_VoyageFlight = "ZZ123";
			transportWrapper = DocTransport.New(Consol, transport, Factory);
			AssertEquals("Carrier is empty", ZString.Empty, ConsolWrapper.GetCarrier(transportWrapper));

			transport.JW_VoyageFlight = airLine.RM_TwoCharacterCode + "12";
			transportWrapper = DocTransport.New(Consol, transport, Factory);
			AssertEquals("Carrier", airLine.RM_AirlineName1, ConsolWrapper.GetCarrier(transportWrapper));
		}

		public void TestIsUSConsol()
		{
			AssertEquals("!IsUSConsol", false, ConsolWrapper.IsUSConsol);

			Transport primariyTransport = Consol.Transports[0];

			var transport = Consol.Transports.AddNew();
			AssertEquals("!IsUSConsol", false, ConsolWrapper.IsUSConsol);

			var nonUSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, "US"));
			transport.JW_RL_NKDiscPort = nonUSUNLOCO.RL_Code;
			AssertEquals("!IsUSConsol", false, ConsolWrapper.IsUSConsol);

			var uSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "US"));
			transport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;
			AssertEquals("IsUSConsol", true, ConsolWrapper.IsUSConsol);

			transport.JW_RL_NKDiscPort = "";
			primariyTransport.JW_RL_NKDiscPort = nonUSUNLOCO.RL_Code;
			AssertEquals("!IsUSConsol", false, ConsolWrapper.IsUSConsol);

			primariyTransport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;
			AssertEquals("IsUSConsol", true, ConsolWrapper.IsUSConsol);
		}

		public void TestGetNewDocShipment()
		{
			AssertNull("Frieght shipment is null", ConsolWrapper.GetNewDocShipment(null));

			var shipment = Consol.Shipments.AddNew();
			AssertNotNull("Frieght shipment", ConsolWrapper.GetNewDocShipment(shipment));
			AssertEquals("Is of type DocForwardingShipment", typeof(DocForwardingShipment), ConsolWrapper.GetNewDocShipment(shipment).GetType());
		}

		public void TestGetNewDocShipmentWithPK()
		{
			AssertNull("Shipment", ConsolWrapper.GetNewDocShipment(null));
			AssertNull("Shipment", ConsolWrapper.GetNewDocShipment(ZGuid.Empty));
			AssertNull("Shipment", ConsolWrapper.GetNewDocShipment(ZGuid.Invalid));

			var shipment = Consol.Shipments.AddNew();
			Factory.Save();
			AssertNotNull("Shipment", ConsolWrapper.GetNewDocShipment(shipment.PK));
			AssertEquals("Is of type DocForwardingShipment", typeof(DocForwardingShipment), ConsolWrapper.GetNewDocShipment(shipment.PK).GetType());
		}

		public void TestSetReportNameAndDocumentDirection()
		{
			ConsolWrapper.SetReportNameForTesting("Consol Report name");
			ConsolWrapper.SetDocumentDirectionForTesting("Import");

			var shipment = Consol.Shipments.AddNew();
			var shipmentWrapper = DocForwardingShipment.New(shipment, Factory);

			AssertEquals("Shipment report name", ConsolWrapper.ReportName, shipmentWrapper.ReportName);
			AssertEquals("Shipment document direction", ConsolWrapper.DocumentDirection, shipmentWrapper.DocumentDirection);
		}

		public void TestGetTransportPlanningTransportDetails()
		{
			AssertEquals("Transport details are empty", "     /     /    ", ConsolWrapper.GetTransportPlanningTransportDetails(""));

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "1234";

			ZString expectedResult = ConsolWrapper.FormatTransportDetails(Consol.JK_JX_JV_NKVessel, Consol.JK_JX_JV_VoyageFlight, vessel.RV_LloydsNumber);
			AssertEquals("Transport details", expectedResult, ConsolWrapper.GetTransportPlanningTransportDetails(""));

			var mainTransport = Consol.Transports.AddNew();
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_Vessel = vessel.RV_Code;
			mainTransport.JW_VoyageFlight = "5678";
			expectedResult = ConsolWrapper.FormatTransportDetails(mainTransport.JW_Vessel, mainTransport.JW_VoyageFlight, vessel.RV_LloydsNumber);
			AssertEquals("Transport details", expectedResult, ConsolWrapper.GetTransportPlanningTransportDetails(""));

			var preCarriageTransport = Consol.Transports.AddNew();
			preCarriageTransport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			preCarriageTransport.JW_Vessel = vessel.RV_Code;
			preCarriageTransport.JW_VoyageFlight = "9012";
			expectedResult = ConsolWrapper.FormatTransportDetails(preCarriageTransport.JW_Vessel, preCarriageTransport.JW_VoyageFlight, vessel.RV_LloydsNumber);
			AssertEquals("Transport details", expectedResult, ConsolWrapper.GetTransportPlanningTransportDetails(Core.Constants.TransportPlanningType.PreCarriage));
		}

		#endregion

		#region Implementation
		DocForwardingConsol ConsolWrapper;
		ForwardingConsol Consol;

		protected override void SetUp()
		{
			Consol = Factory.New<ForwardingConsol>();
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertNotNull("PreCondition: Valid DocForwardingConsol", ConsolWrapper);

			base.SetUp();
		}

		void SetUpConsolWithMinimumRequiredToSave()
		{
			Consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertNotNull("PreCondition: Valid DocForwardingConsol", ConsolWrapper);
		}

		void SetUpConsolWithValidPortCodes()
		{
			Consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.PopulatePortCodes);
			Transport transport = Consol.Transports[0];
			transport.FillWithValidTestData(TestBusinessObjectKind.PopulatePortCodes, null);
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertNotNull("PreCondition: Valid DocForwardingConsol", ConsolWrapper);
		}

		ZString GetAUnLocoCodeForCurrentCountry()
		{
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			return uNLOCO.RL_Code;
		}

		ZString GetAUnLocoCodeForTheUS()
		{
			var uSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "US"));
			return uSUNLOCO.RL_Code;
		}

		#endregion

		#region Old Tests

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestGetAirTransportDetailsWithFlight2()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport = consol.Transports[0];
			AssertEquals("precondition:", Core.Constants.TransportModes.Air, transport.JW_TransportMode);
			AssertEquals("precondition:", Core.Constants.TransportPlanningType.Flight1, transport.JW_TransportType);

			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = DateTime.Today;
			transport.JW_VoyageFlight = "111";
			ZString depTime = consol.JK_JX_JA_E_DEP.ToString("dd-MMM-yy");
			ZString expected = consolWrapper.FormatTransportDetails(consol.JK_JX_JV_VoyageFlight, consol.JK_JX_JB_RL_NKPortOfDischarge, depTime);
			AssertEquals(expected, consolWrapper.GetAirTransportDetails);

			var newTransport = consol.Transports.AddNew();
			newTransport.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			newTransport.JW_ETD = DateTime.Today.AddDays(2);
			depTime = newTransport.JW_ETD.ToString("dd-MMM-yy");
			newTransport.JW_VoyageFlight = "222";
			newTransport.JW_RL_NKLoadPort = "AUSYD";
			newTransport.JW_RL_NKDiscPort = "AUBNE";
			expected += " -> " + consolWrapper.FormatTransportDetails(newTransport.JW_VoyageFlight, newTransport.JW_RL_NKDiscPort, depTime);
			AssertEquals(expected, consolWrapper.GetAirTransportDetails);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			AssertEquals(expected, consolWrapper.GetAirTransportDetails);

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = DateTime.Today.AddDays(1);
			expected = consolWrapper.FormatTransportDetails(consol.JK_JX_JV_VoyageFlight, consol.JK_JX_JB_RL_NKPortOfDischarge, ZString.Empty);
			expected += " -> " + consolWrapper.FormatTransportDetails(newTransport.JW_VoyageFlight, newTransport.JW_RL_NKDiscPort, ZString.Empty);

			transport.JW_ETD = DateTime.Today.AddDays(-1);
			depTime = consol.JK_JX_JA_E_DEP.ToString("dd-MMM-yy");
			expected = consolWrapper.FormatTransportDetails(consol.JK_JX_JV_VoyageFlight, consol.JK_JX_JB_RL_NKPortOfDischarge, depTime);
			expected += " -> " + consolWrapper.FormatTransportDetails(newTransport.JW_VoyageFlight, newTransport.JW_RL_NKDiscPort, depTime);
		}

		public void TestChargeableUnit()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("Chargeable Unit", "M3", consolWrapper.ChargeableUnit);
		}

		public void TestPackDepotAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Pack Depot Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "111 One Street";
			address.OA_Address2 = "Building A";
			consol.JK_OA_PackDepotAddress = address.PK;

			AssertEquals("Depot Name", header.OH_FullName, consolWrapper.PackDepotAddress.CompanyName);
			AssertEquals("Depot Address1", address.OA_Address1, consolWrapper.PackDepotAddress.Address1);
		}

		public void TestUnpackDepotAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Unpack Depot Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "222 Two Street";
			address.OA_Address2 = "Building B";

			consol.JK_OA_UnpackDepotAddress = address.PK;

			AssertEquals("UnpackDepot Name", header.OH_FullName, consolWrapper.UnpackDepotAddress.CompanyName);
			AssertEquals("UnpackDepot Address1", address.OA_Address1, consolWrapper.UnpackDepotAddress.Address1);
		}

		public void TestArrivalCTOAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Arrival CTO Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "333 Three Street";
			address.OA_Address2 = "Building C";
			consol.JK_OA_ArrivalCTOAddress = address.PK;

			AssertEquals("Arrival CTO Name", header.OH_FullName, consolWrapper.ArrivalCTOAddress.CompanyName);
			AssertEquals("Arrival CTO Address1", address.OA_Address1, consolWrapper.ArrivalCTOAddress.Address1);
		}

		public void TestDepartureCTOAddress()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Departure CTO Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "444 Four Street";
			address.OA_Address2 = "Building D";
			Consol.JK_OA_DepartureCTOAddress = address.PK;

			AssertEquals("Departure CTO Name", header.OH_FullName, ConsolWrapper.DepartureCTOAddress.CompanyName);
			AssertEquals("Departure CTO Address1", address.OA_Address1, ConsolWrapper.DepartureCTOAddress.Address1);
		}

		public void TestFreightDepot()
		{
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			var branchUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort));
			var uSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "US"));

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = uSUNLOCO.RL_Code;
			transport.JW_RL_NKDiscPort = branchUNLOCO.RL_Code;

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var arrivalCTOHeader = Factory.New<OrgHeader>();
			arrivalCTOHeader.OH_FullName = "Arrival CTO Ltd";
			var arrivalCTOAddress = arrivalCTOHeader.Addresses.AddNew();
			arrivalCTOAddress.OA_Address1 = "555 Five Street";
			arrivalCTOAddress.OA_Address2 = "Building E";
			Consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.PK;
			AssertEquals("Arrival CTO Address", arrivalCTOAddress.OA_Address1, ConsolWrapper.FreightDepot.Address1);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Unpack Depot Ltd.";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "222 Two Street";
			address.OA_Address2 = "Building B";
			Consol.JK_OA_UnpackDepotAddress = address.PK;
			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			AssertEquals("Unpack depot Address", address.OA_Address1, ConsolWrapper.FreightDepot.Address1);

			transport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			transport.JW_RL_NKLoadPort = uNLOCO.RL_Code;

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var departureCTOHeader = Factory.New<OrgHeader>();
			departureCTOHeader.OH_FullName = "Departure CTO Ltd";
			var departureCTOAddress = departureCTOHeader.Addresses.AddNew();
			departureCTOAddress.OA_Address1 = "444 Four Street";
			departureCTOAddress.OA_Address2 = "Building D";
			Consol.JK_OA_DepartureCTOAddress = departureCTOAddress.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Departure CTO Address", departureCTOAddress.OA_Address1, ConsolWrapper.FreightDepot.Address1);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			var packDepotHeader = Factory.New<OrgHeader>();
			packDepotHeader.OH_FullName = "Arrival CTO Ltd";
			var packDepotAddress = packDepotHeader.Addresses.AddNew();
			packDepotAddress.OA_Address1 = "555 Five Street";
			packDepotAddress.OA_Address2 = "Building E";
			Consol.JK_OA_PackDepotAddress = packDepotAddress.PK;

			ConsolWrapper = DocForwardingConsol.New(Consol, Factory);
			ConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Pack depot Address", packDepotAddress.OA_Address1, ConsolWrapper.FreightDepot.Address1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestTransportDetailsETD()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consol.JK_TransportMode = "AIR";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "VIRGIN222";
			transport.JW_ATD = Env.Time.CurrentLocalDate.AddDays(-5);
			transport.JW_ATA = Env.Time.CurrentLocalDate.AddDays(-5);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USCHI"));
			transport.JW_RL_NKDiscPort = uNLOCO.RL_Code;

			AssertEquals("Transport mode AIR, transport type is 'Flight & Date'", "FLIGHT & DATE", consolWrapper.TransportHeading);
			AssertEquals("Transport mode AIR, transport details is 'Voyage DischargePort ETD'", "VIRGIN222 / " + uNLOCO.RL_Code + " / " + Env.Time.CurrentLocalDate.AddDays(-5).ToString("dd-MMM-yy"), consolWrapper.ConsolTransportInfo.ToString());
			AssertEquals("Transport mode AIR, should print 'Air'", "A" + "ir".ToLower(), consolWrapper.HeadingTransportMode.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestTransportDetailsATD()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consol.JK_TransportMode = "AIR";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "VIRGIN222";
			transport.JW_ETD = Env.Time.CurrentLocalDate.AddDays(-4);
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USCHI"));
			transport.JW_RL_NKDiscPort = uNLOCO.RL_Code;

			AssertEquals("Transport mode AIR, transport details is 'Voyage DischargePort ATD'", "VIRGIN222 / " + uNLOCO.RL_Code + " / " + Env.Time.CurrentLocalDate.AddDays(-4).ToString("dd-MMM-yy"), consolWrapper.ConsolTransportInfo.ToString());
		}

		public void TestAirConsolTable()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consol.JK_TransportMode = "AIR";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "FLY 123";

			AssertEquals("Air shipment, master bill heading is 'MAWB'", "MAWB", consolWrapper.MasterBillHeading);
			AssertEquals("Transport Type", "A" + "IR".ToLower(), consolWrapper.HeadingTransportMode.ToString());
			AssertEquals("Transport field name to show in document", "FLIGHT & DATE", consolWrapper.TransportHeading);
			Assert("Transport", consolWrapper.ConsolTransportInfo == "FLY 123 /     /    ");
			AssertEquals("Chargeable Unit", "KG", consolWrapper.ChargeableUnit.ToString());
			AssertEquals("Container seal number heading", "Rate Class", consolWrapper.ContainerSealNumberHeading);
		}

		public void TestDeliveryLabelHeadings()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Master bill heading is OBL", "OBL", consolWrapper.MasterBillLabelHeading);
			AssertEquals("Transport heading is VESSEL", "VESSEL", consolWrapper.TransportLabelHeading);

			consol.JK_TransportMode = "AIR";
			consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("Master bill heading is MAWB", "MAWB", consolWrapper.MasterBillLabelHeading);
			AssertEquals("Transport heading is FLIGHT", "FLIGHT", consolWrapper.TransportLabelHeading);
		}

		public void TestSCAC()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];

			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			transport.JW_RL_NKDiscPort = "USLAX";

			var receivingHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultReceivingForwarderAddress(receivingHeader);
			ZString expectedValue = receivingHeader.SCACCode != "" ? receivingHeader.SCACCode : new ZString("*MISSING*");
			AssertEquals("Receiving agent SCAC", expectedValue, consolWrapper.AgentSCAC);

			var shippingLineHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultShippingLineAddress(shippingLineHeader);
			expectedValue = shippingLineHeader.SCACCode != "" ? shippingLineHeader.SCACCode : new ZString("*MISSING*");
			AssertEquals("Line SCAC", expectedValue, consolWrapper.LineSCAC);

			AssertEquals("SCAC Heading", "SCAC", consolWrapper.SCACHeading);
		}

		public void TestSCACWithNonUSConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			transport.JW_RL_NKDiscPort = "AUPER";
			AssertEquals("Receiving agent SCAC", "", consolWrapper.AgentSCAC);
			AssertEquals("Line SCAC", "", consolWrapper.LineSCAC);
			AssertEquals("SCAC Heading", "", consolWrapper.SCACHeading);
		}

		public void TestShipmentTotal()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_DocumentedWeight = ZDecimal.Parse("100.50");
			shipment1.JS_DocumentedVolume = ZDecimal.Parse("2.30");
			shipment1.JS_DocumentedChargeable = ZDecimal.Parse("80.23");

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_DocumentedWeight = ZDecimal.Parse("350.50");
			shipment2.JS_DocumentedVolume = ZDecimal.Parse("5.30");
			shipment2.JS_DocumentedChargeable = ZDecimal.Parse("95.77");

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Total documented weight", Decimal.Parse("451"), consolWrapper.TotalDocumentedWeight);
			AssertEquals("Total documented volume", Decimal.Parse("7.60"), consolWrapper.TotalDocumentedVolume);
			AssertEquals("Total documented chargeable", Decimal.Parse("7.6"), consolWrapper.TotalDocumentedChargeable);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Total documented chargeable for Air", Decimal.Parse("1266.667"), consolWrapper.TotalDocumentedChargeable);
		}

		public void TestDirectConsolApprovedExportedCode()
		{
			Consol.JK_AgentType = "DRT";
			var directShipment = Consol.Shipments.AddNew();

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.CountryData.OV_EXExportPermissionDetails = "EXP PMSN";
			directShipment.ConsignorPK = header.PK;
			AssertEquals("Exporter code should be Consignor's Export Permission", "EXP PMSN", ConsolWrapper.ApprovedExporterCode);

			header.CountryData.OV_EXExportPermissionDetails = "";
			AssertEquals("Exporter code should be Consignor's Code", directShipment.Consignor.OH_Code, ConsolWrapper.ApprovedExporterCode);
		}

		public void TestAgentTypeConsolApprovedExportedCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consol.JK_AgentType = "AGT";
			consol.SetDefaultSendingForwarderAddress(ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeaderSchema.Constants.PK, OrgHeaderSchema.Constants.TableName));

			AssertEquals("Exporter code should be Consol's Sending Forwarder's Code", consol.SendingForwarder.OH_Code, consolWrapper.ApprovedExporterCode);
		}

		public void TestLoadListInstructionNote()
		{
			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			var loadListNote = consol.Notes.AddNew();
			loadListNote.ST_Description = PredefinedNoteTypes.Instance.LoadListInstructions.Description;
			loadListNote.ST_Table = consol.TableName;
			loadListNote.ST_ParentID = consol.PK;
			loadListNote.ST_NoteDataAsText = "Load List instruction notes\nContainers are to be stored below 15 degrees C.";
			Factory.Save();

			var otherNotes = consol.Notes.AddNew();
			otherNotes.ST_Table = consol.TableName;
			otherNotes.ST_ParentID = consol.PK;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.InvoicingPreferences.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			Factory.Save();

			AssertEquals("Load List instruction notes\nContainers are to be stored below 15 degrees C.", consolWrapper.LoadListInstructions);
		}
		public void TestExportManifestTotalVolumeAIRMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consolWrapper.SetDocumentDirectionForTesting("DEP");
			consolWrapper.SetReportNameForTesting("Manifest");

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_GoodsDescription = "shipment1";
			shipment1.JS_ActualVolume = 3.500M;
			shipment1.JS_DocumentedVolume = 5.00M;
			shipment1.JS_ManifestedVolume = 2M;
			shipment1.JS_UnitOfVolume = "M3";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_GoodsDescription = "shipment2";
			shipment2.JS_ActualVolume = 30.500M;
			shipment2.JS_DocumentedVolume = 35.00M;
			shipment2.JS_ManifestedVolume = 20M;
			shipment2.JS_UnitOfVolume = "M3";

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_GoodsDescription = "shipment3";
			shipment3.JS_ActualVolume = 10.500M;
			shipment3.JS_DocumentedVolume = 15.00M;
			shipment3.JS_ManifestedVolume = 8M;
			shipment3.JS_UnitOfVolume = "M3";

			if (Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir)
			{
				if (Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual)
				{
					AssertEquals("Total actual volume", "44.5 M3", consolWrapper.ExportManifestTotalVolume);
				}
				else if (Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Client)
				{
					AssertEquals("Total client volume", "55 M3", consolWrapper.ExportManifestTotalVolume);
				}
				else if (Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Carrier)
				{
					AssertEquals("Total carrier volume", "30 M3", consolWrapper.ExportManifestTotalVolume);
				}
				AssertEquals("VOLUME", consolWrapper.ExportManifestVolumeHeading);
			}
			else
			{
				AssertEquals("", consolWrapper.ExportManifestTotalVolume);
				AssertEquals("", consolWrapper.ExportManifestVolumeHeading);
			}
		}

		public void TestExportManifestTotalChargeableSEAMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			consolWrapper.SetDocumentDirectionForTesting("DEP");
			consolWrapper.SetReportNameForTesting("Manifest (Landscape)");

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_UnitOfVolume = "M3";
			shipment1.JS_GoodsDescription = "shipment1";
			shipment1.JS_ActualChargeable = 9.500M;
			shipment1.JS_DocumentedChargeable = 15M;
			shipment1.JS_ManifestedChargeable = 5M;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_UnitOfVolume = "M3";
			shipment2.JS_GoodsDescription = "shipment2";
			shipment2.JS_ActualChargeable = 20.500M;
			shipment2.JS_DocumentedChargeable = 25M;
			shipment2.JS_ManifestedChargeable = 15M;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment3.JS_UnitOfVolume = "M3";
			shipment3.JS_GoodsDescription = "shipment3";
			shipment3.JS_ActualChargeable = 1.500M;
			shipment3.JS_DocumentedChargeable = 2M;
			shipment3.JS_ManifestedChargeable = 1M;

			if (Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenSea)
			{
				if (Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual)
				{
					AssertEquals("Total actual chargeable", "31.5 M3", consolWrapper.ExportManifestTotalChargeable);
				}
				else if (Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Client)
				{
					AssertEquals("Total documented chargeable", "42 M3", consolWrapper.ExportManifestTotalChargeable);
				}
				else if (Env.Registry.ConsolManifestConsolExportWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Carrier)
				{
					AssertEquals("Total carrier chargeable", "21 M3", consolWrapper.ExportManifestTotalChargeable);
				}
				AssertEquals("CHARGEABLE", consolWrapper.ExportManifestChargeableHeading);
			}
			else
			{
				AssertEquals("", consolWrapper.ExportManifestTotalChargeable);
				AssertEquals("", consolWrapper.ExportManifestChargeableHeading);
			}
		}

		#endregion

		public void TestConsignorForCMR()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorForCMR);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorForCMR);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.ConsignorForCMR);

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JAYSCH");
			shipment.ConsignorPK = orgHeader.PK;
			AssertEquals("JAY SCHULZ\n15 LEONIE STREET\nCAMIRA QLD\nAUSTRALIA", ConsolWrapper.ConsignorForCMR);

			Consol.JK_AgentType = "";
			orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MIDINT");
			Consol.SetDefaultSendingForwarderAddress(orgHeader);
			AssertEquals("MIDAS INTERACTIVE ENTERTAINMENT\nUNIT 3/17 LEAD DRIVE\nBURLIEGH\nJUNCTION QLD\nAUSTRALIA", ConsolWrapper.ConsignorForCMR);
		}

		public void TestConsigneeForCMR()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeForCMR);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeForCMR);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.ConsigneeForCMR);

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JAYSCH");
			shipment.ConsigneePK = orgHeader.PK;
			AssertEquals("JAY SCHULZ\n15 LEONIE STREET\nCAMIRA QLD\nAUSTRALIA", ConsolWrapper.ConsigneeForCMR);

			Consol.JK_AgentType = "";
			orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MIDINT");
			Consol.SetDefaultReceivingForwarderAddress(orgHeader);
			AssertEquals("MIDAS INTERACTIVE ENTERTAINMENT\nUNIT 3/17 LEAD DRIVE\nBURLIEGH\nJUNCTION QLD\nAUSTRALIA", ConsolWrapper.ConsigneeForCMR);
		}

		public void TestCarrierForCMR()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.CarrierForCMR);

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "INTAIR");
			Consol.SetDefaultShippingLineAddress(orgHeader);
			AssertEquals("INTERNATIONAL CARGO SERVICES\nAFFLECK ROAD\nPERTH INTERNATIONAL AIRPORT, WA\n6015\nAUSTRALIA", ConsolWrapper.CarrierForCMR);
		}

		public void TestPlaceOfDeliveryForCMR()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.PlaceOfDeliveryForCMR);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.PlaceOfDeliveryForCMR);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.PlaceOfDeliveryForCMR);

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JAYSCH");
			shipment.ConsigneePK = orgHeader.PK;
			AssertEquals("CAMIRA, QLD AUSTRALIA", ConsolWrapper.PlaceOfDeliveryForCMR);

			Consol.JK_AgentType = "";
			orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MIDINT");
			Consol.SetDefaultReceivingForwarderAddress(orgHeader);
			AssertEquals("JUNCTION, QLD AUSTRALIA", ConsolWrapper.PlaceOfDeliveryForCMR);
		}

		public void TestPlaceOfDeliveryForCMR_ReceivingOrganisationHasInvalidPort_DoNotIncludePortInformation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "XXXXX";

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			Consol.SetDefaultReceivingForwarderAddress(orgHeader);

			AssertEquals(String.Empty, ConsolWrapper.PlaceOfDeliveryForCMR);
		}

		public void TestPlaceDateOfTakingOverGoodsForCMR()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.PlaceDateOfTakingOverGoodsForCMR);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.PlaceDateOfTakingOverGoodsForCMR);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.PlaceDateOfTakingOverGoodsForCMR);

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JAYSCH");
			shipment.ConsignorPK = orgHeader.PK;
			AssertEquals("CAMIRA, QLD AUSTRALIA", ConsolWrapper.PlaceDateOfTakingOverGoodsForCMR);

			Consol.JK_AgentType = "";
			orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MIDINT");
			Consol.SetDefaultSendingForwarderAddress(orgHeader);
			AssertEquals("JUNCTION, QLD AUSTRALIA", ConsolWrapper.PlaceDateOfTakingOverGoodsForCMR);
		}

		public void TestEstablishedInForCMR()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.EstablishedInForCMR);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(ZString.Empty, ConsolWrapper.EstablishedInForCMR);

			var shipment = Consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, ConsolWrapper.EstablishedInForCMR);

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JAYSCH");
			shipment.ConsignorPK = orgHeader.PK;
			AssertEquals("CAMIRA", ConsolWrapper.EstablishedInForCMR);

			Consol.JK_AgentType = "";
			orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MIDINT");
			Consol.SetDefaultSendingForwarderAddress(orgHeader);
			AssertEquals("JUNCTION", ConsolWrapper.EstablishedInForCMR);
		}

		public void TestGoodsDescriptionForCMR()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("", consolWrapper.GoodsDescriptionForCMR);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsDescription = "";
			AssertEquals("", consolWrapper.GoodsDescriptionForCMR);

			shipment.JS_GoodsDescription = "SHIPMENT1";

			shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsDescription = "SHIPMENT2";

			shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsDescription = "SHIPMENT3";
			Factory.Save();
			ZString reference = consolWrapper.GoodsDescriptionForCMR;
			Assert(reference.Contains("SHIPMENT1"));
			Assert(reference.Contains("SHIPMENT2"));
			Assert(reference.Contains("SHIPMENT3"));
		}

		public void TestOrderReferenceForCMR()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("", consolWrapper.OrderReferenceForCMR);

			CommonShipment shipment = consol.Shipments.AddNew();
			OrderItem orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "SHIPMENT1";

			shipment = consol.Shipments.AddNew();
			orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "SHIPMENT2";

			shipment = consol.Shipments.AddNew();
			orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "SHIPMENT3";

			Factory.Save();
			ZString reference = consolWrapper.OrderReferenceForCMR;
			Assert(reference.Contains("SHIPMENT1"));
			Assert(reference.Contains("SHIPMENT2"));
			Assert(reference.Contains("SHIPMENT3"));
		}

		public void TestWeightInKgs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals(0m, consolWrapper.WeightInKgs);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualWeight = 5;

			shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100;

			AssertZDecimalEquals("Weight conversion to KG", 50.359m, consolWrapper.WeightInKgs, 0.0001);
		}

		public void TestWeightInKgsForCMR()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals(0m, consolWrapper.WeightInKgsForCMR);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualWeight = 5;

			shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100;

			AssertEquals(50.36m, consolWrapper.WeightInKgsForCMR);
		}

		public void TestWeightInKgsRound3()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals(0m, consolWrapper.WeightInKgsRound3);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualWeight = 5;

			shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100;

			AssertEquals(50.359m, consolWrapper.WeightInKgsRound3);
		}

		public void TestVolumeInM3ForCMR()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals(0m, consolWrapper.VolumeInM3ForCMR);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = 5;

			shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			shipment.JS_ActualVolume = 100;

			AssertEquals(7.83m, consolWrapper.VolumeInM3ForCMR);
		}

		public void TestEmpty()
		{
			AssertEquals("DETAILS", ConsolWrapper.Empty);
		}

		public void TestSendersInstructions()
		{
			AssertEquals("SENDER'S INSTRUCTIONS", ConsolWrapper.SendersInstructions);
		}

		public void TestConsoleRateConfirmationClosingText()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsoleRateConfirmationClosingText);
			DocumentsDataRegistry.Instance.ConsoleRateConfirmationClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Closing text");
			AssertEquals("Closing text", ConsolWrapper.ConsoleRateConfirmationClosingText);
		}

		public void TestConsoleRateConfirmationOpeningText()
		{
			AssertEquals(ZString.Empty, ConsolWrapper.ConsoleRateConfirmationOpeningText);
			DocumentsDataRegistry.Instance.ConsoleRateConfirmationOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Opening text");
			AssertEquals("Opening text", ConsolWrapper.ConsoleRateConfirmationOpeningText);
		}

		#region TestContainerExistsWithSeal2

		public void TestContainerExistsWithSeal2()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightContainer container1 = consol.Containers.AddNew();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			AssertEquals(false, consolWrapper.ContainerExistsWithSeal2);

			container1.JC_AdditionalSealNum = "SEAL 2";
			AssertEquals(true, consolWrapper.ContainerExistsWithSeal2);
		}

		#endregion

		#region TestContainerExistsWithSeal3

		public void TestContainerExistsWithSeal3()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightContainer container1 = consol.Containers.AddNew();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol, Factory);

			AssertEquals(false, consolWrapper.ContainerExistsWithSeal3);

			container1.JC_Additional2SealNum = "SEAL 3";
			AssertEquals(true, consolWrapper.ContainerExistsWithSeal3);
		}

		#endregion

		public void TestLocalTaxTitle()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("LocalTaxTitle", "Local Value", ConsolWrapper.LocalTaxTitle);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertEquals("LocalTaxTitle", "Local(Excl Tax)", ConsolWrapper.LocalTaxTitle);
		}

		#region Implementation

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}

				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;

		static Job GetNewHeader(IJobHeaderParent parent)
		{
			Job result = parent.Factory.NewJobForTesting<Job>();
			result.JH_ParentID = parent.PK;
			result.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.TableName);
			result.Parent = parent;
			return result;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocForwardingConsol.New(Consol, Factory), DocForwardingConsol.New(Factory, Consol.PK) };
		}

		TResult RecreateTestingDocWrapper<T, TResult>(T bizo, Func<T, BusinessObjectFactory, TResult> createWrapper)
			where T : BusinessObject
			where TResult : DocumentWrapper
		{
			Factory.Save();
			bizo.Factory.Save();
			ReleaseFactory();
			return createWrapper(Factory.Load<T>(bizo.PK), Factory);
		}

		#endregion
	}
}
