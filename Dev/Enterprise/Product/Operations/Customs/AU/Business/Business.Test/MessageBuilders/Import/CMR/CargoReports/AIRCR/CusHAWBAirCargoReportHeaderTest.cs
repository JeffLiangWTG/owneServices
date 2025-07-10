using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusHAWBAirCargoReportHeaderTest : TestCaseWithFactory
	{
		public void TestICSRelease()
		{
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "Test 1";
			consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			HAWB.CS_OA_ConsigneeAddress = consignee1.MainAddress.PK;
			AssertEquals("", Header.ConsigneeABN);
			AssertEquals("", Header.ConsigneeCAC);
			AssertEquals("12345678901", Header.ConsigneeIdentifier);

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Test 2";
			consignee2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");

			HAWB.CS_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			AssertEquals("12345678901", Header.ConsigneeABN);
			AssertEquals("123", Header.ConsigneeCAC);
			AssertEquals("", Header.ConsigneeIdentifier);

			var consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "Test 3";
			consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			HAWB.CS_OA_ConsigneeAddress = consignee3.MainAddress.PK;
			AssertEquals("", Header.ConsigneeABN);
			AssertEquals("", Header.ConsigneeCAC);
			AssertEquals("12345678901", Header.ConsigneeIdentifier);

			var consignee4 = Factory.New<OrgHeader>();
			consignee4.OH_Code = "Test 4";
			consignee4.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "123456789");

			HAWB.CS_OA_ConsigneeAddress = consignee4.MainAddress.PK;
			AssertEquals("123456789", Header.ConsigneeABN);
			AssertEquals("", Header.ConsigneeCAC);
			AssertEquals("", Header.ConsigneeIdentifier);

			AssertEquals("", Header.ConsignorIdentifier);
			AssertEquals("", Header.ConsignorVendor);

			var consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "Test 1";
			HAWB.CS_OA_ConsignorAddress = consignor1.MainAddress.PK;
			AssertEquals("", Header.ConsignorIdentifier);
			AssertEquals("", Header.ConsignorVendor);

			var consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "Test 2";
			consignor2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			consignor2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");
			HAWB.CS_OA_ConsignorAddress = consignor2.MainAddress.PK;
			AssertEquals("12345678901", Header.ConsignorIdentifier);
			AssertEquals("123", Header.ConsignorVendor);
		}

		public void TestMethodOfPayment()
		{
			HAWB.CS_FreightPrepaidCollect = "ABC";
			AssertEquals("MethodOfPayment", "ABC", Header.MethodOfPayment);
		}

		public void TestCanDelaySending()
		{
			MAWB.CM_RL_NKDischargePort = "AUSYD";
			MAWB.CM_ArrivalDate = ZDateTime.UtcNow.AddDays(3);
			AssertEquals(true, Header.CanDelaySending);
			MAWB.CM_RL_NKDischargePort = "";
			AssertEquals(false, Header.CanDelaySending);
			MAWB.CM_RL_NKDischargePort = "ZZD#@";
			AssertEquals(false, Header.CanDelaySending);
			MAWB.CM_RL_NKDischargePort = "AUMEL";
			AssertEquals(true, Header.CanDelaySending);
			MAWB.CM_ArrivalDate = ZDateTime.UtcNow.AddDays(-1);
			AssertEquals(false, Header.CanDelaySending);
		}

		public void TestOrigin()
		{
			HAWB.CS_RL_NKOrigin = "ABC";
			AssertEquals("Origin", "ABC", Header.Origin);
		}

		public void TestDestination()
		{
			HAWB.CS_RL_NKDestination = "ABC";
			AssertEquals("Destination", "ABC", Header.Destination);
		}

		public void TestResponsiblePartyID()
		{
			const string ABN1 = "8439283442";
			const string ABN2 = "83058313205";
			MAWB.CM_ResponsiblePartyID = ABN1;
			HAWB.CS_ResponsiblePartyID = ABN2;
			AssertEquals(ABN2, Header.ResponsiblePartyID);
			HAWB.CS_ResponsiblePartyID = ZString.Empty;
			AssertEquals(ABN1, Header.ResponsiblePartyID);
		}

		public void TestResposniblePartyIDFromHAWB()
		{
			MAWB.CM_ResponsiblePartyID = "8439283442";
			AssertEquals("8439283442", Header.ResponsiblePartyID);
		}

		public virtual void TestRoutings()
		{
			HAWB.MAWB.CM_RL_NKRoutePort1 = "SGSIN";
			HAWB.MAWB.CM_RL_NKRoutePort2 = "NZAKL";

			AssertEquals("2 Routings", 2, Header.Routings.Length);
			AssertCollectionContains("SGSIN", Header.Routings);
			AssertCollectionContains("NZAKL", Header.Routings);

			HAWB.MAWB.CM_RL_NKRoutePort3 = "AUMEL";
			AssertEquals("3 Routings", 3, Header.Routings.Length);
			AssertCollectionContains("SGSIN", Header.Routings);
			AssertCollectionContains("NZAKL", Header.Routings);
			AssertCollectionContains("AUMEL", Header.Routings);
		}

		public virtual void TestLoading()
		{
			HAWB.MAWB.CM_RL_NKLoadPort = "ABC";
			AssertEquals("Loading", "ABC", Header.Loading);
		}

		public virtual void TestDischarge()
		{
			HAWB.MAWB.CM_RL_NKDischargePort = "ABC";
			AssertEquals("Discharge", "ABC", Header.Discharge);
		}

		public virtual void TestIsHVLVSpecialReporter()
		{
			HAWB.CS_IsSpecialReporter = true;
			AssertEquals("Special Reporter", true, Header.IsHVLVSpecialReporter);
			HAWB.CS_IsSpecialReporter = false;
			AssertEquals("Special Reporter", false, Header.IsHVLVSpecialReporter);
		}

		public virtual void TestIsRemailSpecialReporter()
		{
			HAWB.CS_IsRemailReporter = true;
			AssertEquals("Remail Special Reporter", true, Header.IsRemailSpecialReporter);
			HAWB.CS_IsRemailReporter = false;
			AssertEquals("Remail Special Reporter", false, Header.IsRemailSpecialReporter);
		}

		public void TestFirstArrivalPort()
		{
			HAWB.MAWB.CM_RL_NKFirstArrivalPort = "ABC";
			HAWB.CS_RL_NKDestination = "USLAX";
			AssertEquals("FirstArrivalPort", "ABC", Header.FirstArrivalPort);
		}

		public void TestFirstArrivalPortIsNotIncluded()
		{
			HAWB.MAWB.CM_RL_NKFirstArrivalPort = "ABC";
			HAWB.MAWB.CM_RL_NKDischargePort = "SGSIN";

			HAWB.CS_RL_NKDestination = "ABC";
			AssertEquals("FirstArrivalPort", "ABC", Header.FirstArrivalPort);

			HAWB.CS_RL_NKDestination = "AUSYD";
			AssertEquals("FirstArrivalPort", "", Header.FirstArrivalPort);

			HAWB.CS_RL_NKDestination = "USLAX";
			AssertEquals("FirstArrivalPort", "ABC", Header.FirstArrivalPort);
		}

		public void TestConsignee()
		{
			HAWB.CS_OA_ConsigneeAddress = Factory.New<OrgHeader>().MainAddress.PK;
			HAWB.Consignee.OH_FullName = "Senior";
			AssertEquals("Consignee", "Senior", Header.ConsigneeName);
		}

		public void TestConsignor()
		{
			HAWB.CS_OA_ConsignorAddress = Factory.New<OrgHeader>().MainAddress.PK;
			HAWB.Consignor.OH_FullName = "Senior";
			AssertEquals("Consignor", "Senior", Header.ConsignorName);
		}

		public virtual void TestMasterHouseBill()
		{
			MAWB.CM_MasterHouseBill = "CBA";
			HAWB.CS_MasterHouseBill = "ABC";
			AssertEquals("MasterHouseBill ", "ABC", Header.MasterHouseBill);
		}

		public virtual void TestMasterHouseBillCanComeFromMAWB()
		{
			MAWB.CM_MasterHouseBill = "CBA";
			AssertEquals("MasterHouseBill ", "CBA", Header.MasterHouseBill);
		}

		public virtual void TestHAWBNum()
		{
			HAWB.CS_HAWB = "ABC";
			AssertEquals("HAWBNum ", "ABC", Header.HAWBNum);
		}

		public virtual void TestMAWB()
		{
			HAWB.MAWB.CM_MAWB = "ABC";
			AssertEquals("MAWB ", "ABC", Header.MAWB);
		}

		public void TestNullMAWB()
		{
			CusHAWB hAWB = Factory.New<CusHAWB>();
			CusHAWBAirCargoReportHeader header = new CusHAWBAirCargoReportHeader(hAWB);

			AssertEquals(ZString.Empty, header.Loading);
			AssertEquals(ZString.Empty, header.Discharge);
			AssertEquals(0, header.Routings.Length);
			AssertEquals(ZString.Empty, header.MAWB);
			AssertEquals(ZString.Empty, header.FlightNo);
			AssertEquals(ZDateTime.Empty, header.ArivalDate);
			AssertEquals(false, header.IsBureau);
		}

		public virtual void TestFlightNo()
		{
			HAWB.MAWB.CM_FlightNo = "ABC";
			AssertEquals("FlightNo ", "ABC", Header.FlightNo);
		}

		public virtual void TestArivalDate()
		{
			HAWB.MAWB.CM_ArrivalDate = new ZDateTime(2005, 1, 1);
			AssertEquals("ArivalDate ", new ZDateTime(2005, 1, 1), Header.ArivalDate);
		}

		public void TestIsMasterHouse()
		{
			HAWB.CS_IsMasterHouse = false;
			AssertEquals("IsMasterHouse ", false, Header.IsMasterHouse);
		}

		public void TestIsDocuments()
		{
			HAWB.IsDocuments = true;
			AssertEquals("IsDocuments ", true, Header.IsDocuments);
		}

		public void TestIsPersonalEffects()
		{
			HAWB.CS_IsPersonalEffects = false;
			AssertEquals("IsPersonalEffects ", false, Header.IsPersonalEffects);
		}

		public void TestIsSelfAssessedClearance()
		{
			HAWB.CS_IsSelfAssessedClearance = true;
			AssertEquals("IsSelfAssessedClearance ", true, Header.IsSelfAssessedClearance);
		}

		public virtual void TestPackageCount()
		{
			HAWB.CS_PiecesManifested = 10;
			AssertEquals("PackageCount ", 10, Header.PackageCount);
		}

		public void TestUnmatchedOrgSendsTheCorrectInfoFromNotes()
		{
			ForwardingShipment shipment = GetShipmentWithConsol();
			string noteText = @"Organisation matching failed to find one or more organisations during data import. Details of the unmatched organisations are shown below:

Consignee:-
NAME: ACTROL INDUSTRIES PTY LTD
ADDRESS: TRANDING AS ACTROL PARTS, 19 KING STREET, BLACKBURN, MELBOURNE, 3130 VICTORIA

Consignor:-
NAME: ADVANCED DIESELS LTD
ADDRESS: 19 MERIDA PLACE, GLENFIELD, AUCKLAND";
			StmNote note = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, noteText);

			Factory.Save();

			CusMAWB masterBill = CusMAWB.CreateNew(shipment.Consols[0]);
			CusHAWB houseBill = CusHAWB.CreateNew(masterBill, shipment);
			CusHAWBAirCargoReportHeader header = new CusHAWBAirCargoReportHeader(houseBill);

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

			CusMAWB masterBill = CusMAWB.CreateNew(shipment.Consols[0]);
			CusHAWB houseBill = CusHAWB.CreateNew(masterBill, shipment);
			CusHAWBAirCargoReportHeader header = new CusHAWBAirCargoReportHeader(houseBill);

			AssertEquals("ACTROL INDUSTRIES PTY LTD", header.ConsigneeName);
			AssertEquals("TRANDING AS ACTROL PARTS 19 KING STREET, BLACKBURN MELBOURNE 3130 VICTORIA", header.ConsigneeGeneralAddress);
			AssertEquals("ADVANCED DIESELS LTD", header.ConsignorName);
			AssertEquals("19 MERIDA PLACE GLENFIELD 1313 AUCKLAND", header.ConsignorGeneralAddress);
		}

		public void TestGoodsDescription()
		{
			HAWB.CS_GoodsDescription = "ABC";
			AssertEquals("GoodsDescription ", "ABC", Header.GoodsDescription);
		}

		public void TestWeight()
		{
			HAWB.CS_Weight = 10m;
			AssertEquals("Weight ", 10m, Header.Weight);
		}

		public void TestWeightUQ()
		{
			HAWB.CS_WeightUQ = "KG";
			AssertEquals("WeightUQ ", "KG", Header.WeightUQ);
		}

		public void TestGoodsValue()
		{
			HAWB.CS_GoodsValue = 20m;
			AssertEquals("GoodsValue ", 20m, Header.GoodsValue);
		}

		public void TestGoodsValueCurrency()
		{
			HAWB.CS_RX_NKGoodsCurrency = "ABC";
			AssertEquals("GoodsValueCurrency", "ABC", Header.GoodsValueCurrency);
		}

		public void TestConsigneeNameAndAddressGeneral()
		{
			HAWB.CS_ConsigneeName = "Name";
			HAWB.CS_ConsigneeStreet = "Street";
			HAWB.CS_ConsigneeStreet2 = "more lol";
			HAWB.CS_ConsigneeCity = "City";
			HAWB.CS_ConsigneeState = "NSW";
			HAWB.CS_ConsigneePostcode = "";
			HAWB.CS_RN_NKConsigneeCountry = "AU";
			AssertEquals("Street more lol City NSW  AU", Header.ConsigneeGeneralAddress);
		}

		public void TestConsignorNameAndAddressGeneral()
		{
			HAWB.CS_ConsignorName = "Name";
			HAWB.CS_ConsignorStreet = "Street";
			HAWB.CS_ConsignorStreet2 = "more lol";
			HAWB.CS_ConsignorCity = "City";
			HAWB.CS_ConsignorState = "NSW";
			HAWB.CS_ConsignorPostcode = "";
			HAWB.CS_RN_NKConsignorCountry = "AU";
			AssertEquals("Street more lol City NSW  AU", Header.ConsignorGeneralAddress);
		}

		public void TestConsigneeName()
		{
			HAWB.CS_ConsigneeName = "ZZZ";
			AssertEquals("ConsigneeName", "ZZZ", Header.ConsigneeName);
		}

		public void TestConsigneeStreet()
		{
			HAWB.CS_ConsigneeStreet = "XXX";
			AssertEquals("ConsigneeStreet", "XXX", Header.ConsigneeStreet);
		}

		public void TestConsigneeStreet2()
		{
			HAWB.CS_ConsigneeStreet2 = "CCC";
			AssertEquals("ConsigneeStreet2", "CCC", Header.ConsigneeStreet2);
		}

		public void TestConsigneeCity()
		{
			HAWB.CS_ConsigneeCity = "PPP";
			AssertEquals("ConsigneeCity", "PPP", Header.ConsigneeCity);
		}

		public void TestConsigneePostCode()
		{
			HAWB.CS_ConsigneePostcode = "123";
			AssertEquals("ConsigneePostcode", "123", Header.ConsigneePostCode);
		}

		public void TestConsigneeCountry()
		{
			HAWB.CS_RN_NKConsigneeCountry = "AU";
			AssertEquals("ConsigneeCountry", "AU", Header.ConsigneeCountry);
		}

		public void TestConsignorName()
		{
			HAWB.CS_ConsignorName = "QQQ";
			AssertEquals("ConsignorName", "QQQ", Header.ConsignorName);
		}

		public void TestConsignorStreet()
		{
			HAWB.CS_ConsignorStreet = "TTT";
			AssertEquals("ConsignorStreet", "TTT", Header.ConsignorStreet);
		}

		public void TestConsignorStreet2()
		{
			HAWB.CS_ConsignorStreet2 = "NNN";
			AssertEquals("ConsignorStreet2", "NNN", Header.ConsignorStreet2);
		}

		public void TestConsignorCity()
		{
			HAWB.CS_ConsignorCity = "RRR";
			AssertEquals("ConsignorCity", "RRR", Header.ConsignorCity);
		}

		public void TestConsignorPostCode()
		{
			HAWB.CS_ConsignorPostcode = "555";
			AssertEquals("ConsignorPostcode", "555", Header.ConsignorPostCode);
		}

		public void TestConsignorCountry()
		{
			HAWB.CS_RN_NKConsignorCountry = "US";
			AssertEquals("ConsignorCountry", "US", Header.ConsignorCountry);
		}

		IAirCargoReportHeader header;
		protected virtual IAirCargoReportHeader Header => header ?? (header = new CusHAWBAirCargoReportHeader(HAWB));

		CusHAWBBase hawb;
		protected virtual CusHAWBBase HAWB => hawb ?? (hawb = ((CusMAWB)MAWB).ChildBills.AddNew());

		CusMAWBBase mawb;
		protected virtual CusMAWBBase MAWB => mawb ?? (mawb = Factory.New<CusMAWB>());

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

		ForwardingShipment GetShipmentWithConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_AgentType = "AGT";
			consol.JK_MasterBillNum = "08165431365";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "CuckooSqueaker";
			shipment.ConsigneePK = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			shipment.ConsignorPK = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

			return shipment;
		}
	}
}
