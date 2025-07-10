using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(VanningAddress))]
	sealed class VanningAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookupsType()
		{
			AssertType<VanningAddressLookups>(address.Lookups);
		}

		public void TestValidationType()
		{
			AssertType<VanningAddressValidation>(address.Validation);
		}

		protected override void SetUp()
		{
			address = EntryInstruction.VanningLocations.AddNew();
		}

		VanningAddress address;

		public void TestE2_AddressSequence()
		{
			AssertEquals(new ZByte(1), address.E2_AddressSequence);

			address = EntryInstruction.VanningLocations.AddNew();
			AssertEquals(new ZByte(2), address.E2_AddressSequence);

			address = EntryInstruction.VanningLocations.AddNew();
			AssertEquals(new ZByte(3), address.E2_AddressSequence);

			EntryInstruction.VanningLocations.RemoveAndDelete(address);

			address = EntryInstruction.VanningLocations.AddNew();
			AssertEquals(new ZByte(3), address.E2_AddressSequence);
		}

		public void TestOrganisationPK()
		{
			var bondedWarehouse = Factory.NewWithValidTestData<OrgHeader>();

			address.OrganisationPK = bondedWarehouse.PK;
			address.E2_OA_Address = bondedWarehouse.MainAddress.PK;

			AssertEquals(bondedWarehouse.PK, address.OrganisationPK);
			AssertEquals(bondedWarehouse.MainAddress.PK, address.E2_OA_Address);
		}

		public void TestSetE2_AddressOverride()
		{
			var address = ((IBindingList)entryInstruction.VanningLocations).AddNew();
			var detachedAddress = (VanningAddress)address;

			detachedAddress.E2_AddressOverride = true;
			AssertEquals("detachedAddress successfully set E2_AddressOverride", true, detachedAddress.E2_AddressOverride);
		}

		public void TestReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertReadOnly(address.E2_CityInfo);
				AssertReadOnly(address.E2_Address1Info);
				AssertReadOnly(address.E2_CompanyNameInfo);
				AssertReadOnly(address.E2_StateInfo);
				AssertReadOnly(address.E2_GovRegNumInfo);
				AssertReadOnly(address.E2_GovRegNumTypeInfo);
				AssertReadOnly(address.E2_RN_NKCountryCodeInfo);
				AssertReadOnly(address.E2_AdditionalAddressInformationInfo);
			});
		}

		void AssertReadOnly(ZPropertyInfo info)
		{
			address.E2_AddressOverride = false;
			Assert($"{info.Name} should be read only now.", info.ReadOnly);

			address.E2_AddressOverride = true;
			Assert($"{info.Name} could be written now", !info.ReadOnly);
		}

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;

		CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
		CusEntryInstruction entryInstruction;
	}
}
