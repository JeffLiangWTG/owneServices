using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressValidationDisabledCountryItemRegistryDataType))]
	sealed class AddressValidationDisabledCountryItemRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AddressValidationDisabledCountryItemRegistryDataType>
	{
		protected override AddressValidationDisabledCountryItemRegistryDataType GetNewDataType()
		{
			return new AddressValidationDisabledCountryItemRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstCollection = new AddressValidationDisabledCountryItemCollection();
			var bizoA = firstCollection.AddNew();
			bizoA.CountryPK = new ZGuid("D370C122-C50B-4A62-877A-FCF13AD09669");
			bizoA.DisabledForAdminPanel = false;
			bizoA.DisabledForOrgAddress = true;
			bizoA.DisabledForOverrideAddress = true;
			bizoA.DisabledForApplicant = true;
			bizoA.DisabledForPerson = true;
			bizoA.DisabledForBranch = true;
			bizoA.DisabledForCompany = true;
			bizoA.DisabledForSalesInquiry = true;
			bizoA.DisabledForStaff = true;
			bizoA.DisabledForHVLVConsignment = true;
			bizoA.DisabledForSupplierBookingLine = true;

			var secondCollection = new AddressValidationDisabledCountryItemCollection();
			var bizoB = secondCollection.AddNew();
			bizoB.CountryPK = new ZGuid("7DE2A5CE-2CA5-4C4E-95DC-F7C1F893892C");
			bizoB.DisabledForAdminPanel = true;
			bizoB.DisabledForOrgAddress = false;
			bizoB.DisabledForOverrideAddress = false;
			bizoB.DisabledForApplicant = false;
			bizoB.DisabledForPerson = false;
			bizoB.DisabledForBranch = false;
			bizoB.DisabledForCompany = false;
			bizoB.DisabledForSalesInquiry = false;
			bizoB.DisabledForStaff = false;
			bizoB.DisabledForHVLVConsignment = false;
			bizoB.DisabledForSupplierBookingLine = false;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(firstCollection, new AddressValidationDisabledCountryItemRegistryDataType().Serialise(firstCollection)),
				new ValidSampleAndBinaryValueInDB(secondCollection, new AddressValidationDisabledCountryItemRegistryDataType().Serialise(secondCollection))
			};
		}

		protected override string ExpectedEditorName => "AddressValidationDisabledCountryItemsRegistryItemEditor";
	}
}
