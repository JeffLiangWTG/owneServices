using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

sealed class NctsDeclarationComparatorHelperTest : TestCaseWithFactory
{
	public void TestGetLatestEffectiveMessage_ChecksAndTQU()
	{
		var header = Factory.New<NctsHeader>();

		var glbBranch = Factory.New<GlbBranch>();
		glbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
		glbBranch.GB_Code = "XAX";

		var (tquMessageNotAcc, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.TransitNcts5Query, DateTime.Now.AddDays(-1));
		tquMessageNotAcc.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
		var (tquMessage, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.TransitNcts5Query, DateTime.Now.AddDays(-2));
		var (_, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.TransitNcts5Query, DateTime.Now.AddDays(-5));

		var (dpdNotAcc, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now);
		dpdNotAcc.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
		var (_, dpdSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now.AddDays(-3), new ZGuid("4BF802C5-C68B-4684-928D-2F7BF6B3396E"));
		var (_, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now.AddDays(-4));

		var (dpmNotAcc, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-1));
		dpmNotAcc.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
		var (_, dpmSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-5), new ZGuid("CA135BCD-6C55-481A-8343-9D7B2F1EBE18"));
		var (_, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-6));

		Factory.Save();

		CombineAssertions(() =>
		{
			var lastMessage = NctsDeclarationComparatorHelper.GetLatestEffectiveMessage(header);
			AssertEquals("LatestEffectiveMessage is TQU", tquMessage.PK, lastMessage.PK);
			AssertNotEquals("LatestEffectiveMessage is not DPD", dpdSent.PK, lastMessage.PK);
			AssertNotEquals("LatestEffectiveMessage is not DPM", dpmSent.PK, lastMessage.PK);
		});
	}

	public void TestGetLatestEffectiveMessage_DPD()
	{
		var header = Factory.New<NctsHeader>();

		var glbBranch = Factory.New<GlbBranch>();
		glbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
		glbBranch.GB_Code = "XAX";

		var (tquMessage, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.TransitNcts5Query, DateTime.Now.AddDays(-2));

		var (dpdNotAcc, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now);
		dpdNotAcc.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
		var (_, dpdSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now.AddDays(-1), new ZGuid("4BF802C5-C68B-4684-928D-2F7BF6B3396E"));
		var (_, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now.AddDays(-4));

		var (_, dpmSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-5), new ZGuid("CA135BCD-6C55-481A-8343-9D7B2F1EBE18"));

		Factory.Save();

		CombineAssertions(() =>
		{
			var lastMessage = NctsDeclarationComparatorHelper.GetLatestEffectiveMessage(header);
			AssertNotEquals("LatestEffectiveMessage is not TQU", tquMessage.PK, lastMessage.PK);
			AssertEquals("LatestEffectiveMessage is DPD", dpdSent.PK, lastMessage.PK);
			AssertNotEquals("LatestEffectiveMessage is not DPM", dpmSent.PK, lastMessage.PK);
		});
	}

	public void TestGetLatestEffectiveMessage_DPM()
	{
		var header = Factory.New<NctsHeader>();

		var glbBranch = Factory.New<GlbBranch>();
		glbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
		glbBranch.GB_Code = "XAX";

		var (tquMessage, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.TransitNcts5Query, DateTime.Now.AddDays(-5));

		var (_, dpdSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now.AddDays(-3), new ZGuid("4BF802C5-C68B-4684-928D-2F7BF6B3396E"));

		var (dpmNotAcc, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-1));
		dpmNotAcc.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
		var (_, dpmSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-2), new ZGuid("CA135BCD-6C55-481A-8343-9D7B2F1EBE18"));
		var (_, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-6));

		Factory.Save();

		CombineAssertions(() =>
		{
			var lastMessage = NctsDeclarationComparatorHelper.GetLatestEffectiveMessage(header);
			AssertNotEquals("LatestEffectiveMessage is not TQU", tquMessage.PK, lastMessage.PK);
			AssertNotEquals("LatestEffectiveMessage is not DPD", dpdSent.PK, lastMessage.PK);
			AssertEquals("LatestEffectiveMessage is DPM", dpmSent.PK, lastMessage.PK);
		});
	}

	public void TestGetLatestEffectiveMessage_AnyTypeMessageDoesntExist()
	{
		var header = Factory.New<NctsHeader>();

		var glbBranch = Factory.New<GlbBranch>();
		glbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
		glbBranch.GB_Code = "XAX";

		var (_, dpdSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DateTime.Now.AddDays(-3), new ZGuid("4BF802C5-C68B-4684-928D-2F7BF6B3396E"));

		var (dpmNotAcc, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-1));
		dpmNotAcc.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
		var (_, dpmSent) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-2), new ZGuid("CA135BCD-6C55-481A-8343-9D7B2F1EBE18"));
		var (_, _) = SetUpResponseEDIMessage(header, glbBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DateTime.Now.AddDays(-6));

		Factory.Save();

		CombineAssertions(() =>
		{
			var lastMessage = NctsDeclarationComparatorHelper.GetLatestEffectiveMessage(header);
			AssertNotEquals("LatestEffectiveMessage is not DPD", dpdSent.PK, lastMessage.PK);
			AssertEquals("LatestEffectiveMessage is DPM", dpmSent.PK, lastMessage.PK);
		});
	}

	public void TestGetLatestEffectiveMessage_NoMessages()
	{
		var header = Factory.New<NctsHeader>();

		CombineAssertions(() =>
		{
			var lastMessage = NctsDeclarationComparatorHelper.GetLatestEffectiveMessage(header);
			AssertNull("LatestEffectiveMessage is null", lastMessage);
		});
	}

	(EDIMessage response, EDIMessage sent) SetUpResponseEDIMessage(NctsHeader header, ZGuid branchPK, string messageType, DateTime createdTime, ZGuid? sentGuid = null)
	{
		var message = Factory.New<TestEdiMessage>();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
		message.EM_SystemCreateTimeUtc = createdTime;
		message.EM_GB = branchPK;
		header.Messages.Add(message);

		TestEdiMessage sentMessage = null;

		if (sentGuid != null)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			message.EM_EI = interchange.PK;
			interchange.EI_SessionGUID = sentGuid.Value;

			var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			sentInterchange.EI_SessionGUID = sentGuid.Value;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_EI = sentInterchange.PK;
			sentMessage.EM_GB = branchPK;
		}

		return (message, sentMessage);
	}

	TestEdiMessage CreateNewReceivedEDIMessage(ZString messageText, ZGuid interchangeID, string messageType)
	{
		var message = Factory.New<TestEdiMessage>();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = "AAA";
		message.EM_GB = new ZGuid();
		message.EM_MessageText = messageText;

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = interchangeID;
		responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
		responseInterchange.EI_To = "CW1";
		responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AH</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>10</SentEDIMessageNumber>
</Headers>");
		responseInterchange.ContainedMessages.Add(message);

		Factory.Save();
		return message;
	}

	public void TestGetComparablePredeclaration_TQU()
	{
		CombineAssertions(() =>
		{
			var interchangeID = new ZGuid("A68F4DF2-1C15-4AB9-B901-62A79800982D");
			var responseFile = ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusPA.txt");
			var message = CreateNewReceivedEDIMessage(responseFile, interchangeID, DeclarationMessageTypeList.Codes.TransitNcts5Query);
			var comparable = NctsDeclarationComparatorHelper.GetComparablePredeclaration(message);
			AssertEquals("Correctly mapped DeclarationType", "T1", comparable.DeclarationType);
			AssertEquals("Correctly mapped HouseConsignments", 2, comparable.HouseConsignments.Count);
		});
	}

	const string ComparatorTestFilesPath = "Enterprise.Customs.ES.NCTS.Business.Testing.NCTS.NctsDeclarationComparator.TestFiles";

	public void TestGetComparablePredeclaration_DPD()
	{
		CombineAssertions(() =>
		{
			var interchangeID = new ZGuid("15A362A0-60C7-46B4-B735-31DC91156CD8");
			var responseFile = ESNctsTestFileReader.GetEmbeddedFileText(ComparatorTestFilesPath, "TestDPD.txt");
			var message = CreateNewReceivedEDIMessage(responseFile, interchangeID, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
			var comparable = NctsDeclarationComparatorHelper.GetComparablePredeclaration(message);
			AssertEquals("Correctly mapped DeclarationType", "T1", comparable.DeclarationType);
			AssertEquals("Correctly mapped HouseConsignments", 2, comparable.HouseConsignments.Count);
		});
	}

	public void TestGetComparablePredeclaration_DPM()
	{
		CombineAssertions(() =>
		{
			var interchangeID = new ZGuid("0DF98C56-B419-488C-848F-DA1ED67B1CD6");
			var responseFile = ESNctsTestFileReader.GetEmbeddedFileText(ComparatorTestFilesPath, "TestDPM.txt");
			var message = CreateNewReceivedEDIMessage(responseFile, interchangeID, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);
			var comparable = NctsDeclarationComparatorHelper.GetComparablePredeclaration(message);
			AssertEquals("Correctly mapped DeclarationType", "T1", comparable.DeclarationType);
			AssertEquals("Correctly mapped HouseConsignments", 1, comparable.HouseConsignments.Count);
		});
	}

	public void TestGetCurrentNctsHeaderComparable()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.MovementHeader.BM_TypeOfSecurity = "EXI";
		var bill = header.Bills.AddNew();
		bill.SequenceNumber = 1;
		var goodsItem = bill.GoodsItems.AddNew();
		goodsItem.BY_DeclarationGoodsItemNumber = 1;
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH3";
		var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = MasterFiles.Business.Testing.X509Certificate2TestHelper.ValidCertificate;
		header.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
		cert.CurrentDecryptedCertificatePassphrase = MasterFiles.Business.Testing.X509Certificate2TestHelper.ValidPassword;

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_FullName = "AH3";
		var cusCode = orgHeader.CustomsCodes.AddNew();
		cusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
		cusCode.OK_CustomsRegNo = "1234Z";
		header.Consignor.OrganisationPK = orgHeader.PK;
		var messageSendingObject = new NctsMessageSendingObject(header, staff);
		var comparable = NctsDeclarationComparatorHelper.GetCurrentNctsHeaderComparable(header, messageSendingObject);

		CombineAssertions(() =>
		{
			AssertNotNull("ConsignorID not null", comparable.ConsignorID);
			AssertNotNull("ConsigneeID not null", comparable.ConsigneeID);
			AssertNotNull("DeclarationType not null", comparable.DeclarationType);
			AssertNotNull("TIRCarnetNum not null", comparable.TIRCarnetNum);
			AssertNotNull("ReducedDatasetIndicator not null", comparable.ReducedDatasetIndicator);
			AssertNotNull("DispatchCountry not null", comparable.DispatchCountry);
			AssertNotNull("DestinationCountry not null", comparable.DestinationCountry);
			AssertNotNull("GrossWeight not null", comparable.GrossWeight);
			AssertNotNull("UnloadingPlace not null", comparable.UnloadingPlace);
			AssertNotNull("Circumstance not null", comparable.Circumstance);
			AssertNotNull("UniqueConsignmentReference not null", comparable.UniqueConsignmentReference);
			AssertNotNull("MethodOfPayment not null", comparable.MethodOfPayment);
			AssertNotNull("CarrierID not null", comparable.CarrierID);
			AssertNotNull("CustomsOfficeOfDeparture not null", comparable.CustomsOfficeOfDeparture);
			AssertNotNull("CustomsOfficeOfDestination not null", comparable.CustomsOfficeOfDestination);
			AssertNotNull("CustomsOfficeOfTransit not null", comparable.CustomsOfficeOfTransit);
			AssertNotNull("CustomsOfficeOfExitForTransit not null", comparable.CustomsOfficeOfExitForTransit);
			AssertNotNull("Guarantees not null", comparable.Guarantees);
			AssertNotNull("Authorizations not null", comparable.Authorizations);
			AssertNotNull("SupportingDocuments not null", comparable.SupportingDocuments);
			AssertNotNull("AdditionalDocumentsTD not null", comparable.AdditionalDocumentsTD);
			AssertNotNull("AdditionalDocumentsAR not null", comparable.AdditionalDocumentsAR);
			AssertNotNull("AdditionalDocumentsAI not null", comparable.AdditionalDocumentsAI);
			AssertNotNull("CountriesOfRouting not null", comparable.CountriesOfRouting);
			AssertNotNull("SupplyChainActors not null", comparable.SupplyChainActors);
			AssertNotNull("HouseConsignments not null", comparable.HouseConsignments);
			AssertEquals("HouseConsignments count", 1, comparable.HouseConsignments.Count);
			var comparableHouse = comparable.HouseConsignments[0];
			AssertNotNull("HouseNumber not null", comparableHouse.HouseNumber);
			AssertNotNull("TotalGrossWeight not null", comparableHouse.TotalGrossWeight);
			AssertNotNull("SupportingDocuments not null", comparableHouse.SupportingDocuments);
			AssertNotNull("AdditionalDocumentsTD not null", comparableHouse.AdditionalDocumentsTD);
			AssertNotNull("AdditionalDocumentsAR not null", comparableHouse.AdditionalDocumentsAR);
			AssertNotNull("AdditionalDocumentsAI not null", comparableHouse.AdditionalDocumentsAI);
			AssertNotNull("PreviousDocuments not null", comparableHouse.PreviousDocuments);
			AssertNotNull("SupplyChainActors not null", comparableHouse.SupplyChainActors);
			AssertNotNull("GoodsItems not null", comparableHouse.GoodsItems);
			AssertEquals("GoodsItems count", 1, comparableHouse.GoodsItems.Count);
			var comparableGoods = comparableHouse.GoodsItems[0];
			AssertNotNull("ItemNumber not null", comparableGoods.ItemNumber);
			AssertNotNull("DeclarationItemNumber not null", comparableGoods.DeclarationItemNumber);
			AssertNotNull("DeclarationType not null", comparableGoods.DeclarationType);
			AssertNotNull("OriginCountry not null", comparableGoods.OriginCountry);
			AssertNotNull("DestinationCountry not null", comparableGoods.DestinationCountry);
			AssertNotNull("CommercialReference not null", comparableGoods.CommercialReference);
			AssertNotNull("ConsigneeID not null", comparableGoods.ConsigneeID);
			AssertNotNull("GoodsDescription not null", comparableGoods.GoodsDescription);
			AssertNotNull("CusCode not null", comparableGoods.CusCode);
			AssertNotNull("CommodityCode not null", comparableGoods.CommodityCode);
			AssertNotNull("GrossWeight not null", comparableGoods.GrossWeight);
			AssertNotNull("NetWeight not null", comparableGoods.NetWeight);
			AssertNotNull("SupplementaryQty not null", comparableGoods.SupplementaryQty);
			AssertNotNull("DangerousGoods not null", comparableGoods.DangerousGoods);
			AssertNotNull("SupplyChainActors not null", comparableGoods.SupplyChainActors);
			AssertNotNull("PackagesAndVehicles not null", comparableGoods.PackagesAndVehicles);
			AssertNotNull("PreviousDocuments not null", comparableGoods.PreviousDocuments);
			AssertNotNull("SupportingDocuments not null", comparableGoods.SupportingDocuments);
			AssertNotNull("AdditionalDocumentsTD not null", comparableGoods.AdditionalDocumentsTD);
			AssertNotNull("AdditionalDocumentsAR not null", comparableGoods.AdditionalDocumentsAR);
			AssertNotNull("AdditionalDocumentsAI not null", comparableGoods.AdditionalDocumentsAI);
		});
	}

	public void TestCompare()
	{
		var interchangeID = new ZGuid("0DF98C56-B419-488C-848F-DA1ED67B1CD6");
		var responseFile = ESNctsTestFileReader.GetEmbeddedFileText(ComparatorTestFilesPath, "TestDPM.txt");
		var message = CreateNewReceivedEDIMessage(responseFile, interchangeID, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);
		var comparableLastMessage = NctsDeclarationComparatorHelper.GetComparablePredeclaration(message);

		var header = Factory.New<NctsHeader>();
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		var movement = header.MovementHeader;
		movement.BM_TypeOfSecurity = "EXI";
		movement.BM_InBondEntryType = "T1";
		movement.BM_RL_NKDestinationPort = "ES";
		movement.BM_GrossWeight = 2;
		movement.BM_UniqueConsignmentReference = "REF";

		var officeDEP = movement.CustomsOffices.AddNew();
		officeDEP.CY_Code = "DEP";
		officeDEP.CY_Data = "ES009999";
		var officeDES = movement.CustomsOffices.AddNew();
		officeDES.CY_Code = "DES";
		officeDES.CY_Data = "ES009998";

		var guarantee = movement.Guarantees.AddNew();
		guarantee.PW_BondType = "1";
		guarantee.PW_BondNumber = "14ES0000DA0000013";
		guarantee.PW_BondAmount = 0.63;

		var authorization1 = movement.CusAuthorizationUsages.AddNew();
		authorization1.AGC_Code = "C521";
		authorization1.AGC_Number = "ESACR02023000004";
		var authorization2 = movement.CusAuthorizationUsages.AddNew();
		authorization2.AGC_Code = "C523";
		authorization2.AGC_Number = "ESSSE02023000002";

		var countryOfRouting = header.CountriesOfRouting.AddNew();
		countryOfRouting.CY_Data = "IT";

		var bill = header.Bills.AddNew();
		bill.SequenceNumber = 1;
		bill.B0_Weight = 2;
		var cusSupRef = bill.CusSupplyChainActorReferences.AddNew();
		cusSupRef.CFR_Code = "CS";
		cusSupRef.CFR_Reference = "ESA78587268";

		var goodsItem = bill.GoodsItems.AddNew();
		goodsItem.BY_DeclarationGoodsItemNumber = 1;
		goodsItem.BY_Description = "MANUFACTURAS DE FUNDICIÓN, DE HIERRO O ACERO Tornillos, pernos, tuercas, tirafondos, escarpias roscadas, remaches, pasadores, chavetas, arandelas [incluidas las arandelas de muelle (resorte)] y artículos similares, de fundición, hierro o acero Los demás Destinados a ciertos tipos";
		goodsItem.BY_HarmonisedTariff = "73182900";
		goodsItem.BY_GrossWeight = 1.3;
		goodsItem.BY_NetWeight = 1;

		var package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "BX";
		package.B5_MarksAndNumbers = "rtdas";

		var prevDoc = goodsItem.PreviousDocuments.AddNew();
		prevDoc.CSI_Code = "IRR0";
		prevDoc.CSI_ReferenceNumber = "20240930";

		var suppDoc = goodsItem.SupportingDocuments.AddNew();
		suppDoc.CSI_LineNo = 1;
		suppDoc.CSI_Code = "N380";
		suppDoc.CSI_ReferenceNumber = "FACT";

		var addInfoTD1 = goodsItem.AdditionalInfos.AddNew();
		addInfoTD1.CSI_LineNo = 1;
		addInfoTD1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		addInfoTD1.CSI_Code = "N705";
		addInfoTD1.CSI_ReferenceNumber = "REF";
		var addInfoTD2 = goodsItem.AdditionalInfos.AddNew();
		addInfoTD2.CSI_LineNo = 2;
		addInfoTD2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		addInfoTD2.CSI_Code = "N703";
		addInfoTD2.CSI_ReferenceNumber = "ref2";
		var addInfoTD3 = goodsItem.AdditionalInfos.AddNew();
		addInfoTD3.CSI_LineNo = 3;
		addInfoTD3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		addInfoTD3.CSI_Code = "N730";
		addInfoTD3.CSI_ReferenceNumber = "N730";

		var addInfoAR = goodsItem.AdditionalInfos.AddNew();
		addInfoAR.CSI_LineNo = 1;
		addInfoAR.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		addInfoAR.CSI_Code = "Y015";
		addInfoAR.CSI_ReferenceNumber = "Y015";

		var addInfoAI = goodsItem.AdditionalInfos.AddNew();
		addInfoAI.CSI_LineNo = 1;
		addInfoAI.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		addInfoAI.CSI_Code = "00200";
		addInfoAI.CSI_Description = "DESC";

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH3";
		var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = MasterFiles.Business.Testing.X509Certificate2TestHelper.ValidCertificate;
		movement.BM_GS_NKCusAgent = staff.GS_Code;
		cert.CurrentDecryptedCertificatePassphrase = MasterFiles.Business.Testing.X509Certificate2TestHelper.ValidPassword;

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_FullName = "Consignee";
		var cusCode = orgHeader.CustomsCodes.AddNew();
		cusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
		cusCode.OK_CustomsRegNo = "ESA78587268";
		header.Consignee.OrganisationPK = orgHeader.PK;

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
			{
				var messageSendingObject = new NctsMessageSendingObject(header, staff);
				var comparableHeader = NctsDeclarationComparatorHelper.GetCurrentNctsHeaderComparable(header, messageSendingObject);

				var changesToPreDeclaration = NctsDeclarationComparatorHelper.Compare(comparableHeader, comparableLastMessage);
				AssertEquals("No differences expected", "", changesToPreDeclaration);

				var extraBill = header.Bills.AddNew();
				comparableHeader = NctsDeclarationComparatorHelper.GetCurrentNctsHeaderComparable(header, messageSendingObject);
				changesToPreDeclaration = NctsDeclarationComparatorHelper.Compare(comparableHeader, comparableLastMessage);
				AssertEquals("Different number of House Consignments", "Consignment/House Consignments: The elements of this data group are different", changesToPreDeclaration);

				extraBill.Delete();

				var extraGoods = bill.GoodsItems.AddNew();
				comparableHeader = NctsDeclarationComparatorHelper.GetCurrentNctsHeaderComparable(header, messageSendingObject);
				changesToPreDeclaration = NctsDeclarationComparatorHelper.Compare(comparableHeader, comparableLastMessage);
				AssertEquals("Different number of Goods Items", "House Consignment(1)/Goods Items: The elements of this data group are different", changesToPreDeclaration);
			}
		});
	}
}
