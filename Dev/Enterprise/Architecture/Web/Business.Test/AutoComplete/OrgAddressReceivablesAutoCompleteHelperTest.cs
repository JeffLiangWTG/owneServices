using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class OrgAddressReceivablesAutoCompleteHelperTest : OrgAddressAutoCompleteHelperTest
	{
		#region Overrides

		protected override AutoCompleteHelper GetHelper()
		{
			return new OrgAddressReceivablesAutoCompleteHelper(Factory);
		}

		protected override BusinessObject GetBusinessObject()
		{
			var orgAddress = (OrgAddress)base.GetBusinessObject();
			orgAddress.OA_OH = ((DependentBizOAutoCompleteHelper)Helper).ParentPK;

			var receivableCapability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
			receivableCapability1.PZ_OA = orgAddress.PK;
			receivableCapability1.PZ_AddressType = OrgConstants.AddressType.Receivables;

			return orgAddress;
		}

		ZGuid ParentPK => ((DependentBizOAutoCompleteHelper)Helper).ParentPK;

		#endregion

		void SetupTestData(bool withReceivables)
		{
			if (withReceivables)
			{
				var receivableAddress = Factory.NewWithValidTestData<OrgAddress>();
				receivableAddress.OA_Address1 = "Receivable Address1";
				receivableAddress.OA_Address2 = "Receivable Address2";
				receivableAddress.OA_Code = "Receivable Code1";
				receivableAddress.OA_OH = ParentPK;
				receivableAddress.OA_IsActive = true;
				var receivableCapability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
				receivableCapability1.PZ_OA = receivableAddress.PK;
				receivableCapability1.PZ_AddressType = OrgConstants.AddressType.Receivables;

				var receivableAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				receivableAddress2.OA_Address1 = "Receivable Address3";
				receivableAddress2.OA_Address2 = "Receivable Address4";
				receivableAddress2.OA_Code = "Receivable Code2";
				receivableAddress2.OA_OH = ParentPK;
				receivableAddress2.OA_IsActive = true;
				var receivableCapability2 = Factory.NewWithValidTestData<OrgAddressCapability>();
				receivableCapability2.PZ_OA = receivableAddress2.PK;
				receivableCapability2.PZ_AddressType = OrgConstants.AddressType.Receivables;
			}

			var officeMainAddress = Factory.NewWithValidTestData<OrgAddress>();
			officeMainAddress.OA_Address1 = "Office Main Address1";
			officeMainAddress.OA_Address2 = "Office Main Address2";
			officeMainAddress.OA_Code = "Office Main Code";
			officeMainAddress.OA_OH = ParentPK;
			officeMainAddress.OA_IsActive = true;
			var officeMainCapability = Factory.NewWithValidTestData<OrgAddressCapability>();
			officeMainCapability.PZ_OA = officeMainAddress.PK;
			officeMainCapability.PZ_AddressType = OrgConstants.AddressType.Office;
			officeMainCapability.PZ_IsMainAddress = true;

			var someOtherAddress = Factory.NewWithValidTestData<OrgAddress>();
			someOtherAddress.OA_Address1 = "Some Other Address3";
			someOtherAddress.OA_Address2 = "Some Other Address4";
			someOtherAddress.OA_Code = "Some Other Code";
			someOtherAddress.OA_OH = ParentPK;
			someOtherAddress.OA_IsActive = true;

			Factory.Save();
		}

		public void TestReceivableAddresses()
		{
			SetupTestData(true);
			var helper = new OrgAddressReceivablesAutoCompleteHelper(Factory)
			{
				ParentPK = ParentPK
			};

			var receivables = helper.GetList("Address");
			AssertEquals("Should be 2 receivable addresses and 1 main", 3, receivables.Count);

			Assert(receivables[0].Contains("Office Main Address1"));
			Assert(receivables[0].Contains("Office Main Address2"));
			Assert(receivables[1].Contains("Receivable Address1"));
			Assert(receivables[1].Contains("Receivable Address2"));
			Assert(receivables[2].Contains("Receivable Address3"));
			Assert(receivables[2].Contains("Receivable Address4"));
		}

		public void TestOnlyMainOfficeAddres()
		{
			SetupTestData(false);
			var helper = new OrgAddressReceivablesAutoCompleteHelper(Factory)
			{
				ParentPK = ParentPK
			};

			var office = helper.GetList("Address");
			AssertEquals("Should be 1 office address", 1, office.Count);

			Assert(office[0].Contains("Office Main Address1"));
			Assert(office[0].Contains("Office Main Address2"));
		}
	}
}
