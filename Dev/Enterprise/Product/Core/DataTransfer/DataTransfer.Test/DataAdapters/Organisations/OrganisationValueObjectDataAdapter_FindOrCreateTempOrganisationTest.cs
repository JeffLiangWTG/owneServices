using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.Core;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class OrganisationValueObjectDataAdapter_FindOrCreateTempOrganisationTest : TestCaseWithFactory
	{
		public void TestDoNotImportUSDeprecatedSAN()
		{
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var regNumbers = new RegistrationNumberCollection();
			var san = regNumbers.AddNew();
			san.NumberType = Xsd.RegistrationNumberTypes.SAN;
			san.Number = "12-12345678";
			san.IsSpecified = true;
			san.CountryOfRegistration = Core.Constants.CountryCodes.UnitedStates;

			var org = new Organisation();
			org.OrganisationDetails.Name = "Something";
			org.OrganisationDetails.RegistrationNumbers = regNumbers;

			var orgHeader = Factory.New<OrgHeader>();

			DataAdapter.ImportFromValueObject(orgHeader, org, context);
			Assert(!orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.USACodeTypes.DeprecatedSpecialAddressNotification
				&& x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates));
			Assert(notify.HasWarnings);
		}

		public void TestExportLightWeight_Address()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.Addresses.AddNewMainAddress();
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Code = "What ever";

			var orgAddress2 = org.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Address2";
			orgAddress2.OA_Code = "What ever2";

			Factory.Save();

			var notifications = new NotificationBuffer();
			var context = new ValueObjectExportContext(notifications);

			context.SimplifiedXML = false;
			var result = DataAdapter.ExportToValueObject(org, context);
			AssertNotEquals(1, result.OrganisationDetails.Addresses.Count);

			context.SimplifiedXML = true;
			result = DataAdapter.ExportToValueObject(org, context);
			AssertEquals(1, result.OrganisationDetails.Addresses.Count);
			var address = result.OrganisationDetails.Addresses.GetMainAddress();
			AssertNotNull(address);
		}

		public void TestMatchWithOrganisationPatternMatching()
		{
			var matchingCriteria = new Organisation();
			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			SetupOrgToMatchAndMatchingCriteria(orgToMatch, matchingCriteria);
			Factory.Save();

			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var orgFound = context.FindOrCreateTempOrganisation(matchingCriteria, null, OrganisationTypes.None);
			AssertEquals("Should find the existing organisation", orgToMatch.PK, orgFound.PK);

			var orgMatchedNotifications = notify.GetEventsByType(NotificationSubscriberType.OrganisationMatched);
			AssertEquals("There should be an OrganisationMatched notification", 1, orgMatchedNotifications.Length);
			AssertEquals("There should be an OrganisationMatched notification", true, orgMatchedNotifications[0] is OrganisationMatchedNotification);
		}

		public void TestCreateTempWhenNoMatchFound()
		{
			var value = new Organisation();
			value.OrganisationDetails = new OrganisationDetail();
			value.OrganisationDetails.Name = "orgname";

			var notificationBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notificationBuffer);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertEquals("Should create a temporary organisation due to no match", true, orgFound.OH_IsTempAccount);

			var notifications = notificationBuffer.GetEventsByType(NotificationSubscriberType.OrganisationUnmatched);
			AssertEquals("There should be an OrganisationUnmatched notification", 1, notifications.Length);
			AssertEquals("There should be an OrganisationUnmatched notification", true, notifications[0] is OrganisationUnmatchedNotification);
		}

		public void TestCreateTempWithFullNameOnly()
		{
			var value = new Organisation();
			value.OrganisationDetails = new OrganisationDetail();
			value.OrganisationDetails.Name = "orgname";
			var notificationBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notificationBuffer);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);

			AssertEquals("Should create a temporary organisation due to no match", true, orgFound.OH_IsTempAccount);
			AssertEquals("Should create a temporary organisation due to no match", "ORGNAM", ((OrgHeader)orgFound).OH_Code);
			orgFound.Delete();

			value.OrganisationDetails.Location = UNLOCO.FromPortCode(Factory, "AUSYD");
			orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertEquals("Should create a temporary organisation due to no match", true, orgFound.OH_IsTempAccount);
			AssertEquals("Should create a temporary organisation due to no match", "ORGNAMSYD", ((OrgHeader)orgFound).OH_Code);
			orgFound.Delete();

			value.OrganisationDetails.Location = UNLOCO.FromPortCode(Factory, "__SYD");
			orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertEquals("Should create a temporary organisation due to no match", true, orgFound.OH_IsTempAccount);
			AssertEquals("Should create a temporary organisation due to no match", "ORGNAM", ((OrgHeader)orgFound).OH_Code);
		}

		public void TestNoMatchOnEDICodeAndNotEnoughDetailToCreateTemp()
		{
			var value = new Organisation();
			value.EDICode = "notfound";

			var notifications = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifications);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertNull("Should return null as nothing can be matched or created", orgFound);
			AssertEquals("When matching on EDICode no match found and no temp could be created", Res.GetString("1a3afe4e-3068-43ea-a0d5-1bb95f18e8d0", "No match found and could not create temporary organization for {0} Code='{1}'", Core.Constants.ProductName, "notfound"), ((INotificationSubscriberNotification)notifications.Events[0]).AdditionalInfo);
		}

		public void TestNoMatchOnOwnerCodeAndNotEnoughDetailToCreateTemp()
		{
			var value = new Organisation();
			value.EDICode = "notfound";
			value.OwnerCode = "notfound";

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertNull("Should return null as nothing can be matched or created", orgFound);
			AssertEquals("When matching on EDICode no match found and no temp could be created", Res.GetString("0791d2d3-b116-4d59-8351-730e3b1e0e6b", "No match found and could not create temporary organization for Owner Code='{0}'", "notfound"), ((INotificationSubscriberNotification)notifcations.Events[0]).AdditionalInfo);
		}

		public void TestMatchOnOwnerCode()
		{
			var decoyOrgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();

			var mappingOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var orgMapping = mappingOrg.CreatePatternMatchOverrideForTest();
			orgMapping.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMapping.OO_ForeignCode = "ownercode";
			orgMapping.OO_LocalGuid = orgToMatch.PK;

			var organisationValue = new Organisation();
			organisationValue.EDICode = decoyOrgToMatch.OH_Code;
			organisationValue.OwnerCode = "ownercode";
			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var matchedOrg = context.FindOrCreateTempOrganisation(organisationValue, null, OrganisationTypes.None);
			AssertEquals("Should match on the owner-code NOT the EDICode", matchedOrg.PK, orgToMatch.PK);

			orgMapping.Delete();
			Factory.Save();

			var noMatchOrg = context.FindOrCreateTempOrganisation(organisationValue, null, OrganisationTypes.None);
			AssertNull("No match as mapping was deleted; should NOT fall back to matching on the EDI code when an OwnerCode is specified", noMatchOrg);
		}

		public void TestMatchOnEDICode()
		{
			var value = new Organisation();
			value.EDICode = "edicode";
			value.OwnerCode = "";

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "orgname ltd";
			organisation.OH_Code = "edicode";

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertEquals("Should find the existing organisation using the EDICode", organisation.PK, orgFound.PK);
		}

		public void TestAddsTailIfDuplicateAndUniqueRegistrySet()
		{
			var defaultAlgorithm = new OrgCodeAlgorithm();

			defaultAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Order = 1;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			defaultAlgorithm.Elements[OrgCodeElementDescription.UnlocoCode].Order = 2;
			defaultAlgorithm.Elements[OrgCodeElementDescription.UnlocoCode].Length = 3;
			defaultAlgorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 3;
			defaultAlgorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 3;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultAlgorithm);

			var existing = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			existing.OH_Code = "ABC";
			existing.OH_FullName = "1 ABC";
			existing.Factory.Save();

			var value = new Organisation();
			value.EDICode = "";
			value.OrganisationDetails.Name = "ABC";

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertEquals("should create a new one and add tail", "ABC1", orgFound.OH_Code);
		}

		public void TestNewOwnerCodeMappingRegisteredOnMatch()
		{
			var notifcations = new NotificationBuffer();
			var mappingOrg = Factory.NewWithValidTestData<OrgHeader>();
			mappingOrg.OH_FullName = "MappingOrg";
			mappingOrg.MainAddress.OA_Address1 = "MappingOrg";

			var orgToMatch = Factory.NewWithValidTestData<OrgHeader>();
			var matchingCriteria = new Organisation();
			SetupOrgToMatchAndMatchingCriteria(orgToMatch, matchingCriteria);
			Factory.Save();

			matchingCriteria.OwnerCode = "NewOwnerCode";
			var interchange = new XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails.Name = "MappingOrg";
			interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "MappingOrg";
			var context = new ValueObjectImportContext(Factory, interchange, notifcations);
			var matchedOrg = context.FindOrCreateTempOrganisation(matchingCriteria, null, OrganisationTypes.None);

			AssertEquals("Should find the match for the test", matchedOrg.PK, orgToMatch.PK);
			AssertEquals("New owner code mapping registered", 1, mappingOrg.PatternMatchOverrides_ForBinding.Count);

			AssertEquals("New owner code mapping registered", Constants.OrgPatternMatchOverrideRelationships.Organisation, mappingOrg.PatternMatchOverrides_ForBinding[0].OO_Relationship);
			AssertEquals("New owner code mapping registered", "NewOwnerCode", mappingOrg.PatternMatchOverrides_ForBinding[0].OO_ForeignCode);
			AssertEquals("New owner code mapping registered", matchedOrg.PK, mappingOrg.PatternMatchOverrides_ForBinding[0].OO_LocalGuid);
		}

		public void TestNotificationsForNoTempOrgCreatedFromOnlyOwnerCode()
		{
			var value = new Organisation();
			value.OwnerCode = "ownercode";

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);

			// expect no exception
			var notUsed = notifcations.AsString;

			AssertEquals(
				"Should show the user a warning indicating an organisation was unmatched",
				true, notifcations.ContainsNotificationType(WarningType.Warning));
			AssertEquals("Error additional info should contain owner code", Res.GetString("0791d2d3-b116-4d59-8351-730e3b1e0e6b", "No match found and could not create temporary organization for Owner Code='{0}'", "ownercode"), ((INotificationSubscriberNotification)notifcations.GetEventsByType(WarningType.Warning)[0]).AdditionalInfo);
		}

		public void TestMatchFoundNotifications()
		{
			var value = new Organisation();
			value.EDICode = "zzzorg";

			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_FullName = "orgname ltd";
			organisation.OH_Code = "zzzorg";
			organisation.Factory.Save();

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			Assert("Should find the organisation for the test", orgFound != null);

			AssertEquals("Should show correct notifications for when a match is found",
						$"Successfully matched organization with code 'zzzorg', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: zzzorg, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True\r\n",
						notifcations.AsString);
		}

		public void TestNoMatchFoundNotifications()
		{
			var value = new Organisation();
			value.OrganisationDetails.Name = "Test Org";
			value.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "Test Org";

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var orgFound = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			var expectedNotifications =
				"Failed to matched organization with code/name 'TESORG' / 'Test Org', Mapping Organization: EDICUS, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True\r\n" +
				"Organization (TESORG) created\r\n";
			AssertEquals("Should show correct notifications for when a match is found", expectedNotifications, notifcations.AsString);
		}

		public void TestImportFromValueObject_2AddressesAndFactorySave()
		{
			var orgValue = new Organisation();
			orgValue.OrganisationDetails.Name = "Test Organisation";

			var mainAddressValue = orgValue.OrganisationDetails.Addresses.AddNew(AddressCapabilityAddressType.MAIN);
			mainAddressValue.AddressLine1 = "Main";
			var phone = mainAddressValue.TelephoneNumbers.AddNew();
			phone.NumberType = TelephoneNumberNumberType.Business;
			phone.Value = "555-12345";

			var addresss2 = orgValue.OrganisationDetails.Addresses.AddNew(AddressCapabilityAddressType.OFC);
			addresss2.AddressLine1 = "Other";
			var phone2 = addresss2.TelephoneNumbers.AddNew();
			phone2.NumberType = TelephoneNumberNumberType.Business;
			phone2.Value = "555-12345";

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var newOrganisation = context.FindOrCreateTempOrganisation(orgValue, null, OrganisationTypes.None);
			AssertEquals("A new organisation should be created", false, newOrganisation.IsInDatabase);
			AssertEquals("No errors should result", false, notifcations.HasErrors);
			Factory.Save();
		}

		#region Implementation

		readonly OrganisationValueObjectDataAdapter DataAdapter = new OrganisationValueObjectDataAdapter();

		void SetupOrgToMatchAndMatchingCriteria(OrgHeader orgToMatch, Organisation matchingCriteria)
		{
			matchingCriteria.OrganisationDetails.Name = "test org";
			matchingCriteria.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "test org";
			matchingCriteria.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(RegistrationNumberTypes.GST).Number = "test org";

			orgToMatch.OH_FullName = "test org";
			orgToMatch.PrimaryRegistrationNumber.Number = "test org";
			orgToMatch.MainAddress.OA_Address1 = "test org";
		}

		#endregion
	}
}
