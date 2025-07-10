using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressValidationDisabledCountryItem))]
	sealed class AddressValidationDisabledCountryItemTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCountryName()
		{
			var collection = new AddressValidationDisabledCountryItemCollection();
			var item = collection.AddNew();
			AssertEquals(ZString.Empty, item.CountryName);

			var country = Factory.LoadTop1<IRefCountry>(new ZQuery());
			item.CountryPK = country.PK;
			AssertEquals(country.RN_Desc, item.CountryName);

			item.CountryPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, item.CountryName);
		}

		public void TestValidateCountryPK()
		{
			var collection = new AddressValidationDisabledCountryItemCollection();
			var item1 = collection.AddNew();
			item1.CountryPK = ZGuid.Invalid;
			AssertHasErrors(item1.CountryPKInfo);

			var country1 = Factory.LoadTop1<IRefCountry>(new ZQuery());
			item1.CountryPK = country1.PK;
			AssertNoErrors(item1.CountryPKInfo);

			var item2 = collection.AddNew();
			var country2 = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, country1.PK));
			item2.CountryPK = country2.PK;
			AssertNoErrors(item1.CountryPKInfo);

			item2.CountryPK = country1.PK;
			AssertHasError(item2.CountryPKInfo, "There are duplicate country/region configuration items, each country/region can't have more than one configuration item.");
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var collection = new AddressValidationDisabledCountryItemCollection();
			var item = collection.AddNew();
			item.CountryPK = Guid.NewGuid();
			item.DisabledForAdminPanel = true;
			item.DisabledForOrgAddress = false;
			item.DisabledForOverrideAddress = true;
			item.DisabledForBranch = false;
			item.DisabledForPerson = true;
			item.DisabledForSalesInquiry = false;
			item.DisabledForStaff = true;
			item.DisabledForApplicant = false;
			item.DisabledForCompany = true;
			item.DisabledForHVLVConsignment = false;
			item.DisabledForSupplierBookingLine = true;

			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
