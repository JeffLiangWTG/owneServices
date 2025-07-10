using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LocationsChargesRegistryItem))]
	sealed class LocationsChargesRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<LocationsChargesCollection>
	{
		public void TestCompanyLevelValue()
		{
			LocationsChargesCollection collection = new LocationsChargesCollection();
			LocationsChargesGroup locationsChargesGroup = collection.AddNew();
			ChargeCodeGroup charge = locationsChargesGroup.Charges.AddNew();

			locationsChargesGroup.Location = "USNYC";

			ZGuid newGuid = ZGuid.NewZGuid();

			using (charge.GetValidationSuspender())
			{
				charge.ChargeCodePK = newGuid;
			}

			RegistryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			LocationsChargesCollection companyValue = RegistryItem.Value;

			AssertEquals("CompanyValue[0].Location", "USNYC", companyValue[0].Location);
			AssertEquals("CompanyValue[0].Charges[0].ChargeCodePK", newGuid, companyValue[0].Charges[0].ChargeCodePK);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			LocationsChargesCollection collection = new LocationsChargesCollection();
			LocationsChargesGroup locationsChargesGroup = collection.AddNew();
			ChargeCodeGroup charge = locationsChargesGroup.Charges.AddNew();

			locationsChargesGroup.Location = "USNYC";

			RegistryItem = new LocationsChargesRegistryItem("", null, null, null, RegistryStorageFlags.Company, collection);
		}

		protected override StronglyTypedRegistryItem<LocationsChargesCollection, LocationsChargesCollection> GetNewRegistryItem()
		{
			return new LocationsChargesRegistryItem("", null, null, null, RegistryStorageFlags.System, new LocationsChargesCollection());
		}

		LocationsChargesRegistryItem RegistryItem;

		#endregion
	}
}
