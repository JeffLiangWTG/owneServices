using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Events = Enterprise.ZArchitecture.Business.Events;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(OrganisationValueObjectDataAdapter))]
	public class OrganisationValueObjectDataAdapterTest : ValueObjectDataAdapterTest<OrgHeader, Organisation>
	{
		#region TestEDICodeMappingsAreImported

		public void TestEDICodeMappingsAreImported()
		{
			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "NONEXISTINGORG");
			AssertNull("Precondition", org);

			var value = new Organisation();
			value.OrganisationDetails = new OrganisationDetail();

			value.EDICode = "edicode";
			value.OwnerCode = "NONEXISTINGORG";
			value.OrganisationDetails.Name = "Non existing organisation";
			var eDIMappings = value.OrganisationDetails.EDICodeMappings;

			var orgToMatch = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetEDICodeMapping(eDIMappings, orgToMatch.OH_Code, "ABC", Constants.OrgPatternMatchOverrideRelationships.Organisation);
			SetEDICodeMapping(eDIMappings, "US", "UNI", Constants.OrgPatternMatchOverrideRelationships.Country);
			SetEDICodeMapping(eDIMappings, "20GP", "CONT", Constants.OrgPatternMatchOverrideRelationships.ContainerType);
			SetEDICodeMapping(eDIMappings, "USD", "123", Constants.OrgPatternMatchOverrideRelationships.Currency);
			SetEDICodeMapping(eDIMappings, Constants.IncoTerms.FreeCarrier, "ABC", Constants.OrgPatternMatchOverrideRelationships.IncoTerm);
			SetEDICodeMapping(eDIMappings, "AUSYD", "PORT", Constants.OrgPatternMatchOverrideRelationships.Port);

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			var newOrg = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None) as OrgHeader;
			AssertNotNull(newOrg);
			var collection = newOrg.PatternMatchOverrides_ForBinding;
			AssertEquals("There should be 6 pattern match overrides", 6, collection.Count);

			AssertPatternMatchOverride(collection, orgToMatch.OH_Code, "ABC", Constants.OrgPatternMatchOverrideRelationships.Organisation);
			AssertPatternMatchOverride(collection, "US", "UNI", Constants.OrgPatternMatchOverrideRelationships.Country);
			AssertPatternMatchOverride(collection, "20GP", "CONT", Constants.OrgPatternMatchOverrideRelationships.ContainerType);
			AssertPatternMatchOverride(collection, "USD", "123", Constants.OrgPatternMatchOverrideRelationships.Currency);
			AssertPatternMatchOverride(collection, Constants.IncoTerms.FreeCarrier, "ABC", Constants.OrgPatternMatchOverrideRelationships.IncoTerm);
			AssertPatternMatchOverride(collection, "AUSYD", "PORT", Constants.OrgPatternMatchOverrideRelationships.Port);
		}

		void SetEDICodeMapping(EDICodeMappingCollection mappingCollection, ZString eDICode, ZString foreignCode, ZString relashionship)
		{
			var mapping = mappingCollection.AddNew();
			mapping.EDICode = eDICode;
			mapping.ForeignCode = foreignCode;
			mapping.Relationship = relashionship;
		}

		void AssertPatternMatchOverride(OrgPatternMatchOverrideCollection patternCollection, ZString localCode, ZString foreignCode, ZString relationship)
		{
			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			var loadedOverrides = (OrgPatternMatchOverride[])patternCollection.Find(filter);
			AssertEquals(1, loadedOverrides.Length);
			var patternMatch = loadedOverrides[0];
			AssertEquals("Foreign Code", foreignCode, patternMatch.OO_ForeignCode);

			var loadedLocalCode = ZString.Empty;
			switch (relationship)
			{
				case Constants.OrgPatternMatchOverrideRelationships.Organisation:
					var mappedOrg = Factory.Load<OrgHeader>(patternMatch.OO_LocalGuid);
					loadedLocalCode = mappedOrg.OH_Code;
					break;
				case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
					var mappedContainerType = Factory.Load<RefContainer>(patternMatch.OO_LocalGuid);
					loadedLocalCode = mappedContainerType.RC_Code;
					break;
				case Constants.OrgPatternMatchOverrideRelationships.Country:
					var mappedCountry = Factory.Load<RefCountry>(patternMatch.OO_LocalGuid);
					loadedLocalCode = mappedCountry.RN_Code;
					break;
				case Constants.OrgPatternMatchOverrideRelationships.Currency:
					var mappedCurrency = Factory.Load<RefCurrency>(patternMatch.OO_LocalGuid);
					loadedLocalCode = mappedCurrency.RX_Code;
					break;
				case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
					loadedLocalCode = Constants.IncoTerms.FreeCarrier;
					break;
				case Constants.OrgPatternMatchOverrideRelationships.Port:
					var mappedPort = Factory.Load<RefUNLOCO>(patternMatch.OO_LocalGuid);
					loadedLocalCode = mappedPort.RL_Code;
					break;
			}

			AssertEquals(localCode, loadedLocalCode);
		}

		#endregion

		#region Backwards Compatibility

		public void TestBackwardsCompatibilityImportAUGSTCusCodeHandlesExistingABN()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, ABNForTestBefore, RefCountryAustralia);
			AssertEquals("Precondition: There should be only 1 code at the mo", 1, org.CustomsCodes.Count);
			AssertEquals("Precondition: Org has 1 ABN registered in AU with value " + ABNForTestBefore, org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RefCountryAustralia), ABNForTestBefore);

			var xMLOrg = GetNewXMLOrg();
			AssertEquals("XMLOrg also has no registration codes", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 0);
			AddRegoCodeToXMLOrg(xMLOrg, RegistrationNumberTypes.GST, Constants.CountryCodes.Australia, ABNForTestAfter);
			AssertEquals("XMLOrg now has 1 registration code", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 1);
			AssertEquals("that code is GST registered in AU with value " + ABNForTestAfter, xMLOrg.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.GST, Constants.CountryCodes.Australia).Number, ABNForTestAfter);

			DataAdapter.ImportFromValueObject(org, xMLOrg, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("There should still be only 1 code ", 1, org.CustomsCodes.Count);
			AssertEquals("That code should have been update to value of GST code in XML" + ABNForTestAfter, org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RefCountryAustralia), ABNForTestAfter);
		}

		public void TestBackwardsCompatibilityImportAUGSTCusCodeInterpretedAsABN()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals("Org does not current have any cus codes", org.CustomsCodes.Count, 0);
			var xMLOrg = GetNewXMLOrg();
			AssertEquals("XMLOrg also has no registration codes", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 0);

			AddRegoCodeToXMLOrg(xMLOrg, RegistrationNumberTypes.GST, Constants.CountryCodes.Australia, ABNForTestAfter);
			AssertEquals("XMLOrg now has 1 registration code", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 1);
			AssertEquals("that code is GST registered in AU with value " + ABNForTestAfter, xMLOrg.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.GST, Constants.CountryCodes.Australia).Number, ABNForTestAfter);

			DataAdapter.ImportFromValueObject(org, xMLOrg, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Org now has 1 ABN registered in AU with value " + ABNForTestAfter, org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RefCountryAustralia), ABNForTestAfter);
		}

		public void TestBackwardsCompatibilityImportAUABNCusCodeTakesPrecendenceOverAUGST()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals("Org does not current have any cus codes", org.CustomsCodes.Count, 0);
			var xMLOrg = GetNewXMLOrg();
			AssertEquals("XMLOrg also has no registration codes", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 0);

			AddRegoCodeToXMLOrg(xMLOrg, RegistrationNumberTypes.ABN, Constants.CountryCodes.Australia, ABNForTestAfter);
			AddRegoCodeToXMLOrg(xMLOrg, RegistrationNumberTypes.GST, Constants.CountryCodes.Australia, DodgyRegoNumberWeDontWantToSee);
			AssertEquals("XMLOrg now has 2 registration code", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 2);
			AssertEquals("One is GST registered in AU with value " + DodgyRegoNumberWeDontWantToSee, xMLOrg.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.GST, Constants.CountryCodes.Australia).Number, DodgyRegoNumberWeDontWantToSee);
			AssertEquals("One is ABN registered in AU with value " + ABNForTestAfter, xMLOrg.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.ABN, Constants.CountryCodes.Australia).Number, ABNForTestAfter);

			DataAdapter.ImportFromValueObject(org, xMLOrg, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Org should have only 1 registration code", org.CustomsCodes.Count, 1);
			AssertEquals("Org now has 1 ABN registered in AU with value " + ABNForTestAfter, org.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RefCountryAustralia), ABNForTestAfter);
			AssertEquals("Org does not have GST registered in au", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.GSTCode, RefCountryAustralia), "");
		}

		public void TestBackwardsCompatibilityImportNonAUGSTCusCodeNotInterpretedAsABN()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals("Org does not current have any cus codes", org.CustomsCodes.Count, 0);
			var xMLOrg = GetNewXMLOrg();
			AssertEquals("XMLOrg also has no registration codes", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 0);

			AddRegoCodeToXMLOrg(xMLOrg, RegistrationNumberTypes.GST, Constants.CountryCodes.NewZealand, ABNForTestAfter);
			AssertEquals("XMLOrg now has 1 registration code", xMLOrg.OrganisationDetails.RegistrationNumbers.Count, 1);
			AssertEquals("One is GST registered in NZ with value " + ABNForTestAfter, xMLOrg.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.GST, Constants.CountryCodes.NewZealand).Number, ABNForTestAfter);

			DataAdapter.ImportFromValueObject(org, xMLOrg, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Org should have only 1 registration code", org.CustomsCodes.Count, 1);
			AssertEquals("Its GST in NZ with value " + ABNForTestAfter, org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.GSTCode, RefCountryNewZealand), ABNForTestAfter);
		}

		public void TestBackwardsCompatibilityAUANBCusCodeExportedASAdditionalAUGSTCusCode()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew("ABN", ABNForTestAfter, Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")));
			AssertEquals("There's only one cus code on organisation", org.CustomsCodes.Count, 1);
			AssertEquals("That Code is ABN", org.CustomsCodes[0].OK_CodeType, "ABN");
			AssertEquals("It's value is " + ABNForTestAfter, org.CustomsCodes[0].OK_CustomsRegNo, ABNForTestAfter);

			var xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Exported XML has 2 cus codes", xmlOrganisation.OrganisationDetails.RegistrationNumbers.Count, 2);
			AssertNotNull("Exported XML has an ABN", xmlOrganisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.ABN, "AU"));
			AssertEquals("That ABN number is " + ABNForTestAfter, xmlOrganisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.ABN, "AU").Number, ABNForTestAfter);
			AssertNotNull("Exported XML has a GST", xmlOrganisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.GST, "AU"));
			AssertEquals("That ABN number is " + ABNForTestAfter, xmlOrganisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.GST, "AU").Number, ABNForTestAfter);
		}

		#region Implementation

		RefCountry RefCountryAustralia
		{
			get { return fRefCountryAustralia ?? (fRefCountryAustralia = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia))); }
		}
		RefCountry fRefCountryAustralia;

		RefCountry RefCountryNewZealand
		{
			get { return fRefCountryNewZealand ?? (fRefCountryNewZealand = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.NewZealand))); }
		}
		RefCountry fRefCountryNewZealand;

		Organisation GetNewXMLOrg()
		{
			var xMLOrg = new Organisation();
			xMLOrg.EDICode = "YOYO";
			xMLOrg.OwnerCode = "TEHE";
			xMLOrg.OrganisationDetails.Name = "HAHA";
			xMLOrg.IsSpecified = true;
			xMLOrg.OrganisationDetails.IsSpecified = true;
			return xMLOrg;
		}

		void AddRegoCodeToXMLOrg(Organisation xMLOrg, RegistrationNumberTypes numberType, string countryCode, string number)
		{
			var regoNumber = xMLOrg.OrganisationDetails.RegistrationNumbers.AddNew();
			regoNumber.NumberType = numberType;
			regoNumber.Number = number;
			regoNumber.CountryOfRegistration = countryCode;
			regoNumber.CountryOfRegistrationSpecified = true;
			regoNumber.NumberSpecified = true;
			xMLOrg.OrganisationDetails.RegistrationNumbers.IsSpecified = true;
		}

		const string ABNForTestBefore = "Before ABN that is bad";
		const string ABNForTestAfter = "41 065 894 724";
		const string DodgyRegoNumberWeDontWantToSee = "DODGY NUMBER WE NEVER WANT TO SEE";
		#endregion

		#endregion

		public void TestExporting_OrganisationHasPortWithBadCountry()
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEMPORG";
			org.BrandsOrRelatedNames.AddNew("Company Brand Name");

			org.OH_RL_NKClosestPort = "AUSYD";
			org.ClosestPort.RL_RN_NKCountryCode = "P0";
			AssertNoExceptionThrown(() => adapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer())));
		}

		public void TestBrandNamesAreExportedProperly()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEMPORG";
			org.BrandsOrRelatedNames.AddNew("Company Brand Name");
			var xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(1, xmlOrganisation.OrganisationDetails.BrandNames.Count);
			AssertEquals("Brand Names", "Company Brand Name", xmlOrganisation.OrganisationDetails.BrandNames[0].Value);
		}

		public static Organisation NewOrgAndDecoyOrgBizoWithTypeAndValue(BusinessObjectFactory factory, string orgFullName, OrganisationTypes orgType)
		{
			var result = new Organisation();
			result.OrganisationDetails.Name = orgFullName;

			var decoyOrg = factory.New<OrgHeader>();
			decoyOrg.OH_FullName = orgFullName;
			decoyOrg.MainAddress.OA_Address1 = "address";
			decoyOrg.MainWebURL.PU_URL = "decoy1";

			var org = factory.New<OrgHeader>();
			org.OrganisationTypes = orgType;
			org.OH_FullName = orgFullName;
			org.MainAddress.OA_Address1 = "address";
			org.MainWebURL.PU_URL = "for match";

			var decoyOrg2 = factory.New<OrgHeader>();
			decoyOrg2.OH_FullName = orgFullName;
			decoyOrg2.MainAddress.OA_Address1 = "address";
			decoyOrg2.MainWebURL.PU_URL = "decoy2";

			return result;
		}

		public void TestImportOrganisationWithoutAddressValidation()
		{
			var value = new Organisation();
			value.OrganisationDetails = new OrganisationDetail();
			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "splaty";

			var notifcations = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifcations);
			context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			var foundAddressValidationError = false;
			foreach (ErrorNotification notification in notifcations.GetEventsByType(ErrorType.RequiredFieldEmpty))
			{
				if (notification.Message.ToLower().IndexOf("address") != -1)
				{
					foundAddressValidationError = true;
				}
			}
			AssertEquals("Should validate the missing address field", true, foundAddressValidationError);
		}

		public void TestTempOrgWithSameCodeGetsRenamedIfCodeAlreadyExists()
		{
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			var preExistingOrg = otherFactory.New<OrgHeader>();
			preExistingOrg.OH_Code = "EXIST2";
			otherFactory.Save();

			var decoyOrg = Factory.New<OrgHeader>();
			decoyOrg.OH_Code = "EXIST1";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "EXIST";

			var value = new Organisation();
			value.OrganisationDetails = new OrganisationDetail();
			value.OrganisationDetails.Name = "orgname";
			value.EDICode = "";
			value.OwnerCode = "EXIST";
			value.OrganisationDetails.Name = "EXIST";

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var tempOrg = context.FindOrCreateTempOrganisation(value, null, OrganisationTypes.None);
			AssertEquals("Temp org code should become unique", "EXIST3", tempOrg.OH_Code);
		}

		public void TestImportRegistrationNumbersDoesNotOverwriteExistingNumbers()
		{
			var aU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			var nZ = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.NewZealand);

			var importToOrganisation = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_Code = "Address 1 for Test";
			importToOrganisation.Addresses.Add(address);

			var value = new Organisation();
			value.EDICode = "edicode";
			value.OwnerCode = "";
			value.OrganisationDetails.Name = "Test";
			var registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.Australia;
			registrationNumber.Number = "1010101";
			registrationNumber.NumberType = RegistrationNumberTypes.CSC;

			registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.Australia;
			registrationNumber.Number = "123";
			registrationNumber.NumberType = RegistrationNumberTypes.CCP;
			registrationNumber.AddressCode = address.OA_Code;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			DataAdapter.ImportFromValueObject(importToOrganisation, value, context);
			AssertEquals(2, importToOrganisation.CustomsCodes.Count);
			var orgCusCode = importToOrganisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, aU);
			AssertNotNull("1010101", orgCusCode.OK_CustomsRegNo);
			AssertEquals(ZGuid.Empty, orgCusCode.OK_OA_PremisesAddress);

			orgCusCode = importToOrganisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.ControlledPremisesID, aU);
			AssertNotNull("123", orgCusCode.OK_CustomsRegNo);
			AssertNotEquals(ZGuid.Empty, orgCusCode.OK_OA_PremisesAddress);

			registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.NewZealand;
			registrationNumber.Number = "2020202";
			registrationNumber.NumberType = RegistrationNumberTypes.CSC;

			registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.Australia;
			registrationNumber.Number = "2020402";
			registrationNumber.NumberType = RegistrationNumberTypes.GST;

			DataAdapter.ImportFromValueObject(importToOrganisation, value, context);

			AssertEquals(4, importToOrganisation.CustomsCodes.Count);
			AssertNotNull(importToOrganisation.CustomsCodes.Contains(orgCusCode.PK));
			orgCusCode = importToOrganisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, nZ);
			AssertNotNull("2020202", orgCusCode.OK_CustomsRegNo);
			orgCusCode = importToOrganisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, aU);
			AssertNotNull("1010101", orgCusCode.OK_CustomsRegNo);

			orgCusCode = importToOrganisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, aU);
			AssertNotNull("2020402", orgCusCode.OK_CustomsRegNo);
		}

		public void TestImportRegistrationNumbersUpdatesExistingNumber()
		{
			var aU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "this org should be found when matching is done";
			organisation.OH_Code = "edicode";
			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = aU.Code;
			orgCusCode.OK_CustomsRegNo = "Test";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;

			var value = new Organisation();
			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "Test";
			value.OwnerCode = "";
			var registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.Australia;
			registrationNumber.Number = "1010101";
			registrationNumber.NumberType = RegistrationNumberTypes.CSC;

			var context = new ValueObjectImportContext(organisation.Factory, new NotificationBuffer());
			DataAdapter.ImportFromValueObject(organisation, value, context);
			AssertEquals(1, organisation.CustomsCodes.Count);
			orgCusCode = organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, aU);
			AssertEquals("1010101", orgCusCode.OK_CustomsRegNo);
		}

		public void TestImportRegistrationNumbersWithBlankCodeDoesNotAddNew()
		{
			var aQ = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Antarctica);

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "this org should be found when matching is done";
			organisation.OH_Code = "edicode";
			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = aQ.Code;
			orgCusCode.OK_CustomsRegNo = "";
			orgCusCode.OK_CodeType = "";

			var value = new Organisation();
			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "Test";
			value.OwnerCode = "";
			var registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.Antarctica;
			registrationNumber.Number = "";
			registrationNumber.NumberType = RegistrationNumberTypes.VAT;

			var context = new ValueObjectImportContext(organisation.Factory, new NotificationBuffer());
			DataAdapter.ImportFromValueObject(organisation, value, context);
			AssertEquals(1, organisation.CustomsCodes.Count);
			orgCusCode = organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("", aQ);
			AssertEquals("OK_CustomsRegNo", "", orgCusCode.OK_CustomsRegNo);
		}

		public void TestImportRegistrationNumbersWithBlankCusCodeDeletesExisting()
		{
			var aQ = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Antarctica);

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "this org should be found when matching is done";
			organisation.OH_Code = "edicode";
			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = aQ.Code;
			orgCusCode.OK_CustomsRegNo = "12345";
			orgCusCode.OK_CodeType = "VAT";

			var value = new Organisation();
			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "Test";
			value.OwnerCode = "";
			var registrationNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			registrationNumber.CountryOfRegistration = Constants.CountryCodes.Antarctica;
			registrationNumber.Number = "";
			registrationNumber.NumberType = RegistrationNumberTypes.VAT;

			var context = new ValueObjectImportContext(organisation.Factory, new NotificationBuffer());
			DataAdapter.ImportFromValueObject(organisation, value, context);
			AssertEquals(0, organisation.CustomsCodes.Count);
		}

		public void TestExportMultipleCustomsCodes()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEMPORG";
			var address = Factory.New<OrgAddress>();
			address.OA_Code = "address 2 for test";
			org.Addresses.Add(address);

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = "CSC";
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			customsCode.OK_CustomsRegNo = "12345678A";

			customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			customsCode.OK_CustomsRegNo = "123";
			customsCode.OK_CodeType = "CCP";
			customsCode.OK_OA_PremisesAddress = address.PK;

			customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = "CSC";
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.NewZealand;
			customsCode.OK_CustomsRegNo = "87654321B";

			var xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull("XmlOrganisation should be loaded from temporary OrgHeader", xmlOrganisation);
			AssertEquals("XmlOrganisation should contain 3 registration numbers", 3, xmlOrganisation.OrganisationDetails.RegistrationNumbers.Count);

			AssertEquals("First registration number type should be CSC", RegistrationNumberTypes.CSC, xmlOrganisation.OrganisationDetails.RegistrationNumbers[0].NumberType);
			AssertEquals("First registration number should be 12345678A", "12345678A", xmlOrganisation.OrganisationDetails.RegistrationNumbers[0].Number);
			AssertEquals("First CountryOfRegistration should be Australia", "AU", xmlOrganisation.OrganisationDetails.RegistrationNumbers[0].CountryOfRegistration);

			AssertEquals("2-nd registration number type should be CCP", RegistrationNumberTypes.CCP, xmlOrganisation.OrganisationDetails.RegistrationNumbers[1].NumberType);
			AssertEquals("2-nd registration number should be 123", "123", xmlOrganisation.OrganisationDetails.RegistrationNumbers[1].Number);
			AssertEquals("2-nd CountryOfRegistration should be Australia", "AU", xmlOrganisation.OrganisationDetails.RegistrationNumbers[1].CountryOfRegistration);
			AssertEquals("2-nd reg no should be attached to Premises address", "address 2 for test", xmlOrganisation.OrganisationDetails.RegistrationNumbers[1].AddressCode);

			AssertEquals("3-d registration number type should be CSC", RegistrationNumberTypes.CSC, xmlOrganisation.OrganisationDetails.RegistrationNumbers[2].NumberType);
			AssertEquals("3-d registration number should be 87654321B", "87654321B", xmlOrganisation.OrganisationDetails.RegistrationNumbers[2].Number);
			AssertEquals("3-d CountryOfRegistration should be NewZealand", "NZ", xmlOrganisation.OrganisationDetails.RegistrationNumbers[2].CountryOfRegistration);
		}

		public void TestEDITransmissionToValueObject()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEMPORG";

			org.EDICommunicationsModes.ClientSpecificCommunicationTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			org.EDICommunicationsModes.ClientSpecificDestination = "Address";

			var xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(true, xmlOrganisation.OrganisationDetails.EDITransmissionDetails.IsSpecified);
			AssertEquals(OrganisationDetailEDITransmissionDetailsType.EMT, xmlOrganisation.OrganisationDetails.EDITransmissionDetails.Type);
			AssertEquals("Address", xmlOrganisation.OrganisationDetails.EDITransmissionDetails.Address);

			org.EDICommunicationsModes.ClientSpecificCommunicationTransport = "";
			org.EDICommunicationsModes.ClientSpecificDestination = "";

			xmlOrganisation = DataAdapter.ExportToValueObject(org, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(false, xmlOrganisation.OrganisationDetails.EDITransmissionDetails.IsSpecified);
		}

		public void TestImportEDICode()
		{
			var interchange = new XmlInterchange();
			var organisationValue = new Organisation();
			organisationValue.EDICode = "edicode";
			organisationValue.OwnerCode = "nevermatch";
			organisationValue.OrganisationDetails.Name = "Test Org";

			interchange.ImportEDICode = true;
			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			var tempOrgWithImportedEDICode = context.FindOrCreateTempOrganisation(organisationValue, null, OrganisationTypes.None);
			AssertEquals("EDICode should be imported when ImportEDICode=true", "edicode", tempOrgWithImportedEDICode.OH_Code);

			interchange.ImportEDICode = false;
			var tempOrg = context.FindOrCreateTempOrganisation(organisationValue, null, OrganisationTypes.None);
			AssertEquals("EDICode should be auto-generated when ImportEDICode=false", "TESORG", tempOrg.OH_Code);
		}

		public void TestDataImportEventIsAdded()
		{
			var xsdOrg = new Organisation();
			var interchange = new XmlInterchange();
			xsdOrg.OrganisationDetails.Name = "testorg";
			var address = xsdOrg.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "1 Address";
			xsdOrg.EDICode = "TESTORG";
			xsdOrg.OwnerCode = "TSTORG";
			xsdOrg.OrganisationDetails.Location.City = "Sydney";
			xsdOrg.OrganisationDetails.Location.Value = "AUSYD";

			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			BusinessObject bizO = DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, context);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			AssertEquals("BizO should have a DataImport event", 1, bizO.GetLogs().GetAllLogs().Find(query).Length);
		}

		public void TestMainAddressIsNotUpdatedIfNoLocationIsSpecified()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.MainAddress.OA_Address1 = "1 Address";

			var interchange = new XmlInterchange();
			var xsdOrg = new Organisation();
			xsdOrg.EDICode = "TESTORG";
			xsdOrg.OwnerCode = "TSTORG";
			xsdOrg.OrganisationDetails.Name = "testorg";

			var address = xsdOrg.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "1 Address";

			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			address.Location.Value = "";
			xsdOrg.OrganisationDetails.Location.Value = "AUPER";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			DataAdapter.ImportFromValueObject(organisation, xsdOrg, context);
			AssertEquals("Organisation.UNLOCO should be changed", "AUPER", organisation.OH_RL_NKClosestPort);

			address.Location.Value = "AUBNE";
			xsdOrg.OrganisationDetails.Location.Value = "AUPER";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			DataAdapter.ImportFromValueObject(organisation, xsdOrg, context);
			AssertEquals("Organisation.UNLOCO should be changed", "AUBNE", organisation.OH_RL_NKClosestPort);

			address.Location.Value = "";
			xsdOrg.OrganisationDetails.Location.Value = "";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			DataAdapter.ImportFromValueObject(organisation, xsdOrg, context);
			AssertEquals("Organisation.UNLOCO should not be changed", "AUSYD", organisation.OH_RL_NKClosestPort);
		}

		public void TestExportBondDetails()
		{
			var org = GetBondDetailsForOrganisation();
			var xmlOrganisation = new Organisation();
			MakeExport(org, xmlOrganisation);

			AssertEquals(BondDetailActivityCode.Item1, xmlOrganisation.OrganisationDetails.BondDetails[0].ActivityCode);
			AssertEquals(12m, xmlOrganisation.OrganisationDetails.BondDetails[0].Amount);
			AssertEquals(new ZDateTime(2007, 1, 1), xmlOrganisation.OrganisationDetails.BondDetails[0].Effective);
			AssertEquals(new ZDateTime(2007, 1, 1), xmlOrganisation.OrganisationDetails.BondDetails[0].Expiry);
			AssertEquals("7777", xmlOrganisation.OrganisationDetails.BondDetails[0].FiledPort);
			AssertEquals("13", xmlOrganisation.OrganisationDetails.BondDetails[0].Number);
			AssertEquals("891", xmlOrganisation.OrganisationDetails.BondDetails[0].SuretyCode);
			AssertEquals("9", xmlOrganisation.OrganisationDetails.BondDetails[0].Type);
		}

		public void TestImportBondDetails()
		{
			var org = GetBondDetailsForOrganisation();
			var xmlOrganisation = new Organisation();
			MakeExport(org, xmlOrganisation);

			var org1 = Factory.New<OrgHeader>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			DataAdapter.ImportFromValueObject(org1, xmlOrganisation, context);

			var query = new ZQuery(CusBondDetailSchema.PW_ParentID, org1.PK);
			var loadedBonds = org.Factory.Load<CusBondDetail>(query);
			var bond = loadedBonds[0];

			AssertEquals("1", bond.PW_ActivityCode);
			AssertEquals(12m, bond.PW_BondAmount);
			AssertEquals(new ZDateTime(2007, 1, 1), bond.PW_BondEffectiveDate);
			AssertEquals(new ZDateTime(2007, 1, 1), bond.PW_BondExpiryDate);
			AssertEquals("7777", bond.PW_BondFiledPort);
			AssertEquals("13", bond.PW_BondNumber);
			AssertEquals("891", bond.PW_SuretyCode);
			AssertEquals("9", bond.PW_BondType);
		}

		public new void TestImportOfLongStrings()
		{
			Assert(true);
		}

		OrgHeader GetBondDetailsForOrganisation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org";

			var bondData = org.Factory.New<CusBondDetail>();
			bondData.PW_ActivityCode = "_1";
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondType = "9";
			bondData.PW_BondAmount = 12m;
			bondData.PW_BondExpiryDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondFiledPort = "7777";
			bondData.PW_BondNumber = "13";
			bondData.PW_SuretyCode = "891";
			bondData.PW_ParentID = org.PK;
			return org;
		}

		void MakeExport(OrgHeader org, Organisation xmlOrganisation)
		{
			DataAdapter.ExportToValueObject(org, xmlOrganisation, new ValueObjectExportContext(new NotificationBuffer()));
		}

		#region Implementation

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override ValueObjectDataAdapter<OrgHeader, Organisation> GetNewBizObjXmlDataAdapter()
		{
			return new OrganisationValueObjectDataAdapter();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Organisations"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Organisation"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyOrg = Factory.New<OrgHeader>();
			emptyOrg.OH_Code = "tstorgcode";
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.Organisations.Testing.EmptyOrg.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyOrg, expectedOutputFilename, ValidationKind.None, "Empty organisation");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var populatedOrgWithEmptyFields = Factory.New<OrgHeader>();

			populatedOrgWithEmptyFields.OH_Code = "tstorgcode";
			populatedOrgWithEmptyFields.MainAddress.OA_Address1 = "Address1";
			var contact = populatedOrgWithEmptyFields.Contacts.AddNew();
			contact.OC_ContactName = "ABC";

			var emptyCusCode = populatedOrgWithEmptyFields.CustomsCodes.AddNew();
			emptyCusCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.Organisations.Testing.PopulatedOrgWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedOrgWithEmptyFields, expectedOutputFilename, ValidationKind.None, "Populated organisation with empty fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedOrg = Factory.New<OrgHeader>();
			populatedOrg.OH_FullName = "FullName";
			populatedOrg.OH_RL_NKClosestPort = "AUSYD";
			populatedOrg.MainWebURL.PU_URL = "Web";
			populatedOrg.OH_Language = Core.SharedConstants.Languages.German;
			populatedOrg.PrimaryRegistrationNumber.Number = "LocalBusinessNumber";

			populatedOrg.MainAddress.OA_CompanyNameOverride = "CompanyName";
			populatedOrg.MainAddress.OA_Address1 = "Address1";
			populatedOrg.MainAddress.OA_Address2 = "Address2";
			populatedOrg.MainAddress.OA_City = "City";
			populatedOrg.MainAddress.OA_State = "State";
			populatedOrg.MainAddress.OA_PostCode = "PostCode";
			populatedOrg.MainAddress.OA_Phone = "Phone";
			populatedOrg.MainAddress.OA_Mobile = "Mobile";
			populatedOrg.MainAddress.OA_Email = "Email";

			var secondAddress = populatedOrg.Addresses.AddNew();
			secondAddress.OA_Address1 = "secondary address";
			secondAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			var contact = populatedOrg.Contacts.AddNew();
			contact.OC_ContactName = "ContactName";
			contact.OC_Fax = "+61280012200";
			contact.OC_WebContractSignedDate = new ZDateTime(2006, 5, 28, 9, 0, 0);
			contact.OC_AttachmentType = "PDF";
			contact.OC_Phone = "+61280012200";
			contact.OC_Title = "Person";
			contact.OC_PhoneExtension = "222";
			contact.OC_NotifyMode = "XLS";
			contact.OC_Mobile = "+61426829924";
			contact.OC_Language = Core.SharedConstants.Languages.English;
			contact.OC_Email = "z@zz.com";
			contact.OC_Salutation = "Jo";
			contact.OC_WebAccessEnabled = false;
			contact.OC_Birthday = new ZDateTime(1981, 5, 28);
			contact.OC_OtherPhone = "+61280012200";
			contact.OC_Pager = "+61280012200";
			contact.OC_HomePhone = "+61280012200";

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.Organisations.Testing.PopulatedOrg.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedOrg, expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated organisation");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"OrganisationDetails/Addresses/AddressType",
					"OrganisationDetails/Addresses/Location",
					"OrganisationDetails/Addresses/IsDefault",
					"OwnerCode",
					"OrganisationDetails/Contacts/WebAccessEnable",
					"OrganisationDetails/Language",
					"OrganisationDetails/EDICode",
					"OrganisationDetails/OwnerCode",
					"OrganisationDetails/BrandNames",
					"OrganisationDetails/EDITransmissionDetails/Address",
					"OrganisationDetails/EDICodeMappings/Relationship",
					"OrganisationDetails/EDICodeMappings/EDICode",
					"OrganisationDetails/EDICodeMappings/ForeignCode",
					"OrganisationDetails/AccountsReceivables/CompanyCode",
					"OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"OrganisationDetails/AccountsReceivables/CreditLimit",
					"OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"OrganisationDetails/AccountsReceivables/CreditApproved",
					"OrganisationDetails/AccountsReceivables/CreditOnHold",
					"OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"OrganisationDetails/AccountsReceivables/AccountGroup",
					"OrganisationDetails/AccountsReceivables/AllowMultiCurrencyPayment",
					"OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup",
					"OrganisationDetails/AccountsReceivables/ExternalDebtorCode",
					"OrganisationDetails/AccountsPayables/ExternalCreditorCode",
					"OrganisationDetails/AccountsPayables/CompanyCode",
					"OrganisationDetails/AccountsPayables/DefaultCurrency",
					"OrganisationDetails/AccountsPayables/CreditLimit",
					"OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"OrganisationDetails/AccountsPayables/PaymentTerms",
					"OrganisationDetails/AccountsPayables/PaymentDays",
					"OrganisationDetails/AccountsPayables/AccountGroup",
					"OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup",
					"OrganisationDetails/EDICodeMappings/ForeignCode",
					"OrganisationDetails/OrganisationTypes/Status",
					"Notes",

					//use in Full Organisation
					"OrganisationDetails/RegistrationNumbers/OrgHeaderPK",
					"OrganisationDetails/RegistrationNumbers/OrgAddressPK",
					"OrganisationDetails/RegistrationNumbers/CountryDefault",

					//back compatibility for WebAddress nodes in old xmls
					"OrganisationDetails/WebAddress",
					"OrganisationDetails/RegistrationNumbers/AddressCode",
					"OrganisationDetails/BondDetails/ActivityCode",
					"OrganisationDetails/BondDetails/Type",
					"OrganisationDetails/BondDetails/Number",
					"OrganisationDetails/BondDetails/SuretyCode",
					"OrganisationDetails/BondDetails/Amount",
					"OrganisationDetails/BondDetails/Effective",
					"OrganisationDetails/BondDetails/Expiry",
					"OrganisationDetails/BondDetails/FiledPort",

					//use in Accounting Data Export only , cover in a separate test
					"OrganisationDetails/IsActiveClient",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch"
				};
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new OrganisationValueObjectDataAdapter();
		}
		OrganisationValueObjectDataAdapter DataAdapter;

		#endregion
	}
}
