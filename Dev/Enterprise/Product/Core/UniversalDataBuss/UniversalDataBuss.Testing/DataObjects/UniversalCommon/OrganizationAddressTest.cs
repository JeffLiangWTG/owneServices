using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(OrganizationAddress))]
	class OrganizationAddressTest : DataObjectTestCase<OrganizationAddress>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(OrganizationAddress.AddressType), typeof(OrganizationAddress).GetProperties().First(x => x.Name == nameof(OrganizationAddress.AddressType)).GetCustomAttributes(true).OfType<MaxLengthAttribute>().First().MaxLength }, // Non DB Field
				{ nameof(OrganizationAddress.AdditionalAddressInformation), typeof(OrganizationAddress).GetProperties().First(x => x.Name == nameof(OrganizationAddress.AdditionalAddressInformation)).GetCustomAttributes(true).OfType<MaxLengthAttribute>().First().MaxLength }, // Non DB Field
				{ nameof(OrganizationAddress.AddressShortCode), OrgAddressSchema.OA_Code.MaxLength },
				{ nameof(OrganizationAddress.OrganizationCode), OrgHeaderSchema.OH_Code.MaxLength },
				{ nameof(OrganizationAddress.OrganizationCategory), OrgHeaderSchema.OH_Category.MaxLength },
				{ nameof(OrganizationAddress.Address1), Math.Max(OrgAddressSchema.OA_Address1.MaxLength, JobDocAddressSchema.E2_Address1.MaxLength) },
				{ nameof(OrganizationAddress.Address2), Math.Max(OrgAddressSchema.OA_Address2.MaxLength, JobDocAddressSchema.E2_Address2.MaxLength) },
				{ nameof(OrganizationAddress.AddressOverride), JobDocAddressSchema.E2_AddressOverride.MaxLength },
				{ nameof(OrganizationAddress.City), Math.Max(OrgAddressSchema.OA_City.MaxLength, JobDocAddressSchema.E2_City.MaxLength) },
				{ nameof(OrganizationAddress.CompanyName), Math.Max(OrgAddressSchema.OA_CompanyNameOverride.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength) },
				{ nameof(OrganizationAddress.Contact), Math.Max(OrgContactSchema.OC_ContactName.MaxLength, JobDocAddressSchema.E2_Contact.MaxLength) },
				{ nameof(OrganizationAddress.Email), Math.Max(OrgAddressSchema.OA_Email.MaxLength, JobDocAddressSchema.E2_Email.MaxLength) },
				{ nameof(OrganizationAddress.Fax), Math.Max(OrgAddressSchema.OA_Fax.MaxLength, JobDocAddressSchema.E2_Fax.MaxLength) },
				{ nameof(OrganizationAddress.GovRegNum), Math.Max(OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, JobDocAddressSchema.E2_GovRegNum.MaxLength) },
				{ nameof(OrganizationAddress.Mobile), Math.Max(OrgContactSchema.OC_Mobile.MaxLength, JobDocAddressSchema.E2_Mobile.MaxLength) },
				{ nameof(OrganizationAddress.Phone), Math.Max(OrgAddressSchema.OA_Phone.MaxLength, JobDocAddressSchema.E2_Phone.MaxLength) },
				{ nameof(OrganizationAddress.Postcode), Math.Max(OrgAddressSchema.OA_PostCode.MaxLength, JobDocAddressSchema.E2_Postcode.MaxLength) },
				{ nameof(OrganizationAddress.State), Math.Max(OrgAddressSchema.OA_State.MaxLength, JobDocAddressSchema.E2_State.MaxLength) },
				{ nameof(OrganizationAddress.UniversalNettingCode), OrgCusCodeSchema.OK_CustomsRegNo.MaxLength },
				{ nameof(OrganizationAddress.UniversalOfficeCode), OrgCusCodeSchema.OK_CustomsRegNo.MaxLength }
			};
		}

		public void TestState()
		{
			var orgAddress = new OrganizationAddress();
			AssertNull(orgAddress.State);
			AssertEquals(null, (string)orgAddress.State);
			AssertEquals(false, ((ZString?)orgAddress.State).HasValue);
			AssertEquals("", ((ZString?)orgAddress.State).GetValueOrDefault());

			orgAddress.State = "";
			AssertEquals("", ((ZString?)orgAddress.State).Value);

			orgAddress.State = "012";
			AssertEquals("012", (string)orgAddress.State);
			AssertEquals("012", ((ZString?)orgAddress.State).Value);
			AssertEquals("012", orgAddress.State.ToString());
		}

		public void TestHasTypeOnly()
		{
			var orgAddress = new OrganizationAddress();
			AssertEquals(true, orgAddress.HasTypeOnly());
			orgAddress.State = "012";
			AssertEquals(false, orgAddress.HasTypeOnly());

			orgAddress.State = "";
			AssertEquals(true, orgAddress.HasTypeOnly());

			orgAddress.AddressType = "TEST";
			orgAddress.AddressShortCode = "";
			orgAddress.OrganizationCode = null;
			orgAddress.OrganizationCategory = null;
			orgAddress.AdditionalAddressInformation = "";
			orgAddress.Address1 = "";
			orgAddress.Address2 = "";
			orgAddress.AddressOverride = null;
			orgAddress.City = "";
			orgAddress.CompanyName = "";
			orgAddress.Contact = "";
			orgAddress.Port = null;
			orgAddress.Country = null;
			orgAddress.Email = "";
			orgAddress.Fax = "";
			orgAddress.GovRegNum = "";
			orgAddress.GovRegNumType = null;
			orgAddress.Mobile = "";
			orgAddress.Phone = "";
			orgAddress.Postcode = "";
			orgAddress.ScreeningStatus = null;
			orgAddress.State = "";
			orgAddress.UniversalNettingCode = "";
			orgAddress.UniversalOfficeCode = "";
			orgAddress.IsResidential = null;
			orgAddress.SuppressAddressValidationError = null;
			AssertEquals(true, orgAddress.HasTypeOnly());
		}

		public void TestIOrganizationAddress()
		{
			var orgAddress = new OrganizationAddress();
			IOrganizationAddress iOrgAddress = orgAddress;

			AssertEquals(false, iOrgAddress.OrganizationCode.HasValue);
			iOrgAddress.OrganizationCode = null;
			AssertEquals(false, iOrgAddress.OrganizationCode.HasValue);
			iOrgAddress.OrganizationCode = "012";
			AssertEquals(true, iOrgAddress.OrganizationCode.HasValue);
			AssertEquals("012", iOrgAddress.OrganizationCode);

			AssertEquals(false, iOrgAddress.OrganizationCategory.HasValue);
			iOrgAddress.OrganizationCategory = null;
			AssertEquals(false, iOrgAddress.OrganizationCategory.HasValue);
			iOrgAddress.OrganizationCategory = "BUS";
			AssertEquals(true, iOrgAddress.OrganizationCategory.HasValue);
			AssertEquals("BUS", iOrgAddress.OrganizationCategory);

			AssertNull(iOrgAddress.Country);
			orgAddress.Country = new Country() { Code = "NZ" };
			AssertEquals("NZ", iOrgAddress.Country.Code);

			AssertEquals(false, iOrgAddress.State.HasValue);
			iOrgAddress.State = null;
			AssertEquals(false, iOrgAddress.State.HasValue);
			iOrgAddress.State = "012";
			AssertEquals(true, iOrgAddress.State.HasValue);
			AssertEquals("012", iOrgAddress.State);
		}
	}
}

