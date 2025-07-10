using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	[UseSnapshotProtection]
	public class OrgScreeningStatusPostUpdateTest : TestCase
	{
		#region TestOrgScreenStatusShouldBeNotScreened

		public void TestOrgScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsNotScreened()
		{
			AssertOrgScreeningStatusShouldBeNotScreened_WhenFirstImport(ScreeningStatusesList.Codes.NotScreened);
		}

		public void TestOrgScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsUnknown()
		{
			AssertOrgScreeningStatusShouldBeNotScreened_WhenFirstImport(ScreeningStatusesList.Codes.Unknown);
		}

		public void TestOrgScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsMatched()
		{
			AssertOrgScreeningStatusShouldBeNotScreened_WhenFirstImport(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsClear()
		{
			AssertOrgScreeningStatusShouldBeNotScreened_WhenFirstImport(ScreeningStatusesList.Codes.Clear);
		}

		public void TestOrgScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsPermanentClear()
		{
			AssertOrgScreeningStatusShouldBeNotScreened_WhenFirstImport(ScreeningStatusesList.Codes.PermanentClear);
		}

		#endregion

		#region TestOrgScreeningStatusWithoutPropertyChanges

		public void TestOrgScreeningStatusWithoutPropertyChanges_WhenImportScreeningStatusIsMatched()
		{
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusWithoutPropertyChanges_WhenImportScreeningStatusIsClear()
		{
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear);
		}

		public void TestOrgScreeningStatusWithoutPropertyChanges_WhenImportScreeningStatusIsNotScreened()
		{
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened);
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.NotScreened);
		}

		public void TestOrgScreeningStatusWithoutPropertyChanges_WhenImportScreeningStatusIsUnknown()
		{
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Unknown);
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown);
		}

		public void TestOrgScreeningStatusWithoutPropertyChanges_WhenImportScreeningStatusIsPermanentClear()
		{
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.PermanentClear);
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.PermanentClear);
		}

		public void TestOrgScreeningStatusWithoutPropertyChanges_WhenImportScreeningStatusIsUndefined()
		{
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Clear, "123");
			AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Matched, "123");
		}

		#endregion

		#region TestOrgScreeningStatusChangedToUnknown

		public void TestOrgScreeningStatusChangedToUnknown_WhenOrgNameInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenOrgNameInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenOrgNameInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenAddress1InfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenAddress1InfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenAddress1InfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenAddress2InfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenAddress2InfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenAddress2InfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenCityInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenCityInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenCityInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenStateCodeInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenStateCodeInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenStateCodeInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenPostCodeInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenPostCodeInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenPostCodeInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenCountryCodeInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenCountryCodeInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenCountryCodeInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenClosestPortInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenClosestPortInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenClosestPortInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenBrandOrRelatedNameInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenBrandOrRelatedNameInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenBrandOrRelatedNameInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusHasNotChange_WhenContactNameInfoHasChanges()
		{
			AssertOrgScreeningStatusHasNotChange_WhenContactNameInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusHasNotChange_WhenContactNameInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenOrgActiveInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenOrgActiveInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenOrgActiveInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenRelatedPortInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenRelatedPortInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenRelatedPortInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenCusCodeInfoHasChanges()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenCusCodeInfoHasChanges(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenCusCodeInfoHasChanges(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenANewAddressAdded()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenANewAddressAdded(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenANewAddressAdded(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenANewBrandOrRelatedNameAdded()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenANewBrandOrRelatedNameAdded(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenANewBrandOrRelatedNameAdded(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusHasNotChange_WhenANewContactAdded()
		{
			AssertOrgScreeningStatusHasNotChange_WhenANewContactAdded(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusHasNotChange_WhenANewContactAdded(ScreeningStatusesList.Codes.Matched);
		}

		public void TestOrgScreeningStatusChangedToUnknown_WhenANewCusCodeAdded()
		{
			AssertOrgScreeningStatusChangedToUnknown_WhenANewCusCodeAdded(ScreeningStatusesList.Codes.Clear);
			AssertOrgScreeningStatusChangedToUnknown_WhenANewCusCodeAdded(ScreeningStatusesList.Codes.Matched);
		}

		#endregion

		#region TestOrgScreeningStatusShouldNotBeChanged

		public void TestOrgScreeningStatusShouldNotBeChanged_WhenPropertyHasChange_ButOriginalStatusIsPermanentClear()
		{
			var newName = "NEW NAME";
			AssertNotEquals(OrgFullName, newName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(ScreeningStatusesList.Codes.PermanentClear, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgFullName, newName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("Org name should change", newName, org.OH_FullName);
				AssertEquals("Screening status should be PermanentClear", ScreeningStatusesList.Codes.PermanentClear, org.OH_ScreeningStatus);
			});
			AssertNoLogs(org);
		}

		public void TestOrgScreeningStatusShouldNotBeChanged_WhenPropertyHasChange_ButOriginalStatusIsNotScreened()
		{
			var newName = "NEW NAME";
			AssertNotEquals(OrgFullName, newName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(ScreeningStatusesList.Codes.NotScreened, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgFullName, newName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("Org name should change", newName, org.OH_FullName);
				AssertEquals("Screening status should be NotScreened", ScreeningStatusesList.Codes.NotScreened, org.OH_ScreeningStatus);
			});
			AssertNoLogs(org);
		}

		public void TestOrgScreeningStatusShouldNotBeChanged_WhenPropertyHasChange_ButOriginalStatusIsUnKnown()
		{
			var newName = "NEW NAME";
			AssertNotEquals(OrgFullName, newName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(ScreeningStatusesList.Codes.Unknown, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgFullName, newName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("Org name should change", newName, org.OH_FullName);
				AssertEquals("Screening status should be Unknown", ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
			});
			AssertNoLogs(org);
		}

		#endregion

		#region Implementation

		const string OrgCode = "DODPRASYD";
		const string OrgFullName = "DODGY PRACTICES INC";
		const string OrgMainAddress1 = "100 GEORGE STREET";
		const string OrgMainAddress2 = "SUITE 302 LEVEL 1";
		const string OrgPostCode = "2000";
		const string OrgStateCode = "NSW";
		const string OrgCity = "ROCKS";
		const string OrgCountryCode = "AU";
		const string OrgClosestPortCode = "AUSYD";
		const string OrgRelatedPortCode = "AUSYD";
		const string OrgRelatedName = "DODGY RELATED NAME";

		const string ContactName = "GREAT KING";
		readonly ZDateTime contactBirthday = new ZDateTime(1999, 12, 31, 0, 0, 0, 0);

		const string CusCodeType = "CBP";
		const string CusCodeNo = "AUSCUSOS_AU";
		const string CusCodeCountryCode = "US";

		const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		void AssertOrgScreeningStatusShouldBeNotScreened_WhenFirstImport(string screeningStatus)
		{
			var org = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, OrgCode);
			AssertNull("Org should be null", org);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(screeningStatus, false, false, out var orgPk);
			Import(nativeXml);

			org = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, OrgCode);
			Assert("OrgHeader should be in the database after importing", org.IsInDatabase);
			AssertEquals("Screening status should be Not Screened", ScreeningStatusesList.Codes.NotScreened, org.OH_ScreeningStatus);
			AssertNoLogs(org);
		}

		void AssertOrgScreeningStatusShouldNotChange_WhenImportAndStatusIsDifferent(string orgScreeningStatus, string importScreeningStatus)
		{
			var nativeXml = CreateNativeXmlOrgHeaderStringFull(orgScreeningStatus, true, false, out var orgPk);

			var org = factory.Load<OrgHeader>(orgPk);
			AssertNotNull("Precondition: Org should not be null", org);

			nativeXml = nativeXml.Replace($"<ScreeningStatus>{orgScreeningStatus}</ScreeningStatus>", $"<ScreeningStatus>{importScreeningStatus}</ScreeningStatus>");
			Import(nativeXml);

			org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals($"Import screening status '{importScreeningStatus}'. Screening status should not change", orgScreeningStatus, org.OH_ScreeningStatus);
			AssertNoLogs(org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenOrgNameInfoHasChanges(string previousScreeningStatus)
		{
			var newName = "NEW NAME";
			AssertNotEquals("Org name", OrgFullName, newName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgFullName, newName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org name should change", newName, org.OH_FullName);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenAddress1InfoHasChanges(string previousScreeningStatus)
		{
			var newAddress = "NEW ADDRESS 1";
			AssertNotEquals("Main address 1", OrgMainAddress1, newAddress);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace($"<Address1>{OrgMainAddress1}</Address1>", $"<Address1>{newAddress}</Address1>");
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org address 1 should change", newAddress, org.MainAddress.Address1);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenAddress2InfoHasChanges(string previousScreeningStatus)
		{
			var newAddress = "NEW ADDRESS 2";
			AssertNotEquals("Main address 2", OrgMainAddress2, newAddress);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace($"<Address2>{OrgMainAddress2}</Address2>", $"<Address2>{newAddress}</Address2>");
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org address 2 should change", newAddress, org.MainAddress.Address2);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenCityInfoHasChanges(string previousScreeningStatus)
		{
			var newCity = "AUBURN";
			AssertNotEquals("Main address city", OrgCity, newCity);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgCity, newCity);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org city should change", newCity, org.MainAddress.City);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenStateCodeInfoHasChanges(string previousScreeningStatus)
		{
			var newStateCode = "QLD";
			AssertNotEquals("Main address state code", OrgStateCode, newStateCode);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgStateCode, newStateCode);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org state code should change", newStateCode, org.MainAddress.StateCode);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenPostCodeInfoHasChanges(string previousScreeningStatus)
		{
			var newPostCode = "2113";
			AssertNotEquals("Main address post code", OrgPostCode, newPostCode);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace($"<PostCode>{OrgPostCode}</PostCode>", $"<PostCode>{newPostCode}</PostCode>");
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org post code should change", newPostCode, org.MainAddress.Postcode);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenCountryCodeInfoHasChanges(string previousScreeningStatus)
		{
			var newCountryCode = "GB";
			AssertNotEquals("Main address country code", OrgCountryCode, newCountryCode);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgCountryCode, newCountryCode);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org country code should change", newCountryCode, org.MainAddress.OA_RN_NKCountryCode);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenClosestPortInfoHasChanges(string previousScreeningStatus)
		{
			var newClosestPortCode = "AUART";
			AssertNotEquals("Closet port", OrgClosestPortCode, newClosestPortCode);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgClosestPortCode, newClosestPortCode);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org closest port should change", newClosestPortCode, org.OH_RL_NKClosestPort);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenBrandOrRelatedNameInfoHasChanges(string previousScreeningStatus)
		{
			var newRelatedName = "NEW RELATED NAME";
			AssertNotEquals("Related name", OrgRelatedName, newRelatedName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(OrgRelatedName, newRelatedName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org related name should change", newRelatedName, org.BrandsOrRelatedNames[0].P1_RelatedName);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusHasNotChange_WhenContactNameInfoHasChanges(string previousScreeningStatus)
		{
			var newContactName = "NEW CONTACT NAME";
			AssertNotEquals("Contact name", ContactName, newContactName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace(ContactName, newContactName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org contact name should change", newContactName, org.Contacts[0].OC_ContactName);
			AssertOrgScreeningStatusHasNotChange(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenOrgActiveInfoHasChanges(string previousScreeningStatus)
		{
			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			nativeXml = nativeXml.Replace("<IsActive>true</IsActive>", "<IsActive>false</IsActive>");
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org active status should change", false, org.OH_IsActive);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenRelatedPortInfoHasChanges(string previousScreeningStatus)
		{
			var newRelatedPortCode = "AUT8R";
			AssertNotEquals("Related port code", OrgRelatedPortCode, newRelatedPortCode);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, true, out var orgPk);
			nativeXml = nativeXml.Replace(OrgRelatedPortCode, newRelatedPortCode);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			AssertEquals("Org related port code should change", newRelatedPortCode, org.MainAddress.OA_RL_NKRelatedPortCode);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenCusCodeInfoHasChanges(string previousScreeningStatus)
		{
			var newCusCodeType = "CBR";
			AssertNotEquals("Reg Code Type", CusCodeType, newCusCodeType);

			var nativeXmlWithCusCodeTypeChange = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPkWithCusCodeTypeChange);
			nativeXmlWithCusCodeTypeChange = nativeXmlWithCusCodeTypeChange.Replace($"<CodeType>{CusCodeType}</CodeType>", $"<CodeType>{newCusCodeType}</CodeType>");
			Import(nativeXmlWithCusCodeTypeChange);

			var orgWithCusCodeTypeChange = new BusinessObjectFactory().Load<OrgHeader>(orgPkWithCusCodeTypeChange);
			AssertEquals("Reg Code Type should change", newCusCodeType, orgWithCusCodeTypeChange.CustomsCodes[0].OK_CodeType);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, orgWithCusCodeTypeChange);

			var newCusCodeCountryCode = "AU";
			AssertNotEquals("Reg Code Country", CusCodeCountryCode, newCusCodeCountryCode);

			var nativeXmlWithCusCodeCountryChange = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPkWithCusCodeCountryChange);
			nativeXmlWithCusCodeCountryChange = nativeXmlWithCusCodeCountryChange.Replace($"<Code>US</Code>", $"<Code>{newCusCodeCountryCode}</Code>");
			Import(nativeXmlWithCusCodeCountryChange);

			var orgWithCusCodeCountryChange = new BusinessObjectFactory().Load<OrgHeader>(orgPkWithCusCodeCountryChange);
			AssertEquals("Reg Code Country should change", newCusCodeCountryCode, orgWithCusCodeCountryChange.CustomsCodes[0].OK_RN_NKCodeCountry);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, orgWithCusCodeCountryChange);

			var newCusCodeNo = "AUSCUS_AUTEST";
			AssertNotEquals("Reg Code No", CusCodeNo, newCusCodeNo);

			var nativeXmlWithCusCodeNoChange = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPkWithCusCodeNoChange);
			nativeXmlWithCusCodeNoChange = nativeXmlWithCusCodeNoChange.Replace($"<CustomsRegNo>{CusCodeNo}</CustomsRegNo>", $"<CustomsRegNo>{newCusCodeNo}</CustomsRegNo>");
			Import(nativeXmlWithCusCodeNoChange);

			var orgWithCusCodeNoChange = new BusinessObjectFactory().Load<OrgHeader>(orgPkWithCusCodeNoChange);
			AssertEquals("Reg Code No should change", newCusCodeNo, orgWithCusCodeNoChange.CustomsCodes[0].OK_CustomsRegNo);
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, orgWithCusCodeNoChange);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenANewAddressAdded(string previousScreeningStatus)
		{
			var newAddress1 = "NEW ADDRESS 1";
			AssertNotEquals("Main address 1", OrgMainAddress1, newAddress1);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, true, out var orgPk);
			var newAddr = $@"
           <OrgAddress Action = ""INSERT"">
             <IsActive>true</IsActive>
             <Code>{newAddress1}</Code>
             <CompanyNameOverride/>
             <Address1>{newAddress1}</Address1>
             <Address2></Address2>
             <City>RANDWICK</City>
             <State>NSW</State>
             <PostCode>2031</PostCode>
             <Language>EN</Language>
             <OrgAddressCapabilityCollection>
               <OrgAddressCapability Action=""MERGE"">
                 <AddressType>PAD</AddressType>
                 <IsMainAddress>true</IsMainAddress>
               </OrgAddressCapability>
             </OrgAddressCapabilityCollection>
             <RelatedPortCode TableName=""RefUNLOCO"">
               <Code>AUSYD</Code>
             </RelatedPortCode>
             <CountryCode TableName=""RefCountry"">
               <Code>AU</Code>
			 </CountryCode>
           </OrgAddress>
";
			var idx = nativeXml.IndexOf("</OrgAddress>");
			nativeXml = nativeXml.Insert(idx + "</OrgAddress>".Length, newAddr);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("Org addresses should have 2 addresses", 2, org.Addresses.Count);
				AssertEquals("Org addresses should have new address", 1, org.Addresses.Count(x => (x as OrgAddress).OA_Address1 == newAddress1));
			});
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenANewBrandOrRelatedNameAdded(string previousScreeningStatus)
		{
			var newRelatedName = "NEW RELATED NAME";
			AssertNotEquals("Related name", OrgRelatedName, newRelatedName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, true, out var orgPk);
			var newBrandOrRelatedName = $@"
          <OrgBrandOrRelatedName Action=""INSERT"">
            <RelatedName>{newRelatedName}</RelatedName>
          </OrgBrandOrRelatedName>
";
			var idx = nativeXml.IndexOf("</OrgBrandOrRelatedName>");
			nativeXml = nativeXml.Insert(idx + "</OrgBrandOrRelatedName>".Length, newBrandOrRelatedName);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("Org brands or related names should have 2 brand or related names", 2, org.BrandsOrRelatedNames.Count);
				AssertEquals("Org brands or related names should have new related name", 1, org.BrandsOrRelatedNames.Count(x => (x as OrgBrandOrRelatedName).P1_RelatedName == newRelatedName));
			});
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusHasNotChange_WhenANewContactAdded(string previousScreeningStatus)
		{
			var newContactName = "NEW CONTACT NAME";
			AssertNotEquals("Contact name", ContactName, newContactName);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, false, out var orgPk);
			var newContact = $@"
          <OrgContact Action = ""INSERT"">
            <IsActive>true</IsActive>
            <ContactName>{newContactName}</ContactName>
          </OrgContact>
";
			var idx = nativeXml.IndexOf("</OrgContact>");
			nativeXml = nativeXml.Insert(idx + "</OrgContact>".Length, newContact);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("Org contacts should have 2 contact names", 2, org.Contacts.Count);
				AssertEquals("Org contacts should have new contact name", 1, org.Contacts.Count(x => (x as OrgContact).OC_ContactName == newContactName));
			});
			AssertOrgScreeningStatusHasNotChange(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown_WhenANewCusCodeAdded(string previousScreeningStatus)
		{
			var newCustomsRegNo = "AUSCUS_AU";
			AssertNotEquals("Reg Code", CusCodeNo, newCustomsRegNo);

			var nativeXml = CreateNativeXmlOrgHeaderStringFull(previousScreeningStatus, true, true, out var orgPk);
			var newAddr = $@"
          <OrgCusCode Action=""MERGE"">
            <PK>0B6FBE5B-75FF-4F9C-8F00-856115FDA76C</PK>
            <CustomsRegNo>{newCustomsRegNo}</CustomsRegNo>
            <CodeType>CBP</CodeType>
            <CountryDefault>false</CountryDefault>
            <PremisesAddress TableName=""OrgAddress"" />
            <CodeCountry TableName=""RefCountry"">
              <Code>BG</Code>
              <PK>f7198328-0a00-479b-8d60-8520b1f8fd00</PK>
            </CodeCountry>
          </OrgCusCode>
";
			var idx = nativeXml.IndexOf("</OrgCusCode>");
			nativeXml = nativeXml.Insert(idx + "</OrgCusCode>".Length, newAddr);
			Import(nativeXml);

			var org = new BusinessObjectFactory().Load<OrgHeader>(orgPk);
			CombineAssertions(() =>
			{
				AssertEquals("OrgCusCode should have 2 cusCode", 2, org.CustomsCodes.Count);
				AssertEquals("OrgCusCode should have new cusCode", 1, org.CustomsCodes.Count(x => (x as OrgCusCode).OK_CustomsRegNo == newCustomsRegNo));
			});
			AssertOrgScreeningStatusChangedToUnknown(previousScreeningStatus, org);
		}

		void AssertOrgScreeningStatusChangedToUnknown(string previousScreeningStatus, OrgHeader org)
		{
			AssertEquals($"Screening status should change from {previousScreeningStatus} to {ScreeningStatusesList.Codes.Unknown}", ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
			AssertHasLogs(org, previousScreeningStatus);
		}

		void AssertOrgScreeningStatusHasNotChange(string previousScreeningStatus, OrgHeader org)
		{
			AssertEquals("Screening status should not change", previousScreeningStatus, org.OH_ScreeningStatus);
			AssertNoLogs(org);
		}

		void AssertNoLogs(OrgHeader org)
		{
			var stmALogRows = org.Logs.GetAllLogs();
			AssertEquals("AssertNoLogs: Message count", 0, stmALogRows.Count(x => x["SL_Reference"].ToString().StartsWith("Screening status from")));

			var logCollection = org.ScreeningLogCollection;
			AssertEquals("AssertNoLogs: OrgScreeningLog count", 0, logCollection.Count);
		}

		void AssertHasLogs(OrgHeader org, string previousScreeningStatus)
		{
			var stmALogRows = org.Logs.GetAllLogs();
			AssertEquals("AssertHasLogs: Message count", 1, stmALogRows.Count(x => x["SL_SE_NKEvent"].ToString() == AutoEvents.StatusChangeCode));

			var logCollection = org.ScreeningLogCollection;
			AssertEquals("AssertHasLogs: OrgScreeningLog count", 1, logCollection.Count);

			var screeningLog = logCollection[0] as StmEntityScreeningLog;
			AssertNotNull("AssertHasLogs: OrgScreeningLog should not be null", screeningLog);

			CombineAssertions(() =>
			{
				AssertEquals("AssertHasLogs: PJ_ParentID", org.PK, screeningLog.PJ_ParentID);
				AssertEquals("AssertHasLogs: PJ_ParentTableCode", org.TablePrefix, screeningLog.PJ_ParentTableCode);
				AssertEquals("AssertHaslogs: PJ_SourceID", org.PK, screeningLog.PJ_SourceID);
				AssertEquals("AssertHasLogs: PJ_SourceTableCode", org.TablePrefix, screeningLog.PJ_SourceTableCode);
				AssertEquals("AssertHasLogs: PJ_IsForcedRescreen", false, screeningLog.PJ_IsForcedRescreen);
			});

			var currentTimeUTc = ZDateTime.UtcNow;

			CombineAssertions(() =>
			{
				AssertEquals("AssertHasLogs: PJ_SystemCreateTimeUtc.Kind", DateTimeKind.Utc, screeningLog.PJ_SystemCreateTimeUtc.Kind);
				AssertDateTime("AssertHasLogs: PJ_SystemCreateTimeUtc", currentTimeUTc, screeningLog.PJ_SystemCreateTimeUtc);
				AssertDateTime("AssertHasLogs: PJ_SystemLastEditTimeUtc", currentTimeUTc, screeningLog.PJ_SystemLastEditTimeUtc);
				AssertEquals("AssertHasLogs: PJ_SystemCreateUser", GlbStaff.CurrentUser.GS_Code, screeningLog.PJ_SystemCreateUser);
				AssertEquals("AssertHasLogs: PJ_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code, screeningLog.PJ_SystemLastEditUser);
			});

			void AssertDateTime(string assertionMessage, ZDateTime expectedDay, ZDateTime actualDay)
			{
				Assert(assertionMessage, (expectedDay - actualDay).TotalHours <= 1);
			}
		}

		void Import(string nativeXml)
		{
			var xmlBytes = Encoding.Default.GetBytes(nativeXml);
			var manager = new ImportHandler(new AncillaryImportServices());
			manager.Import(new MemoryStream(xmlBytes));
		}

		string CreateNativeXmlOrgHeaderStringFull(string orgScreeningStatus, bool saveOrgToDataBase, bool isGlobalAccount, out ZGuid orgPk)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = OrgCode;
			org.OH_FullName = OrgFullName;
			org.OH_ScreeningStatus = orgScreeningStatus;
			org.MainAddress.Address1 = OrgMainAddress1;
			org.MainAddress.Address2 = OrgMainAddress2;
			org.MainAddress.StateCode = OrgStateCode;
			org.MainAddress.City = OrgCity;
			org.MainAddress.Postcode = OrgPostCode;
			org.MainAddress.OA_RN_NKCountryCode = OrgCountryCode;
			org.OH_IsGlobalAccount = isGlobalAccount;
			org.OH_RL_NKClosestPort = OrgClosestPortCode;
			org.MainAddress.OA_RL_NKRelatedPortCode = OrgRelatedPortCode;

			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = OrgRelatedName;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = ContactName;
			contact.OC_Birthday = contactBirthday;

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = CusCodeNo;
			cusCode.OK_CodeType = CusCodeType;
			cusCode.OK_RN_NKCodeCountry = CusCodeCountryCode;

			if (saveOrgToDataBase)
			{
				factory.Save();
				if (org.OH_ScreeningStatus != orgScreeningStatus)
				{
					org.OH_ScreeningStatus = orgScreeningStatus; // To fix first time can't change the OH_ScreeningStatus
					factory.Save();
				}
			}

			org = factory.Load<OrgHeader>(org.PK);
			orgPk = org.PK;

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Org fullname", OrgFullName, org.OH_FullName);
				AssertEquals("Org IsActive", true, org.OH_IsActive);
				AssertEquals("Org IsGlobalAccount", isGlobalAccount, org.OH_IsGlobalAccount);
				AssertEquals("Org screening status", orgScreeningStatus, org.OH_ScreeningStatus);
				AssertEquals("Org main address 1", OrgMainAddress1, org.MainAddress.Address1);
				AssertEquals("Org main address 2", OrgMainAddress2, org.MainAddress.Address2);
				AssertEquals("Org state code", OrgStateCode, org.MainAddress.StateCode);
				AssertEquals("Org city", OrgCity, org.MainAddress.City);
				AssertEquals("Org post code", OrgPostCode, org.MainAddress.Postcode);
				AssertEquals("Org country code", OrgCountryCode, org.MainAddress.OA_RN_NKCountryCode);
				AssertEquals("Org releated port code", OrgRelatedPortCode, org.MainAddress.OA_RL_NKRelatedPortCode);
				AssertEquals("Org closest port code", OrgClosestPortCode, org.OH_RL_NKClosestPort);
				AssertEquals("Org related name", OrgRelatedName, org.BrandsOrRelatedNames[0].P1_RelatedName);
				AssertEquals("Contact name", ContactName, org.Contacts[0].OC_ContactName);
				AssertEquals("Contact birthday", contactBirthday, org.Contacts[0].OC_Birthday);
				AssertEquals("Contact IsActive", true, org.Contacts[0].OC_IsActive);
				AssertEquals("CusCode CustomsRegNo", CusCodeNo, org.CustomsCodes[0].OK_CustomsRegNo);
				AssertEquals("CusCode CodeType", CusCodeType, org.CustomsCodes[0].OK_CodeType);
				AssertEquals("CusCode CountryCode", CusCodeCountryCode, org.CustomsCodes[0].OK_RN_NKCodeCountry);
			});

			return string.Format(CultureInfo.InvariantCulture, fullOrgHeaderInformationXml
				, org.PK
				, org.OH_Code
				, org.OH_FullName
				, org.OH_ScreeningStatus
				, org.MainAddress.Address1
				, org.MainAddress.Address2
				, org.MainAddress.City
				, org.MainAddress.StateCode
				, org.MainAddress.Postcode
				, org.MainAddress.OA_RN_NKCountryCode
				, org.MainAddress.OA_RL_NKRelatedPortCode
				, org.OH_RL_NKClosestPort
				, brand.PK
				, brand.P1_RelatedName
				, contact.PK
				, contact.OC_ContactName
				, contact.OC_Birthday.ToString(DateTimeFormat)
				, org.OH_IsGlobalAccount ? "true" : "false"
				, cusCode.PK
				, cusCode.OK_CustomsRegNo
				, cusCode.OK_CodeType
				, CusCodeCountryCode
				);
		}

		readonly string fullOrgHeaderInformationXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>CARGOWSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version = ""2.0"">
      <OrgHeader Action=""MERGE"">
        <PK>{0}</PK>
        <Code>{1}</Code>
        <FullName>{2}</FullName>
        <IsConsignee>true</IsConsignee>
        <IsActive>true</IsActive>
        <ScreeningStatus>{3}</ScreeningStatus>
        <IsGlobalAccount>{17}</IsGlobalAccount>
        <OrgAddressCollection>
          <OrgAddress Action = ""MERGE"">
            <IsActive>true</IsActive>
            <Code>{4}</Code>
            <CompanyNameOverride/>
            <Address1>{4}</Address1>
            <Address2>{5}</Address2>
            <City>{6}</City>
            <State>{7}</State>
            <PostCode>{8}</PostCode>
            <Language>EN</Language>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>{10}</Code>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>{9}</Code>
			</CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgBrandOrRelatedNameCollection>
          <OrgBrandOrRelatedName Action=""MERGE"">
            <PK>{12}</PK>
            <RelatedName>{13}</RelatedName>
          </OrgBrandOrRelatedName>
        </OrgBrandOrRelatedNameCollection>
        <OrgContactCollection>
          <OrgContact Action = ""MERGE"">
            <PK>{14}</PK>
            <IsActive>true</IsActive>
            <ContactName>{15}</ContactName>
            <Birthday>{16}</Birthday>
          </OrgContact>
		</OrgContactCollection>
        <OrgCusCodeCollection>
          <OrgCusCode Action=""MERGE"">
            <PK>{18}</PK>
            <CustomsRegNo>{19}</CustomsRegNo>
            <CodeType>{20}</CodeType>
            <CountryDefault>false</CountryDefault>
            <PremisesAddress TableName=""OrgAddress"" />
            <CodeCountry TableName=""RefCountry"">
              <Code>{21}</Code>
            </CodeCountry>
          </OrgCusCode>
        </OrgCusCodeCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>{11}</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>
";

		#endregion
	}
}
