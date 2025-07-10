using System;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.NCTS.Business.NctsHeader;
using CusGuaranteeHeader = Enterprise.Customs.ES.Business.CusGuaranteeHeader;
using GlbExternalPassword = Enterprise.Customs.ES.Business.GlbExternalPassword;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;
using NctsMessageStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMessageStatusList;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;
using NctsUnloadedStateList = Enterprise.Customs.EU.NCTS.Business.NctsUnloadedStateList;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
{
	public void TestClone()
	{
		var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Arrival);
		SetUpForClone(nctsHeader);

		CombineAssertions(() =>
		{
			var clone = (NctsHeader)nctsHeader.TemplateCopy();
			AssertCloneResult_Arrival_Phase5(clone);

			var esHeader = clone.ESNctsHeader;
			AssertEquals("CEN_AutomaticCompletion", true, esHeader.CEN_AutomaticCompletion);
			AssertEquals("CEN_AutomaticTranshipment", true, esHeader.CEN_AutomaticCompletion);
			AssertEquals("CEN_TIRArrival", true, esHeader.CEN_AutomaticCompletion);
			AssertEquals("CEN_TIRPartialUnloading", true, esHeader.CEN_AutomaticCompletion);
			AssertEquals("CEN_SummaryType", "AH", esHeader.CEN_SummaryType);

			var representative = clone.ArrivalMovementHeader.Representative;
			AssertEquals("Representative", "GBR/022/AH34567", representative.Organisation.GetRegoCodeOfThisOrg("TIR"));
		});
	}

	public void TestModifyReleaseStatusInPredeclaration()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Arrival);

			AssertEquals("Prereq: ReleaseStatus empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			nctsHeader.ModifyReleaseStatusInPredeclaration();
			AssertEquals("When not departure", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.ModifyReleaseStatusInPredeclaration();
			AssertEquals("When departure but CustomsStatus is not PRE", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			nctsHeader.ModifyReleaseStatusInPredeclaration();
			AssertEquals("When departure and CustomsStatus is PRE", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestCheckPreDeclarationChanges() => CombineAssertions(() =>
	{
		var expectedError = @"Consignment/Declaration Type
Consignment/Destination Country-Region
Consignment/Gross Weight
Consignment/Unique Consignment Reference";

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH3";
		var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = MasterFiles.Business.Testing.X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = MasterFiles.Business.Testing.X509Certificate2TestHelper.ValidPassword;

		var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
		var movement = nctsHeader.MovementHeader;
		movement.BM_GS_NKCusAgent = staff.GS_Code;
		var sendingObject = new NctsMessageSendingObject(nctsHeader, staff);

		var result = nctsHeader.CheckPreDeclarationChanges(sendingObject);
		AssertEquals("No changes because there is no last message available", ZString.Empty, result);

		var glbBranch = Factory.New<GlbBranch>();
		glbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
		glbBranch.GB_Code = "XAX";

		var sentGuid = new ZGuid("A7436A81-BEBB-4A8F-81AD-80E4212F7C37");
		const string ComparatorTestFilesPath = "Enterprise.Customs.ES.NCTS.Business.Testing.NCTS.NctsDeclarationComparator.TestFiles";
		var messageText = ESNctsTestFileReader.GetEmbeddedFileText(ComparatorTestFilesPath, "TestDPM_Simplified.txt");

		var message = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
		message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
		message.EM_SystemCreateTimeUtc = DateTime.Now;
		message.EM_GB = glbBranch.PK;

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = new ZGuid("464EF95C-010C-42B2-971A-8AC2DEFD4822");
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

		nctsHeader.Messages.Add(message);

		var interchange = Factory.NewWithValidTestData<EDIInterchange>();
		message.EM_EI = interchange.PK;
		interchange.EI_SessionGUID = sentGuid;

		var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
		sentInterchange.EI_SessionGUID = sentGuid;
		sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		var sentMessage = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
		sentMessage.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
		sentMessage.EM_EI = sentInterchange.PK;
		sentMessage.EM_GB = glbBranch.PK;
		sentMessage.EM_MessageText = messageText;

		result = nctsHeader.CheckPreDeclarationChanges(sendingObject);
		AssertEquals("There are changes because now there is last message available", expectedError, result);

		movement.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T1;
		movement.BM_TypeOfSecurity = "EXI";
		movement.BM_RL_NKDestinationPort = "ES";
		movement.BM_GrossWeight = 2;
		movement.BM_UniqueConsignmentReference = "REF";

		result = nctsHeader.CheckPreDeclarationChanges(sendingObject);
		AssertEquals("No changes because current declaration is equals to customs declaration", ZString.Empty, result);
	});

	public void TestOnChangedConsignorDocumentaryAddress()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			nctsHeader.Consignor.E2_OA_Address = orgAddress.PK;

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var orgAddress2 = Factory.New<OrgAddress>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress2.OA_OH = orgHeader2.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			nctsHeader.Consignor.E2_OA_Address = orgAddress2.PK;
			AssertEquals("When CustomsStatus is PRE and Consignor changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestOnChangedConsigneeDocumentaryAddress()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			nctsHeader.Consignee.E2_OA_Address = orgAddress.PK;

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var orgAddress2 = Factory.New<OrgAddress>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress2.OA_OH = orgHeader2.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			nctsHeader.Consignee.E2_OA_Address = orgAddress2.PK;
			AssertEquals("When CustomsStatus is PRE and Consignee changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestOnChangedGuarantees()
	{
		var nctsHeader = GetPhase4Header(NctsMovementType.Codes.Departure);
		CombineAssertions(() =>
		{
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			guarantee.PW_BondNumber = "AH3";
			AssertEquals("When CustomsStatus is PRE and Guarantee changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.GetEffectiveGuarantees(), "Guarantees");
		});
	}

	public void TestOnChangedCusAuthorizationUsages()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			authorizationUsage.AGC_Code = "AH3";
			AssertEquals("When CustomsStatus is PRE and authorizationUsage changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.MovementHeader.CusAuthorizationUsages, "authorizationUsages");
		});
	}

	public void TestOnChangedSupportingDocuments()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			supportingDocument.CSI_Code = "AH3";
			AssertEquals("When CustomsStatus is PRE and SupportingDocuments changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.MovementHeader.SupportingDocuments, "SupportingDocuments");
		});
	}

	public void TestOnChangedAdditionalDocuments()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			additionalDocument.CSI_Code = "AH3";
			AssertEquals("When CustomsStatus is PRE and AdditionalDocuments changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.AdditionalDocuments, "AdditionalDocuments");
		});
	}

	public void TestOnChangedPreviousDocuments()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var previousDocument = nctsHeader.PreviousDocuments.AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			previousDocument.CSI_Code = "AH3";
			AssertEquals("When CustomsStatus is PRE and PreviousDocuments changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.PreviousDocuments, "PreviousDocuments");
		});
	}

	public void TestOnChangedCountriesOfRouting()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var countryOfRouting = nctsHeader.CountriesOfRouting.AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			countryOfRouting.CY_Code = "AH3";
			AssertEquals("When CustomsStatus is PRE and CountriesOfRouting changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.CountriesOfRouting, "CountriesOfRouting");
		});
	}

	public void TestOnChangedCusSupplyChainActors()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
			var cusSupplyChainActor = nctsHeader.MovementHeader.CusSupplyChainActors.AddNew();

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			cusSupplyChainActor.CFR_Code = "AH3";
			AssertEquals("When CustomsStatus is PRE and CusSupplyChainActors changes", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertReleaseStatusOnChangedCollection_Add_Delete(nctsHeader, nctsHeader.MovementHeader.CusSupplyChainActors, "CusSupplyChainActors");
		});
	}

	public void TestGetNewCustomsOfficesForDeparture()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsESOfficeCodeCollectionForDepartureGrid>(nctsHeader.CustomsOfficesForDeparture);
	}

	public void TestPrincipal_ReadOnly()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
		AssertEquals("BM_CustomStatus is not PRE", false, nctsHeader.Principal.ReadOnly);

		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
		AssertEquals("BM_CustomStatus is PRE", true, nctsHeader.Principal.ReadOnly);

		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
		AssertEquals("BM_CustomStatus is not PRE", false, nctsHeader.Principal.ReadOnly);
	}

	public void TestConsignee_ReadOnly()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;

		CombineAssertions(() =>
		{
			AssertEquals("BM_Phase is not TNN, consignee is not ReadOnly", false, nctsHeader.Consignee.ReadOnly);

			nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("BM_Phase is TNN, consignee isReadOnly", true, nctsHeader.Consignee.ReadOnly);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("BM_Phase is TNN, consignee not ReadOnly (NCTS4)", false, nctsHeader.Consignee.ReadOnly);
		});
	}

	public void TestDestinationTraderChangedWithTNNCopyFieldToConsignee()
	{
		var destinationTrader1 = Factory.New<OrgHeader>();
		destinationTrader1.OH_Code = "TRA1";
		var destinationTrader2 = Factory.New<OrgHeader>();
		destinationTrader2.OH_Code = "TRA2";
		var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Arrival);
		var arrivalMovement = nctsHeader.ArrivalMovementHeader;
		nctsHeader.DestinationTrader.OrganisationPK = destinationTrader1.PK;
		nctsHeader.ArrivalMrnFromUser = "ES123456";
		nctsHeader.ESNctsHeader.CEN_TNNArrival = false;

		var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
		tnnDataCodeInfo.AcceptanceDate = ZDateTime.Now;
		tnnDataCodeInfo.ClearanceDate = ZDateTime.Now.AddDays(-1);

		arrivalMovement.GenerateTNNDeparture(tnnDataCodeInfo);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("DestinationTrader in arrivalMovement is destinationTraderPK", destinationTrader1.PK, nctsHeader.DestinationTrader.OrganisationPK);
			AssertEquals("DestinationTrader in arrivalMovement HeaderTNN is destinationTraderPK (GenerateTNNDeparture)", destinationTrader1.PK, arrivalMovement.HeaderTNN.Consignee.OrganisationPK);

			nctsHeader.DestinationTrader.OrganisationPK = destinationTrader2.PK;
			AssertEquals("DestinationTrader in arrivalMovement is destinationTrader2.PK", destinationTrader2.PK, nctsHeader.DestinationTrader.OrganisationPK);
			AssertEquals("DestinationTrader in arrivalMovement HeaderTNN destinationTrader2.PK", destinationTrader2.PK, arrivalMovement.HeaderTNN.Consignee.OrganisationPK);

			nctsHeader.DestinationTrader.OrganisationPK = ZGuid.Empty;
			AssertEquals("DestinationTrader in arrivalMovement is empty (Change with value to empty)", ZGuid.Empty, nctsHeader.DestinationTrader.OrganisationPK);
			AssertEquals("DestinationTrader in arrivalMovement HeaderTNN empty (Change with value to empty)", ZGuid.Empty, arrivalMovement.HeaderTNN.Consignee.OrganisationPK);
		});
	}

	void SetUpForClone(NctsHeader header)
	{
		var esHeader = header.ESNctsHeader;
		esHeader.CEN_AutomaticCompletion = true;
		esHeader.CEN_AutomaticTranshipment = true;
		esHeader.CEN_TIRArrival = true;
		esHeader.CEN_TIRPartialUnloading = true;
		esHeader.CEN_SummaryType = "AH";

		_ = EU.NCTS.Business.Testing.NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", header.ArrivalMovementHeader.Representative, "1", traderTir: "GBR/022/AH34567");
	}

	public void TestCusSupplyChainActors()
	{
		AssertType<EU.Business.Declaration.CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nctsHeader.CusSupplyChainActors);
	}

	public void TestValidation()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		AssertType<NctsHeaderValidation>(nctsHeader.Validation);

		var nctsHeader2 = Factory.New<NctsHeader>();
		nctsHeader2.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsHeaderPhase5Validation>(nctsHeader2.Validation);
	}

	public void TestCommonGoodsItemsIntegratorCore()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		AssertType<NctsCommonGoodsItemsIntegrator>(nctsHeader.CommonGoodsItemsIntegrator);
	}

	public void TestPreviousDocuments()
	{
		AssertType<EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>>(nctsHeader.PreviousDocuments);
	}

	public void TestAdditionalInfos()
	{
		AssertType<EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>>(nctsHeader.AdditionalDocuments);
	}

	public void TestGetCusSupportingInfoTypes_PreviousDocument()
	{
		AssertEquals(typeof(CommonPreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsHeader).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.PreviousDocument]);
	}

	public void TestGetCusSupportingInfoTypes_AdditionalInfo()
	{
		AssertEquals(typeof(NctsAdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsHeader).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestIsDepartureTabReadOnly_Phase4()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = "A";
			AssertEquals("When is Arrival, the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.BH_HeaderType = "D";

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DAC', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
			AssertEquals("When is Departure and BM_CustomsStatus is 'AAA', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DMA', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationDataRequested;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DDR', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DCC', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.ArrivalRejected;
			AssertEquals("When is Departure and BM_CustomsStatus is 'ARR', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DGN', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationExpired;
			AssertEquals("When is Departure and BM_CustomsStatus is 'EXP', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DNR', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = "BBB";
			AssertEquals("When is Departure and BM_CustomsStatus is 'BBB', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DRL', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DRJ', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DCA', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = "CCC";
			AssertEquals("When is Departure and BM_CustomsStatus is 'CCC', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			AssertEquals("When is Departure and EffectiveMessageStatus is 'MDS', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.EffectiveMessageStatus = "AAA";
			AssertEquals("When is Departure and EffectiveMessageStatus is 'AAA', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.CancellationRequestSent;
			AssertEquals("When is Departure and EffectiveMessageStatus is 'MCR', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.CancellationAccepted;
			AssertEquals("When is Departure and EffectiveMessageStatus is 'MCA', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.MessageQueued;
			AssertEquals("When is Departure and EffectiveMessageStatus is 'MQU', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);

			nctsHeader.EffectiveMessageStatus = ZString.Empty;
			AssertEquals("When is Departure and EffectiveMessageStatus is empty, the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
		});
	}

	public void TestIsDepartureTabReadOnly_Phase5()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = "A";
			AssertEquals("When is Arrival, the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.BH_HeaderType = "D";

			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
			AssertEquals("When is Departure and BM_CustomsStatus is 'C01', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DAC', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
			AssertEquals("When is Departure and BM_CustomsStatus is 'GIV', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DCC', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			AssertEquals("When is Departure and BM_CustomsStatus is 'NRL', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
			AssertEquals("When is Departure and BM_CustomsStatus is 'AAA', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			AssertEquals("When is Departure and BM_CustomsStatus is 'REL', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DRJ', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;
			AssertEquals("When is Departure and BM_CustomsStatus is 'CAN', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DMA', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Invalidated;
			AssertEquals("When is Departure and BM_CustomsStatus is 'INV', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = "BBB";
			AssertEquals("When is Departure and BM_CustomsStatus is 'BBB', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DGP', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.RequestForAmendment;
			AssertEquals("When is Departure and BM_CustomsStatus is 'R4A', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			AssertEquals("When is Departure and BM_CustomsStatus is 'MRN', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationDataRequested;
			AssertEquals("When is Departure and BM_CustomsStatus is 'DDR', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;
			nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("When is Departure and BM_MessageStatus is 'SNT', the result should be true", true, nctsHeader.IsDepartureTabReadOnly);
			nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			AssertEquals("When is Departure and BM_MessageStatus is 'MDS', the result should be false", false, nctsHeader.IsDepartureTabReadOnly);

			nctsHeader.MovementHeader.BM_MessageStatus = ZString.Empty;
			AssertEquals("When is Departure and BM_MessageStatus is empty, the result should be false", false, nctsHeader.IsDepartureTabReadOnly);
		});
	}

	public void TestIsArrivalTabReadOnly_Phase4()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = "D";
			AssertEquals("When is Departure, the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.BH_HeaderType = "A";

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'AUP', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = "AAA";
			AssertEquals("When is Arrival and BM_CustomsStatus is 'AAA', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'AWO', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationDataRequested;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DDR', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DCC', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.RequestForAmendment;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'R4A', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'ART', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DCA', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			AssertEquals("When is Arrival and EffectiveMessageStatus is 'MAS', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.CancellationAccepted;
			AssertEquals("When is Arrival and EffectiveMessageStatus is 'MCA', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;
			AssertEquals("When is Arrival and EffectiveMessageStatus is 'MUS', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.MessageQueued;
			AssertEquals("When is Arrival and EffectiveMessageStatus is 'MQU', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksRejected;
			AssertEquals("When is Arrival and EffectiveMessageStatus is 'MUR', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);

			nctsHeader.EffectiveMessageStatus = ZString.Empty;
			AssertEquals("When is Arrival and EffectiveMessageStatus is empty, the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
		});
	}

	public void TestIsArrivalTabReadOnly_Phase5()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = "D";
			AssertEquals("When is Departure, the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.BH_HeaderType = "A";

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'UAP', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = "AAA";
			AssertEquals("When is Arrival and BM_CustomsStatus is 'AAA', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'CL1', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DCC', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'C01', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'ART', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'CD4', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationDataRequested;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DDR', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MAS', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.CancellationAccepted;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MCA', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MUS', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.MessageQueued;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MQU', the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksRejected;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MUR', the result should be true", true, nctsHeader.IsArrivalTabReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = ZString.Empty;
			AssertEquals("When is Arrival and BM_MessageStatus is empty, the result should be false", false, nctsHeader.IsArrivalTabReadOnly);
		});
	}

	public void TestIsArrivalDetailsReadOnly_Phase5()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = "D";
			AssertEquals("When is Departure, the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.BH_HeaderType = "A";

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'UAP', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = "AAA";
			AssertEquals("When is Arrival and BM_CustomsStatus is 'AAA', the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'CL1', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DCC', the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'C01', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'ART', the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'CD4', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationDataRequested;
			AssertEquals("When is Arrival and BM_CustomsStatus is 'DDR', the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MAS', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.CancellationAccepted;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MCA', the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MUS', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.MessageQueued;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MQU', the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksRejected;
			AssertEquals("When is Arrival and BM_MessageStatus is 'MUR', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = ZString.Empty;
			AssertEquals("When is Arrival and BM_MessageStatus is empty, the result should be false", false, nctsHeader.IsArrivalDetailsReadOnly);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("When is Arrival and BM_MessageStatus is 'SNT', the result should be true", true, nctsHeader.IsArrivalDetailsReadOnly);
		});
	}

	public void TestIsUnloadingRemarksTabReadOnly()
	{
		nctsHeader = GetPhase5Header(NctsMovementType.Codes.Arrival);
		CombineAssertions(() =>
		{
			AssertIsUnloadingRemarksTabReadOnly(
				customsStatus: ZString.Empty,
				phaseStatus: ZString.Empty,
				isReadOnly: false);
			AssertIsUnloadingRemarksTabReadOnly(
				customsStatus: NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
				phaseStatus: ZString.Empty,
				isReadOnly: true);
			AssertIsUnloadingRemarksTabReadOnly(
				customsStatus: NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease,
				phaseStatus: ZString.Empty,
				isReadOnly: true);
			AssertIsUnloadingRemarksTabReadOnly(
				customsStatus: ZString.Empty,
				phaseStatus: NCTS5ArrivalPhaseList.Codes.UnloadingRemarks,
				isReadOnly: false);
		});
	}

	void AssertIsUnloadingRemarksTabReadOnly(ZString customsStatus, ZString phaseStatus, bool isReadOnly)
	{
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
		nctsHeader.ArrivalMovementHeader.BM_Phase = phaseStatus;
		AssertEquals($"When BM_CustomsStatus is {customsStatus} and BM_Phase is {phaseStatus.IfEmptyUse(() => (ZString)"(empty)")}", isReadOnly, nctsHeader.IsUnloadingRemarksTabReadOnly);
	}

	public void TestCanCreateExsDeclaration_Phase4()
	{
		AssignHeaderValuesForCreateEXS_Phase4();

		var unloadingMovement = nctsHeader.UnloadingMovementHeader;
		var unloadingGoodsItem = unloadingMovement.GoodsItems.AddNew();
		unloadingGoodsItem.BY_LineNo = 1;
		unloadingGoodsItem.HasDifferences = false;
		unloadingGoodsItem.IsMissing = true;
		unloadingGoodsItem.IsNew = false;

		CombineAssertions(() =>
		{
			nctsHeader.CanCreateExsDeclaration();
			AssertEquals("Can't create EXS declaration with all missing lines", false, nctsHeader.CanCreateExsDeclaration());
			var unloadingGoodsItem1 = unloadingMovement.GoodsItems.AddNew();
			unloadingGoodsItem1.BY_LineNo = 2;
			unloadingGoodsItem1.HasDifferences = false;
			unloadingGoodsItem1.IsMissing = false;
			unloadingGoodsItem1.IsNew = false;
			AssertEquals("Can create EXS declaration with with some non-missing line", true, nctsHeader.CanCreateExsDeclaration());
		});
	}

	public void TestCanCreateExsDeclaration_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();

		var bill = nctsHeader.Bills[0];
		var unloadingGoodsItem = bill.ArrivalGoodsItems.AddNew();
		unloadingGoodsItem.BY_LineNo = 1;
		unloadingGoodsItem.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;

		CombineAssertions(() =>
		{
			AssertEquals("Can't create EXS declaration with all missing lines", false, nctsHeader.CanCreateExsDeclaration());
			var unloadingGoodsItem1 = bill.ArrivalGoodsItems.AddNew();
			unloadingGoodsItem1.BY_LineNo = 2;
			unloadingGoodsItem.UnloadedStatus = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("Can create EXS declaration with with some non-missing line", true, nctsHeader.CanCreateExsDeclaration());
		});
	}

	public void TestCreateEXSFromArrivalWithNewLine_Phase4()
	{
		AssignHeaderValuesForCreateEXS_Phase4();

		var arrivalMovementeHeaderGoodsItemNew = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemNew.BY_GrossWeight = 0;
		arrivalMovementeHeaderGoodsItemNew.BY_NetWeight = 0;
		arrivalMovementeHeaderGoodsItemNew.BY_Description = "Desc";

		var lineNew = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineNew.BY_LineNo = arrivalMovementeHeaderGoodsItemNew.BY_LineNo;
		lineNew.HasDifferences = false;
		lineNew.IsMissing = false;
		lineNew.IsNew = true;
		lineNew.BY_HarmonisedTariff = UnloadingTariffNew;
		lineNew.BY_Description = UnloadingDescriptionNew;
		lineNew.BY_GrossWeight = UnloadingGrossWeightNew;
		lineNew.BY_NetWeight = UnloadingNetWeightNew;

		var containersNew = lineNew.Containers.AddNew();
		containersNew.ContainerNumber = LineContainerContainerNumber;

		var packagesNew = lineNew.Packages.AddNew();
		packagesNew.B5_UnitType = LinePackagesUnitType;
		packagesNew.B5_UnitCount = LinePackagesUnitCount;
		packagesNew.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		Factory.Save();

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration LineTariff from unloading with new", UnloadingTariffNew, factoryDeclaration.InvoiceLines[0].JI_Tariff);
			AssertEquals("Declaration LineDescription from unloading with new", UnloadingDescriptionNew, factoryDeclaration.InvoiceLines[0].JI_Description);
			AssertEquals("Declaration LineGrossWeight from unloading with new", UnloadingGrossWeightNew, factoryDeclaration.InvoiceLines[0].JI_Weight);
			AssertEquals("Declaration LineNetWeight from unloading with new", UnloadingNetWeightNew, factoryDeclaration.InvoiceLines[0].JI_NetWeight);

			AssertEquals("Declaration LineContainerContainerNumber from unloading with new", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration Container Mode from unloading with new", Enterprise.Core.Constants.ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);

			AssertEquals("Declaration LinePackagesUnitType from unloading with new", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
			AssertEquals("Declaration LinePackagesUnitCount from unloading with new", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
			AssertEquals("Declaration LinePackagesMarksAndNumbers from unloading with new", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);
		});
	}

	public void TestCreateEXSFromArrivalWithNew_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.NEW,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.NEW,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.NEW,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.NEW,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.NEW
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with new",
			hasInvoiceLine: true,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.NEW,
			hasPackage: true,
			hasContainer: true
		);
	}

	public void TestCreateEXSFromArrivalWithMissing_Phase4()
	{
		AssignHeaderValuesForCreateEXS_Phase4();

		var arrivalMovementeHeaderGoodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();

		var unloadingMovement = nctsHeader.UnloadingMovementHeader;
		var line1 = unloadingMovement.GoodsItems.AddNew();
		line1.HasDifferences = false;
		line1.IsMissing = true;
		line1.IsNew = false;

		Factory.Save();

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration with missing", true, factoryDeclaration.InvoiceLines.IsNullOrEmpty());
		});
	}

	public void TestCreateEXSFromArrivalWithMissingBill_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.MIS,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.DEC
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with missing bill",
			hasInvoiceLine: false,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			hasPackage: false,
			hasContainer: false
		);
	}

	public void TestCreateEXSFromArrivalWithMissingGoodsItem_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.MIS,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.DEC
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with missing goods item",
			hasInvoiceLine: false,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.MIS,
			hasPackage: false,
			hasContainer: false
		);
	}

	public void TestCreateEXSFromArrivalWithMissingPackage_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.MIS,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.DEC
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with missing package",
			hasInvoiceLine: true,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			hasPackage: false,
			hasContainer: false
		);
	}

	public void TestCreateEXSFromArrivalWithMissingContainer_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.MIS,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.DEC
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with missing container",
			hasInvoiceLine: true,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			hasPackage: true,
			hasContainer: false
		);
	}

	public void TestCreateEXSFromArrivalWithMissingSupportingDocument_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.MIS
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with missing supporting document",
			hasInvoiceLine: true,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			hasPackage: true,
			hasContainer: true
		);
	}

	public void TestCreateEXSFromArrivalWithOthers_Phase4()
	{
		AssignHeaderValuesForCreateEXS_Phase4();

		var arrivalMovementeHeaderGoodsItemOthers = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemOthers.BY_GrossWeight = ArrivalGrossWeightOthers;
		arrivalMovementeHeaderGoodsItemOthers.BY_NetWeight = ArrivalNetWeightOthers;
		arrivalMovementeHeaderGoodsItemOthers.BY_Description = ArrivalDescriptionOthers;
		arrivalMovementeHeaderGoodsItemOthers.BY_HarmonisedTariff = ArrivalTariffOthers;

		var lineOthers = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineOthers.BY_LineNo = arrivalMovementeHeaderGoodsItemOthers.BY_LineNo;
		lineOthers.HasDifferences = false;
		lineOthers.IsMissing = false;
		lineOthers.IsNew = false;

		var containersOthers = arrivalMovementeHeaderGoodsItemOthers.Containers.AddNew();
		containersOthers.ContainerNumber = LineContainerContainerNumber;

		var packagesOthers = arrivalMovementeHeaderGoodsItemOthers.Packages.AddNew();
		packagesOthers.B5_UnitType = LinePackagesUnitType;
		packagesOthers.B5_UnitCount = LinePackagesUnitCount;
		packagesOthers.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		Factory.Save();

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration LineTariff from unloading with others", ArrivalTariffOthers, factoryDeclaration.InvoiceLines[0].JI_Tariff);
			AssertEquals("Declaration LineDescription from unloading with others", ArrivalDescriptionOthers, factoryDeclaration.InvoiceLines[0].JI_Description);
			AssertEquals("Declaration LineGrossWeight from unloading with others", ArrivalGrossWeightOthers, factoryDeclaration.InvoiceLines[0].JI_Weight);
			AssertEquals("Declaration LineNetWeight from unloading with others", ArrivalNetWeightOthers, factoryDeclaration.InvoiceLines[0].JI_NetWeight);

			AssertEquals("Declaration LineContainerContainerNumber from unloading with others", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration Container Mode from unloading with others", Enterprise.Core.Constants.ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);

			AssertEquals("Declaration LinePackagesUnitType from unloading with others", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
			AssertEquals("Declaration LinePackagesUnitCount from unloading with others", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
			AssertEquals("Declaration LinePackagesMarksAndNumbers from unloading with others", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);
		});
	}

	public void TestCreateEXSFromArrivalWithDeclared_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.DEC
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with declared",
			hasInvoiceLine: true,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			hasPackage: true,
			hasContainer: true
		);
	}

	public void TestCreateEXSFromArrivalWithDifferences_Phase4()
	{
		AssignHeaderValuesForCreateEXS_Phase4();

		var arrivalMovementeHeaderGoodsItemDifference = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemDifference.BY_GrossWeight = 1;
		arrivalMovementeHeaderGoodsItemDifference.BY_NetWeight = 1;
		arrivalMovementeHeaderGoodsItemDifference.BY_Description = ArrivalDescriptionOthers;
		arrivalMovementeHeaderGoodsItemDifference.BY_HarmonisedTariff = ArrivalTariffOthers;

		var lineDifference = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineDifference.BY_LineNo = arrivalMovementeHeaderGoodsItemDifference.BY_LineNo;
		lineDifference.HasDifferences = true;
		lineDifference.IsMissing = false;
		lineDifference.IsNew = false;
		lineDifference.BY_HarmonisedTariff = UnloadingTariff;
		lineDifference.BY_Description = UnloadingDescription;
		lineDifference.BY_GrossWeight = UnloadingGrossWeight;
		lineDifference.BY_NetWeight = UnloadingNetWeight;

		var containersDifference = lineDifference.Containers.AddNew();
		containersDifference.ContainerNumber = LineContainerContainerNumber;

		var packagesDifference = lineDifference.Packages.AddNew();
		packagesDifference.B5_UnitType = LinePackagesUnitType;
		packagesDifference.B5_UnitCount = LinePackagesUnitCount;
		packagesDifference.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var declarationPK = nctsHeader.CreateEXSFromArrival();

		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration LineTariff with differences", UnloadingTariff, factoryDeclaration.InvoiceLines[0].JI_Tariff);
			AssertEquals("Declaration LineDescription with differences", UnloadingDescription, factoryDeclaration.InvoiceLines[0].JI_Description);
			AssertEquals("Declaration LineGrossWeight with differences", UnloadingGrossWeight, factoryDeclaration.InvoiceLines[0].JI_Weight);
			AssertEquals("Declaration LineNetWeight with differences", UnloadingNetWeight, factoryDeclaration.InvoiceLines[0].JI_NetWeight);

			AssertEquals("Declaration LineContainerContainerNumber with differences", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration Container Mode with differences", Enterprise.Core.Constants.ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);

			AssertEquals("Declaration LinePackagesUnitType with differences", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
			AssertEquals("Declaration LinePackagesUnitCount with differences", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
			AssertEquals("Declaration LinePackagesMarksAndNumbers with differences", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);
		});
	}

	public void TestCreateEXSFromArrivalWithDifferences_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		SetDataForCreateEXS_Phase5(
			billUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DIF,
			packageUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			containerUnloadedStatus: NctsUnloadedStateList.Codes.DEC,
			supportingDocumentUnloadedStatus: NctsUnloadedStateList.Codes.DEC
		);

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		AssertCreateEXSFromArrival_Phase5(
			factoryDeclaration,
			"from arrival with differences",
			hasInvoiceLine: true,
			goodsItemUnloadedStatus: NctsUnloadedStateList.Codes.DIF,
			hasPackage: true,
			hasContainer: true
		);
	}

	public void TestCreateEXSFromArrivalWithoutArrivalMovementHeaderGoodsItem_Phase4()
	{
		AssignHeaderValuesForCreateEXS_Phase4();

		var lineDifference = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineDifference.HasDifferences = false;
		lineDifference.IsMissing = false;
		lineDifference.IsNew = true;
		lineDifference.BY_HarmonisedTariff = UnloadingTariff;
		lineDifference.BY_Description = UnloadingDescription;
		lineDifference.BY_GrossWeight = UnloadingGrossWeight;
		lineDifference.BY_NetWeight = UnloadingNetWeight;

		var containersDifference = lineDifference.Containers.AddNew();
		containersDifference.ContainerNumber = LineContainerContainerNumber;

		var packagesDifference = lineDifference.Packages.AddNew();
		packagesDifference.B5_UnitType = LinePackagesUnitType;
		packagesDifference.B5_UnitCount = LinePackagesUnitCount;
		packagesDifference.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var declarationPK = nctsHeader.CreateEXSFromArrival();

		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration LineTariff Without Arrival Movement Header GoodsItem", UnloadingTariff, factoryDeclaration.InvoiceLines[0].JI_Tariff);
			AssertEquals("Declaration LineDescription Without Arrival Movement Header GoodsItem", UnloadingDescription, factoryDeclaration.InvoiceLines[0].JI_Description);
			AssertEquals("Declaration LineGrossWeight Without Arrival Movement Header GoodsItem", UnloadingGrossWeight, factoryDeclaration.InvoiceLines[0].JI_Weight);
			AssertEquals("Declaration LineNetWeight Without Arrival Movement Header GoodsItem", UnloadingNetWeight, factoryDeclaration.InvoiceLines[0].JI_NetWeight);

			AssertEquals("Declaration LineContainerContainerNumber Without Arrival Movement Header GoodsItem", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration Container Mode Without Arrival Movement Header GoodsItem", Enterprise.Core.Constants.ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);

			AssertEquals("Declaration LinePackagesUnitType Without Arrival Movement Header GoodsItem", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
			AssertEquals("Declaration LinePackagesUnitCount Without Arrival Movement Header GoodsItem", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
			AssertEquals("Declaration LinePackagesMarksAndNumbers Without Arrival Movement Header GoodsItem", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);
		});
	}

	void AssignHeaderValuesForCreateEXS_Phase4()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_JobReference = JobReference;
		nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.DestinationCustomsOfficeCodeForArrival = DestinationCustomsOfficeCode;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;

		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = SummaryEntryNumber;
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		Factory.Save();
	}

	void AssignHeaderValuesForCreateEXS_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.BH_JobReference = JobReference;
		nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = DestinationCustomsOfficeCode;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.AdditionalIdentifier = LocationOfGoodsCode;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.LocalReferenceNumber = LocalReferenceNumber;
		nctsHeader.Bills.AddNew();

		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = SummaryEntryNumber;
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		Factory.Save();
	}

	void SetDataForCreateEXS_Phase5(string billUnloadedStatus, string goodsItemUnloadedStatus, string packageUnloadedStatus, string containerUnloadedStatus, string supportingDocumentUnloadedStatus)
	{
		var bill = nctsHeader.Bills[0];
		bill.UnloadedStatus = billUnloadedStatus;

		var goodsItem = bill.ArrivalGoodsItems.AddNew();
		goodsItem.UnloadedStatus = goodsItemUnloadedStatus;
		if (goodsItemUnloadedStatus == NctsUnloadedStateList.Codes.NEW)
		{
			goodsItem.BY_HarmonisedTariff = UnloadingTariffNew;
			goodsItem.BY_Description = UnloadingDescriptionNew;
			goodsItem.BY_GrossWeight = UnloadingGrossWeightNew;
			goodsItem.BY_NetWeight = UnloadingNetWeightNew;
			goodsItem.BY_DeclarationGoodsItemNumber = DeclarationGoodsItemNumberNew;
		}
		else
		{
			goodsItem.BY_HarmonisedTariff = ArrivalTariffOthers;
			goodsItem.BY_Description = ArrivalDescriptionOthers;
			goodsItem.BY_GrossWeight = ArrivalGrossWeightOthers;
			goodsItem.BY_NetWeight = ArrivalNetWeightOthers;
			goodsItem.BY_DeclarationGoodsItemNumber = DeclarationGoodsItemNumberOthers;
			if (goodsItemUnloadedStatus == NctsUnloadedStateList.Codes.DIF)
			{
				goodsItem.BY_DeclarationGoodsItemNumber = DeclarationGoodsItemNumberDif;
				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = UnloadingTariff;
				goodsItem.UnloadedGoodsItem.BY_Description = UnloadingDescription;
				goodsItem.UnloadedGoodsItem.BY_GrossWeight = UnloadingGrossWeight;
				goodsItem.UnloadedGoodsItem.BY_NetWeight = UnloadingNetWeight;
			}
		}

		var package = goodsItem.Packages.AddNew();
		package.UnloadedStatus = packageUnloadedStatus;
		package.B5_UnitType = LinePackagesUnitType;
		package.B5_UnitCount = LinePackagesUnitCount;
		package.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var container = nctsHeader.ArrivalHeaderContainers.AddNew();
		container.BC_UnloadedState = containerUnloadedStatus;
		container.BC_ContainerNum = LineContainerContainerNumber;
		container.BC_Mode = ContainerModes.Containerised;
		package.ContainersPivot.AddPivotFor(container);

		var supportingDocument = goodsItem.SupportingDocuments.AddNew();
		supportingDocument.UnloadedStatus = supportingDocumentUnloadedStatus;
		supportingDocument.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocument.CSI_Code = LineSupportingDocumentsType;
		supportingDocument.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		Factory.Save();
	}

	void AssertCreateEXSFromArrival_Phase5(JobDeclaration factoryDeclaration, string assertionMessage, bool hasInvoiceLine, string goodsItemUnloadedStatus, bool hasPackage, bool hasContainer)
	{
		CombineAssertions(() =>
		{
			AssertEquals($"Declaration Lines is not empty {assertionMessage}", hasInvoiceLine, !factoryDeclaration.InvoiceLines.IsNullOrEmpty());
			if (hasInvoiceLine)
			{
				var invoiceLine1 = factoryDeclaration.InvoiceLines[0];
				if (goodsItemUnloadedStatus == NctsUnloadedStateList.Codes.NEW)
				{
					AssertEquals($"Declaration LineTariff {assertionMessage}", UnloadingTariffNew, invoiceLine1.JI_Tariff);
					AssertEquals($"Declaration LineDescription {assertionMessage}", UnloadingDescriptionNew, invoiceLine1.JI_Description);
					AssertEquals($"Declaration LineGrossWeight {assertionMessage}", UnloadingGrossWeightNew, invoiceLine1.JI_Weight);
					AssertEquals($"Declaration LineNetWeight {assertionMessage}", UnloadingNetWeightNew, invoiceLine1.JI_NetWeight);
					AssertEXSPreviousDocument("invoiceLine1", invoiceLine1, 99001);
				}
				else if (goodsItemUnloadedStatus == NctsUnloadedStateList.Codes.DIF)
				{
					AssertEquals($"Declaration LineTariff {assertionMessage}", UnloadingTariff, invoiceLine1.JI_Tariff);
					AssertEquals($"Declaration LineDescription {assertionMessage}", UnloadingDescription, invoiceLine1.JI_Description);
					AssertEquals($"Declaration LineGrossWeight {assertionMessage}", UnloadingGrossWeight, invoiceLine1.JI_Weight);
					AssertEquals($"Declaration LineNetWeight {assertionMessage}", UnloadingNetWeight, invoiceLine1.JI_NetWeight);
					AssertEXSPreviousDocument("invoiceLine1", invoiceLine1, 4);
				}
				else
				{
					AssertEquals($"Declaration LineTariff {assertionMessage}", ArrivalTariffOthers, invoiceLine1.JI_Tariff);
					AssertEquals($"Declaration LineDescription {assertionMessage}", ArrivalDescriptionOthers, invoiceLine1.JI_Description);
					AssertEquals($"Declaration LineGrossWeight {assertionMessage}", ArrivalGrossWeightOthers, invoiceLine1.JI_Weight);
					AssertEquals($"Declaration LineNetWeight {assertionMessage}", ArrivalNetWeightOthers, invoiceLine1.JI_NetWeight);
					AssertEXSPreviousDocument("invoiceLine1", invoiceLine1, 6);
				}
			}

			if (hasPackage)
			{
				AssertEquals($"Declaration LinePackagesUnitType {assertionMessage}", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
				AssertEquals($"Declaration LinePackagesUnitCount {assertionMessage}", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
				AssertEquals($"Declaration LinePackagesMarksAndNumbers {assertionMessage}", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);
			}
			else
			{
				AssertNotEquals($"Declaration LinePackagesUnitType {assertionMessage}", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
				AssertNotEquals($"Declaration LinePackagesUnitCount {assertionMessage}", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
				AssertNotEquals($"Declaration LinePackagesMarksAndNumbers {assertionMessage}", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);
			}

			AssertEquals($"Declaration Containers is not empty {assertionMessage}", hasContainer, !factoryDeclaration.CusContainers.IsNullOrEmpty());
			if (hasContainer)
			{
				AssertEquals($"Declaration LineContainerContainerNumber {assertionMessage}", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
				AssertEquals($"Declaration Container Mode {assertionMessage}", ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);
			}
		});
	}

	void AssertEXSPreviousDocument(ZString invoiceLineName, JobComInvoiceLine invoiceLine, ZInt lineNo)
	{
		AssertEquals("Declaration LinePreviousDocumentsType " + invoiceLineName, NctsPreviousDocumentTypeCodeList.Codes.SumDocument, invoiceLine.PreviousDocuments[0].CSI_Code);
		AssertEquals("Declaration LinePreviousDocumentsSubType " + invoiceLineName, PreviousDocumentClassList.Codes.SummaryDeclaration, invoiceLine.PreviousDocuments[0].CSI_SubType);
		AssertEquals("Declaration CSI_LineNo " + invoiceLineName, lineNo, invoiceLine.PreviousDocuments[0].CSI_LineNo);
		AssertEquals("Declaration LinePreviousDocumentsReferenceNumber " + invoiceLineName, LinePreviousDocumentsReferenceNumber, invoiceLine.PreviousDocuments[0].CSI_ReferenceNumber);
	}

	public void TestCreateEXSFromArrival_Phase4()
	{
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_JobReference = JobReference;
		nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.DestinationCustomsOfficeCodeForArrival = DestinationCustomsOfficeCode;
		nctsHeader.LocalReferenceNumber = LocalReferenceNumber;
		nctsHeader.ArrivalMovementHeader.BM_LocationOfGoodsCode = LocationOfGoodsCode;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;

		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = SummaryEntryNumber;
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		Factory.Save();

		var org = Factory.NewWithValidTestData<OrgHeader>();
		nctsHeader.DestinationTrader.OrganisationPK = org.PK;

		var arrivalMovementeHeaderGoodsItemMissing = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemMissing.BY_GrossWeight = 99;
		arrivalMovementeHeaderGoodsItemMissing.BY_NetWeight = 99;
		arrivalMovementeHeaderGoodsItemMissing.BY_Description = "Missing Desc";
		arrivalMovementeHeaderGoodsItemMissing.BY_HarmonisedTariff = "";

		var lineMissing = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineMissing.BY_LineNo = arrivalMovementeHeaderGoodsItemMissing.BY_LineNo;
		lineMissing.HasDifferences = false;
		lineMissing.IsMissing = true;
		lineMissing.IsNew = false;

		var arrivalMovementeHeaderGoodsItemOthers = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemOthers.BY_GrossWeight = ArrivalGrossWeightOthers;
		arrivalMovementeHeaderGoodsItemOthers.BY_NetWeight = ArrivalNetWeightOthers;
		arrivalMovementeHeaderGoodsItemOthers.BY_Description = ArrivalDescriptionOthers;
		arrivalMovementeHeaderGoodsItemOthers.BY_HarmonisedTariff = ArrivalTariffOthers;

		var lineOthers = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineOthers.BY_LineNo = arrivalMovementeHeaderGoodsItemOthers.BY_LineNo;
		lineOthers.HasDifferences = false;
		lineOthers.IsMissing = false;
		lineOthers.IsNew = false;

		var containersOthers = arrivalMovementeHeaderGoodsItemOthers.Containers.AddNew();
		containersOthers.ContainerNumber = LineContainerContainerNumber;

		var containersOthers2 = arrivalMovementeHeaderGoodsItemOthers.Containers.AddNew();
		containersOthers2.ContainerNumber = LineContainerContainerNumber1;

		var packagesOthers = arrivalMovementeHeaderGoodsItemOthers.Packages.AddNew();
		packagesOthers.B5_UnitType = LinePackagesUnitType;
		packagesOthers.B5_UnitCount = LinePackagesUnitCount;
		packagesOthers.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var supportingDocumentsOthers = arrivalMovementeHeaderGoodsItemOthers.SupportingDocuments.AddNew();
		supportingDocumentsOthers.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsOthers.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsOthers.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		var arrivalMovementeHeaderGoodsItemDifference = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemDifference.BY_GrossWeight = 1;
		arrivalMovementeHeaderGoodsItemDifference.BY_NetWeight = 1;
		arrivalMovementeHeaderGoodsItemDifference.BY_Description = ArrivalDescriptionOthers;
		arrivalMovementeHeaderGoodsItemDifference.BY_HarmonisedTariff = ArrivalTariffOthers;

		var lineDifference = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineDifference.BY_LineNo = arrivalMovementeHeaderGoodsItemDifference.BY_LineNo;
		lineDifference.HasDifferences = true;
		lineDifference.IsMissing = false;
		lineDifference.IsNew = false;
		lineDifference.BY_HarmonisedTariff = UnloadingTariff;
		lineDifference.BY_Description = UnloadingDescription;
		lineDifference.BY_GrossWeight = UnloadingGrossWeight;
		lineDifference.BY_NetWeight = UnloadingNetWeight;

		var containersDifference = lineDifference.Containers.AddNew();
		containersDifference.ContainerNumber = LineContainerContainerNumber;

		var packagesDifference = lineDifference.Packages.AddNew();
		packagesDifference.B5_UnitType = LinePackagesUnitType;
		packagesDifference.B5_UnitCount = LinePackagesUnitCount;
		packagesDifference.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var supportingDocumentsDifference = lineDifference.SupportingDocuments.AddNew();
		supportingDocumentsDifference.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsDifference.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsDifference.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		var arrivalMovementeHeaderGoodsItemNew = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		arrivalMovementeHeaderGoodsItemNew.BY_GrossWeight = 0;
		arrivalMovementeHeaderGoodsItemNew.BY_NetWeight = 0;
		arrivalMovementeHeaderGoodsItemNew.BY_Description = "Desc";

		var lineNew = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
		lineNew.BY_LineNo = arrivalMovementeHeaderGoodsItemNew.BY_LineNo;
		lineNew.HasDifferences = false;
		lineNew.IsMissing = false;
		lineNew.IsNew = true;
		lineNew.BY_GrossWeight = UnloadingGrossWeightNew;
		lineNew.BY_NetWeight = UnloadingNetWeightNew;
		lineNew.BY_Description = UnloadingDescriptionNew;
		lineNew.BY_HarmonisedTariff = UnloadingTariffNew;

		var containersNew = arrivalMovementeHeaderGoodsItemNew.Containers.AddNew();
		containersNew.ContainerNumber = LineContainerContainerNumber;

		var packagesNew = arrivalMovementeHeaderGoodsItemNew.Packages.AddNew();
		packagesNew.B5_UnitType = LinePackagesUnitType;
		packagesNew.B5_UnitCount = LinePackagesUnitCount;
		packagesNew.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var supportingDocumentsNew = arrivalMovementeHeaderGoodsItemNew.SupportingDocuments.AddNew();
		supportingDocumentsNew.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsNew.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsNew.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		Factory.Save();

		var declarationPK = nctsHeader.CreateEXSFromArrival();

		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration DestinationCustomsOfficeCode", DestinationCustomsOfficeCode, factoryDeclaration.JE_CustomsOffice);
			AssertEquals("Declaration LocationOfGoodsCode", LocationOfGoodsCode, factoryDeclaration.JE_LocationOfGoods);
			AssertEquals("Declaration DestinationTrader", org.PK, factoryDeclaration.JE_OH_Supplier);
			AssertEquals("Declaration Entry type", MessageTypeList.Codes.Export, factoryDeclaration.JE_MessageType);
			AssertEquals("Declaration Entry Style", EntryStyleListExport.Codes.ExportNormal, factoryDeclaration.JE_EntryStyle);
			AssertEquals("Declaration ApplicationCode", Customs.Business.DeclarationApplicationCodeList.Codes.Builtin, factoryDeclaration.JE_ApplicationCode);
			AssertEquals("Declaration LocalReferenceNumber", LocalReferenceNumber, factoryDeclaration.JE_HouseBill);

			AssertEquals("Declaration Entry Instructions Sub Stype", ExsEntrySubStyleList.Codes.EXS, factoryDeclaration.CustomsEntryInstructions[0].CEI_SubStyle);

			AssertEquals("Declaration LocalReferenceNumber Bill type", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, factoryDeclaration.Packages[0].Bill.CU_BillType);
			AssertEquals("Declaration LocalReferenceNumber Bill number", LocalReferenceNumber, factoryDeclaration.Packages[0].Bill.CU_BillNum);

			AssertEquals("Declaration LocalReferenceNumber Invoice Header", LocalReferenceNumber, factoryDeclaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("Declaration LocalReferenceNumber Invoice Date", ZDateTime.Today, factoryDeclaration.Invoices[0].JZ_InvoiceDate);

			var invoiceLine1 = factoryDeclaration.InvoiceLines[0];
			var invoiceLine2 = factoryDeclaration.InvoiceLines[1];
			var invoiceLine3 = factoryDeclaration.InvoiceLines[2];

			AssertEquals("Declaration LineTariff from arrival with others", ArrivalTariffOthers, invoiceLine1.JI_Tariff);
			AssertEquals("Declaration LineDescription from arrival with others", ArrivalDescriptionOthers, invoiceLine1.JI_Description);
			AssertEquals("Declaration LineGrossWeight from arrival with others", ArrivalGrossWeightOthers, invoiceLine1.JI_Weight);
			AssertEquals("Declaration LineNetWeight from arrival with others", ArrivalNetWeightOthers, invoiceLine1.JI_NetWeight);
			AssertEquals("Declaration LineInvoiceLine package link with others, line 1", true, invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals("Declaration LineInvoiceLine package link with others line 2", false, invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			AssertEquals("Declaration LineTariff from unloading with differences", UnloadingTariff, invoiceLine2.JI_Tariff);
			AssertEquals("Declaration LineDescription from unloading with differences", UnloadingDescription, invoiceLine2.JI_Description);
			AssertEquals("Declaration LineGrossWeight from unloading with differences", UnloadingGrossWeight, invoiceLine2.JI_Weight);
			AssertEquals("Declaration LineNetWeight from unloading with differences", UnloadingNetWeight, invoiceLine2.JI_NetWeight);
			AssertEquals("Declaration LineInvoiceLine package link with differences, line 1", false, invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals("Declaration LineInvoiceLine package link with differences, line 2", true, invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			AssertEquals("Declaration LineTariff from unloading with new", UnloadingTariffNew, invoiceLine3.JI_Tariff);
			AssertEquals("Declaration LineDescription from unloading with new", UnloadingDescriptionNew, invoiceLine3.JI_Description);
			AssertEquals("Declaration LineGrossWeight from unloading with new", UnloadingGrossWeightNew, invoiceLine3.JI_Weight);
			AssertEquals("Declaration LineNetWeight from unloading with new", UnloadingNetWeightNew, invoiceLine3.JI_NetWeight);
			AssertEquals("Declaration LineInvoiceLine package link with new, line 1", false, invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals("Declaration LineInvoiceLine package link with new, line 2", false, invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			AssertEquals("Declaration LineContainerContainerNumber", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration Container Mode", Enterprise.Core.Constants.ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);

			AssertEquals("Declaration LinePackagesUnitType", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
			AssertEquals("Declaration LinePackagesUnitCount", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
			AssertEquals("Declaration LinePackagesMarksAndNumbers", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);

			AssertEquals("Declaration container for invoiceline 1 link", false, invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("Declaration container for invoiceline 2 link", false, invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("Declaration container for invoiceline 3 link", false, invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			AssertEquals("Declaration LineContainerContainerNumber, two containers", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration LineContainerContainerNumber1, two containers", LineContainerContainerNumber1, factoryDeclaration.CusContainers[1].CO_ContainerNumber);
			AssertEquals("Declaration Container 1 link, invoiceline 1", false, invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("Declaration Container 2 link, invoiceline 1", true, invoiceLine1.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine);

			AssertEquals("Declaration LineSupportingDocumentsType invoiceLine1", LineSupportingDocumentsType, invoiceLine1.SupportingDocuments[0].CSI_Code);
			AssertEquals("Declaration LineSupportingDocumentsReferenceNumber invoiceLine1", LineSupportingDocumentsReferenceNumber, invoiceLine1.SupportingDocuments[0].CSI_ReferenceNumber);

			AssertEXSPreviousDocument("invoiceLine1", invoiceLine1, 1);
			AssertEXSPreviousDocument("invoiceLine2", invoiceLine2, 2);
			AssertEXSPreviousDocument("invoiceLine3", invoiceLine3, 99001);
		});
	}

	public void TestCreateEXSFromArrival_Phase5()
	{
		AssignHeaderValuesForCreateEXS_Phase5();

		var org = Factory.NewWithValidTestData<OrgHeader>();
		nctsHeader.DestinationTrader.OrganisationPK = org.PK;

		var bill = nctsHeader.Bills[0];

		var container1 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container1.BC_ContainerNum = LineContainerContainerNumber;
		container1.BC_Mode = ContainerModes.Containerised;
		var container2 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container2.BC_ContainerNum = LineContainerContainerNumber1;
		container2.BC_Mode = ContainerModes.Containerised;
		var container3 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container3.BC_ContainerNum = "NCT01234567";
		container3.BC_Mode = ContainerModes.NonContainerised;

		var goodsItemMissing = bill.ArrivalGoodsItems.AddNew();
		goodsItemMissing.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		goodsItemMissing.BY_GrossWeight = 99;
		goodsItemMissing.BY_NetWeight = 99;
		goodsItemMissing.BY_Description = "Missing Desc";
		goodsItemMissing.BY_HarmonisedTariff = "";
		goodsItemMissing.BY_DeclarationGoodsItemNumber = 99;

		var packagesMissing = goodsItemMissing.Packages.AddNew();
		packagesMissing.B5_UnitType = LinePackagesUnitType;
		packagesMissing.B5_UnitCount = LinePackagesUnitCount;
		packagesMissing.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;
		packagesMissing.ContainersPivot.AddPivotFor(container1);
		packagesMissing.ContainersPivot.AddPivotFor(container3);

		var goodsItemDeclared = bill.ArrivalGoodsItems.AddNew();
		goodsItemDeclared.UnloadedStatus = NctsUnloadedStateList.Codes.DEC;
		goodsItemDeclared.BY_GrossWeight = ArrivalGrossWeightOthers;
		goodsItemDeclared.BY_NetWeight = ArrivalNetWeightOthers;
		goodsItemDeclared.BY_Description = ArrivalDescriptionOthers;
		goodsItemDeclared.BY_HarmonisedTariff = ArrivalTariffOthers;
		goodsItemDeclared.BY_DeclarationGoodsItemNumber = DeclarationGoodsItemNumberOthers;

		var supportingDocumentHeader = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
		supportingDocumentHeader.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentHeader.CSI_Code = HeaderSupportingDocumentsType;
		supportingDocumentHeader.CSI_ReferenceNumber = HeaderSupportingDocumentsReferenceNumber;

		var supportingDocumentsBill = bill.SupportingDocuments.AddNew();
		supportingDocumentsBill.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsBill.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsBill.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		var packagesDeclared = goodsItemDeclared.Packages.AddNew();
		packagesDeclared.B5_UnitType = LinePackagesUnitType;
		packagesDeclared.B5_UnitCount = LinePackagesUnitCount;
		packagesDeclared.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;
		packagesDeclared.ContainersPivot.AddPivotFor(container1);
		packagesDeclared.ContainersPivot.AddPivotFor(container3);

		var supportingDocumentsDeclared = goodsItemDeclared.SupportingDocuments.AddNew();
		supportingDocumentsDeclared.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsDeclared.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsDeclared.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		var goodsItemDifference = bill.ArrivalGoodsItems.AddNew();
		goodsItemDifference.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
		goodsItemDifference.BY_GrossWeight = 1;
		goodsItemDifference.BY_NetWeight = 1;
		goodsItemDifference.BY_Description = ArrivalDescriptionOthers;
		goodsItemDifference.BY_HarmonisedTariff = ArrivalTariffOthers;
		goodsItemDifference.BY_DeclarationGoodsItemNumber = DeclarationGoodsItemNumberDif;
		goodsItemDifference.UnloadedGoodsItem.BY_HarmonisedTariff = UnloadingTariff;
		goodsItemDifference.UnloadedGoodsItem.BY_Description = UnloadingDescription;
		goodsItemDifference.UnloadedGoodsItem.BY_GrossWeight = UnloadingGrossWeight;
		goodsItemDifference.UnloadedGoodsItem.BY_NetWeight = UnloadingNetWeight;

		var packagesDifference = goodsItemDifference.Packages.AddNew();
		packagesDifference.B5_UnitType = LinePackagesUnitType;
		packagesDifference.B5_UnitCount = LinePackagesUnitCount;
		packagesDifference.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;

		var supportingDocumentsDifference = goodsItemDifference.SupportingDocuments.AddNew();
		supportingDocumentsDifference.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsDifference.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsDifference.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		var goodsItemNew = bill.ArrivalGoodsItems.AddNew();
		goodsItemNew.UnloadedStatus = NctsUnloadedStateList.Codes.NEW;
		goodsItemNew.BY_GrossWeight = UnloadingGrossWeightNew;
		goodsItemNew.BY_NetWeight = UnloadingNetWeightNew;
		goodsItemNew.BY_Description = UnloadingDescriptionNew;
		goodsItemNew.BY_HarmonisedTariff = UnloadingTariffNew;
		goodsItemNew.BY_DeclarationGoodsItemNumber = DeclarationGoodsItemNumberNew;

		var packagesNew = goodsItemNew.Packages.AddNew();
		packagesNew.B5_UnitType = LinePackagesUnitType;
		packagesNew.B5_UnitCount = LinePackagesUnitCount;
		packagesNew.B5_MarksAndNumbers = LinePackagesMarksAndNumbers;
		packagesNew.ContainersPivot.AddPivotFor(container2);

		var supportingDocumentsNew = goodsItemNew.SupportingDocuments.AddNew();
		supportingDocumentsNew.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsNew.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsNew.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		Factory.Save();

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("Declaration DestinationCustomsOfficeCode", DestinationCustomsOfficeCode, factoryDeclaration.JE_CustomsOffice);
			AssertEquals("Declaration LocationOfGoodsCode", LocationOfGoodsCode, factoryDeclaration.JE_LocationOfGoods);
			AssertEquals("Declaration DestinationTrader", org.PK, factoryDeclaration.JE_OH_Supplier);
			AssertEquals("Declaration Entry type", MessageTypeList.Codes.Export, factoryDeclaration.JE_MessageType);
			AssertEquals("Declaration Entry Style", EntryStyleListExport.Codes.ExportNormal, factoryDeclaration.JE_EntryStyle);
			AssertEquals("Declaration ApplicationCode", Customs.Business.DeclarationApplicationCodeList.Codes.Builtin, factoryDeclaration.JE_ApplicationCode);
			AssertEquals("Declaration LocalReferenceNumber", LocalReferenceNumber, factoryDeclaration.JE_HouseBill);

			AssertEquals("Declaration Entry Instructions Sub Style", ExsEntrySubStyleList.Codes.EXS, factoryDeclaration.CustomsEntryInstructions[0].CEI_SubStyle);

			AssertEquals("Declaration LocalReferenceNumber Bill type", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, factoryDeclaration.Packages[0].Bill.CU_BillType);
			AssertEquals("Declaration LocalReferenceNumber Bill number", LocalReferenceNumber, factoryDeclaration.Packages[0].Bill.CU_BillNum);

			AssertEquals("Declaration LocalReferenceNumber Invoice Header", LocalReferenceNumber.ToUpper(), factoryDeclaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("Declaration LocalReferenceNumber Invoice Date", ZDateTime.Today, factoryDeclaration.Invoices[0].JZ_InvoiceDate);

			AssertNull("Declartion SupportingDocument", factoryDeclaration.SupportingDocuments[0]);

			var invoiceLine1 = factoryDeclaration.InvoiceLines[0];
			var invoiceLine2 = factoryDeclaration.InvoiceLines[1];
			var invoiceLine3 = factoryDeclaration.InvoiceLines[2];

			AssertEquals("Declaration LineTariff from arrival with declared", ArrivalTariffOthers, invoiceLine1.JI_Tariff);
			AssertEquals("Declaration LineDescription from arrival with declared", ArrivalDescriptionOthers, invoiceLine1.JI_Description);
			AssertEquals("Declaration LineGrossWeight from arrival with declared", ArrivalGrossWeightOthers, invoiceLine1.JI_Weight);
			AssertEquals("Declaration LineNetWeight from arrival with declared", ArrivalNetWeightOthers, invoiceLine1.JI_NetWeight);
			AssertEquals("Declaration LineInvoiceLine package link with declared, line 1", true, invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals("Declaration LineInvoiceLine package link with declared line 2", false, invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			AssertEquals("Declaration LineTariff from arrival with differences", UnloadingTariff, invoiceLine2.JI_Tariff);
			AssertEquals("Declaration LineDescription from arrival with differences", UnloadingDescription, invoiceLine2.JI_Description);
			AssertEquals("Declaration LineGrossWeight from arrival with differences", UnloadingGrossWeight, invoiceLine2.JI_Weight);
			AssertEquals("Declaration LineNetWeight from arrival with differences", UnloadingNetWeight, invoiceLine2.JI_NetWeight);
			AssertEquals("Declaration LineInvoiceLine package link with differences, line 1", false, invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals("Declaration LineInvoiceLine package link with differences, line 2", true, invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			AssertEquals("Declaration LineTariff from arrival with new", UnloadingTariffNew, invoiceLine3.JI_Tariff);
			AssertEquals("Declaration LineDescription from arrival with new", UnloadingDescriptionNew, invoiceLine3.JI_Description);
			AssertEquals("Declaration LineGrossWeight from arrival with new", UnloadingGrossWeightNew, invoiceLine3.JI_Weight);
			AssertEquals("Declaration LineNetWeight from arrival with new", UnloadingNetWeightNew, invoiceLine3.JI_NetWeight);
			AssertEquals("Declaration LineInvoiceLine package link with new, line 1", false, invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals("Declaration LineInvoiceLine package link with new, line 2", false, invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			AssertEquals("Declaration CusContainers from containerised containers only", 2, factoryDeclaration.CusContainers.Count);
			AssertEquals("Declaration LineContainerContainerNumber", LineContainerContainerNumber, factoryDeclaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Declaration LineContainerContainerNumber", LineContainerContainerNumber, invoiceLine1.ContainersPivot[0].ContainerNumber);
			AssertEquals("Declaration LineContainerContainerNumber1", LineContainerContainerNumber1, factoryDeclaration.CusContainers[1].CO_ContainerNumber);
			AssertEquals("Declaration Container Mode", ContainerModes.Containerised, factoryDeclaration.JE_ContainerMode);

			AssertEquals("Declaration LinePackagesUnitType", LinePackagesUnitType, factoryDeclaration.Packages[0].CW_PackType);
			AssertEquals("Declaration LinePackagesUnitCount", LinePackagesUnitCount, factoryDeclaration.Packages[0].CW_PackQty);
			AssertEquals("Declaration LinePackagesMarksAndNumbers", LinePackagesMarksAndNumbers, factoryDeclaration.Packages[0].CW_MarksAndNos);

			AssertNull("Declartion LineSupportingDocument", invoiceLine1.SupportingDocuments[0]);

			AssertEXSPreviousDocument("invoiceLine1", invoiceLine1, 6);
			AssertEXSPreviousDocument("invoiceLine2", invoiceLine2, 4);
			AssertEXSPreviousDocument("invoiceLine3", invoiceLine3, 99001);
		});
	}

	public void TestCreateEXSFromArrival_Phase5_InvoicedNumber()
	{
		AssignHeaderValuesForCreateEXS_Phase5();

		nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = "NCTSHeaderNumber";
		var org = Factory.NewWithValidTestData<OrgHeader>();
		nctsHeader.DestinationTrader.OrganisationPK = org.PK;

		var bill = nctsHeader.Bills[0];
		var goodsItemDeclared = bill.ArrivalGoodsItems.AddNew();

		var supportingDocumentHeader = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
		supportingDocumentHeader.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentHeader.CSI_Code = HeaderSupportingDocumentsType;
		supportingDocumentHeader.CSI_ReferenceNumber = HeaderSupportingDocumentsReferenceNumber;
		var supportingDocumentsDeclared = goodsItemDeclared.SupportingDocuments.AddNew();
		supportingDocumentsDeclared.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsDeclared.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsDeclared.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;
		var supportingDocumentsBill = bill.SupportingDocuments.AddNew();
		supportingDocumentsBill.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
		supportingDocumentsBill.CSI_Code = LineSupportingDocumentsType;
		supportingDocumentsBill.CSI_ReferenceNumber = LineSupportingDocumentsReferenceNumber;

		Factory.Save();

		var declarationPK = nctsHeader.CreateEXSFromArrival();
		var factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);

		CombineAssertions(() =>
		{
			AssertEquals("BM_PaperlessInbondNum InvoiceNumber", nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum.ToUpper(), factoryDeclaration.Invoices[0].JZ_InvoiceNumber);

			AddSupportingDocument(goodsItemDeclared.SupportingDocuments.AddNew(), LineSupportingDocumentsReferenceNumberN380);
			AssertLocalReferenceNumber("LineSupportingDocument", LineSupportingDocumentsReferenceNumberN380.ToUpper());

			AddSupportingDocument(bill.SupportingDocuments.AddNew(), InvoiceSupportingDocumentsReferenceNumberN380);
			AssertLocalReferenceNumber("BillSupportingDocument", InvoiceSupportingDocumentsReferenceNumberN380.ToUpper());

			AddSupportingDocument(nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew(), HeaderSupportingDocumentsReferenceNumberN380);
			AssertLocalReferenceNumber("HeaderSupportingDocument", HeaderSupportingDocumentsReferenceNumberN380.ToUpper());

			goodsItemDeclared.SupportingDocuments[1].Delete();
			bill.SupportingDocuments[1].Delete();
			AssertLocalReferenceNumber("HeaderSupportingDocument", HeaderSupportingDocumentsReferenceNumberN380.ToUpper());

			AddSupportingDocument(goodsItemDeclared.SupportingDocuments.AddNew(), LineSupportingDocumentsReferenceNumberN380);
			AssertLocalReferenceNumber("HeaderSupportingDocument", HeaderSupportingDocumentsReferenceNumberN380.ToUpper());

			goodsItemDeclared.SupportingDocuments[1].Delete();
			AddSupportingDocument(bill.SupportingDocuments.AddNew(), InvoiceSupportingDocumentsReferenceNumberN380);
			AssertLocalReferenceNumber("HeaderSupportingDocument", HeaderSupportingDocumentsReferenceNumberN380.ToUpper());

			nctsHeader.ArrivalMovementHeader.SupportingDocuments[1].Delete();
			AssertLocalReferenceNumber("BillSupportingDocument", InvoiceSupportingDocumentsReferenceNumberN380.ToUpper());

			void AssertLocalReferenceNumber (string description, string expectedValue)
			{
				Factory.Save();
				declarationPK = nctsHeader.CreateEXSFromArrival();
				factoryDeclaration = Factory.Load<JobDeclaration>(declarationPK);
				AssertEquals($"LocalReferenceNumber - {description}", expectedValue, factoryDeclaration.Invoices[0].JZ_InvoiceNumber);
			}

			void AddSupportingDocument(NctsSupportingDocument document, string referenceNumber)
			{
				document.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
				document.CSI_Code = SupportingDocumentsTypeN380;
				document.CSI_ReferenceNumber = referenceNumber;
			}
		});
	}

	public void TestDocumentSupporter()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
		AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
	}

	public void TestHeaderContainerCollectionType()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<EU.NCTS.Business.NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>>(header.DepartureHeaderContainers);
	}

	public void TestCustomsOffices()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsESOfficeCodeCollection>(header.CustomsOffices);
	}

	public void TestGetCusCodeDataType()
	{
		var header = Factory.New<NctsHeader>();
		var iCusCodeDataTypeSupporter = header as Integration.Customs.ICusCodeDataTypeSupporter;
		var dataTypes = iCusCodeDataTypeSupporter.GetCusCodeDataTypes();
		AssertEquals(3, dataTypes.Count);
		AssertEquals(typeof(Seal), dataTypes["SEL"]);
		AssertEquals(typeof(NctsESOfficeCode), dataTypes["EUO"]);
		AssertEquals(typeof(EU.NCTS.Business.CountryOfRouting), dataTypes["COR"]);
	}

	public void TestPopulateGuaranteeFromPrincipal()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var address = Factory.NewWithValidTestData<OrgAddress>();
		address.OA_Address1 = "ES address";
		address.OA_OH = org.PK;

		var guaranteeHeaderTRA = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeaderTRA.CPH_Number = "255";
		guaranteeHeaderTRA.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
		guaranteeHeaderTRA.CPH_SubType = "1";
		guaranteeHeaderTRA.CPH_OH_PermitHolder = org.PK;
		guaranteeHeaderTRA.MainAccessCode = ZString.Empty;

		var guaranteeHeaderIMP = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeaderIMP.CPH_Number = "198";
		guaranteeHeaderIMP.CPH_Type = "IMP";
		guaranteeHeaderIMP.CPH_OH_PermitHolder = org.PK;
		guaranteeHeaderIMP.MainAccessCode = ZString.Empty;

		CombineAssertions(() =>
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals("Prereq: no guarantees in the departure movement", 0, header.GetEffectiveGuarantees().Count);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("Added the unique TRA guarantee", 1, header.GetEffectiveGuarantees().Count);
			var guarantee = header.GetEffectiveGuarantees()[0];
			AssertEquals("255", guarantee.PW_BondNumber);
			AssertEquals("1", guarantee.PW_BondType);

			header.Principal.E2_OA_Address = ZGuid.Empty;
			guaranteeHeaderTRA = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeaderTRA.CPH_Number = "220";
			guaranteeHeaderTRA.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeaderTRA.CPH_SubType = "1";
			guaranteeHeaderTRA.CPH_OH_PermitHolder = org.PK;
			guaranteeHeaderTRA.MainAccessCode = ZString.Empty;

			header.GetEffectiveGuarantees().RemoveAndDeleteAll();
			AssertEquals("Prereq: no guarantees in the departure movement", 0, header.GetEffectiveGuarantees().Count);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("Not defaulted when multiple TRA guarantees", 0, header.GetEffectiveGuarantees().Count);
		});
	}

	public void TestIsSafetyAndSecurityUncheckedWithExistingSecurityData()
	{
		nctsHeader.BH_FTZMove = true;
		AssertEquals("When safety and security is checked and not security fields define", false, nctsHeader.IsSafetyAndSecurityUncheckedWithExistingSecurityData);
		nctsHeader.BH_FTZMove = false;
		AssertEquals("When safety and security is not checked and not security fields define", false, nctsHeader.IsSafetyAndSecurityUncheckedWithExistingSecurityData);
		nctsHeader.AddSecurityData();
		nctsHeader.BH_FTZMove = true;
		AssertEquals("When safety and security is checked and security fields define", false, nctsHeader.IsSafetyAndSecurityUncheckedWithExistingSecurityData);
		nctsHeader.BH_FTZMove = false;
		AssertEquals("When safety and security not is checked and security fields define", true, nctsHeader.IsSafetyAndSecurityUncheckedWithExistingSecurityData);
	}

	public void TestMaxLength()
	{
		CombineAssertions(() =>
		{
			AssertEquals(2, nctsHeader.ClearanceCriteriaInfo.MaxLength);
			AssertEquals(1, nctsHeader.TADPrintProcedureInfo.MaxLength);
			AssertEquals(1, nctsHeader.TNNDocumentTypeInfo.MaxLength);
		});
	}

	public void TestDescriptions()
	{
		CombineAssertions(() =>
		{
			nctsHeader.ClearanceCriteria = "A2";
			nctsHeader.TADPrintProcedure = "2";
			AssertEquals("[A2] Normal Procedure", nctsHeader.ClearanceCriteriaDescription);
			AssertEquals("[2] Operator: print copies \"A\" and \"B\" of TAD", nctsHeader.TADPrintProcedureDescription);

			nctsHeader.ClearanceCriteria = "B2";
			nctsHeader.TADPrintProcedure = "9";
			AssertEquals(ZString.Empty, nctsHeader.ClearanceCriteriaDescription);
			AssertEquals(ZString.Empty, nctsHeader.TADPrintProcedureDescription);
		});
	}

	public void TestTNNDocumentType()
	{
		CombineAssertions(() =>
		{
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(nctsHeader.TNNDocumentTypeInfo, ((ISupportMultipleResourceStringData)nctsHeader).MultipleKeysToUse);
			AssertEquals("Caption", "TNN Document", DataBoundResourceStrings.GetDataForProperty(nctsHeader.TNNDocumentTypeInfo).Caption);
			AssertEquals("MediumCaption", "TNN Doc.", DataBoundResourceStrings.GetDataForProperty(nctsHeader.TNNDocumentTypeInfo).MediumCaption);
			AssertEquals("ShortCaption", "TNN D.", DataBoundResourceStrings.GetDataForProperty(nctsHeader.TNNDocumentTypeInfo).ShortCaption);
			AssertEquals("FullDescription", "TNN Document Type", DataBoundResourceStrings.GetDataForProperty(nctsHeader.TNNDocumentTypeInfo).FullDescription);
		});
	}

	public void TestUnloadingRemarksAllowedOverride()
	{
		CombineAssertions(() =>
		{
			nctsHeader.CombinedMessage = true;
			AssertEquals(true, nctsHeader.UnloadingRemarksAllowedOverride);
			nctsHeader.CombinedMessage = false;
			AssertEquals(false, nctsHeader.UnloadingRemarksAllowedOverride);
		});
	}

	public void TestIsUnloadingAllowedOrCompleteOverride()
	{
		CombineAssertions(() =>
		{
			AssertEquals("IsUnloadingAllowedOrComplete is false cause there is no incoming message", false, nctsHeader.IsUnloadingAllowedOrComplete);

			var outgoingMessage = nctsHeader.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = DeclarationMessageTypeList.Codes.NctsUnloadingRemarks;
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			var incomingMessage = nctsHeader.Messages.AddNew();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = DeclarationMessageTypeList.Codes.NctsUnloadingRemarks;
			incomingMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
			incomingMessage.EM_Status = EDIMessage.Status.Received;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			AssertEquals("IsUnloadingAllowedOrComplete is true cause the last incoming message is an OBS", true, nctsHeader.IsUnloadingAllowedOrComplete);

			incomingMessage.EM_MessageType = DeclarationMessageTypeList.Codes.NctsArrivalNotification;
			AssertEquals("IsUnloadingAllowedOrComplete is false cause the last incoming message is no OBS", false, nctsHeader.IsUnloadingAllowedOrComplete);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			AssertEquals("IsUnloadingAllowedOrComplete is true cause Status is AUP", true, nctsHeader.IsUnloadingAllowedOrComplete);
		});
	}

	public void TestMovementHeader()
	{
		AssertType<NctsDepartureMovementHeader>(nctsHeader.MovementHeader);
	}

	public void TestArrivalMovementHeader()
	{
		AssertType<NctsArrivalMovementHeader>(nctsHeader.ArrivalMovementHeader);
	}

	public void TestUnloadingMovementHeader()
	{
		AssertType<NctsUnloadingMovementHeader>(nctsHeader.UnloadingMovementHeader);
	}

	public void TestBillCollectionType()
	{
		AssertType<NctsBillCollection>(nctsHeader.Bills);
	}

	public void TestDefaultValues()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "C#@";
		company.GC_Name = "COMP TEST";
		company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
		var branch = company.Branches.AddNew();
		branch.GB_Code = "B#@";
		branch.GB_BranchName = "BRANCH TEST";
		branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
		nctsHeader.BH_GB = branch.PK;

		AssertEquals("DeclarantAddressPK", branch.OrgProxy.MainAddress.PK, nctsHeader.DeclarantAddressPK);
		AssertEquals("DeclarantOrgPK", branch.GB_OH_OrgProxy, nctsHeader.DeclarantOrgPK);
	}

	public void TestDeclEmailAddrValue()
	{
		const string clearanceEmail = "mail1.mail@mail.com";
		const string mailboxEmail = "mail2.mail@mail.com";

		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, nctsHeader.DeclEmailAddr);
			}

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, nctsHeader.DeclEmailAddr);
			}

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, nctsHeader.DeclEmailAddr);
			}
		});
	}

	public void TestClearanceEntryNumber()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("ClearanceEntryNumber->Not Null", nctsHeader.ClearanceEntryNumber);
			AssertEquals("ClearanceEntryNumber.CE_RN_NKCountryCode", nctsHeader.CountryCode, nctsHeader.ClearanceEntryNumber.CE_RN_NKCountryCode);
			AssertEquals("ClearanceEntryNumber.CE_EntryType", CusEntryNumberTypes.Spain.ClearanceCSV, nctsHeader.ClearanceEntryNumber.CE_EntryType);
		});
	}

	public void TestSupportsCombinedArrivalAndDepartureMessage()
	{
		AssertEquals(true, nctsHeader.SupportsCombinedArrivalAndDepartureMessage);
	}

	public void TestIsACEAuthorizedHolder()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		nctsHeader.DestinationTrader.OrganisationPK = org.PK;

		var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
		authorisationHeader.CPH_OH_PermitHolder = org.PK;

		AssertEquals("Destination Trader is an ACE authorized holder", true, nctsHeader.IsACEAuthorizedHolder);
	}

	public void TestDefaultBrokerOnSaving_Departure()
	{
		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = "XZX";
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = true;

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_GS_NKCusAgent = ZString.Empty;
				Factory.Save();
				AssertNull("Do not default BM_GS_NKCusAgent if current user is not Broker", nctsHeader.MovementHeader.CusAgent);

				staffCurrentUser.GS_IsSystemAccount = false;
				var nctsHeader2 = Factory.New<NctsHeader>();
				nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				nctsHeader2.MovementHeader.BM_GS_NKCusAgent = ZString.Empty;
				Factory.Save();
				AssertEquals("Default BM_GS_NKCusAgent if current user is Broker and CusAgent is empty", GlbStaff.CurrentUser.GS_Code, nctsHeader2.MovementHeader.BM_GS_NKCusAgent);

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AAA";
				staff.GS_LoginName = "AAA User";
				var nctsHeader3 = Factory.New<NctsHeader>();
				nctsHeader3.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				nctsHeader3.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
				Factory.Save();
				AssertEquals("BM_GS_NKCusAgent stays as is if it is not empty", staff, nctsHeader3.MovementHeader.CusAgent);
			});
		}
	}

	public void TestDefaultBrokerOnSaving_Arrival()
	{
		var staffCurrentUser = Factory.New<GlbStaff>();
		staffCurrentUser.GS_Code = "XZX";
		staffCurrentUser.GS_LoginName = "Current User";
		staffCurrentUser.GS_IsSystemAccount = true;

		using (Env.SetTemporaryUserContext(new UserContext(staffCurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
				Factory.Save();
				AssertNull("Do not default BM_GS_NKCusAgent if current user is not Broker", nctsHeader.ArrivalMovementHeader.CusAgent);

				staffCurrentUser.GS_IsSystemAccount = false;
				var nctsHeader2 = Factory.New<NctsHeader>();
				nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				nctsHeader2.ArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
				Factory.Save();
				AssertEquals("Default BM_GS_NKCusAgent if current user is Broker and CusAgent is empty", GlbStaff.CurrentUser.GS_Code, nctsHeader2.ArrivalMovementHeader.BM_GS_NKCusAgent);

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AAA";
				staff.GS_LoginName = "AAA User";
				var nctsHeader3 = Factory.New<NctsHeader>();
				nctsHeader3.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				nctsHeader3.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
				Factory.Save();
				AssertEquals("BM_GS_NKCusAgent stays as is if it is not empty", staff, nctsHeader3.ArrivalMovementHeader.CusAgent);
			});
		}
	}

	public void TestIESMessageInfoProvider_Broker_Departure()
	{
		CombineAssertions(() =>
		{
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			var esMessageInfoProvider = nctsHeaderDeparture as IESMessageInfoProvider;
			nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeaderDeparture.MovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			nctsHeaderDeparture.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIESMessageInfoProvider_Broker_Arrival()
	{
		CombineAssertions(() =>
		{
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			var esMessageInfoProvider = nctsHeaderArrival as IESMessageInfoProvider;
			nctsHeaderArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			nctsHeaderArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIESMessageInfoProvider_Broker_DepartureAndArrival()
	{
		CombineAssertions(() =>
		{
			var esMessageInfoProvider = nctsHeader as IESMessageInfoProvider;
			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIESMessageInfoProvider_EntryReference()
	{
		var esMessageInfoProvider = nctsHeader as IESMessageInfoProvider;
		nctsHeader.BH_JobReference = JobReference;
		AssertEquals("EntryReference has the correct value", JobReference, esMessageInfoProvider.EntryReference);
	}

	public void TestIESMessageInfoProvider_MRN()
	{
		var esMessageInfoProvider = nctsHeader as IESMessageInfoProvider;
		CombineAssertions(() =>
		{
			AssertEquals("MRN has the correct value (empty when nctsHeader has no mrn)", ZString.Empty, esMessageInfoProvider.MRN);

			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999830001277";
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals("MRN has the correct value", "20ES00999830001277", esMessageInfoProvider.MRN);
		});
	}

	public void TestIESResponseBusinessObject_BranchPK()
	{
		var esResponseBusinessObject = nctsHeader as IESResponseBusinessObject;
		AssertEquals("BranchPK has the correct value", nctsHeader.Branch.PK, esResponseBusinessObject.BranchPK);
	}

	public void TestIESResponseBusinessObject_MessageCollection()
	{
		var esResponseBusinessObject = nctsHeader as IESResponseBusinessObject;
		nctsHeader.Messages.AddNew();
		AssertEquals("MessageCollection has the correct value", nctsHeader.Messages, esResponseBusinessObject.MessageCollection);
	}

	public void TestIESResponseBOMessageStatus_MessageStatus()
	{
		var esResponseBusinessObject = nctsHeader as IESResponseBOMessageStatus;
		esResponseBusinessObject.MessageStatus = "AAA";
		AssertEquals("MessageStatus has set the correct value", "AAA", nctsHeader.EffectiveMessageStatus);
	}

	public void TestIPollingTransactionParent_CertificateName()
	{
		CombineAssertions(() =>
		{
			var pollingTransactionParent = nctsHeader as IPollingTransactionParent;
			nctsHeader.BH_CustomsProfile = ZString.Empty;
			AssertEquals("CertificateName is empty", ZString.Empty, pollingTransactionParent.CertificateName);

			nctsHeader.BH_CustomsProfile = "cert";
			AssertEquals("CertificateName is not empty", "cert", pollingTransactionParent.CertificateName);
		});
	}

	public void TestIPollingTransactionParent_IsTest()
	{
		var pollingTransactionParent = nctsHeader as IPollingTransactionParent;

		CombineAssertions(() =>
		{
			var registrationMock = new Mock<IProductRegistration>();
			registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				AssertEquals("IsTest is false when PRD and external environment", false, pollingTransactionParent.IsTest);
			}

			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				AssertEquals("IsTest is true when TST and external environment", true, pollingTransactionParent.IsTest);
			}

			registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				AssertEquals("IsTest is false when PRD and internal environment", false, pollingTransactionParent.IsTest);
			}

			registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				nctsHeader.TrainingEntry = true;
				AssertEquals("IsTest is true when TST and internal environment when flag is checked", true, pollingTransactionParent.IsTest);

				nctsHeader.TrainingEntry = false;
				AssertEquals("IsTest is false when TST and internal environment when flag is not checked", false, pollingTransactionParent.IsTest);
			}
		});
	}

	public void TestIPollingTransactionParent_Broker_Departure()
	{
		CombineAssertions(() =>
		{
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			var esMessageInfoProvider = nctsHeaderDeparture as IPollingTransactionParent;
			nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeaderDeparture.MovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			nctsHeaderDeparture.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIPollingTransactionParent_Broker_Arrival()
	{
		CombineAssertions(() =>
		{
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			var esMessageInfoProvider = nctsHeaderArrival as IPollingTransactionParent;
			nctsHeaderArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			nctsHeaderArrival.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIPollingTransactionParent_Broker_DepartureAndArrival()
	{
		CombineAssertions(() =>
		{
			var esMessageInfoProvider = nctsHeader as IPollingTransactionParent;
			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIPollingTransactionParent_Declarant()
	{
		var orgHeaderPrincipal = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeaderRepresentative = Factory.NewWithValidTestData<OrgHeader>();
		CombineAssertions(() =>
		{
			var pollingTransactionParent = nctsHeader as IPollingTransactionParent;
			nctsHeader.MovementHeader.Representative.OrganisationPK = ZGuid.Empty;
			nctsHeader.Principal.OrganisationPK = ZGuid.Empty;
			AssertNull("Declarant is null when Representative and Principal are not declared", pollingTransactionParent.Declarant);

			nctsHeader.Principal.OrganisationPK = orgHeaderPrincipal.PK;
			AssertEquals("Declarant is not null and shows Principal when declared and Representative is not declared", orgHeaderPrincipal, pollingTransactionParent.Declarant);

			nctsHeader.MovementHeader.Representative.OrganisationPK = orgHeaderRepresentative.PK;
			AssertEquals("Declarant is not null and shows Representative when declared", orgHeaderRepresentative, pollingTransactionParent.Declarant);
		});
	}

	public void TestIsPhase4()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NCT";
		AssertEquals("ApplicationCode is not Phase 4", true, header.IsPhase4);

		header.BH_ApplicationCode = "XXX";
		AssertEquals("ApplicationCode is not Phase 4", false, header.IsPhase4);
	}

	public void TestTrainingEntry_GenAddOnColumnAndDefault()
	{
		var header = Factory.NewWithValidTestData<NctsHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("TrainingEntry should be true by default", true, header.TrainingEntry);

			header.TrainingEntry = false;
			AssertEquals("TrainingEntry should be false when set", false, header.TrainingEntry);
		});
	}

	public void TestTrainingEntryPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_NCTS_TrainingEntry");

		var header = Factory.NewWithValidTestData<NctsHeader>();
		header.TrainingEntry = true;

		CombineAssertions(() =>
		{
			AssertEquals("TrainingEntry", true, header.TrainingEntry);
			AssertNotNull("TrainingEntry is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestRequestDispatch_GenAddOnColumn()
	{
		var header = Factory.NewWithValidTestData<NctsHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("RequestDispatch should be empty by default", "", header.RequestDispatch);

			header.RequestDispatch = "Y";
			AssertEquals("RequestDispatch should be true", "Y", header.RequestDispatch);
		});
	}

	public void TestRequestDispatchPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_NCTS_RequestDispatch");

		var header = Factory.NewWithValidTestData<NctsHeader>();
		header.RequestDispatch = "Y";

		CombineAssertions(() =>
		{
			AssertEquals("RequestDispatch", "Y", header.RequestDispatch);
			AssertNotNull("RequestDispatch is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestUpdatePreDeclaration_GenAddOnColumn()
	{
		var header = Factory.NewWithValidTestData<NctsHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("UpdatePreDeclaration should be false by default", false, header.UpdatePreDeclaration);

			header.UpdatePreDeclaration = true;
			AssertEquals("UpdatePreDeclaration should be true", true, header.UpdatePreDeclaration);
		});
	}

	public void TestUpdatePreDeclarationPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "ES_NCTS_UpdatePreDeclaration");

		var header = Factory.NewWithValidTestData<NctsHeader>();
		header.UpdatePreDeclaration = true;

		CombineAssertions(() =>
		{
			AssertEquals("UpdatePreDeclaration", true, header.UpdatePreDeclaration);
			AssertNotNull("UpdatePreDeclaration is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestIsDepartureAmendmentAllowed()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		var departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
		Assert(nctsHeader.IsDepartureAmendmentAllowed);

		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
		Assert(nctsHeader.IsDepartureAmendmentAllowed);

		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
		Assert(!nctsHeader.IsDepartureAmendmentAllowed);
	}

	public void TestIsDepartureCancellationAllowed()
	{
		var nctsHeader = GetPhase5Header(NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		Assert(nctsHeader.IsDepartureCancellationAllowed);

		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
		Assert(nctsHeader.IsDepartureCancellationAllowed);

		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
		Assert(nctsHeader.IsDepartureCancellationAllowed);

		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
		Assert(!nctsHeader.IsDepartureCancellationAllowed);
	}

	public void TestBH_CustomsProfileDefaulted_Departure()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AA";
		staff2.GS_LoginName = "aatest";
		var wrapper2 = GlbStaffWrapper.Get(staff2);
		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert4";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff3 = Factory.New<GlbStaff>();
		staff3.GS_Code = "AZ";
		staff3.GS_LoginName = "aztest";

		Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsDepartureMovementHeader = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff2.GS_Code;
			AssertEquals("BH_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", nctsHeader.BH_CustomsProfile);

			nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff3.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			AssertEquals("BH_CustomsProfile has value when broker not empty", "TestCert2", nctsHeader.BH_CustomsProfile);

			nctsDepartureMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertEquals("BH_CustomsProfile has been cleared when broker is empty", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsHeader.BH_CustomsProfile = "TESTCERT1";
			nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", nctsHeader.BH_CustomsProfile);

			nctsDepartureMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, nctsHeader.BH_CustomsProfile);
		});
	}

	public void TestBH_CustomsProfileDefaulted_Arrival()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AA";
		staff2.GS_LoginName = "aatest";
		var wrapper2 = GlbStaffWrapper.Get(staff2);
		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert4";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff3 = Factory.New<GlbStaff>();
		staff3.GS_Code = "AZ";
		staff3.GS_LoginName = "aztest";

		Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var nctsArrivalMovementHeader = nctsHeader.ArrivalMovementHeader;

		CombineAssertions(() =>
		{
			nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff2.GS_Code;
			AssertEquals("BH_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", nctsHeader.BH_CustomsProfile);

			nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff3.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			AssertEquals("BH_CustomsProfile has value when broker not empty", "TestCert2", nctsHeader.BH_CustomsProfile);

			nctsArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			AssertEquals("BH_CustomsProfile has been cleared when broker is empty", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsHeader.BH_CustomsProfile = "TESTCERT1";
			nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", nctsHeader.BH_CustomsProfile);

			nctsArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, nctsHeader.BH_CustomsProfile);
		});
	}

	public void TestCloneDepartureAndArrivalDeclaration()
	{
		nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
		var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
		container1.BC_ContainerNum = "CONTAINER1";
		nctsHeader.MovementHeader.GoodsItems.AddNew();

		var clonedNctsHeader = (NctsHeader)nctsHeader.TemplateCopy();

		CombineAssertions(() =>
		{
			AssertEquals("Original NctsHeader's HeaderType", NctsMovementType.Codes.DepartureAndArrival, nctsHeader.BH_HeaderType);
			AssertEquals("Cloned NctsHeader's HeaderType", NctsMovementType.Codes.Departure, clonedNctsHeader.BH_HeaderType);
			AssertEquals("Should only be one movement header (Departure)", 1, clonedNctsHeader.MovementHeaders.Count);
			AssertEquals("EffectiveMessageStatus should be 'MDN'", NctsMessageStatusList.Codes.DepartureDeclarationNotSent, clonedNctsHeader.EffectiveMessageStatus);
			AssertEquals("BM_CustomsStatus should be ''", ZString.Empty, clonedNctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("MovementHeader.GoodsItems.Count", 1, clonedNctsHeader.MovementHeader.GoodsItems.Count);
		});
	}

	public void TestLocalReferenceNumberReadOnly_Departure()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header is departure and arrival", true, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			AssertEquals("When header is not departure and arrival", false, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.Messages.AddNew();
			AssertEquals("When header has at least one message", true, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.Messages.RemoveAndDeleteAll();
			AssertEquals("When header has no messages", false, nctsHeader.LocalReferenceNumberReadOnly);

			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";
			AssertEquals("When header is departure and has mrn", true, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			AssertEquals("When header BM_CustomsStatus DRJ and EffectiveMessageStatus not MDS", false, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			AssertEquals("When header BM_CustomsStatus DRJ and EffectiveMessageStatus MDS", true, nctsHeader.LocalReferenceNumberReadOnly);
		});
	}

	public void TestLocalReferenceNumberReadOnly_Arrival()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header is departure and arrival", true, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			AssertEquals("When header is not departure and arrival", false, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.Messages.AddNew();
			AssertEquals("When header has at least one message", true, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.Messages.RemoveAndDeleteAll();
			AssertEquals("When header has no messages", false, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.Messages.AddNew();
			AssertEquals("When header has at least one message", true, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			AssertEquals("When header BM_CustomsStatus DRJ and EffectiveMessageStatus not MAS", false, nctsHeader.LocalReferenceNumberReadOnly);

			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			AssertEquals("When header BM_CustomsStatus DRJ and EffectiveMessageStatus MAS", true, nctsHeader.LocalReferenceNumberReadOnly);
		});
	}

	public void TestArrivalMrnFromUserReadOnly()
	{
		CombineAssertions(() =>
		{
			nctsHeader = GetPhase5Header(NctsMovementType.Codes.Arrival);
			AssertEquals("When default ArrivalMrnFromUser should be not readonly", false, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("When EffectiveMessageStatus is SNT, ArrivalMrnFromUser ArrivalMrnFromUser should be readonly", true, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			AssertEquals("When GoodItems and EffectiveMessageStatus is not SNT or ACK, ArrivalMrnFromUser should not be readonly", false, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			AssertEquals("When GoodItems and BM_CustomsStatus is CL1 ArrivalMrnFromUser should be readonly", true, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;

			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			AssertEquals("When CEN_TNNArrival is true but there is no TNN ArrivalMrnFromUser should not be readonly", false, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
			AssertEquals("When CEN_TNNArrival is false ArrivalMrnFromUser should not be readonly", false, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.ArrivalMrnFromUser = "23ES00999912345678";
			var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
			tnnDataCodeInfo.AcceptanceDate = ZDateTime.Now;
			tnnDataCodeInfo.ClearanceDate = ZDateTime.Now.AddDays(-1);
			nctsHeader.ArrivalMovementHeader.GenerateTNNDeparture(tnnDataCodeInfo);
			Factory.Save();
			nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			AssertEquals("When TNN Customs Status is not empty and CEN_TNNArrival is true ArrivalMrnFromUser should be readonly", true, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.BM_CustomsStatus = ZString.Empty;
			AssertEquals("When TNN Customs Status is empty and CEN_TNNArrival is true ArrivalMrnFromUser should not be readonly", false, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);

			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
			AssertEquals("When TNN Customs Status is empty but CEN_TNNArrival is false ArrivalMrnFromUser should not be readonly", false, nctsHeader.ArrivalMrnFromUserInfo.ReadOnly);
		});
	}

	public void TestGetUrlToLaunchNcts_Departure()
	{
		CombineAssertions(() =>
		{
			AssertGetUrlNcts(NctsMovementType.Codes.Departure);
		});
	}

	public void TestGetUrlToLaunchNcts_Arrival_Phase4()
	{
		CombineAssertions(() =>
		{
			AssertGetUrlNcts(NctsMovementType.Codes.Arrival);
		});
	}

	public void TestGetUrlToLaunchNcts_Arrival_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			AssertGetUrlNcts(NctsMovementType.Codes.Arrival);
		});
	}

	public void TestGetUrlToLaunchNcts_DepartureAndArrival()
	{
		CombineAssertions(() =>
		{
			AssertGetUrlNcts(NctsMovementType.Codes.DepartureAndArrival);
		});
	}

	public void TestCanLaunchNctsUrl_Phase4()
	{
		CombineAssertions(() =>
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertCanLaunchNctsUrl(nctsHeader);

			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			AssertCanLaunchNctsUrl(nctsHeader);

			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertCanLaunchNctsUrl(nctsHeader);
		});
	}

	public void TestCanLaunchNctsUrl_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertCanLaunchNctsUrl(nctsHeader);

			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			AssertCanLaunchNctsUrl(nctsHeader);

			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertCanLaunchNctsUrl(nctsHeader);
		});
	}

	void AssertGetUrlNcts(ZString nctsMovementType)
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=" + expectedMRN;
		nctsHeader.BH_HeaderType = nctsMovementType;

		AssertNullOrEmpty("No url was returned when no mrn", nctsHeader.GetUrlToLaunch());

		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = expectedMRN;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		AssertEquals("The correct url has been launched", expectedUrl, nctsHeader.GetUrlToLaunch());
	}

	void AssertCanLaunchNctsUrl(NctsHeader nctsHeader, string assertionMessage = "")
	{
		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = ZString.Empty;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		AssertEquals(nctsHeader.BH_HeaderType + assertionMessage + " False when NctsHeader has no mrn", false, nctsHeader.CanLaunchNctsUrl());

		newEntryNumber.CE_EntryNum = "AACCRRRRRRNNNNNNNN";
		AssertEquals(nctsHeader.BH_HeaderType + assertionMessage + " True when NctsHeader has mrn", true, nctsHeader.CanLaunchNctsUrl());
	}

	public void TestGetTrasitStatusFromCustomsAndAddTransactionIfNeeded_WrongData()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var mrnCode = "20ES00999950012811";

		CombineAssertions(() =>
		{
			var writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA but has no jobreference/mrn/broker/certificate", ZString.Empty, "Excluded (not registered)");

			nctsHeader.BH_JobReference = JobReference;
			writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA but has no mrn/broker/certificate", JobReference, "Excluded (not registered)");

			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = mrnCode;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA but has no broker/certificate", JobReference, "Excluded (review broker)", mrnCode);

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = "TestCert1";
			var mockNctsHeader = new Mock<NctsHeader>(Factory, ((IBusinessObjectFactoryInternals)Factory).RowFactory.LoadFromPK("CusInBondHeader", nctsHeader.PK));
			mockNctsHeader.Setup(a => a.CreateRequestAndGetResponse(mrnCode, cert)).Returns(GetNotWrittenOffTransitFromCustomsTestFileContents());
			mockNctsHeader.CallBase = true;
			var mockNctsHeaderObject = mockNctsHeader.Object;
			writeOffResult = mockNctsHeaderObject.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA and has all data", JobReference, "Not Written Off", mrnCode, "Despachado");

			mockNctsHeaderObject.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			writeOffResult = mockNctsHeaderObject.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header has all data but is A", JobReference, "Excluded (not a departure)", mrnCode);

			mockNctsHeaderObject.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			writeOffResult = mockNctsHeaderObject.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is D but has no broker", JobReference, "Excluded (review broker)", mrnCode);

			mockNctsHeaderObject.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			writeOffResult = mockNctsHeaderObject.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is D and has all data", JobReference, "Not Written Off", mrnCode, "Despachado");

			var nctsHeaderWithUrlException = SetDataForGetTransitStatusFromCustoms(MRNCode, throwException: true);
			writeOffResult = nctsHeaderWithUrlException.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertEquals("LastKeyReported has exception when certificate has been revoked", "NctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		});
	}

	public void TestGetTrasitStatusFromCustomsAndAddTransactionIfNeeded_NoTransactionAdded()
	{
		var nctsHeader = SetDataForGetTransitStatusFromCustoms("22ES00999950009215", GetNotWrittenOffTransitFromCustomsTestFileContents());

		var guaranteeReference = "Guarantee1";
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.TRA, nctsHeader.MovementReferenceNumber, -60m, 0);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;
		guarantee.PW_BondAmount = 1000m;

		CombineAssertions(() =>
		{
			var writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA and has all data with correct certificate", JobReference, "Not Written Off", "22ES00999950009215", "Despachado");

			var guarantee1Transactions = LoadCusGuaranteeHeaderList().First(x => x.CPH_Number == guaranteeReference).GetTransactions();
			AssertEquals("5 Original transactions, no transactions added", 5, guarantee1Transactions.Count());
		});
	}

	public void TestGetTrasitStatusFromCustomsAndAddTransactionIfNeeded_NoTransactionAdded_NoPendingDebt()
	{
		var nctsHeader = SetDataForGetTransitStatusFromCustoms(MRNCode, GetWrittenOffTransitFromCustomsTestFileContent());

		var guaranteeReference = "Guarantee1";
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.TRA, nctsHeader.MovementReferenceNumber, 60m, 0);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;
		guarantee.PW_BondAmount = 1000m;

		CombineAssertions(() =>
		{
			var writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA and has all data with correct certificate", JobReference, "Excluded (no pending debt)", MRNCode, "Ultimado");

			var guarantee1Transactions = LoadCusGuaranteeHeaderList().First(x => x.CPH_Number == guaranteeReference).GetTransactions();
			AssertEquals("5 Original transactions, no transactions added", 5, guarantee1Transactions.Count());
		});
	}

	public void TestGetTrasitStatusFromCustomsAndAddTransactionIfNeeded_TransactionAdded()
	{
		var nctsHeader = SetDataForGetTransitStatusFromCustoms(MRNCode, GetWrittenOffTransitFromCustomsTestFileContent());

		var guaranteeReference = "Guarantee1";
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.TRA, nctsHeader.MovementReferenceNumber, -60m, 0);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;
		guarantee.PW_BondAmount = 1000m;

		CombineAssertions(() =>
		{
			var writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA and has all data with correct certificate", JobReference, "Written Off", MRNCode, "Ultimado");

			var guarantee1Transactions = LoadCusGuaranteeHeaderList().First(x => x.CPH_Number == guaranteeReference).GetTransactions();
			AssertEquals("5 Original transactions + 1 new one", 6, guarantee1Transactions.Count());

			var transaction = guarantee1Transactions.First(x => x.CPL_Comment.StartsWith("Write-off"));
			AssertEquals("Transaction.CPL_Reference", MRNCode, transaction.CPL_Reference);
			AssertEquals("Transaction.CPL_Comment", "Write-off NCTS Departure AH3", transaction.CPL_Comment);
			AssertEquals("Transaction.CPL_TranValue", 120m, transaction.CPL_TranValue);
			AssertEquals("Transaction.CPL_TransactionDate", new ZDateTime(2022, 04, 20), transaction.CPL_TransactionDate);
			AssertEquals("Transaction.CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
		});
	}

	[TestDate(2021, 10, 05, 09, 36, 0)]
	public void TestGetTrasitStatusFromCustomsAndAddTransactionIfNeeded_TransactionAdded_NoDate()
	{
		var nctsHeader = SetDataForGetTransitStatusFromCustoms(MRNCode, GetWrittenOffWithoutDateTransitFromCustomsTestFile());

		var guaranteeReference = "Guarantee1";
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.TRA, nctsHeader.MovementReferenceNumber, -60m, 0);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;
		guarantee.PW_BondAmount = 1000m;

		CombineAssertions(() =>
		{
			var writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA and has all data with correct certificate", JobReference, "Written Off", MRNCode, "Ultimado");

			var guarantee1Transactions = LoadCusGuaranteeHeaderList().First(x => x.CPH_Number == guaranteeReference).GetTransactions();
			AssertEquals("5 Original transactions + 1 new one", 6, guarantee1Transactions.Count());

			var transaction = guarantee1Transactions.First(x => x.CPL_Comment.StartsWith("Write-off"));
			AssertEquals("Transaction.CPL_Reference", MRNCode, transaction.CPL_Reference);
			AssertEquals("Transaction.CPL_Comment", "Write-off NCTS Departure AH3", transaction.CPL_Comment);
			AssertEquals("Transaction.CPL_TranValue", 120m, transaction.CPL_TranValue);
			AssertEquals("Transaction.CPL_TransactionDate", new ZDateTime(2021, 10, 05, 09, 36, 0), transaction.CPL_TransactionDate);
			AssertEquals("Transaction.CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
		});
	}

	public void TestGetTrasitStatusFromCustomsAndAddTransactionIfNeeded_ErrorReturned()
	{
		var nctsHeader = SetDataForGetTransitStatusFromCustoms(MRNCode, GetWrittenOffTransitFromCustomsTestFileContent());

		var guaranteeReference = "Guarantee1";
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.TRA, nctsHeader.MovementReferenceNumber, 600m, 0);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;
		guarantee.PW_BondAmount = 1000m;

		CombineAssertions(() =>
		{
			var writeOffResult = nctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded();
			AssertWriteOffResult(writeOffResult, "When header is DA and has all data with correct certificate", JobReference, "Not Written Off (at least one positive balance)", MRNCode, "Ultimado");

			var guarantee1Transactions = LoadCusGuaranteeHeaderList().First(x => x.CPH_Number == guaranteeReference).GetTransactions();
			AssertEquals("5 Original transactions, no transactions added", 5, guarantee1Transactions.Count());
		});
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_Validations_CanBeSent()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var guaranteeReference = "16ESAGL9990000096";

		var expectedTQUMessageInfo = new TQUMessageInfo()
		{
			MessageCanBeSent = false,
			MessageSentCorrectly = false,
			DeclarationWithBrokerError = ZString.Empty
		};

		CombineAssertions(() =>
		{
			AssertTQUMessageInfo("When header is DA but has no mrn", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertTQUMessageInfo("When header is DA with mrn but without guarantees associated", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeReference;
			AssertTQUMessageInfo("When header is DA with mrn but without a correct guarantee associated (does not exist in CusGuaranteeHeader)", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			CreateGuaranteeHeaderDetail(Factory, guaranteeReference, addOBLTransaction: false);
			AssertTQUMessageInfo("When header is DA with mrn but without a correct guarantee associated (CusGuaranteeHeader does not have OBL transaction)", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			CreateGuaranteeHeaderDetail(Factory, guaranteeReference, withOldEndDate: true);
			AssertTQUMessageInfo("When header is DA with mrn but without a correct guarantee associated (CusGuaranteeHeader has expired)", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeaderArrival, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeReference;
			AssertTQUMessageInfo("When header is A", expectedTQUMessageInfo, nctsHeaderArrival.SendTQUForGuaranteeWriteOffIfPossible());
		});
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_Validations_BrokerError_DA()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		nctsHeader.BH_JobReference = JobReference;
		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "20ES00999950012811";
		newEntryNumber.CE_EntryIsSystemGenerated = true;

		var guaranteeReference = "16ESAGL9990000096";
		CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;

		var expectedTQUMessageInfo = new TQUMessageInfo()
		{
			MessageCanBeSent = true,
			MessageSentCorrectly = false,
			DeclarationWithBrokerError = JobReference
		};

		CombineAssertions(() =>
		{
			AssertTQUMessageInfo("Message cannot be sent because there is no broker declared", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = ZString.Empty;
			AssertTQUMessageInfo("Message cannot be sent because there is no certificate declared", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			nctsHeader.BH_CustomsProfile = "INVALID";
			AssertTQUMessageInfo("Message cannot be sent because the certificate declared is not valid", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			nctsHeader.BH_CustomsProfile = cert.GP_Name;
			AssertTQUMessageInfo("Message cannot be sent because current user has no authorisation for certificate declared", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());
		});
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_Validations_BrokerError_D()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.BH_JobReference = JobReference;
		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "20ES00999950012811";
		newEntryNumber.CE_EntryIsSystemGenerated = true;

		var guaranteeReference = "16ESAGL9990000096";
		CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = guaranteeReference;

		var expectedTQUMessageInfo = new TQUMessageInfo()
		{
			MessageCanBeSent = true,
			MessageSentCorrectly = false,
			DeclarationWithBrokerError = JobReference
		};

		CombineAssertions(() =>
		{
			AssertTQUMessageInfo("Message cannot be sent because there is no broker declared", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = ZString.Empty;
			AssertTQUMessageInfo("Message cannot be sent because there is no certificate declared", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			nctsHeader.BH_CustomsProfile = "INVALID";
			AssertTQUMessageInfo("Message cannot be sent because the certificate declared is not valid", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());

			nctsHeader.BH_CustomsProfile = cert.GP_Name;
			AssertTQUMessageInfo("Message cannot be sent because current user has no authorisation for certificate declared", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());
		});
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_DA_Phase4()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = cert.GP_Name;
			nctsHeader.BH_JobReference = JobReference;
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			var guaranteeReference = "16ESAGL9990000096";
			CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeReference;

			Factory.Save();

			AssertSendTQUForGuaranteeWriteOffIfPossible(nctsHeader, NctsMessageStatusList.Codes.ArrivalNotificationSent);
		}
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_DA_Phase5()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = cert.GP_Name;
			nctsHeader.BH_JobReference = JobReference;
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			var guaranteeReference = "16ESAGL9990000096";
			CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeReference;

			Factory.Save();

			AssertSendTQUForGuaranteeWriteOffIfPossible(nctsHeader, LogicalStatusList.Codes.Sent);
		}
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_D_Phase4()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = cert.GP_Name;
			nctsHeader.BH_JobReference = JobReference;
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			var guaranteeReference = "16ESAGL9990000096";
			CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeReference;

			Factory.Save();

			AssertSendTQUForGuaranteeWriteOffIfPossible(nctsHeader, NctsMessageStatusList.Codes.DepartureDeclarationSent);
		}
	}

	public void TestSendTQUForGuaranteeWriteOffIfPossible_D_Phase5()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = cert.GP_Name;
			nctsHeader.BH_JobReference = JobReference;
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			var guaranteeReference = "16ESAGL9990000096";
			CreateGuaranteeHeaderDetail(Factory, guaranteeReference);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeReference;

			Factory.Save();

			AssertSendTQUForGuaranteeWriteOffIfPossible(nctsHeader, LogicalStatusList.Codes.Sent);
		}
	}

	void AssertSendTQUForGuaranteeWriteOffIfPossible(NctsHeader nctsHeader, ZString expectedMessageStatus)
	{
		var expectedTQUMessageInfo = new TQUMessageInfo()
		{
			MessageCanBeSent = true,
			MessageSentCorrectly = true,
			DeclarationWithBrokerError = ZString.Empty
		};

		CombineAssertions(() =>
		{
			AssertTQUMessageInfo("Message was sent correctly", expectedTQUMessageInfo, nctsHeader.SendTQUForGuaranteeWriteOffIfPossible());
			AssertEquals("Entry Message Status has changed", expectedMessageStatus, nctsHeader.EffectiveMessageStatus);

			var messages = nctsHeader.Messages;
			AssertEquals("1 EDIMessage was created for the entry", 1, messages.Count);
			var message = messages[0];
			AssertEquals("Sent message has correct EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("Sent message has correct EM_MessageType", DeclarationMessageTypeList.Codes.TransitNcts5Query, message.EM_MessageType);
		});
	}

	void AssertTQUMessageInfo(ZString assertText, TQUMessageInfo expected, TQUMessageInfo returned)
	{
		AssertEquals(assertText + " MessageCanBeSent", expected.MessageCanBeSent, returned.MessageCanBeSent);
		AssertEquals(assertText + " MessageSentCorrectly", expected.MessageSentCorrectly, returned.MessageSentCorrectly);
		AssertEquals(assertText + " DeclarationWithBrokerError", expected.DeclarationWithBrokerError, returned.DeclarationWithBrokerError);
	}

	public void TestSetClearanceInfoClearanceNumber()
	{
		nctsHeader.BH_JobReference = "ES00001";
		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "CLEARANCE1230000";
		nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";

		CombineAssertions(() =>
		{
			nctsHeader.SetClearanceInfoClearanceNumber("CLEARANCE1234567");
			AssertEquals("CSV clearance has been changed when the pop up was accepted", "CLEARANCE1234567", newEntryNumber.CE_EntryNum);
			AssertEquals("CustomsStatus has been changed to DRL", NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("New event in logs", "|NEW=CLEARANCE1234567|OLD=CLEARANCE1230000|RES=Manually Modify Clearance Number to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
			nctsHeader.SetClearanceInfoClearanceNumber("CLEARANCE1234567");
			AssertEquals("CustomsStatus is left as is", "AAA", nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("The Last Event is still the same cause the CSV Clearance does not change", "|NEW=CLEARANCE1234567|OLD=CLEARANCE1230000|RES=Manually Modify Clearance Number to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			nctsHeader.SetClearanceInfoClearanceNumber("AAAAAAAAAAAAAAAA");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", newEntryNumber.CE_EntryNum);
			AssertEquals("CustomsStatus has been changed to DRL", NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("New event in logs for second change", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Modify Clearance Number to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestSetClearanceInfoClearanceDate()
	{
		nctsHeader.BH_JobReference = "ES00001";
		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_IssueDate = ZDateTime.Today;

		CombineAssertions(() =>
		{
			nctsHeader.SetClearanceInfoClearanceDate(ZDateTime.Today.AddDays(1));
			AssertEquals("Clearance Date has been changed when the pop up was accepted", ZDateTime.Today.AddDays(1).ToString(), newEntryNumber.CE_IssueDate.ToString());
			AssertEquals("New event in logs", "|NEW=" + ZDateTime.Today.AddDays(1).ToString() + "|OLD=" + ZDateTime.Today.ToString() + "|RES=Manually Modify Clearance Date to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			nctsHeader.SetClearanceInfoClearanceDate(ZDateTime.Today.AddDays(1));
			AssertEquals("The Last Event is still the same cause the Clearance Date does not change", "|NEW=" + ZDateTime.Today.AddDays(1).ToString() + "|OLD=" + ZDateTime.Today.ToString() + "|RES=Manually Modify Clearance Date to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			nctsHeader.SetClearanceInfoClearanceDate(ZDateTime.Today.AddDays(2));
			AssertEquals("Clearance Date has been changed when the pop up was accepted a second time", ZDateTime.Today.AddDays(2).ToString(), newEntryNumber.CE_IssueDate.ToString());
			AssertEquals("New event in logs for second change", "|NEW=" + ZDateTime.Today.AddDays(2).ToString() + "|OLD=" + ZDateTime.Today.AddDays(1).ToString() + "|RES=Manually Modify Clearance Date to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestSetClearanceInfoArrivalLimitDate()
	{
		nctsHeader.BH_JobReference = "ES00001";
		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_ExpiryDate = ZDateTime.Today;

		CombineAssertions(() =>
		{
			nctsHeader.SetClearanceInfoArrivalLimitDate(ZDateTime.Today.AddDays(1));
			AssertEquals("Arrival Limit Date has been changed when the pop up was accepted", ZDateTime.Today.AddDays(1).ToString(), newEntryNumber.CE_ExpiryDate.ToString());
			AssertEquals("New event in logs", "|NEW=" + ZDateTime.Today.AddDays(1).ToString() + "|OLD=" + ZDateTime.Today.ToString() + "|RES=Manually Modify Arrival Limit Date to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			nctsHeader.SetClearanceInfoArrivalLimitDate(ZDateTime.Today.AddDays(1));
			AssertEquals("The Last Event is still the same cause the Arrival Limit Date does not change", "|NEW=" + ZDateTime.Today.AddDays(1).ToString() + "|OLD=" + ZDateTime.Today.ToString() + "|RES=Manually Modify Arrival Limit Date to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			// 5ms sleep to ensure second change log's event time is after first change log's time, taking into account SQL datetime precision (3ms).
			Thread.Sleep(5);

			nctsHeader.SetClearanceInfoArrivalLimitDate(ZDateTime.Today.AddDays(2));
			AssertEquals("Arrival Limit Date has been changed when the pop up was accepted a second time", ZDateTime.Today.AddDays(2).ToString(), newEntryNumber.CE_ExpiryDate.ToString());
			AssertEquals("New event in logs for second change", "|NEW=" + ZDateTime.Today.AddDays(2).ToString() + "|OLD=" + ZDateTime.Today.AddDays(1).ToString() + "|RES=Manually Modify Arrival Limit Date to Entry ES00001|TYP=CLR", nctsHeader.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestDepartureAndArrivalDefaultMessageStatus()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.TurnArrivalIntoDepartureAndArrival();

		AssertEquals("For Departure and Arrival, Message Status should be MAN", nctsHeader.EffectiveMessageStatus, NctsMessageStatusList.Codes.ArrivalNotificationNotSent);
	}

	public void TestICusStorageDocPivotTypeSupporter_ReloadCollection()
	{
		_ = nctsHeader.EDocPivotCollection;
		var provider = (ICusStorageDocPivotTypeSupporter)nctsHeader;
		var pivot = Factory.New<NctsCusStorageDocPivot>();
		pivot.CSD_ParentID = nctsHeader.PK;
		pivot.CSD_ParentTableCode = nctsHeader.TablePrefix;
		provider.ReloadCollection();
		AssertEquals(1, nctsHeader.EDocPivotCollection.Count);
	}

	public void TestCusStorageDocPivotInterfaces()
	{
		var shipment = Factory.New<ForwardingShipment>();
		nctsHeader.BH_ParentID = shipment.PK;

		var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "First.pdf", "CIV");
		var eDoc2 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Second.pdf", "CIV");
		var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "Shipment.pdf", "CIV");

		CombineAssertions(() =>
		{
			var pivotParent = (INctsCusStorageDocPivotParent)nctsHeader;
			AssertNotNull("EDocPivotCollection is not null", pivotParent.EDocPivotCollection);
			AssertEquals("There are 2 documents in EDocCollections", 2, pivotParent.EDocCollections.Count());
			AssertEquals("ICusStorageDocPivotParent method edoc1", true, pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc2", true, pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc3", true, pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc3.UniqueKey.ToGuid()) != null));

			var newPivot = pivotParent.EDocPivotCollection.AddNew();
			newPivot.CSD_DocType = "T1";
			Factory.Save();

			var loadedPivot = new BusinessObjectFactory().Load<BaseCusStorageDocPivot>(newPivot.PK);
			AssertType("ICusStorageDocPivotTypeSupporter method, should have returned the correct type", typeof(NctsCusStorageDocPivot), loadedPivot);
		});
	}

	public void TestEDocCollectionsWhenDepartureTNN()
	{
		var nctsHeaderTNN = Factory.New<NctsHeader>();
		nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = "mrnCode";
		var tnnMovement = nctsHeaderTNN.MovementHeader;

		var shipmentD = Factory.New<ForwardingShipment>();
		nctsHeaderTNN.BH_ParentID = shipmentD.PK;

		var eDoc1 = nctsHeaderTNN.DocManagerInfo.AddFileOrDocument(new byte[1], "FirstD.pdf", "CIV");
		var eDoc2 = nctsHeaderTNN.DocManagerInfo.AddFileOrDocument(new byte[1], "SecondD.pdf", "CIV");
		var eDoc3 = shipmentD.DocManagerInfo.AddFileOrDocument(new byte[1], "ShipmentD.pdf", "CIV");

		var nctsHeaderArrival = Factory.New<NctsHeader>();
		nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeaderArrival.MovementReferenceEntryNumber.CE_EntryNum = "mrnCode";
		nctsHeaderArrival.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

		var shipmentA = Factory.New<ForwardingShipment>();
		nctsHeaderArrival.BH_ParentID = shipmentA.PK;

		var eDoc4 = nctsHeaderArrival.DocManagerInfo.AddFileOrDocument(new byte[1], "FirstA.pdf", "CIV");
		var eDoc5 = nctsHeaderArrival.DocManagerInfo.AddFileOrDocument(new byte[1], "SecondA.pdf", "CIV");
		var eDoc6 = shipmentA.DocManagerInfo.AddFileOrDocument(new byte[1], "ShipmentA.pdf", "CIV");

		CombineAssertions(() =>
		{
			var eDocCollections = ((INctsCusStorageDocPivotParent)nctsHeaderTNN).EDocCollections;
			AssertEquals("There are 2 EDocCollections when header is departure and not TNN", 2, eDocCollections.Count());
			AssertEquals("ICusStorageDocPivotParent method edoc1 when header is departure and not TNN", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc2 when header is departure and not TNN", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc3 when header is departure and not TNN", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc3.UniqueKey.ToGuid()) != null));

			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = false;
			tnnMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = false;
			eDocCollections = ((INctsCusStorageDocPivotParent)nctsHeaderTNN).EDocCollections;
			AssertEquals("There are 2 EDocCollections when header is departure and TNN and has Arrival associated (by mrn) but CEN_TNNArrival is false", 2, eDocCollections.Count());
			AssertEquals("ICusStorageDocPivotParent method edoc1 when header is departure and TNN and has Arrival associated (by mrn) but CEN_TNNArrival is false", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc2 when header is departure and TNN and has Arrival associated (by mrn) but CEN_TNNArrival is false", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc3 when header is departure and TNN and has Arrival associated (by mrn) but CEN_TNNArrival is false", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc3.UniqueKey.ToGuid()) != null));

			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = true;
			eDocCollections = ((INctsCusStorageDocPivotParent)nctsHeaderTNN).EDocCollections;
			AssertEquals("There are 2 EDocCollections when header is departure and TNN and has Arrival associated (by mrn) with CEN_TNNArrival true", 2, eDocCollections.Count());
			AssertEquals("ICusStorageDocPivotParent method edoc4 when header is departure and TNN and has Arrival associated (by mrn) with CEN_TNNArrival true", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc4.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc5 when header is departure and TNN and has Arrival associated (by mrn) with CEN_TNNArrival true", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc5.UniqueKey.ToGuid()) != null));
			AssertEquals("ICusStorageDocPivotParent method edoc6 when header is departure and TNN and has Arrival associated (by mrn) with CEN_TNNArrival true", true, eDocCollections.Any(x => x.GetFromUniqueKey(eDoc6.UniqueKey.ToGuid()) != null));
		});
	}

	public void TestGetAllEDocPivotsToSend()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		Factory.Save();

		CombineAssertions(() =>
		{
			var pivotList = nctsHeader.GetAllSendableEDocPivots();
			AssertEquals("GetAllEDocPivotsToSend returns empty pivotList since there are no pivots in the colection", 0, pivotList.Count);

			var docPivot = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();
			pivotList = nctsHeader.GetAllSendableEDocPivots();
			AssertEquals("GetAllEDocPivotsToSend returns pivotList with one element since there is only one pivot in the colection", 1, pivotList.Count);

			var docPivot2 = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeader.EDocPivotCollection.Add(docPivot2);
			Factory.Save();
			pivotList = nctsHeader.GetAllSendableEDocPivots();
			AssertEquals("GetAllEDocPivotsToSend returns pivotList with two elements since there are two pivots in the colection with empty status", 2, pivotList.Count);

			var docPivot3 = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeader.EDocPivotCollection.Add(docPivot3);
			var docPivot4 = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeader.EDocPivotCollection.Add(docPivot4);

			var message1 = SetEDIMessageAndGenPivot(nctsHeader, docPivot2);
			var message2 = SetEDIMessageAndGenPivot(nctsHeader, docPivot3);
			var message3 = SetEDIMessageAndGenPivot(nctsHeader, docPivot4);
			Factory.Save();
			pivotList = nctsHeader.GetAllSendableEDocPivots();
			AssertEquals("GetAllEDocPivotsToSend returns pivotList with one element since all pivots in collection are associated to send messages but one (last pivot)", 1, pivotList.Count);

			message1.EM_Status = EDIMessage.Status.Rejected;
			message2.EM_Status = EDIMessage.Status.Failed;
			message3.EM_Status = EDIMessage.Status.Error;

			var docPivot5 = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeader.EDocPivotCollection.Add(docPivot5);
			var message4 = SetEDIMessageAndGenPivot(nctsHeader, docPivot5);
			message4.EM_Status = EDIMessage.Status.Received;
			Factory.Save();
			pivotList = nctsHeader.GetAllSendableEDocPivots();
			AssertEquals("GetAllEDocPivotsToSend returns pivotList with 4 elements (messages have error/failed/rejected/empty status)", 4, pivotList.Count);
		});
	}

	public void TestHasAnnexesSentWithoutResponse()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		Factory.Save();

		CombineAssertions(() =>
		{
			nctsHeader.EDocPivotCollection.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("HasAnnexesSentWithoutResponse is false when EDocPivotCollection is empty", false, nctsHeader.HasAnnexesSentWithoutResponse());

			var docPivot = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeader.EDocPivotCollection.Add(docPivot);
			Factory.Save();
			AssertEquals("HasAnnexesSentWithoutResponse is false when nctsHeader has pivots in EDocPivotCollection but no EDIMessages associated to the pivot", false, nctsHeader.HasAnnexesSentWithoutResponse());

			var message = SetEDIMessageAndGenPivot(nctsHeader, docPivot);
			Factory.Save();
			AssertEquals("HasAnnexesSentWithoutResponse is true when nctsHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot", true, nctsHeader.HasAnnexesSentWithoutResponse());

			message.EM_Status = EDIMessage.Status.Rejected;
			Factory.Save();
			AssertEquals("HasAnnexesSentWithoutResponse is false when nctsHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is not awaiting response", false, nctsHeader.HasAnnexesSentWithoutResponse());
		});
	}

	public void TestRequiresAnnexes()
	{
		var nctsHeaderDeparture = Factory.New<NctsHeader>();
		nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;

		CombineAssertions(() =>
		{
			AssertEquals("RequiresAnnexes is true when Departure and BM_Phase is TNN", true, nctsHeaderDeparture.RequiresAnnexes());

			nctsHeaderDeparture.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("RequiresAnnexes is false when Departure, BM_Phase is not TNN and MRN is empty", false, nctsHeaderDeparture.RequiresAnnexes());

			nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "MRN-TEST";
			AssertEquals("RequiresAnnexes is true when Departure, BM_Phase is not TNN, MRN is not empty and CLR is empty", true, nctsHeaderDeparture.RequiresAnnexes());

			nctsHeaderDeparture.ClearanceEntryNumber.CE_EntryNum = "CSV-TEST";
			AssertEquals("RequiresAnnexes is false when Departure, BM_Phase is not TNN and MRN is not empty but CLR is not empty and there are no annexes", false, nctsHeaderDeparture.RequiresAnnexes());

			var docPivot = Factory.NewWithValidTestData<NctsCusStorageDocPivot>();
			nctsHeaderDeparture.EDocPivotCollection.Add(docPivot);
			var message = SetEDIMessageAndGenPivot(nctsHeaderDeparture, docPivot);
			message.EM_Status = EDIMessage.Status.Received;
			Factory.Save();
			AssertEquals("RequiresAnnexes is true when Departure, BM_Phase is not TNN, MRN is not empty, CLR is not empty and there are Annexes with Message Status RCV", true, nctsHeaderDeparture.RequiresAnnexes());

			message.EM_Status = EDIMessage.Status.Rejected;
			AssertEquals("RequiresAnnexes is false when Departure, BM_Phase is not TNN, MRN is not empty, CLR is not empty but no Annexes with Message Status RCV or SNT", false, nctsHeaderDeparture.RequiresAnnexes());

			nctsHeaderDeparture.EDocPivotCollection.Add(docPivot);
			message = SetEDIMessageAndGenPivot(nctsHeaderDeparture, docPivot);
			Factory.Save();
			AssertEquals("RequiresAnnexes is true when Departure, BM_Phase is not TNN, MRN is not empty, CLR is not empty and there are Annexes with Message Status SNT", true, nctsHeaderDeparture.RequiresAnnexes());

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("RequiresAnnexes is false when Arrival, even when BM_Phase is TNN", false, nctsHeaderArrival.RequiresAnnexes());
		});
	}

	public void TestAcceptanceDate()
	{
		var mrnEntryNumber = nctsHeader.MovementReferenceEntryNumber;
		var date = ZDateTime.Now;
		mrnEntryNumber.CE_IssueDate = date;
		AssertEquals(date, nctsHeader.AcceptanceDate);
	}

	public void TestSummaryEntryNumber()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		CombineAssertions(() =>
		{
			AssertEquals("CE_EntryNum", SummaryEntryNumber, nctsHeader.SummaryEntryNumber.CE_EntryNum);
			AssertEquals("CE_IssueDate", ZDateTime.Today, nctsHeader.SummaryEntryNumber.CE_IssueDate);
		});
	}

	public void TestReleaseDate()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		CombineAssertions(() =>
		{
			AssertEquals("CE_IssueDate", nctsHeader.SummaryEntryNumber.CE_IssueDate, nctsHeader.ReleaseDate);
		});
	}

	public void TestArrivalSummaryDeclaration()
	{
		AssignHeaderValuesForCreateEXS_Phase5();
		CombineAssertions(() =>
		{
			AssertEquals("CE_EntryNum", nctsHeader.SummaryEntryNumber.CE_EntryNum, nctsHeader.ArrivalSummaryDeclaration);
			AssertEquals("Actual value", SummaryEntryNumber, nctsHeader.ArrivalSummaryDeclaration);
		});
	}

	public void TestArrivalSummaryDeclarationUrl() => CombineAssertions(() =>
	{
		var summaryUrl = "https://url.com?Recinto=%recinto%&Anio=%anio%&Numero=%numero%&MRN=%mrn%";
		using (ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, summaryUrl))
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			AssertEquals($"No Summary Declaration", ZString.Empty, nctsHeader.ArrivalSummaryDeclarationUrl);

			var newEntryNumberSummary = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumberSummary.CE_EntryNum = "99982000174";
			newEntryNumberSummary.CE_EntryIsSystemGenerated = true;
			var newEntryNumberMrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumberMrn.CE_EntryNum = "20ES00999830001277";
			newEntryNumberMrn.CE_EntryIsSystemGenerated = true;
			Factory.Save();

			var expected = "https://url.com?Recinto=9998&Anio=2&Numero=000174&MRN=";
			AssertEquals("Summary Declaration length = 11", expected, nctsHeader.ArrivalSummaryDeclarationUrl);

			newEntryNumberSummary.CE_EntryNum = "9998200017";
			Factory.Save();
			AssertEquals($"Summary Declaration length < 11", ZString.Empty, nctsHeader.ArrivalSummaryDeclarationUrl);

			newEntryNumberSummary.CE_EntryNum = "999820001745";
			Factory.Save();
			expected = "https://url.com?Recinto=&Anio=&Numero=&MRN=999820001745";
			AssertEquals("Summary Declaration length > 11", expected, nctsHeader.ArrivalSummaryDeclarationUrl);
		}
	});

	public void TestArrivalSummaryDeclaration_Caption()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(nctsHeader.ArrivalSummaryDeclarationInfo);
			AssertEquals("Caption", "Arrival Summary Declaration", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Arr. SD Number", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Arr. SD", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Summary Declaration Number assigned to the Arrival by ES Customs", captionResourceString.FullDescription);
		});
	}

	public void TestCircuitStatus()
	{
		CodeDescriptionPairList circuitCodeList = Factory.GetCachedValue<CircuitCodeList>();
		var mrnEntryNumber = nctsHeader.MovementReferenceEntryNumber;

		mrnEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.GREEN;
		AssertEquals(circuitCodeList.GetDescriptionFromCode(CircuitCodeList.Codes.GREEN), nctsHeader.Circuit);

		mrnEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.RED;
		AssertEquals(circuitCodeList.GetDescriptionFromCode(CircuitCodeList.Codes.RED), nctsHeader.Circuit);

		mrnEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.ORANGE;
		AssertEquals(circuitCodeList.GetDescriptionFromCode(CircuitCodeList.Codes.ORANGE), nctsHeader.Circuit);

		mrnEntryNumber.CE_EntryStatus = "EXP";
		AssertEquals("EXP", nctsHeader.Circuit);
	}

	public void TestCircuit_Caption()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(nctsHeader.CircuitInfo);
			AssertEquals("Caption", "Circuit", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Circuit", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Circuit", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Circuit assigned by ES Customs", captionResourceString.FullDescription);
		});
	}

	public void TestClearOtherObjsWhenDeleting()
	{
		var newPivot = nctsHeader.EDocPivotCollection.AddNew();
		newPivot.CSD_DocType = "T1";
		var newPivot2 = nctsHeader.EDocPivotCollection.AddNew();
		newPivot2.CSD_DocType = "T2";

		CombineAssertions(() =>
		{
			var pivotParent = (INctsCusStorageDocPivotParent)nctsHeader;
			AssertNotNull("EDocPivotCollection is not null", pivotParent.EDocPivotCollection);
			AssertEquals("There are 2 documents in EDocPivotCollection", 2, pivotParent.EDocPivotCollection.Count);

			nctsHeader.Delete();
			pivotParent = nctsHeader;
			AssertEquals("EDocPivotCollection should have been deleted. Count should be 0", 0, pivotParent.EDocPivotCollection.Count);
		});
	}

	public void TestIsPhaseStatusTNN()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When header is departure and arrival and BM_Phase is empty in both MovementHeader and ArrivalMovementHeader", false, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
			AssertEquals("When header is departure and arrival and BM_Phase is TNN in MovementHeader", true, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("When header is departure and arrival and BM_Phase is not TNN in either MovementHeader or ArrivalMovementHeader", false, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("When header is departure and arrival and BM_Phase is TNN in ArrivalMovementHeader", true, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;

			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			AssertEquals("When header is departure and BM_Phase is not TNN in MovementHeader", false, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("When header is departure and BM_Phase is TNN in MovementHeader", true, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			AssertEquals("When header is arrival and BM_Phase is not TNN in ArrivalMovementHeader", false, nctsHeader.IsPhaseStatusTNN);

			nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("When header is arrival and BM_Phase is TNN in ArrivalMovementHeader", true, nctsHeader.IsPhaseStatusTNN);
		});
	}

	public void TestIsSent()
	{
		nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Failed;
		CombineAssertions(() =>
		{
			AssertEquals("When Message Status is not SNT, then false", false, nctsHeader.IsSent);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("When Message Status is SNT, then true", true, nctsHeader.IsSent);
		});
	}

	public void TestHeaderContainersLineNumberGenerator()
	{
		AssertType<NctsHeaderContainerSequenceNumberGenerator>("Sequence generator for HeaderContainers", nctsHeader.HeaderContainersLineNumberGenerator);
	}

	public void TestValidateConsignee()
	{
		var consignee = nctsHeader.Consignee;
		consignee.OrganisationPK = ZGuid.Empty;
		nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("When Consignee is not filled and Ncts is TNN and is not Phase5, no error expected", consignee.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.BM_Phase = ZString.Empty;
			consignee.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("When Consignee is not filled and Ncts is not TNN and is Phase5, no error expected", consignee.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			consignee.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("When Consignee is not filled and Ncts is TNN and is Phase5, error expected", consignee.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			var consigneeOrgHeader = Factory.New<OrgHeader>();
			consignee.OrganisationPK = consigneeOrgHeader.PK;
			AssertNoMessageErrorContaining("When Consignee is filled and Ncts is TNN and is Phase5, no error expected", consignee.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestSeals()
	{
		AssertEquals("Type", typeof(EU.NCTS.Business.SealCollection<Seal>), nctsHeader.Seals.GetType());
	}

	public void TestGuarantees_Phase5Departure()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		CombineAssertions(() =>
		{
			nctsHeader.GetEffectiveGuarantees().AddNew();
			var result = nctsHeader.MovementHeader.Guarantees;
			AssertEquals("Parent", nctsHeader.MovementHeader.PK, result[0].PW_ParentID);
			AssertType<EU.NCTS.Business.NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
		});
	}

	public void TestGuarantees_Phase4()
	{
		CombineAssertions(() =>
		{
			nctsHeader.GetEffectiveGuarantees().AddNew();
			var result = nctsHeader.GetEffectiveGuarantees();
			AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
			AssertType<EU.NCTS.Business.NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
		});
	}

	public void TestCustomsOfficesForDeparture_ListChangedTaxType()
	{
		SetUpTariffAndTax();

		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var movementHeader = header.MovementHeader;
		movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

		var bill1 = header.Bills.AddNew();
		var goodItem11 = bill1.GoodsItems.AddNew();
		goodItem11.BY_HarmonisedTariff = "65";
		var goodItem12 = bill1.GoodsItems.AddNew();
		goodItem12.BY_HarmonisedTariff = "65";

		var bill2 = header.Bills.AddNew();
		var goodItem2 = bill2.GoodsItems.AddNew();
		goodItem2.BY_HarmonisedTariff = "65";

		CombineAssertions(() =>
		{
			AssertEquals("[PreReq] Bill1 GoodItem1 TaxType is IV1 by default", "IV1", goodItem11.BY_ZZF_NKTaxType);
			AssertEquals("[PreReq] Bill1 GoodItem12 TaxType is IV1 by default", "IV1", goodItem11.BY_ZZF_NKTaxType);
			AssertEquals("[PreReq] Bill2 GoodItem2 TaxType is IV1 by default", "IV1", goodItem11.BY_ZZF_NKTaxType);

			var customsOffice = movementHeader.CustomsOfficesForDeparture.AddNew("DEP");
			customsOffice.CY_Data = "ES003551";

			AssertEquals("When Departure Custom offices change to Canary Island Bill1 GoodItem1 TaxType is IG1 by default", "IG1", goodItem11.BY_ZZF_NKTaxType);
			AssertEquals("When Departure Custom offices change to Canary Island Bill1 GoodItem12 TaxType is IG1 by default", "IG1", goodItem11.BY_ZZF_NKTaxType);
			AssertEquals("When Departure Custom offices change to Canary Island Bill2 GoodItem2 TaxType is IG1 by default", "IG1", goodItem11.BY_ZZF_NKTaxType);

			customsOffice.CY_Data = "ES009999";

			AssertEquals("When Departure Custom offices change to non Canary Island Bill1 GoodItem1 TaxType is IV1 by default", "IV1", goodItem11.BY_ZZF_NKTaxType);
			AssertEquals("When Departure Custom offices change to non Canary Island Bill1 GoodItem12 TaxType is IV1 by default", "IV1", goodItem11.BY_ZZF_NKTaxType);
			AssertEquals("When Departure Custom offices change to non Canary Island Bill2 GoodItem2 TaxType is IV1 by default", "IV1", goodItem11.BY_ZZF_NKTaxType);
		});
	}

	public void TestIsMovementReferenceNumberES()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Arrival);

		CombineAssertions(() =>
		{
			AssertEquals("MovementReferenceNumber is not ES cause is null", false, header.IsMovementReferenceNumberES);

			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "24ES00999830001277";
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			AssertEquals("MovementReferenceNumber is ES", true, header.IsMovementReferenceNumberES);

			newEntryNumber.CE_EntryNum = "24FR00999830001277";
			AssertEquals("MovementReferenceNumber is not ES", false, header.IsMovementReferenceNumberES);
		});
	}

	void SetUpTariffAndTax()
	{
		var countryCode = Core.Constants.CountryCodes.Spain;

		var helper = new EU.NCTS.Business.Testing.UniversalReferenceTestDataHelper(Factory);

		var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(countryCode, "Latvia", parentDataGrouping);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
		Factory.Save();

		var tariff = helper.CreateTariff(countryCode, tariffType.PK, "65", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroup, "EU");
		helper.CreateTaxOrFee("IV1", 0.21m, countryCode);
		helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV1");

		Factory.Save();
	}

	const string NctsHeaderTestFilePath = "Enterprise.Customs.ES.NCTS.Business.Testing.NCTS.NctsHeader.TestFiles";

	string GetNotWrittenOffTransitFromCustomsTestFileContents() => ESNctsTestFileReader.GetEmbeddedFileText(NctsHeaderTestFilePath, "NotWrittenOffTransitFromCustoms.txt");
	string GetWrittenOffTransitFromCustomsTestFileContent() => ESNctsTestFileReader.GetEmbeddedFileText(NctsHeaderTestFilePath, "WrittenOffTransitFromCustoms.txt");
	string GetWrittenOffWithoutDateTransitFromCustomsTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(NctsHeaderTestFilePath, "WrittenOffWithoutDateTransitFromCustoms.txt");

	NctsHeader SetDataForGetTransitStatusFromCustoms(ZString mrn, string responseFromUrl = "", bool throwException = false)
	{
		var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);

		var mockHeader = Factory.NewMoq<NctsHeader>();
		if (throwException)
		{
			mockHeader.Setup(m => m.CreateRequestAndGetResponse(It.IsAny<ZString>(), It.IsAny<GlbExternalPassword>())).Throws(new WebException());
		}
		else
		{
			mockHeader.Setup(m => m.CreateRequestAndGetResponse(It.IsAny<ZString>(), It.IsAny<GlbExternalPassword>())).Returns(responseFromUrl);
		}
		var mockHeaderObject = mockHeader.Object;

		mockHeaderObject.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
		mockHeaderObject.BH_JobReference = JobReference;
		var newEntryNumber = CusEntryNumber.LoadOrCreate(mockHeaderObject, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = mrn;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		mockHeaderObject.ArrivalMovementHeader.BM_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;
		mockHeaderObject.BH_CustomsProfile = staffWithCertificateHelperTest.Certificate.CertificateName;
		mockHeaderObject.LocalReferenceNumber = "AH3";

		return mockHeaderObject;
	}

	void SetUpGuarantee(string reference, string type, string mrnCode, decimal transactionValue, decimal balance)
	{
		var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_Number = reference;
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = type;
		guaranteeHeader.CPH_RN_NKCountryCode = CountryCodes.Spain;
		guaranteeHeader.CPH_Balance = balance;

		var oblTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		oblTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		oblTransaction.CPL_Reference = mrnCode;

		AddTransaction(guaranteeHeader, mrnCode, "First-CON", transactionValue, PermitTransactionStatusList.Codes.Confirmed);
		AddTransaction(guaranteeHeader, mrnCode, "Second-CON", 30m, PermitTransactionStatusList.Codes.Confirmed);
		AddTransaction(guaranteeHeader, mrnCode, "Third-CON", -90m, PermitTransactionStatusList.Codes.Confirmed);
		AddTransaction(guaranteeHeader, mrnCode, "Fourth-PEN", -40m, PermitTransactionStatusList.Codes.Pending);

		Factory.Save();
	}

	protected void AddTransaction(CusGuaranteeHeader guaranteeHeader, ZString mrnCode, ZString comment, ZDecimal value, ZString status)
	{
		var transaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction1.CPL_Reference = mrnCode;
		transaction1.CPL_TranValue = value;
		transaction1.CPL_TransactionStatus = status;
		transaction1.CPL_Comment = comment;
		transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
	}

	CusGuaranteeHeader[] LoadCusGuaranteeHeaderList()
	{
		var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
		return Factory.Load<CusGuaranteeHeader>(query);
	}

	void AssertWriteOffResult(WriteOffResult writeOffResult, string message, string jobNumber, string guaranteeStatus, string mrn = "", string declarationStatus = "-")
	{
		AssertEquals(message + ", JobNumber", jobNumber, writeOffResult.JobNumber);
		AssertEquals(message + ", Mrn", mrn, writeOffResult.Mrn);
		AssertEquals(message + ", GuaranteeStatus", guaranteeStatus, writeOffResult.GuaranteeStatus);
		AssertEquals(message + ", DeclarationStatus", declarationStatus, writeOffResult.DeclarationStatus);
	}

	public void AddSecurityData()
	{
		nctsHeader.MovementHeader.BM_BTAIndicator = "S";
		nctsHeader.MovementHeader.BM_MethodOfPayment = "P";
		nctsHeader.MovementHeader.BM_AdditionalText = "7";
		nctsHeader.MovementHeader.BM_ConveyanceNumber = "S7";
		nctsHeader.PlaceOfUnloadingCode = "UNLOD";
		nctsHeader.SecurityConsignor.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		nctsHeader.SecurityConsignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		nctsHeader.BH_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
		nctsHeader.BH_UniqueVoyageIdentifier = "112233";
	}

	ESEDIMessage SetEDIMessageAndGenPivot(NctsHeader header, NctsCusStorageDocPivot docPivot)
	{
		var message = Factory.New<ESEDIMessage>();
		header.Messages.Add(message);
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Sent;
		var messagePivot = Factory.New<GenPivot>();
		messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
		messagePivot.XX_Relation1ID = docPivot.PK;
		messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
		messagePivot.XX_Relation2ID = message.PK;
		messagePivot.XX_Relation2TableCode = message.TablePrefix;

		return message;
	}

	void CreateGuaranteeHeaderDetail(BusinessObjectFactory factory, ZString guaranteeCode, bool addOBLTransaction = true, bool withOldEndDate = false)
	{
		var holder = factory.NewWithValidTestData<OrgHeader>();

		var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_Number = guaranteeCode;
		guaranteeHeader.CPH_OH_PermitHolder = holder.PK;
		guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
		guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

		if (addOBLTransaction)
		{
			AddOBLTransaction(guaranteeHeader, 1000m);
		}

		if (withOldEndDate)
		{
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-10);
		}
	}

	void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value)
	{
		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = value;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
	}

	void AssertReleaseStatusOnChangedCollection_Add_Delete(NctsHeader nctsHeader, IBusinessObjectCollection collection, string collectionName)
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;
		AssertEquals("Prereq: Departure", NctsMovementType.Codes.Departure, nctsHeader.BH_HeaderType);
		AssertEquals("Prereq: ReleaseStatus empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

		_ = collection.AddNew();
		AssertEquals($"When CustomsStatus is not PRE and {collectionName} add new", ZString.Empty, nctsHeader.BH_ReleaseStatus);

		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;

		var item = collection.AddNew();
		AssertEquals($"When CustomsStatus is PRE and {collectionName} collection add new", "1", nctsHeader.BH_ReleaseStatus);

		nctsHeader.BH_ReleaseStatus = ZString.Empty;
		item.Delete();
		AssertEquals($"When CustomsStatus is PRE and {collectionName} collection delete one record", "1", nctsHeader.BH_ReleaseStatus);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
	}
	NctsHeader nctsHeader;

	NctsHeader GetPhase5Header(string movementType)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(movementType);

		return nctsHeader;
	}

	NctsHeader GetPhase4Header(string movementType)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(movementType);
		return nctsHeader;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var result = (NctsHeader)base.GetNewBusinessObjectForDeleteTest(factory);
		result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		_ = result.CustomsOffices.AddNew();
		return result;
	}

	const string MRNCode = "22ES00999950008716";
	const string JobReference = "Reference";

	const string DestinationCustomsOfficeCode = "ES009999";
	const string LocationOfGoodsCode = "LOCATIONCODE";
	const string LocalReferenceNumber = "AH3";

	const string UnloadingTariffNew = "2208905400";
	const string UnloadingDescriptionNew = "Desc New";
	const decimal UnloadingGrossWeightNew = 12;
	const decimal UnloadingNetWeightNew = 10;
	const int DeclarationGoodsItemNumberNew = 3;

	const string ArrivalTariffOthers = "2208905401";
	const string ArrivalDescriptionOthers = "Desc Other";
	const decimal ArrivalGrossWeightOthers = 13;
	const decimal ArrivalNetWeightOthers = 11;
	const int DeclarationGoodsItemNumberOthers = 6;

	const string UnloadingTariff = "2208905402";
	const string UnloadingDescription = "Desc Unloading";
	const decimal UnloadingGrossWeight = 22;
	const decimal UnloadingNetWeight = 20;
	const int DeclarationGoodsItemNumberDif = 4;

	const string LinePackagesUnitType = "MX";
	const int LinePackagesUnitCount = 5;

	const string LineContainerContainerNumber = "HXDU1234567";
	const string LineContainerContainerNumber1 = "HXDU1234568";

	const string LinePackagesMarksAndNumbers = "Marks";
	const string HeaderSupportingDocumentsType = "A001";
	const string HeaderSupportingDocumentsReferenceNumber = "ReferenceSupHeader";
	const string LineSupportingDocumentsType = "A001";
	const string LineSupportingDocumentsReferenceNumber = "ReferenceSup";
	const string LinePreviousDocumentsReferenceNumber = "99982000174";
	const string SupportingDocumentsTypeN380 = "N380";
	const string LineSupportingDocumentsReferenceNumberN380 = "ReferenceSupN380";
	const string InvoiceSupportingDocumentsReferenceNumberN380 = "InvReferenceSupN380";
	const string HeaderSupportingDocumentsReferenceNumberN380 = "HeadReferenceSupN380";

	const string SummaryEntryNumber = "99982000174";

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsHeader;

	protected override BusinessObject GetNewBusinessObject() => nctsHeader;
}
