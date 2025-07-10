using System.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrgAddressReceivablesFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class OrgAddressReceivablesFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		OrgAddressReceivablesFilterBusinessObject FilterObject
		{
			get
			{
				if (filterObject == null)
				{
					filterObject = (OrgAddressReceivablesFilterBusinessObject)GetNewBusinessObject();
				}
				return filterObject;
			}
		}
		OrgAddressReceivablesFilterBusinessObject filterObject;

		public void TestFilterForOrgAddress_Main()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Active Address1";
			address.OA_Address2 = "Active Address2";
			address.OA_Code = "Active Code";
			address.OA_OH = org.PK;
			address.OA_IsActive = true;
			Factory.Save();

			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = org.PK.ToString();

			var activeMatchingAddresses = new OrgAddressCollection(Factory, FilterObject.Filter);
			activeMatchingAddresses.Load();

			AssertEquals(1, activeMatchingAddresses.Count);
			AssertEquals("Should Return the main address added by default when we add org ", activeMatchingAddresses[0], org.MainAddress);
		}

		public void TestFilterForOrgAddress_AddressType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Active Address1";
			address.OA_Address2 = "Active Address2";
			address.OA_Code = "Active Code";
			address.OA_OH = org.PK;
			address.OA_IsActive = true;
			var receivableCapability = Factory.NewWithValidTestData<OrgAddressCapability>();
			receivableCapability.PZ_OA = address.PK;
			receivableCapability.PZ_AddressType = OrgAddressType.Receivables.Code;
			Factory.Save();

			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = org.PK.ToString();

			var activeMatchingAddresses = new OrgAddressCollection(Factory, FilterObject.Filter);
			activeMatchingAddresses.Load();

			AssertEquals(2, activeMatchingAddresses.Count);
			AssertContainsExactElementsInAnyOrder("Should Return receivable and main office addresses when they are active and exist", activeMatchingAddresses, new[] { org.MainAddress, address });

			address.OA_IsActive = false;
			Factory.Save();

			var anotherMatchingAddresses = new OrgAddressCollection(Factory, FilterObject.Filter);
			anotherMatchingAddresses.Load();

			AssertEquals(1, anotherMatchingAddresses.Count);
			AssertEquals("Should only have main address if no active receivable addresses types", anotherMatchingAddresses[0], org.MainAddress);
		}
	}
}
