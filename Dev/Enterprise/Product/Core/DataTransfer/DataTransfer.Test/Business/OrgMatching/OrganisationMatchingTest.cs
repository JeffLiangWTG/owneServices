using System;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrganisationMatchingTest : TestCaseWithFactory
	{
		#region Logging Tests

		public void TestMappingOrgText()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OrganisationDetails.Name = "Test Org";
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationUnmatched, "Mapping Organization: " + GlbCompany.CurrentCompany.OrgProxy.OH_Code);
		}

		#region Local code Match

		public void TestMatchByLocalCode()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.EDICode = "~ChiMai";
			orgValue.OrganisationDetails.Name = "Freestyle";
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationUnmatched, $"Matching by {BrandingFactory.Instance.ProductName} Code: ~ChiMai, Using: Default Matcher, Found match: False");
		}

		public void TestMatchByLocalCodeMatchFound()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "Helga";
			Factory.Save();

			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.EDICode = "Helga";
			orgValue.OrganisationDetails.Name = "Mill";
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationMatched, $"Matching by {BrandingFactory.Instance.ProductName} Code: Helga, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True");
		}

		public void TestMatchByLocalCodePatternMatch_Found()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.EDICode = "Space";
			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			SetupOrgToMatchAndMatchingCriteria(orgToMatch, orgValue);
			Factory.Save();

			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationMatched, $"Matching by {BrandingFactory.Instance.ProductName} Code: Space, Using: Similarity Matcher, Found match: True");
		}

		#endregion

		#region Foreign Code Match

		public void TestMatchByForeignCode()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OwnerCode = "BOAR";
			orgValue.OrganisationDetails.Name = "Portvein";
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationUnmatched, "Matching by Foreign code: BOAR, Using: Default Matcher");
		}

		public void TestMatchByForeignCodeMatchFound()
		{
			OrgHeader orgToMatch = Factory.New<OrgHeader>();
			orgToMatch.OH_Code = "~MATCH~";
			Factory.Save();

			OrgHeader orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var newOrgMapping = orgProxy.CreatePatternMatchOverrideForTest();
			newOrgMapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			newOrgMapping.OO_ForeignCode = "MoneyYeah";
			newOrgMapping.OO_LocalGuid = orgToMatch.PK;
			Factory.Save();

			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OwnerCode = "MoneyYeah";
			orgValue.OrganisationDetails.Name = "Carpet";
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationMatched, "Matching by Foreign code: MoneyYeah, Using: Foreign Code Matcher, Found match: True");
		}

		public void TestMatchByForeignCodePatternMatch_Found()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OwnerCode = "Beta";
			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			SetupOrgToMatchAndMatchingCriteria(orgToMatch, orgValue);
			Factory.Save();

			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertContainsMessage(buffer, NotificationSubscriberType.OrganisationMatched, "Matching by Foreign code: Beta, Using: Similarity Matcher, Found match: True");
		}

		#endregion

		public void TestShouldCreateTempOrganization()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OrganisationDetails.Name = "Test Org";
			var notifications = new NotificationBuffer();
			organisationMatching = new TestOrganisationMatching(Factory, new Xml.XsdVersion1.XmlInterchange(), notifications);

			var result = organisationMatching.Match(orgValue, OrganisationTypes.None, true);
			Assert(result.AdditionalInfo.ContainsCode("Should create Temp. Organization"));
			AssertEquals("True", result.AdditionalInfo["Should create Temp. Organization"].Description);

			result = organisationMatching.Match(orgValue, OrganisationTypes.None, false);
			Assert(result.AdditionalInfo.ContainsCode("Should create Temp. Organization"));
			AssertEquals("False", result.AdditionalInfo["Should create Temp. Organization"].Description);
		}

		public void TestCriteriaHasNameOrCode()
		{
			UseUnmatched(false);
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OrganisationDetails.Name = "Test Org";
			var notifications = new NotificationBuffer();
			organisationMatching = new TestOrganisationMatching(Factory, new Xml.XsdVersion1.XmlInterchange(), notifications);
			var result = organisationMatching.Match(orgValue, OrganisationTypes.None);
			Assert(result.AdditionalInfo.ContainsCode("Criteria has Name/Code"));
			Assert(result.AdditionalInfo.ContainsCode("Temp. Organization created"));
			AssertEquals("True", result.AdditionalInfo["Criteria has Name/Code"].Description);
			AssertEquals("True", result.AdditionalInfo["Temp. Organization created"].Description);

			var orgValue2 = new Xml.XsdVersion1.Organisation();
			notifications = new NotificationBuffer();
			organisationMatching = new TestOrganisationMatching(Factory, new Xml.XsdVersion1.XmlInterchange(), notifications);
			result = organisationMatching.Match(orgValue2, OrganisationTypes.None);
			Assert(result.AdditionalInfo.ContainsCode("Criteria has Name/Code"));
			Assert(result.AdditionalInfo.ContainsCode("Temp. Organization created"));
			AssertEquals("False", result.AdditionalInfo["Criteria has Name/Code"].Description);
			AssertEquals("False", result.AdditionalInfo["Temp. Organization created"].Description);
		}

		void UseUnmatched(bool enable)
		{
			var reg = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			reg.IsEnabled = enable;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
		}

		void AssertContainsMessage(NotificationBuffer buffer, NotificationSubscriberType type, string expectedMessage)
		{
			var notifications = buffer.GetEventsByType(type);
			bool found = false;
			string allMessages = string.Empty;
			foreach (NotificationSubscriberNotification notif in notifications)
			{
				allMessages += notif.Message + "\r\n";
				if (notif.Message.Contains(expectedMessage))
				{
					found = true;
					break;
				}
			}

			Assert("Expected part of message:\r\n" + expectedMessage + "\r\n\r\nActual messages:\r\n" + allMessages, found);
		}

		#endregion

		public void TestFindOrCreateTempOrganisationPK()
		{
			var orgValue = new Xml.XsdVersion1.Organisation();
			orgValue.OrganisationDetails.Name = "Test Org";

			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			var createdOrgPK = context.FindOrCreateTempOrganisationPK(orgValue, null, OrganisationTypes.None);
			AssertEquals("Should create an organisation and return a PK", true, createdOrgPK.IsValid);

			var notifications = buffer.GetEventsByType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated);
			AssertNotNull(notifications);
			AssertEquals(true, notifications.Length > 0);
			var newOrgFound = false;

			foreach (NotificationSubscriberNotification bizObjNotification in notifications)
			{
				if (bizObjNotification.Message == "Organization (TESORG) created")
				{
					newOrgFound = true;
					break;
				}
			}
			AssertEquals(true, newOrgFound);
		}

		public void TestCreateTempWithOrganisationType()
		{
			var value = new Xml.XsdVersion1.Organisation();
			value.OrganisationDetails.Name = "Test Org";

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var orgCreated = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.Sales);
			AssertEquals("Organisation should be created", false, orgCreated.IsInDatabase);
			AssertEquals("Organisation should be sales lead", true, orgCreated.OH_IsSalesLead);
		}

		public void TestMatchedToUnmatched()
		{
			var reg = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			reg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);

			try
			{
				var value = new Xml.XsdVersion1.Organisation();
				value.OrganisationDetails = new Xml.XsdVersion1.OrganisationDetail();
				value.OrganisationDetails.Name = "Some Organisation Name";
				value.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "Address line 1";
				value.OrganisationDetails.Location.Value = "CATOR";
				var dummyBO = Factory.New<DummyBizOWithPredefinedUnmatchedNoteType>();

				var matchingResult = organisationMatching.FindOrganisation(value, null, OrganisationTypes.None);
				AssertEquals("Should have matched to the UNMATCHED org", reg.Organisation, matchingResult.PK);
				var foundNotes = dummyBO.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be no note text because no BO was passed in", 0, foundNotes.Length);

				matchingResult = organisationMatching.FindOrganisation(value, dummyBO, OrganisationTypes.Consignee);
				AssertEquals("Should have matched to the UNMATCHED org", reg.Organisation, matchingResult.PK);
				foundNotes = dummyBO.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note on the org", 1, foundNotes.Length);
				ZString serialisedNoteText = "Organisation Type: Consignee\r\nOwner Code: \r\nEDI Code: \r\nOrganisation Name: Some Organisation Name\r\nAddress Line 1: Address line 1\r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: CA\r\nDoc Address Type: \r\n ";
				AssertEquals("Note text should describe Import Forwarder", serialisedNoteText, foundNotes[0].ST_NoteText);

				value.OrganisationDetails.Addresses.GetOrCreateMainAddress().Location.Value = "TWTPE";

				var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder };
				organisationMatching.FindOrganisation(value, dummyBO, OrganisationTypes.Forwarder, unmatchOrgRecordCriteria);
				foundNotes = dummyBO.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note on the org", 1, foundNotes.Length);
				serialisedNoteText = "Organisation Type: Consignee\r\nOwner Code: \r\nEDI Code: \r\nOrganisation Name: Some Organisation Name\r\nAddress Line 1: Address line 1\r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: CA\r\nDoc Address Type: \r\n \r\nOrganisation Type: Receiving Forwarder\r\nOwner Code: \r\nEDI Code: \r\nOrganisation Name: Some Organisation Name\r\nAddress Line 1: Address line 1\r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: TW\r\nDoc Address Type: \r\n ";
				AssertEquals("Note text should describe Import Forwarder", serialisedNoteText, foundNotes[0].ST_NoteText);
			}
			finally
			{
				reg.IsEnabled = false;
				OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
			}
		}

		public void TestMatchedToUnmatchedWhenOwnerCodeNotFound()
		{
			var reg = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			reg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);

			try
			{
				var value = new Xml.XsdVersion1.Organisation();
				value.OrganisationDetails = new Xml.XsdVersion1.OrganisationDetail();
				value.OrganisationDetails.Name = "Some Organisation Name";
				value.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "Address line 1";
				value.OwnerCode = "SOMORG1";
				var dummyBO = Factory.New<DummyBizOWithPredefinedUnmatchedNoteType>();

				IOrgHeaderForMatching matchingResult = organisationMatching.FindOrganisation(value, dummyBO, OrganisationTypes.Consignee);
				AssertEquals("Should have matched to the UNMATCHED org", reg.Organisation, matchingResult.PK);
				var foundNotes = dummyBO.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note on the org", 1, foundNotes.Length);
				ZString serialisedNoteText = "Organisation Type: Consignee\r\nOwner Code: SOMORG1\r\nEDI Code: \r\nOrganisation Name: Some Organisation Name\r\nAddress Line 1: Address line 1\r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: \r\nDoc Address Type: \r\n ";
				AssertEquals("Note text should describe Import Forwarder", serialisedNoteText, foundNotes[0].ST_NoteText);

				dummyBO = Factory.New<DummyBizOWithPredefinedUnmatchedNoteType>();

				var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder };
				matchingResult = organisationMatching.FindOrganisation(value, dummyBO, OrganisationTypes.Forwarder, unmatchOrgRecordCriteria);
				AssertEquals("Should have matched to the UNMATCHED org", reg.Organisation, matchingResult.PK);
				foundNotes = dummyBO.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note on the org", 1, foundNotes.Length);
				serialisedNoteText = "Organisation Type: Receiving Forwarder\r\nOwner Code: SOMORG1\r\nEDI Code: \r\nOrganisation Name: Some Organisation Name\r\nAddress Line 1: Address line 1\r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: \r\nDoc Address Type: \r\n ";
				AssertEquals("Note text should describe Import Forwarder", serialisedNoteText, foundNotes[0].ST_NoteText);
			}
			finally
			{
				reg.IsEnabled = false;
				OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
			}
		}

		public void TestMatchOnOwnerCode()
		{
			var decoyOrgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			var mappingOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var orgMapping = mappingOrg.CreatePatternMatchOverrideForTest();
			orgMapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMapping.OO_ForeignCode = "ownercode";
			orgMapping.OO_LocalGuid = orgToMatch.PK;

			var organisationValue = new Xml.XsdVersion1.Organisation();
			organisationValue.EDICode = decoyOrgToMatch.OH_Code;
			organisationValue.OrganisationDetails.Name = "FullName";
			organisationValue.OwnerCode = "ownercode";
			new NotificationBuffer();

			var matchResult = organisationMatching.Match(organisationValue, OrganisationTypes.None);
			Assert("Should match on the owner-code NOT the EDICode", matchResult.MatchFound);
			AssertEquals("Should match on the owner-code NOT the EDICode", matchResult.Match.PK, orgToMatch.PK);

			orgMapping.Delete();
			Factory.Save();

			var noMatchResult = organisationMatching.Match(organisationValue, OrganisationTypes.None);
			Assert("No match as mapping was deleted; should NOT fall back to matching on the EDI code when an OwnerCode is specified", !noMatchResult.MatchFound);
			Assert("No match as mapping was deleted; should NOT fall back to matching on the EDI code when an OwnerCode is specified", noMatchResult.Match.OH_IsTempAccount);
		}

		public void TestMatchOnEDICode()
		{
			var value = new Xml.XsdVersion1.Organisation();
			value.EDICode = "edicode";
			value.OwnerCode = "";

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "orgname ltd";
			organisation.OH_Code = "edicode";

			var matchingResult = organisationMatching.Match(value, OrganisationTypes.None);
			Assert("Should find the existing organisation using the EDICode", matchingResult.MatchFound);
			AssertEquals("Should find the existing organisation using the EDICode", matchingResult.Match.PK, organisation.PK);
		}

		public void TestMatchWithOrganisationPatternMatching()
		{
			var matchingCriteria = new Xml.XsdVersion1.Organisation();
			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			SetupOrgToMatchAndMatchingCriteria(orgToMatch, matchingCriteria);
			Factory.Save();

			var matchingResult = organisationMatching.Match(matchingCriteria, OrganisationTypes.None);
			Assert("Should find the existing organisation", matchingResult.MatchFound);
			AssertEquals("Should find the existing organisation", matchingResult.Match.PK, matchingResult.Match.PK);
		}

		public void TestCreateTempOrgAsMatchingResult_WhenNoTempCanBeCreated()
		{
			var matchingCriteria = new Xml.XsdVersion1.Organisation();
			matchingCriteria.OrganisationDetails.Name = "";
			matchingCriteria.EDICode = "";

			var matchingResult = organisationMatching.Match(matchingCriteria, OrganisationTypes.Sales);
			AssertEquals("TempOrgCouldNotBeCreated should be true", true, matchingResult.TempOrgCouldNotBeCreated);
			AssertEquals("MatchFound should be false", false, matchingResult.MatchFound);
			AssertNull("No match should be returned", matchingResult.Match);

			// expect no exception due to the temporary org without OH_Code being saved
			Factory.Save();
		}

		public void TestCreateTempOrgAsMatchingResult_WhenNoMatchFound()
		{
			var value = new Xml.XsdVersion1.Organisation();
			value.OrganisationDetails = new Xml.XsdVersion1.OrganisationDetail();
			value.OrganisationDetails.Name = "orgname";

			var matchingResult = organisationMatching.Match(value, OrganisationTypes.None);
			AssertEquals("Should create a temporary organisation due to no match", false, matchingResult.MatchFound);
			AssertEquals("Should create a temporary organisation due to no match", true, matchingResult.Match.OH_IsTempAccount);
		}

		public void TestCreateTempOrgAsMatchingResult_WithCorrectOrganisationType()
		{
			var value = new Xml.XsdVersion1.Organisation();
			value.OrganisationDetails.Name = "Test Org";

			var matchingResult = organisationMatching.Match(value, OrganisationTypes.Sales);
			AssertEquals("Should create temp organisation", false, matchingResult.MatchFound);
			AssertEquals("Should create temp organisation", true, matchingResult.Match.OH_IsTempAccount);
			AssertEquals("Organisation should be sales lead", true, matchingResult.Match.OH_IsSalesLead);
		}

		public void TestCreateTemporaryOrUnmatchOrgIfNoMatchFound_False()
		{
			TestCreateTemporaryOrUnmatchOrgIfNoMatchFound(false);
		}

		public void TestCreateTemporaryOrUnmatchOrgIfNoMatchFound_True()
		{
			TestCreateTemporaryOrUnmatchOrgIfNoMatchFound(true);
		}

		void TestCreateTemporaryOrUnmatchOrgIfNoMatchFound(bool createTemporaryOrUnmatchOrgIfNoMatchFound)
		{
			organisationMatching.SetCreateTemporaryOrUnmatchOrgIfNoMatchFound(createTemporaryOrUnmatchOrgIfNoMatchFound);
			var criteria = new Xml.XsdVersion1.Organisation();
			criteria.OrganisationDetails.Name = "Test Org";

			var matchingResult = organisationMatching.Match(criteria, OrganisationTypes.Consignee);
			AssertEquals("No match should be found", false, matchingResult.MatchFound);
			AssertEquals("A temporary organisation was not created, but it could have been", false, matchingResult.TempOrgCouldNotBeCreated);

			if (createTemporaryOrUnmatchOrgIfNoMatchFound)
			{
				AssertEquals("Should return a temporary organisation for the match", true, matchingResult.Match.OH_IsTempAccount);
			}
			else
			{
				AssertEquals("Returned match is null", null, matchingResult.Match);
			}
		}

		public void TestNewOwnerCodeMappingRegisteredOnMatch()
		{
			var mappingOrg = Factory.NewWithValidTestData<OrgHeader>();
			mappingOrg.OH_FullName = "MappingOrg";
			mappingOrg.MainAddress.OA_Address1 = "MappingOrg";

			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			var matchingCriteria = new Xml.XsdVersion1.Organisation();
			SetupOrgToMatchAndMatchingCriteria(orgToMatch, matchingCriteria);
			Factory.Save();

			matchingCriteria.OwnerCode = "NewOwnerCode";
			var interchange = new Xml.XsdVersion1.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails.Name = "MappingOrg";
			interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "MappingOrg";

			var notifications = new NotificationBuffer();
			organisationMatching = new TestOrganisationMatching(Factory, interchange, notifications);
			var matchingResult = organisationMatching.Match(matchingCriteria, OrganisationTypes.None);

			Assert("Should find the match for the test", matchingResult.MatchFound);
			AssertEquals("Should find the match for the test", matchingResult.Match.PK, orgToMatch.PK);
			var matches = Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK));
			AssertEquals("New owner code mapping registered", 1, matches.Length);

			AssertEquals("New owner code mapping registered", Core.Constants.OrgPatternMatchOverrideRelationships.Organisation, matches[0].OO_Relationship);
			AssertEquals("New owner code mapping registered", "NewOwnerCode", matches[0].OO_ForeignCode);
			AssertEquals("New owner code mapping registered", matchingResult.Match.PK, matches[0].OO_LocalGuid);
		}

		#region Test Classes

		class TestOrganisationMatching : OrganisationMatching
		{
			public TestOrganisationMatching(BusinessObjectFactory factory, Xsd.XmlInterchange interchange, INotifications notifications)
				: base(new BusinessObjectFactoryProvider(factory), interchange, notifications)
			{
			}

			protected override bool CreateTemporaryOrUnmatchOrgIfNoMatchFound
			{
				get { return fCreateTemporaryOrUnmatchOrgIfNoMatchFound; }
			}
			public bool fCreateTemporaryOrUnmatchOrgIfNoMatchFound = true;

			public void SetCreateTemporaryOrUnmatchOrgIfNoMatchFound(bool value)
			{
				fCreateTemporaryOrUnmatchOrgIfNoMatchFound = value;
			}
		}

		#endregion

		#region Implementation

		void SetupOrgToMatchAndMatchingCriteria(OrgHeader orgToMatch, AutoOrganisation matchingCriteria)
		{
			matchingCriteria.OrganisationDetails.Name = "test org";
			matchingCriteria.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "test org";
			matchingCriteria.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(RegistrationNumberTypes.GST).Number = "test org";

			orgToMatch.OH_FullName = "test org";
			orgToMatch.PrimaryRegistrationNumber.Number = "test org";
			orgToMatch.MainAddress.OA_Address1 = "test org";
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisationMatching = new TestOrganisationMatching(Factory, XmlInterchange.Empty, new NotificationBuffer());
		}

		TestOrganisationMatching organisationMatching;

		#endregion
	}
}
