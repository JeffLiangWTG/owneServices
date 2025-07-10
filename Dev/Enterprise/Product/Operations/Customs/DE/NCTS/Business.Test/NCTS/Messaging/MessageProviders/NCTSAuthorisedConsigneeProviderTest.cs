using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSAuthorisedConsigneeProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSAuthorisedConsigneeProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL JobDecAddress", NCTSAuthorisedConsigneeProvider.NewOrNull(null));
				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_OA_Address = ZGuid.Invalid;
				AssertNull("Invalid Address", NCTSAuthorisedConsigneeProvider.NewOrNull(null));
			});
		}

		public void TestEoriNumber_Empty()
		{
			AssertEquals("Empty", null, dataProvider.EoriNumber);
		}

		public void TestEoriNumber()
		{
			orgAddress.OA_Address1 = "Address 1";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Greece);
			AssertEquals("GR12345", dataProvider.EoriNumber);
		}

		public void TestEoriBranchSuffix_Empty()
		{
			AssertEquals("Empty", null, dataProvider.EoriBranchSuffix);
		}

		public void TestEoriBranchSuffix()
		{
			orgAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS2", Core.Constants.CountryCodes.Germany);
			AssertEquals("EBS2", dataProvider.EoriBranchSuffix);
		}

		public void TestName_Empty()
		{
			AssertEquals("Empty", null, dataProvider.Name);
		}

		public void TestName()
		{
			orgContact.OC_ContactName = "VIC";
			AssertEquals("VIC", dataProvider.Name);
		}

		public void TestPosition_Empty()
		{
			AssertEquals("Empty", null, dataProvider.Position);
		}

		public void TestPosition()
		{
			orgContact.OC_Title = "DEV";
			AssertEquals("DEV", dataProvider.Position);
		}

		public void TestPhoneNumber_Empty()
		{
			AssertEquals("Empty", null, dataProvider.PhoneNumber);
		}

		public void TestPhoneNumber()
		{
			orgContact.OC_Phone = "12345";
			AssertEquals("12345", dataProvider.PhoneNumber);
		}

		public void TestFacsimileNumber_Empty()
		{
			AssertEquals("Empty", null, dataProvider.FacsimileNumber);
		}

		public void TestFacsimileNumber()
		{
			orgContact.OC_Fax = "VICFFF";
			AssertEquals("VICFFF", dataProvider.FacsimileNumber);
		}

		public void TestMailAddress_Empty()
		{
			AssertEquals("Empty", null, dataProvider.MailAddress);
		}

		public void TestMailAddress()
		{
			orgContact.OC_Email = "VIC@Wisetechglobal.com";
			AssertEquals("VIC@Wisetechglobal.com", dataProvider.MailAddress);
		}

		public void TestTCUNumber()
		{
			AssertNull(dataProvider.TCUNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress = org.Addresses.AddNew();
			orgContact = org.Contacts.AddNew();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.DestinationTrader.ContactPK = orgContact.PK;
			nctsHeader.DestinationTrader.E2_OA_Address = orgAddress.PK;
			dataProvider = NCTSAuthorisedConsigneeProvider.NewOrNull(nctsHeader.DestinationTrader);
		}
		OrgHeader org;
		OrgAddress orgAddress;
		OrgContact orgContact;
		NCTSAuthorisedConsigneeProvider dataProvider;

		protected override NCTSAuthorisedConsigneeProvider GetProvider() => dataProvider;
	}
}
