using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class OrgHeaderAutoCompleteHelperTest : AutoCompleteHelperTest
	{
		#region Test Inactive Organization

		[HttpContextEnabledTest]
		public void TestInactiveOrganization()
		{
			OrgHeader org = (OrgHeader)GetBusinessObject();
			org.OH_IsActive = false;
			org.OH_FullName = "orgTest";

			Factory.Save();

			var res = Helper.GetList("orgTest");
			AssertEquals("Inactive org should not be returned", 0, res.Count);
		}

		#endregion

		#region Test Additional Parameters Serialization

		[HttpContextEnabledTest]
		public void TestAdditionalParamsSerialization()
		{
			OrgHeaderAutoCompleteHelper helper = (OrgHeaderAutoCompleteHelper)Helper;

			helper.IsConsignor = true;
			helper.IsConsignee = true;
			helper.NewOrgRelationType = NewOrgRelationTypes.Supplier;

			string paramsStr = helper.SerializeAdditionalParamsToString();

			helper.IsConsignor = false;
			helper.IsConsignee = false;
			helper.NewOrgRelationType = NewOrgRelationTypes.Unknown;

			helper.RestoreAdditionalParamsFromSerializedString(paramsStr);

			AssertEquals("Consignor", true, helper.IsConsignor);
			AssertEquals("Consignee", true, helper.IsConsignee);
			AssertEquals("NewOrgRelationType", NewOrgRelationTypes.Supplier, helper.NewOrgRelationType);
		}

		#endregion

		public void TestNoLoggedInUser()
		{
			var org = (OrgHeader)GetBusinessObject();
			org.OH_IsActive = true;
			org.OH_FullName = "orgTest";

			Factory.Save();

			var results = Helper.GetList("anything");

			AssertEquals("Should not expose any orgs if user is not logged in", 0, results.Count);
		}

		#region Overrides

		protected override AutoCompleteHelper GetHelper()
		{
			return new OrgHeaderAutoCompleteHelper(Factory);
		}

		protected override BusinessObject GetBusinessObject()
		{
			OrgHeader org = (OrgHeader)base.GetBusinessObject();
			org.OH_IsConsignor = true;

			if (WebEnv.CurrentUser != null)
			{
				OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
				link.OL_OH_Buyer = ((OrgContact)WebEnv.CurrentUser).OC_OH;
				link.OL_OH_Supplier = org.PK;
			}

			Factory.Save();

			return org;
		}

		[HttpContextEnabledTest]
		public override void TestGetKeyAndText()
		{
			BusinessObject bizO = GetBusinessObject();

			ZString text = "Unique name 1"; // "Test"
			ZPropertyAccessor.Set(bizO, TextColumn.ObjectName, text);

			IZType key = (IZType)ZPropertyAccessor.Get(bizO, KeyColumn.ObjectName);
			((OrgHeaderAutoCompleteHelper)Helper).IsConsignor = true;

			Factory.Save();

			AssertEquals("Expected key", key, Helper.GetKey(text));  // "Test"
			AssertEquals("Expected text", text, Helper.GetText(key));  // "Test"
		}

		[HttpContextEnabledTest]
		public override void TestGetList()
		{
			BusinessObject[] bizOs = new BusinessObject[] { GetBusinessObject(), GetBusinessObject(), GetBusinessObject() };
			ZPropertyAccessor.Set(bizOs[0], TextColumn.ObjectName, (ZString)"Unique name 1");
			ZPropertyAccessor.Set(bizOs[1], TextColumn.ObjectName, (ZString)"Unique name 2A");
			ZPropertyAccessor.Set(bizOs[2], TextColumn.ObjectName, (ZString)"Unique name 3");
			((OrgHeaderAutoCompleteHelper)Helper).IsConsignor = true;
			Factory.Save();

			Helper.MaxOptionsCount = 10;
			AssertGetList("Unique na", 3);
			AssertGetList("Unique name 2", 1);

			Helper.MaxOptionsCount = 2;
			AssertGetList("Unique na", 2);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			((OrgHeaderAutoCompleteHelper)Helper).IsConsignor = true;
		}
	}
}
