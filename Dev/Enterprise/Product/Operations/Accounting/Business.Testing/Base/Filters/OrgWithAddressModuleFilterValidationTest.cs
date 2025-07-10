using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Filters.Testing
{
	internal sealed class OrgWithAddressModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOrganization()
		{
			Filter.Organization = ZGuid.NewZGuid();
			AssertNoNotifications(Filter.OrganizationInfo);

			Filter.Organization = ZGuid.Invalid;
			AssertHasError(Filter.OrganizationInfo, "Enter a valid selection.");

			Filter.Organization = ZGuid.Empty;
			AssertNoNotifications(Filter.OrganizationInfo);
		}

		public void TestAddress()
		{
			Filter.Address = ZGuid.NewZGuid();
			AssertNoNotifications(Filter.AddressInfo);

			Filter.Address = ZGuid.Invalid;
			AssertHasError(Filter.AddressInfo, "Enter a valid selection.");

			Filter.Address = ZGuid.Empty;
			AssertNoNotifications(Filter.AddressInfo);
		}

		#region Implementation

		OrgWithAddressFilter Filter
		{
			get { return filter ?? (filter = new OrgWithAddressFilter("description", (x, y) => { return new ZQuery(); }, true)); }
		}
		OrgWithAddressFilter filter;

		#endregion
	}
}
