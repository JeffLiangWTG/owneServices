using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	class OrgAddressAutoCompleteHelperTest : DependentBizOAutoCompleteHelperTest
	{
		#region Overrides

		protected override AutoCompleteHelper GetHelper()
		{
			return new OrgAddressAutoCompleteHelper(Factory);
		}

		protected override BusinessObject GetBusinessObject()
		{
			var orgAddress = (OrgAddress)base.GetBusinessObject();
			orgAddress.OA_OH = ((DependentBizOAutoCompleteHelper)Helper).ParentPK;
			return orgAddress;
		}

		protected override void SetUp()
		{
			base.SetUp();
			((DependentBizOAutoCompleteHelper)Helper).ParentPK = Factory.NewWithValidTestData<OrgHeader>().PK;
		}
		#endregion

		#region Test Inactive Address

		public void TestInactiveAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var activeAddress = Factory.NewWithValidTestData<OrgAddress>();
			activeAddress.OA_Address1 = "Active Address1";
			activeAddress.OA_Address2 = "Active Address2";
			activeAddress.OA_Code = "Active Code";
			activeAddress.OA_OH = org.PK;
			activeAddress.OA_IsActive = true;

			var inactiveAddress = Factory.NewWithValidTestData<OrgAddress>();
			inactiveAddress.OA_Address1 = "Inactive Address3";
			inactiveAddress.OA_Address2 = "Inactive Address4";
			inactiveAddress.OA_Code = "Inactive Code";
			inactiveAddress.OA_OH = org.PK;
			inactiveAddress.OA_IsActive = false;

			Factory.Save();

			var autoCompleteHelper = new OrgAddressAutoCompleteHelper(null);
			autoCompleteHelper.ParentPK = org.PK;

			var res = autoCompleteHelper.GetList("Address");
			AssertEquals("Inactive address should not be returned", 1, res.Count);

			AssertEquals("active address should contains", true, res[0].Contains("Active Address1"));
			AssertEquals("active address should contains", true, res[0].Contains("Active Address2"));
			AssertEquals("active address should contains", true, res[0].Contains("Active Code"));
		}

		public void TestAddress_MaxLength()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTORG";

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = new string('A', OrgAddressSchema.OA_Address1.MaxLength);
			address1.OA_Address2 = "Address2";
			address1.OA_IsActive = true;

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address1";
			address2.OA_Address2 = new string('B', OrgAddressSchema.OA_Address2.MaxLength);
			address2.OA_IsActive = true;

			var code = org.Addresses.AddNew();
			code.OA_Address1 = "Address1";
			code.OA_Address2 = "Address2";
			code.OA_Code = new string('C', OrgAddressSchema.OA_Code.MaxLength);
			code.OA_IsActive = true;

			Factory.Save();

			var autoCompleteHelper = new OrgAddressAutoCompleteHelper(null)
			{
				ParentPK = org.PK
			};

			var address1MaxString = new string('A', OrgAddressSchema.OA_Address1.MaxLength + 1);
			AssertEquals("Exceeded text column max length. Should select by Address1", 1, autoCompleteHelper.GetList(address1MaxString).Count);

			var address2MaxString = new string('B', OrgAddressSchema.OA_Address2.MaxLength + 1);
			AssertEquals("Exceeded text column max length. Should select by Address2", 1, autoCompleteHelper.GetList(address2MaxString).Count);

			var codeMaxString = new string('C', OrgAddressSchema.OA_Code.MaxLength + 1);
			AssertEquals("Exceeded text column max length. Should select by Code", 1, autoCompleteHelper.GetList(codeMaxString).Count);
		}

		#endregion

	}
}
