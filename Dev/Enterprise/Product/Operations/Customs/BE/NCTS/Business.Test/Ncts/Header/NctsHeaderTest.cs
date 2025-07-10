using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
sealed class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
{
	public void TestCanCancel() => CombineAssertions(() =>
	{
		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType("A");
		var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;
		var departureHeader = Factory.New<NctsHeader>();
		departureHeader.SetMovementType("D");
		var departureMovementHeader = departureHeader.MovementHeader;
		var message = "Cannot deactivate when Customs Status is ";

		var listArrivalCustomsStatuses = new List<string> { "CL1", "CL3", "CD2", "CD4", "UAP", "ULR" };
		var listDepartureCustomsStatuses = new List<string> { "ACK", "AMR", "CAN", "CAR", "CO0", "CO1", "CO2", "DIS", "ENQ", "GIV", "INC", "MRN", "NRL", "PRE", "REL", "RFA", "RFR", "URP", "WRO" };

		foreach (var customsStatus in new NctsTransitStatusList().GetAllCodes())
		{
			arrivalMovementHeader.BM_CustomsStatus = customsStatus;
			departureMovementHeader.BM_CustomsStatus = customsStatus;

			AssertEquals($"Arrival with CustomsStatus {customsStatus}", listArrivalCustomsStatuses.Contains(customsStatus) ? $"{message}{customsStatus}." : null, arrivalHeader.CanCancel());
			AssertEquals($"Departure with CustomsStatus {customsStatus}", listDepartureCustomsStatuses.Contains(customsStatus) ? $"{message}{customsStatus}." : null, departureHeader.CanCancel());
		}
	});

