using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseSeaCargoReportHeaderTest : TestCaseWithFactory
	{
		public void TestICS()
		{
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "Test 1";
			consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			House.CA_OA_ConsigneeAddress = consignee1.MainAddress.PK;
			AssertEquals("", Header.ConsigneeABN);
			AssertEquals("", Header.ConsigneeCAC);
			AssertEquals("12345678901", Header.ConsigneeIdentifier);

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Test 2";
			consignee2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");

			House.CA_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			AssertEquals("12345678901", Header.ConsigneeABN);
			AssertEquals("123", Header.ConsigneeCAC);
			AssertEquals("", Header.ConsigneeIdentifier);

			var consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "Test 3";
			consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			House.CA_OA_ConsigneeAddress = consignee3.MainAddress.PK;
			AssertEquals("", Header.ConsigneeABN);
			AssertEquals("", Header.ConsigneeCAC);
			AssertEquals("12345678901", Header.ConsigneeIdentifier);

			AssertEquals("", Header.ConsignorIdentifier);
			AssertEquals("", Header.ConsignorVendor);

			var consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "Test 1";
			House.CA_OA_ConsignorAddress = consignor1.MainAddress.PK;
			AssertEquals("", Header.ConsignorIdentifier);
			AssertEquals("", Header.ConsignorVendor);

			var consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "Test 2";
			consignor2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			consignor2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");
			House.CA_OA_ConsignorAddress = consignor2.MainAddress.PK;
			AssertEquals("12345678901", Header.ConsignorIdentifier);
			AssertEquals("123", Header.ConsignorVendor);
		}

		public void TestMethodOfPayment()
		{
			House.CA_PrepaidCollectOther = "ABC";
			AssertEquals("MethodOfPayment", "ABC", Header.MethodOfPayment);
		}

		public void TestOrigin()
		{
			House.CA_RL_NK_PortOfOrigin = "NZAKL";
			AssertEquals("Origin", "NZAKL", Header.Origin);
		}

		public void TestDestination()
		{
			House.CA_RL_NK_PortOfDestination = "AUSYD";
			AssertEquals("Destination", "AUSYD", Header.Destination);
		}

		public void TestLoading()
		{
			OceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("Loading", "NZAKL", Header.Loading);
		}

		public void TestRoutings()
		{
			AssertNull(Header.Routings);
		}

		public void TestDischarge()
		{
			OceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("Discharge", "AUSYD", Header.Discharge);
		}

		public void TestFirstArrivalPort()
		{
			OceanBill.CB_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("FirstArrivalPort", "AUSYD", Header.FirstArrivalPort);
		}

		public void TestConsignee()
		{
			House.CA_OA_ConsigneeAddress = CreateOrganisation("LOC DAWG").MainAddress.PK;
			AssertEquals("Consignee", "LOC DAWG", Header.ConsigneeName);
		}

		public void TestConsigneeStreet1()
		{
			House.CA_ConsigneeAddress1 = "Address1";
			AssertEquals("Consignee Address1", "Address1", Header.ConsigneeStreet);
		}

		public void TestConsigneeStreet2()
		{
			House.CA_ConsigneeAddress2 = "Address2";
			AssertEquals("Consignee Address2", "Address2", Header.ConsigneeStreet2);
		}

		public void TestConsigneeSuburb()
		{
			House.CA_ConsigneeSuburb = "Suburb";
			AssertEquals("Consignee Suburb", "Suburb", Header.ConsigneeCity);
		}

		public void TestConsigneePostCode()
		{
			House.CA_ConsigneePostcode = "2222";
			AssertEquals("Consignee PostCode", "2222", Header.ConsigneePostCode);
		}

		public void TestConsignor()
		{
			House.CA_OA_ConsignorAddress = CreateOrganisation("LOC DAWG").MainAddress.PK;
			AssertEquals("Consignor", "LOC DAWG", Header.ConsignorName);
		}

		public void TestConsignorStreet1()
		{
			House.CA_ConsignorAddress1 = "Address1";
			AssertEquals("Consignor Address1", "Address1", Header.ConsignorStreet);
		}

		public void TestConsignorStreet2()
		{
			House.CA_ConsignorAddress2 = "Address2";
			AssertEquals("Consignor Address2", "Address2", Header.ConsignorStreet2);
		}

		public void TestConsignorSuburb()
		{
			House.CA_ConsignorSuburb = "Suburb";
			AssertEquals("Consignor Suburb", "Suburb", Header.ConsignorCity);
		}

		public void TestConsignorPostCode()
		{
			House.CA_ConsignorPostcode = "2222";
			AssertEquals("Consignor PostCode", "2222", Header.ConsignorPostCode);
		}

		public void TestOceanBill()
		{
			OceanBill.CB_OceanBill = "135";
			AssertEquals("OceanBill", "135", Header.OceanBill);
		}

		public void TestHouseBill()
		{
			House.CA_HouseBill = "531";
			AssertEquals("HouseBill", "531", Header.HouseBill);
		}

		public void TestParentBill()
		{
			House.CA_MasterHouseBill = "246";
			AssertEquals("HouseBill", "246", Header.ParentBill);
		}

		public void TestParentBillCanComeFromHeader()
		{
			OceanBill.CB_MasterHouseBill = "987";
			AssertEquals("HouseBill", "987", Header.ParentBill);
		}

		public void TestParentBillComeFromLineBeforeHeader()
		{
			OceanBill.CB_MasterHouseBill = "987";
			House.CA_MasterHouseBill = "246";
			AssertEquals("HouseBill", "246", Header.ParentBill);
		}

		public void TestVoyage()
		{
			OceanBill.CB_Voyage = "109";
			AssertEquals("Voyage", "109", Header.Voyage);
		}

		public void TestLloydsNumber()
		{
			OceanBill.CB_VesselName = "ADMIRalENGRACHT";
			AssertEquals("LloydsNumber", "8811924", Header.LloydsNumber);
		}

		public void TestNotifyParty()
		{
			House.CA_OH_Notify = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("NotifyParty ", House.Notify, Header.NotifyParty);
		}

		public void TestOriginCountry()
		{
			House.CA_RN_NKGoodsOrigin = "US";
			AssertEquals("OriginCountry ", "US", Header.OriginCountry);
		}

		public void TestCanDelaySending()
		{
			using (AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 48))
			{
				OceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
				OceanBill.CB_DateOfArrival = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", ZDateTime.UtcNow.AddHours(49).ToDateTime());
				AssertEquals(true, Header.CanDelaySending);
				OceanBill.CB_RL_NKPortOfDischarge = "";
				AssertEquals(false, Header.CanDelaySending);
				OceanBill.CB_RL_NKPortOfDischarge = "ZZD#@";
				AssertEquals(false, Header.CanDelaySending);
				OceanBill.CB_RL_NKPortOfDischarge = "AUMEL";
				AssertEquals(true, Header.CanDelaySending);
				OceanBill.CB_DateOfArrival = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUMEL", ZDateTime.UtcNow.AddHours(47).ToDateTime());
				AssertEquals(false, Header.CanDelaySending);
			}
		}

		public void TestIsConsolidation()
		{
			House.CA_IsMasterHouse = true;
			AssertEquals("IsConsolidation ", true, Header.IsConsolidation);
		}

		public void TestIsBureau()
		{
			House.OceanBill.CB_IsBureau = true;
			AssertEquals("Is Bureau", true, Header.IsBureau);
		}

		public void TestBusinessObject()
		{
			AssertEquals("BusinessObject ", House, Header.BusinessObject);
		}

		public void TestLines()
		{
			AssertEquals("Lines.Length", 0, Header.Lines.Length);
			OceanBill.Containers.AddNew();
			House.Pivot.AddNew();
			House.Pivot[0].CV_CN = OceanBill.Containers[0].PK;
			AssertEquals("Lines.Length", 1, Header.Lines.Length);
			AssertEquals("Lines[0].GetType()", typeof(CusSCAPivotSeaCargoReportLine), Header.Lines[0].GetType());
		}

		public void TestLinesDoesntIncludeContainerlessPivots()
		{
			AssertEquals("Lines.Length", 0, Header.Lines.Length);
			OceanBill.Containers.AddNew();
			House.Pivot.AddNew();
			AssertEquals("Lines.Length", 0, Header.Lines.Length);
			House.Pivot[0].CV_CN = OceanBill.Containers[0].PK;
			AssertEquals("Lines.Length", 1, Header.Lines.Length);
		}

		public void TestDatabaseLines()
		{
			OceanBill.Containers.AddNew();
			House.Pivot.AddNew();
			House.Pivot[0].CV_CN = OceanBill.Containers[0].PK;
			AssertEquals("DatabaseLines.Length", 0, Header.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("DatabaseLines.Length", 1, Header.DatabaseLines.Length);
			AssertEquals("DatabaseLines[0].GetType()", typeof(CusSCAPivotSeaCargoReportLine), Header.DatabaseLines[0].GetType());
		}

		public void TestUnmatchedOrgSendsTheCorrectInfoFromNotes()
		{
			var shipment = GetShipmentWithConsol();
			string noteText = @"Organisation matching failed to find one or more organisations during data import. Details of the unmatched organisations are shown below:

Consignee:-
NAME: ACTROL INDUSTRIES PTY LTD
ADDRESS: TRANDING AS ACTROL PARTS, 19 KING STREET, BLACKBURN, MELBOURNE, 3130 VICTORIA

Consignor:-
NAME: ADVANCED DIESELS LTD
ADDRESS: 19 MERIDA PLACE, GLENFIELD, AUCKLAND";
			var note = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, noteText);
			Factory.Save();

			var synchroniser = new CMRSeaCargoSynchroniser(shipment.Consols[0]);
			var houseBill = synchroniser.GetHouseBill(shipment);
			var header = new CusSCAHouseSeaCargoReportHeader(houseBill);
			AssertEquals("ACTROL INDUSTRIES PTY LTD", header.ConsigneeName);
			AssertEquals("TRANDING AS ACTROL PARTS, 19 KING STREET, BLACKBURN, MELBOURNE, 3130 VICTORIA", header.ConsigneeGeneralAddress);
			AssertEquals("ADVANCED DIESELS LTD", header.ConsignorName);
			AssertEquals("19 MERIDA PLACE, GLENFIELD, AUCKLAND", header.ConsignorGeneralAddress);
		}

		public void TestUnmatchedOrgSendsTheCorrectInfoFromSerializableNotes()
		{
			ForwardingShipment shipment = GetShipmentWithConsol();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, UnmatchOrgRecords.AsXml());
			Factory.Save();

			var synchroniser = new CMRSeaCargoSynchroniser(shipment.Consols[0]);
			var houseBill = synchroniser.GetHouseBill(shipment);
			var header = new CusSCAHouseSeaCargoReportHeader(houseBill);
			AssertEquals("ACTROL INDUSTRIES PTY LTD", header.ConsigneeName);
			AssertEquals("TRANDING AS ACTROL PARTS 19 KING STREET, BLACKBURN MELBOURNE 3130 VICTORIA", header.ConsigneeGeneralAddress);
			AssertEquals("ADVANCED DIESELS LTD", header.ConsignorName);
			AssertEquals("19 MERIDA PLACE GLENFIELD 1313 AUCKLAND", header.ConsignorGeneralAddress);
		}

		public void TestConsigneeNameAndAddress()
		{
			House.CA_ConsigneeAddress1 = "Address1";
			House.CA_ConsigneeAddress2 = "Address2";
			House.CA_ConsigneeSuburb = "";
			House.CA_ConsigneePostcode = "PostCode";
			House.CA_RN_NKConsigneeCountryCode = "AU";
			AssertEquals("Address1 Address2  PostCode AU", Header.ConsigneeGeneralAddress);
		}

		public void TestConsigneeNameAndAddressConditionality()
		{
			House.CA_ConsigneeAddress1 = "x";
			House.CA_ConsigneeAddress2 = "x";
			House.CA_ConsigneeSuburb = "x";
			House.CA_ConsigneePostcode = "x";
			House.CA_RN_NKConsigneeCountryCode = "x";
			AssertEquals(false, Header.ConsigneeGeneralAddress.IsEmpty);

			AssertConsigneeGeneralAddress(House.CA_ConsigneeAddress1Info);
			AssertConsigneeGeneralAddress(House.CA_ConsigneeAddress2Info);
			AssertConsigneeGeneralAddress(House.CA_ConsigneeSuburbInfo);
			AssertConsigneeGeneralAddress(House.CA_ConsigneePostcodeInfo);
			AssertConsigneeGeneralAddress(House.CA_RN_NKConsigneeCountryCodeInfo);
		}

		public void TestConsignorNameAndAddress()
		{
			House.CA_ConsignorAddress1 = "Address1";
			House.CA_ConsignorAddress2 = "Address2";
			House.CA_ConsignorSuburb = "";
			House.CA_ConsignorPostcode = "PostCode";
			House.CA_RN_NKConsignorCountryCode = "CN";
			AssertEquals("Address1 Address2  PostCode CN", Header.ConsignorGeneralAddress);
		}

		public void TestConsignorNameAndAddressConditionality()
		{
			House.CA_ConsignorAddress1 = "x";
			House.CA_ConsignorAddress2 = "x";
			House.CA_ConsignorSuburb = "x";
			House.CA_ConsignorPostcode = "x";
			House.CA_RN_NKConsignorCountryCode = "x";
			AssertEquals(false, Header.ConsignorGeneralAddress.IsEmpty);

			AssertConsignorGeneralAddress(House.CA_ConsignorAddress1Info);
			AssertConsignorGeneralAddress(House.CA_ConsignorAddress2Info);
			AssertConsignorGeneralAddress(House.CA_ConsignorSuburbInfo);
			AssertConsignorGeneralAddress(House.CA_ConsignorPostcodeInfo);
			AssertConsignorGeneralAddress(House.CA_RN_NKConsignorCountryCodeInfo);
		}

		public void TestConsigneeName()
		{
			House.CA_ConsigneeName = "Name";
			AssertEquals("Name", Header.ConsigneeName);
		}

		public void TestConsigneeCountry()
		{
			House.CA_RN_NKConsigneeCountryCode = "NZ";
			AssertEquals("NZ", Header.ConsigneeCountry);
		}

		public void TestConsignorName()
		{
			House.CA_ConsignorName = "Name";
			AssertEquals("Name", Header.ConsignorName);
		}

		public void TestConsignorCountry()
		{
			House.CA_RN_NKConsignorCountryCode = "US";
			AssertEquals("US", Header.ConsignorCountry);
		}

		public void TestResponsiblePartyID()
		{
			OceanBill.CB_ResponsiblePartyID = ABN1;
			OceanBill.CB_PrincipalID = ABN2;
			AssertEquals("Responsible Party ID", ABN1, Header.ResponsiblePartyID);
		}

		public void TestResponsiblePartyIDFromHouse()
		{
			OceanBill.CB_ResponsiblePartyID = ABN1;
			House.CA_ResponsiblePartyID = ABN2;
			AssertEquals("Responsible Party ID", ABN2, Header.ResponsiblePartyID);
			House.CA_ResponsiblePartyID = ZString.Empty;
			AssertEquals("Responsible Party ID", ABN1, Header.ResponsiblePartyID);
		}

		public void TestPrincipalID()
		{
			OceanBill.CB_ResponsiblePartyID = ABN1;
			OceanBill.CB_PrincipalID = ABN2;
			AssertEquals("Principal ID", ABN2, Header.PrincipalID);
		}

		public void TestMissingOceanBillNullReferenceException_I00009536()
		{
			var houseWithoutOceanBill = Factory.New<CusSCAHouse>();
			CusSCAHouseSeaCargoReportHeader header = new CusSCAHouseSeaCargoReportHeader(houseWithoutOceanBill, House.Messages);
			AssertNotNull(header.OceanBill);
			AssertNotNull(header.Loading);
			AssertNotNull(header.Discharge);
			AssertNotNull(header.FirstArrivalPort);
			AssertNotNull(header.Voyage);
			AssertNotNull(header.LloydsNumber);
			AssertNotNull(header.ResponsiblePartyID);
			AssertNotNull(header.IsBureau);
			AssertNotNull(header.PrincipalID);
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		ISeaCargoReportHeader header;
		ISeaCargoReportHeader Header => header ?? (header = new CusSCAHouseSeaCargoReportHeader(House));

		CusSCAHouse house;
		CusSCAHouse House => house ?? (house = OceanBill.HouseBills.AddNew());

		CusSCAOceanBill oceanBill;
		CusSCAOceanBill OceanBill => oceanBill ?? (oceanBill = Factory.New<CusSCAOceanBill>());

		OrgHeader CreateOrganisation(ZString name)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			return result;
		}

		ForwardingShipment GetShipmentWithConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_AgentType = "AGT";
			consol.JK_MasterBillNum = "08165431365";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "CuckooSqueaker";
			shipment.ConsigneePK = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			shipment.ConsignorPK = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

			return shipment;
		}

		UnmatchOrgRecords unmatchOrgRecords;
		UnmatchOrgRecords UnmatchOrgRecords
		{
			get
			{
				if (unmatchOrgRecords == null)
				{
					unmatchOrgRecords = new UnmatchOrgRecords(Factory);
					unmatchOrgRecords.OrgDetailsList = new UnmatchOrgRecord[]
					{
						ConsigneeRec,
						ConsignorRec
					};
				}
				return unmatchOrgRecords;
			}
		}

		UnmatchOrgRecord consigneeRec;
		UnmatchOrgRecord ConsigneeRec
		{
			get
			{
				if (consigneeRec == null)
				{
					consigneeRec = CreateUnmatchOrgRecord(OrganisationTypes.Consignee,
						nameof(OrganisationTypes.Consignee),
						"TRANDING AS ACTROL PARTS",
						"19 KING STREET, BLACKBURN",
						"ACTROL INDUSTRIES PTY LTD",
						"3130",
						"VICTORIA",
						"MELBOURNE",
						"consignee",
						"CNEOwnerCode");
				}
				return consigneeRec;
			}
		}

		UnmatchOrgRecord consignorRec;
		UnmatchOrgRecord ConsignorRec
		{
			get
			{
				if (consignorRec == null)
				{
					consignorRec = CreateUnmatchOrgRecord(OrganisationTypes.Consignor,
						nameof(OrganisationTypes.Consignor),
						"19 MERIDA PLACE",
						"",
						"ADVANCED DIESELS LTD",
						"1313",
						"AUCKLAND",
						"GLENFIELD",
						"consignor",
						"CNRownerCode");
				}
				return consignorRec;
			}
		}

		UnmatchOrgRecord CreateUnmatchOrgRecord(OrganisationTypes orgType, ZString orgSubType, ZString addressLine1, ZString addressLine2, ZString orgName, ZString postCode, ZString state, ZString city, ZString ediCode, ZString ownerCode)
		{
			return new UnmatchOrgRecord()
			{
				OrganisationType = orgType.ToString(),
				AddressLine1 = addressLine1,
				AddressLine2 = addressLine2,
				OrganisationName = orgName,
				PostCode = postCode,
				StateOrProvince = state,
				OrganisationSubType = orgSubType,
				OwnerCode = ownerCode,
				EDICode = ediCode,
				City = city
			};
		}

		void AssertConsigneeGeneralAddress(ZPropertyInfo info)
		{
			info.Value = ZString.Empty;
			AssertEquals(false, Header.ConsigneeGeneralAddress.IsEmpty);
			info.Value = new ZString("x");
			AssertEquals(false, Header.ConsigneeGeneralAddress.IsEmpty);
		}

		void AssertConsignorGeneralAddress(ZPropertyInfo info)
		{
			info.Value = ZString.Empty;
			AssertEquals(false, Header.ConsignorGeneralAddress.IsEmpty);
			info.Value = new ZString("x");
			AssertEquals(false, Header.ConsignorGeneralAddress.IsEmpty);
		}

		const string ABN1 = "36103224237";
		const string ABN2 = "83058313205";
	}
}
