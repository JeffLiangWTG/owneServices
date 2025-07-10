using System.ComponentModel;
using System.Linq;
using System.Web;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrgAddressFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class OrgAddressFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		OrgAddressFilterBusinessObject FilterObject
		{
			get
			{
				if (filterObject == null)
				{
					filterObject = (OrgAddressFilterBusinessObject)GetNewBusinessObject();
				}
				return filterObject;
			}
		}
		OrgAddressFilterBusinessObject filterObject;

		public void TestFilterForOrgAddress()
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
			activeMatchingAddresses.Sort(new WebCollectionSorter(OrgAddress.Schema.OA_Code, ListSortDirection.Descending));

			AssertEquals("Shoud Return two Active Addresses,one of them added by defualt when we add org ", 2, activeMatchingAddresses.Count);

			AssertEquals(activeMatchingAddresses[0].Address1, "Active Address1");
			AssertEquals(activeMatchingAddresses[0].Address2, "Active Address2");
			AssertEquals(activeMatchingAddresses[0].OA_Code, "Active Code");

			AssertEquals(activeMatchingAddresses[1].Address1, "#1");
			AssertEquals(activeMatchingAddresses[1].Address2, ZString.Empty);
			AssertEquals(activeMatchingAddresses[1].OA_Code, "#1");

			address.OA_Address1 = "Inactive Address1";
			address.OA_Address2 = "Inactive Address2";
			address.OA_Code = "Inactive Code";
			address.OA_IsActive = false;
			Factory.Save();

			var anotherMatchingAddresses = new OrgAddressCollection(Factory, FilterObject.Filter);
			anotherMatchingAddresses.Load();
			activeMatchingAddresses.Sort(new WebCollectionSorter(OrgAddress.Schema.OA_Code, ListSortDirection.Descending));

			AssertEquals("Shoud Return One Active Addresse ", 1, anotherMatchingAddresses.Count);

			AssertEquals(anotherMatchingAddresses[0].Address1, "#1");
			AssertEquals(anotherMatchingAddresses[0].Address2, ZString.Empty);
			AssertEquals(anotherMatchingAddresses[0].OA_Code, "#1");
		}

		public void TestFilterForOrgAddress_MaxLength()
		{
			var filter = FilterObject;
			var expectedAddress1 = new string('A', OrgAddressSchema.OA_Address1.MaxLength);
			var expectedAddress2 = new string('A', OrgAddressSchema.OA_Address2.MaxLength);
			var expectedCity = new string('A', OrgAddressSchema.OA_City.MaxLength);
			var expectedPortCode = new string('A', OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength);

			var tooLong = new string('A', GetLargestMaxLength() + 1);
			filter.SoughtText = tooLong;
			var expectedSubFilter = $"(OA_Address1 = '{expectedAddress1}' or OA_Address2 = '{expectedAddress2}' or OA_City = '{expectedCity}' or OA_RL_NKRelatedPortCode = '{expectedPortCode}') and OA_IsActive = 1";
			AssertContains(expectedSubFilter, filter.Filter.LiteralTextADO);

			int GetLargestMaxLength()
			{
				var maxLengths = new[]
				{
					OrgAddressSchema.OA_Address1.MaxLength,
					OrgAddressSchema.OA_Address2.MaxLength,
					OrgAddressSchema.OA_City.MaxLength,
					OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength,
				};

				return maxLengths.Max();
			}
		}
	}
}