	public void TestSetDefaults_CommunicationLanguage()
	{
		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.Basque;
			var header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is not in the list, so the Communication Language should be blank.", string.Empty, header.BH_CommunicationLanguage);

			GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.Dutch;
			header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is NL-NL, so the Communication Language should be defaulted to NL.", Core.Constants.CountryCodes.Netherlands, header.BH_CommunicationLanguage);

			GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.French;
			header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is FR-FR, so the Communication Language should be defaulted to FR.", Core.Constants.CountryCodes.France, header.BH_CommunicationLanguage);

			GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.German;
			header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is DE-DE, so the Communication Language should be defaulted to DE.", Core.Constants.CountryCodes.Germany, header.BH_CommunicationLanguage);

			GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.English;
			header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is EN, so the Communication Language should be defaulted to EN.", SharedConstants.Languages.English, header.BH_CommunicationLanguage);
		});
	}

	public void TestLookups()
	{
		AssertType<NctsHeaderLookups>(header.Lookups);
	}

	public void TestIncidents()
	{
		AssertType<EnRouteIncidentCollection>(header.EnRouteIncidents);
	}

	public void TestBills()
	{
		AssertType<NctsBillCollection<NctsBill>>(header.Bills);
	}

	public void TestValidation()
	{
		AssertType<NctsHeaderPhase5Validation>(header.Validation);
	}

	public void TestMovementHeader()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType(typeof(NctsDepartureMovementHeader), header.MovementHeader);
	}

	public void TestHeaderContainers()
	{
		AssertType<NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, EU.NCTS.Business.NctsHeader>>(header.DepartureHeaderContainers);
	}

	public void TestCorrelationIdentifierEntryNumber_Arrival()
	{
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var entryNumber = CusEntryNumber.LoadOrCreate(header.ArrivalMovementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, Core.Constants.CountryCodes.Belgium);
		entryNumber.CE_EntryNum = "XXXXX";

		AssertEquals("XXXXX", header.CorrelationIdentifierEntryNumber.CE_EntryNum);
	}

	public void TestCorrelationIdentifierEntryNumber_Departure()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var entryNumber = CusEntryNumber.LoadOrCreate(header.MovementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, Core.Constants.CountryCodes.Belgium);
		entryNumber.CE_EntryNum = "XXXXX";

		AssertEquals("XXXXX", header.CorrelationIdentifierEntryNumber.CE_EntryNum);
	}

	public void TestPreviousDocuments()
	{
		AssertType<CommonPreviousDocumentCollection<CommonPreviousDocument>>(header.PreviousDocuments);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes();
		CombineAssertions(() =>
		{
			AssertEquals("#CusSupportingInfoTypes", 3, cusSupportingInfoTypes.Count);
			AssertEquals("PRE", typeof(CommonPreviousDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			AssertEquals("OTH", typeof(NctsAdditionalInfo), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("IRD", typeof(RequestedDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument]);
		});
	}

	public void TestConfiguration()
	{
		AssertType<NctsConfiguration>(header.Configuration);
	}

	public void TestEventExists()
	{
		CombineAssertions(() =>
		{
			header.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: DateTime.Now, reference: "REL"));
			AssertEquals("Event REL should exist", true, header.HasLogWith(Events.CustomsEntryStatus.Code, "REL"));
			AssertEquals("Event XYZ should not exist", false, header.HasLogWith(Events.CustomsEntryStatus.Code, "XYZ"));
		});
	}

	public void TestDocumentSupporter()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
		AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
	}

	public void TestNotUpdateAuthorizationDataWhenDestinationTraderChanged()
	{
		var org1 = Factory.New<OrgHeader>();
		var org2 = Factory.New<OrgHeader>();

		CombineAssertions(() =>
		{
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals("phase4Arrival, ArrivalMovementHeader.AuthorizationOwner won't be updated", ZGuid.Empty, header.ArrivalMovementHeader.AuthorizationOwner);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.DestinationTrader.E2_OA_Address = org1.MainAddress.PK;
			AssertEquals("When phase5Arrival and flag false (BE), ArrivalMovementHeader.AuthorizationOwner is NOT set with DestinationTrader's org (org1)", ZGuid.Empty, header.ArrivalMovementHeader.AuthorizationOwner);

			header.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals("When phase5Arrival and flag false (BE), ArrivalMovementHeader.AuthorizationOwner is NOT set with DestinationTrader's org (org2)", ZGuid.Empty, header.ArrivalMovementHeader.AuthorizationOwner);

			header.ArrivalMovementHeader.AuthorizationOwner = org1.PK;
			header.DestinationTrader.E2_OA_Address = ZGuid.Empty;
			AssertEquals("When phase5Arrival and flag false (BE), ArrivalMovementHeader.AuthorizationOwner is NOT set with DestinationTrader's org (empty)", org1.PK, header.ArrivalMovementHeader.AuthorizationOwner);
		});
	}

	public void TestCloneAdditionalDocuments()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
		additionalInfo1.CSI_Code = "4008";
		additionalInfo1.CSI_SubType = "REF";

		var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
		additionalInfo2.CSI_Code = "1234";
		additionalInfo2.CSI_SubType = "REF";

		var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
		additionalInfo3.CSI_Code = "4009";
		additionalInfo3.CSI_SubType = "INF";

		var clonedHeader = (NctsHeader)nctsHeader.Clone();

		CombineAssertions(() => {
			AssertEquals("AdditionalInfo with different CSI_Code should be included.", clonedHeader.AdditionalDocuments.Any(info => info.CSI_Code == "1234"), true);
			AssertEquals("AdditionalInfo with different CSI_SubType should be included.", clonedHeader.AdditionalDocuments.Any(info => info.CSI_SubType == "INF"), true);
			AssertEquals("Filtered AdditionalInfo should be excluded.", clonedHeader.AdditionalDocuments.Any(info => (info.CSI_Code == "4008" || info.CSI_Code == "4009") && info.IsAnAdditionalReference), false);
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		return Factory.New<NctsHeader>();
	}

	public static NctsHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		return header;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
	}
	NctsHeader header;
}
