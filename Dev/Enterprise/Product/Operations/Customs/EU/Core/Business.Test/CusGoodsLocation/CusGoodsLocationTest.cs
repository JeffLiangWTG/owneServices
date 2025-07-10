using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestParentLoaderForCusTempStorageJobHeader()
		{
			var tempStorageJobHeader = Factory.New<CusTempStorageJobHeader>();
			location.CGL_ParentID = tempStorageJobHeader.PK;
			location.CGL_ParentTableCode = tempStorageJobHeader.TablePrefix;
			NUnit.Framework.Assert.That(location.Parent, NUnit.Framework.Is.SameAs(tempStorageJobHeader));

			var tempStorageJobHeaderUCC6 = Factory.New<TemporaryStorageHeader>();
			location.CGL_ParentID = tempStorageJobHeaderUCC6.PK;
			location.CGL_ParentTableCode = tempStorageJobHeaderUCC6.TablePrefix;
			NUnit.Framework.Assert.That(location.Parent, NUnit.Framework.Is.SameAs(tempStorageJobHeaderUCC6));
		}

		[ExpectNoExceptions]
		public void TestAddressIsRegisteredEditableChildObject()
		{
			NUnit.Framework.Assert.That(location.IsRegisteredEditableChildObject(location.Address), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		public void TestCusGoodsLocationAddressIsDeletedCorrectly()
		{
			var location = Factory.New<CusGoodsLocation>();
			location.Parent = Factory.NewWithValidTestData<JobDeclaration>();
			location.CGL_LocationUse = "DEP";
			var locationAddress = location.Address;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			location = newFactory.Load<CusGoodsLocation>(location.PK);
			location.Delete();
			newFactory.Save();
			AssertEquals("locationAddress.IsDeleted", true, locationAddress.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreateCusGoodsLocationAddress()
		{
			var locationAddress = location.Address;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationAddress, NUnit.Framework.Is.TypeOf<CusGoodsLocationAddress>(), "Type");
				NUnit.Framework.Assert.That(locationAddress.HasChanges, NUnit.Framework.Is.EqualTo(false), "HasChanges should not be set on creating");
				NUnit.Framework.Assert.That(locationAddress.E2_ParentID, NUnit.Framework.Is.EqualTo(location.PK), "E2_ParentID");
				NUnit.Framework.Assert.That(locationAddress.E2_ParentTableCode, NUnit.Framework.Is.EqualTo(location.TablePrefix).Using(CustomComparers.TypeComparison), "E2_ParentTableCode");
				NUnit.Framework.Assert.That(locationAddress.E2_AddressType, NUnit.Framework.Is.EqualTo(DocAddressTypes.Codes.Location).Using(CustomComparers.TypeComparison), "E2_AddressType");
				NUnit.Framework.Assert.That(locationAddress.E2_AddressOverride, NUnit.Framework.Is.EqualTo(ZBool.True), "E2_AddressOverride");
				NUnit.Framework.Assert.That(locationAddress.E2_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_RN_NKCountryCode");
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedLocation = newFactory.Load<CusGoodsLocation>(location.PK);
				var loadedLocationAddress = loadedLocation.Address;
				NUnit.Framework.Assert.That(loadedLocationAddress.PK, NUnit.Framework.Is.EqualTo(locationAddress.PK), "Same Location Address");
			});
		}

		[ExpectNoExceptions]
		public void TestCanSetAddressProperties_IdentificationHolderNotSelected()
		{
			var locationAddress = location.Address;
			locationAddress.E2_RN_NKCountryCode = "AU";
			locationAddress.E2_GeoLocation = ZGeography.CreatePoint(123.123, 12.12);
			locationAddress.E2_GovRegNum = "123";
			locationAddress.E2_GovRegNumType = "EOR";
			locationAddress.E2_Address1AndE2_Address2 = "100 Sydney St";
			locationAddress.E2_City = "Sydney";
			locationAddress.E2_Postcode = "2000";
			locationAddress.E2_Contact = "John Smith";
			locationAddress.E2_Phone = "92838271";
			locationAddress.E2_Email = "john.smith@wisetechglobal.com";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationAddress.HasRealAddress, NUnit.Framework.Is.EqualTo(false), "HasRealAddress");
				NUnit.Framework.Assert.That(locationAddress.E2_RN_NKCountryCode, NUnit.Framework.Is.EqualTo("AU").Using(CustomComparers.TypeComparison), "E2_RN_NKCountryCode");
				NUnit.Framework.Assert.That(locationAddress.E2_GeoLocation, NUnit.Framework.Is.EqualTo(ZGeography.CreatePoint(123.123, 12.12)), "E2_GeoLocation");
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNum, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "E2_GovRegNum");
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNumType, NUnit.Framework.Is.EqualTo("EOR").Using(CustomComparers.TypeComparison), "E2_GovRegNumType");
				NUnit.Framework.Assert.That(locationAddress.E2_Address1AndE2_Address2, NUnit.Framework.Is.EqualTo("100 Sydney St").Using(CustomComparers.TypeComparison), "E2_Address1AndE2_Address2");
				NUnit.Framework.Assert.That(locationAddress.E2_City, NUnit.Framework.Is.EqualTo("Sydney").Using(CustomComparers.TypeComparison), "E2_City");
				NUnit.Framework.Assert.That(locationAddress.E2_Postcode, NUnit.Framework.Is.EqualTo("2000").Using(CustomComparers.TypeComparison), "E2_Postcode");
				NUnit.Framework.Assert.That(locationAddress.E2_Contact, NUnit.Framework.Is.EqualTo("John Smith").Using(CustomComparers.TypeComparison), "E2_Contact");
				NUnit.Framework.Assert.That(locationAddress.E2_Phone, NUnit.Framework.Is.EqualTo("92838271").Using(CustomComparers.TypeComparison), "E2_Phone");
				NUnit.Framework.Assert.That(locationAddress.E2_Email, NUnit.Framework.Is.EqualTo("john.smith@wisetechglobal.com").Using(CustomComparers.TypeComparison), "E2_Email");
			});
		}

		[ExpectNoExceptions]
		public void TestAddressE2_ValidationStatus()
		{
			var locationAddress = location.Address;
			using (Environment.Env.Registry.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				locationAddress.E2_RN_NKCountryCode = "US";
				locationAddress.E2_ValidationStatus = "INV";
				locationAddress.Validation.ValidateE2_ValidationStatus();
				NUnit.Framework.Assert.That(locationAddress.IgnoreValidationStatusError, NUnit.Framework.Is.EqualTo(true), "IgnoreValidationStatusError should be equal true");
				NUnit.Framework.Assert.That(locationAddress.E2_ValidationStatusInfo.Notifications.Count(), NUnit.Framework.Is.EqualTo(0), "CheckE2_ValidationStatus does not add error notification");
			}
		}

		[ExpectNoExceptions]
		public void TestCanSetAddressProperties_IdentificationHolderSelected()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var locationAddress = location.Address;
			locationAddress.IdentificationHolderPK = orgHeader.PK;
			locationAddress.E2_RN_NKCountryCode = "AU";
			locationAddress.E2_GeoLocation = ZGeography.CreatePoint(123.123, 12.12);
			locationAddress.E2_GovRegNum = "123";
			locationAddress.E2_GovRegNumType = "EOR";
			locationAddress.E2_Address1AndE2_Address2 = "100 Sydney St";
			locationAddress.E2_City = "Sydney";
			locationAddress.E2_Postcode = "2000";
			locationAddress.E2_Contact = "John Smith";
			locationAddress.E2_Phone = "92838271";
			locationAddress.E2_Email = "john.smith@wisetechglobal.com";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationAddress.HasRealAddress, NUnit.Framework.Is.EqualTo(false), "HasRealAddress");
				NUnit.Framework.Assert.That(locationAddress.E2_RN_NKCountryCode, NUnit.Framework.Is.EqualTo("AU").Using(CustomComparers.TypeComparison), "E2_RN_NKCountryCode");
				NUnit.Framework.Assert.That(locationAddress.E2_GeoLocation, NUnit.Framework.Is.EqualTo(ZGeography.CreatePoint(123.123, 12.12)), "E2_GeoLocation");
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNum, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "E2_GovRegNum");
				NUnit.Framework.Assert.That(locationAddress.E2_GovRegNumType, NUnit.Framework.Is.EqualTo("EOR").Using(CustomComparers.TypeComparison), "E2_GovRegNumType");
				NUnit.Framework.Assert.That(locationAddress.E2_Address1AndE2_Address2, NUnit.Framework.Is.EqualTo("100 Sydney St").Using(CustomComparers.TypeComparison), "E2_Address1AndE2_Address2");
				NUnit.Framework.Assert.That(locationAddress.E2_City, NUnit.Framework.Is.EqualTo("Sydney").Using(CustomComparers.TypeComparison), "E2_City");
				NUnit.Framework.Assert.That(locationAddress.E2_Postcode, NUnit.Framework.Is.EqualTo("2000").Using(CustomComparers.TypeComparison), "E2_Postcode");
				NUnit.Framework.Assert.That(locationAddress.E2_Contact, NUnit.Framework.Is.EqualTo("John Smith").Using(CustomComparers.TypeComparison), "E2_Contact");
				NUnit.Framework.Assert.That(locationAddress.E2_Phone, NUnit.Framework.Is.EqualTo("92838271").Using(CustomComparers.TypeComparison), "E2_Phone");
				NUnit.Framework.Assert.That(locationAddress.E2_Email, NUnit.Framework.Is.EqualTo("john.smith@wisetechglobal.com").Using(CustomComparers.TypeComparison), "E2_Email");
			});
		}

		[ExpectNoExceptions]
		public void TestCGL_QualifierReadonly()
		{
			NUnit.Framework.Assert.That(location.CGL_QualifierInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "CGL_Qualifier readonly");
		}

		[ExpectNoExceptions]
		public void TestCGL_TypeReadonly()
		{
			NUnit.Framework.Assert.That(location.CGL_TypeInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "CGL_Type readonly");
		}

		[ExpectNoExceptions]
		public void TestContactPersonDataVisible() => CombineAssertions(() =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			NUnit.Framework.Assert.That(location.ContactPersonDataVisible, NUnit.Framework.Is.EqualTo(true), "ContactPersonDataVisible");

			location.CGL_Qualifier = ZString.Empty;
			NUnit.Framework.Assert.That(location.ContactPersonDataVisible, NUnit.Framework.Is.EqualTo(false), "ContactPersonDataVisible");
		});

		[ExpectNoExceptions]
		public void TestAddressIdentificationHolderPKChanged_EoriNumber_OneEori()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "DEEOR123");
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			var address = location.Address;
			address.IdentificationHolderPK = orgHeader.PK;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(orgHeader.PK), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo("DEEOR123").Using(CustomComparers.TypeComparison), "E2_GovRegNum");
				NUnit.Framework.Assert.That(address.E2_GovRegNumType, NUnit.Framework.Is.EqualTo("EOR").Using(CustomComparers.TypeComparison), "E2_GovRegNumType");
				NUnit.Framework.Assert.That(address.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "E2_GovRegNum disabled");
			});
		}

		[ExpectNoExceptions]
		public void TestAddressIdentificationHolderPKChanged_EoriNumber_EoriFromCurrentCompanyCountry()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "DEEOR123", Core.Constants.CountryCodes.Germany);
			orgHeader.CustomsCodes.AddNew("EOR", "LVEOR123", Core.Constants.CountryCodes.Latvia);
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			var address = location.Address;
			address.IdentificationHolderPK = orgHeader.PK;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(orgHeader.PK), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo("LVEOR123").Using(CustomComparers.TypeComparison), "E2_GovRegNum");
				NUnit.Framework.Assert.That(address.E2_GovRegNumType, NUnit.Framework.Is.EqualTo("EOR").Using(CustomComparers.TypeComparison), "E2_GovRegNumType");
				NUnit.Framework.Assert.That(address.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "E2_GovRegNum disabled");
			});
		}

		[ExpectNoExceptions]
		public void TestAddressIdentificationHolderPKChanged_EoriNumber_EoriFromRandomCountry()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "ITEOR123", Core.Constants.CountryCodes.Italy);
			orgHeader.CustomsCodes.AddNew("EOR", "DEEOR123", Core.Constants.CountryCodes.Germany);
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			var address = location.Address;
			address.IdentificationHolderPK = orgHeader.PK;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(orgHeader.PK), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo("ITEOR123").Using(CustomComparers.TypeComparison), "E2_GovRegNum");
				NUnit.Framework.Assert.That(address.E2_GovRegNumType, NUnit.Framework.Is.EqualTo("EOR").Using(CustomComparers.TypeComparison), "E2_GovRegNumType");
				NUnit.Framework.Assert.That(address.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "E2_GovRegNum disabled");
			});
		}

		[ExpectNoExceptions]
		public void TestAddressIdentificationHolderPKChanged_EoriNumber_NoEori()
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			location.CGL_AdditionalIdentifier = "Additional Identifier";
			var address = location.Address;
			address.IdentificationHolderPK = Factory.New<OrgHeader>().PK;
			address.E2_GovRegNum = "DEEOR1234";
			address.E2_GovRegNumType = "EOR";
			address.E2_GovRegNumReadOnly = true;

			var orgHeader = Factory.New<OrgHeader>();
			address.IdentificationHolderPK = orgHeader.PK;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(orgHeader.PK), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNum cleared");
				NUnit.Framework.Assert.That(address.E2_GovRegNumType, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNumType cleared");
				NUnit.Framework.Assert.That(address.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "E2_GovRegNum enabled");
				NUnit.Framework.Assert.That(location.CGL_AdditionalIdentifier, NUnit.Framework.Is.EqualTo("Additional Identifier").Using(CustomComparers.TypeComparison), "CGL_AdditionalIdentifier not updated");
			});
		}

		[ExpectNoExceptions]
		public void TestAddressIdentificationHolderPKChanged_EoriNumber_OrganisationInvalid()
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			location.CGL_AdditionalIdentifier = "Additional Identifier";
			var address = location.Address;
			address.IdentificationHolderPK = Factory.New<OrgHeader>().PK;
			address.E2_GovRegNum = "DEEOR1234";
			address.E2_GovRegNumType = "EOR";
			address.E2_GovRegNumReadOnly = true;

			address.IdentificationHolderPK = ZGuid.Invalid;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Invalid), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNum cleared");
				NUnit.Framework.Assert.That(address.E2_GovRegNumType, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNumType cleared");
				NUnit.Framework.Assert.That(address.E2_GovRegNumInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "E2_GovRegNum enabled");
				NUnit.Framework.Assert.That(location.CGL_AdditionalIdentifier, NUnit.Framework.Is.EqualTo("Additional Identifier").Using(CustomComparers.TypeComparison), "CGL_AdditionalIdentifier not updated");
			});
		}

		[ExpectNoExceptions]
		public void TestAddressIdentificationHolderPKChanged_AuthorisationNumber()
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_AdditionalIdentifier = "Additional Identifier";
			var address = location.Address;
			address.IdentificationHolderPK = Factory.New<OrgHeader>().PK;
			address.E2_GovRegNum = "DEEOR1234";

			var orgHeader = Factory.New<OrgHeader>();
			address.IdentificationHolderPK = orgHeader.PK;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(orgHeader.PK), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNum cleared");
				NUnit.Framework.Assert.That(location.CGL_AdditionalIdentifier, NUnit.Framework.Is.EqualTo("Additional Identifier").Using(CustomComparers.TypeComparison), "CGL_AdditionalIdentifier not updated");
			});
		}

		[ExpectNoExceptions]
		public void TestCGL_Qualifier_ClearFields()
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			location.CGL_AdditionalIdentifier = "Additional Identifier";
			var address = location.Address;
			var orgHeader = Factory.New<OrgHeader>();
			address.E2_RN_NKCountryCode = "DE";
			address.E2_GeoLocation = ZGeography.CreatePoint(100.21, 60.37);
			address.IdentificationHolderPK = orgHeader.PK;
			address.OrganisationPK = orgHeader.PK;
			address.E2_GovRegNum = "DEEOR1234";
			address.E2_GovRegNumType = "EOR";
			address.E2_GovRegNumReadOnly = true;
			address.E2_Address1AndE2_Address2 = "100 Sydney Street";
			address.E2_City = "Sydney";
			address.E2_Postcode = "2000";
			address.E2_Contact = "John Smith";
			address.E2_Phone = "9283 2938";
			address.E2_Email = "j.smith@wisetechglobal.com";

			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(address.E2_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_RN_NKCountryCode");
				NUnit.Framework.Assert.That(address.E2_GeoLocation, NUnit.Framework.Is.EqualTo(ZGeography.Empty), "E2_GeoLocation");
				NUnit.Framework.Assert.That(address.IdentificationHolderPK, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "IdentificationHolderPK");
				NUnit.Framework.Assert.That(address.OrganisationPK, NUnit.Framework.Is.EqualTo(ZGuid.Empty), "OrganisationPK");
				NUnit.Framework.Assert.That(address.E2_GovRegNum, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNum");
				NUnit.Framework.Assert.That(address.E2_GovRegNumType, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_GovRegNumType");
				NUnit.Framework.Assert.That(address.E2_GovRegNumReadOnly, NUnit.Framework.Is.EqualTo(false), "E2_GovRegNumReadOnly");
				NUnit.Framework.Assert.That(address.E2_Address1AndE2_Address2, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_Address1AndE2_Address2");
				NUnit.Framework.Assert.That(address.E2_City, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_City");
				NUnit.Framework.Assert.That(address.E2_Postcode, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_Postcode");
				NUnit.Framework.Assert.That(address.E2_Contact, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_Contact");
				NUnit.Framework.Assert.That(address.E2_Phone, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_Phone");
				NUnit.Framework.Assert.That(address.E2_Email, NUnit.Framework.Is.EqualTo(ZString.Empty), "E2_Email");
			});
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(location.Lookups, NUnit.Framework.Is.TypeOf<CusGoodsLocationLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(location.Validation, NUnit.Framework.Is.TypeOf<CusGoodsLocationValidation>());
		}

		[ExpectNoExceptions]
		public void TestUnlocode()
		{
			location.Unlocode = "UNLocode123";
			NUnit.Framework.Assert.That(location.CGL_AdditionalIdentifier, NUnit.Framework.Is.EqualTo("UNLocode123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalIdentifier()
		{
			location.AdditionalIdentifier = "Test";
			NUnit.Framework.Assert.That(location.CGL_AdditionalIdentifier, NUnit.Framework.Is.EqualTo("Test").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalIdentifier_MaxLength()
		{
			NUnit.Framework.Assert.That(location.AdditionalIdentifierInfo.MaxLength, NUnit.Framework.Is.EqualTo(4));
		}

		[ExpectNoExceptions]
		public void TestAdditionalIdentifier_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(typeof(CusGoodsLocation), nameof(CusGoodsLocation.CGL_AdditionalIdentifier)).Caption, NUnit.Framework.Is.EqualTo("Additional Identifier"));
		}

		[ExpectNoExceptions]
		public void TestUnlocode_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(typeof(CusGoodsLocation), nameof(CusGoodsLocation.Unlocode)).Caption, NUnit.Framework.Is.EqualTo("UNLOCODE"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOffice_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(typeof(CusGoodsLocation), nameof(CusGoodsLocation.CGL_CustomsOffice)).Caption, NUnit.Framework.Is.EqualTo("Customs Office"));
		}

		[ExpectNoExceptions]
		public void TestDisplayText()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(location.DisplayText, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "All fields empty");

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				location.CGL_AdditionalIdentifier = "WiseTech Global Sydney";
				location.CGL_CustomsOffice = "FR230023";
				var address = location.Address;
				address.E2_Latitude = -33.9164669;
				address.E2_Longitude = 151.1944765;
				address.E2_GovRegNum = "EOR0001";
				address.E2_Address1AndE2_Address2 = "WiseTech Global Sydney Headquarter, 74 O'Riordan Street";
				address.E2_Postcode = "2015";
				address.E2_City = "Alexandria";
				address.E2_RN_NKCountryCode = "AU";
				address.E2_Contact = "John Smith";
				address.E2_Phone = "02 8001 2200";
				address.E2_Email = "john.smith@wisetechglobal.com";
				NUnit.Framework.Assert.That(location.DisplayText, NUnit.Framework.Is.EqualTo("Z;C;-33.9164669,151.1944765;EOR0001;WiseTech Global Sydney Headquarter, 74 O'Riordan Street;2015;WiseTech Global Sydney;FR230023;Alexandria;AU;Contact John Smith;Ph 02 8001 2200;john.smith@wisetechglobal.com").Using(CustomComparers.TypeComparison), "All fields filled");
			});
		}

		public void TestDisplayText_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				location.DisplayTextInfo,
				multipleResourceKey: JobDeclaration.CaptionKeyImportUCC6,
				caption: "Goods Location",
				fullDescription: string.Empty
			);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				location.DisplayTextInfo,
				multipleResourceKey: string.Empty,
				caption: "Goods Location",
				fullDescription: string.Empty
			);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var location = Factory.New<CusGoodsLocation>();
			location.Parent = Factory.NewWithValidTestData<JobDeclaration>();
			location.CGL_LocationUse = "DEP";
			return location;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			location = Factory.New<CusGoodsLocation>();
			location.Parent = Factory.NewWithValidTestData<JobDeclaration>();
			location.CGL_LocationUse = "DEP";
		}
		CusGoodsLocation location;
	}
}
