using CargoWise.Types;

namespace Enterprise.Registry.Business.Testing
{
	public static class DisabledCountryItemCollectionTestHelper
	{
		public static AddressValidationDisabledCountryItemCollection GetCollection(ZGuid countryPK, bool disabledForAdminPanel = false, bool disabledForOrgAddress = false, bool disabledForOverrideAddress = false, bool disabledForBranch = false, bool disabledForPerson = false, bool disabledForSalesInquiry = false, bool disabledForStaff = false, bool disabledForApplicant = false, bool disabledForCompany = false, bool disabledForHVLVConsignment = false, bool disabledForSupplierBookingLine = false)
		{
			var collection = new AddressValidationDisabledCountryItemCollection();
			var item = collection.AddNew();
			item.CountryPK = countryPK;
			item.DisabledForAdminPanel = disabledForAdminPanel;
			item.DisabledForOrgAddress = disabledForOrgAddress;
			item.DisabledForOverrideAddress = disabledForOverrideAddress;
			item.DisabledForBranch = disabledForBranch;
			item.DisabledForPerson = disabledForPerson;
			item.DisabledForSalesInquiry = disabledForSalesInquiry;
			item.DisabledForStaff = disabledForStaff;
			item.DisabledForApplicant = disabledForApplicant;
			item.DisabledForCompany = disabledForCompany;
			item.DisabledForHVLVConsignment = disabledForHVLVConsignment;
			item.DisabledForSupplierBookingLine = disabledForSupplierBookingLine;
			return collection;
		}
	}
}
