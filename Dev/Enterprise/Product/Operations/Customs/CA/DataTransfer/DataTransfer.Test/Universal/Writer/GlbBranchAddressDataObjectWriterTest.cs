using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class GlbBranchAddressDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "SG";

			var activeState = Factory.New<RefCountryStates>();
			activeState.RW_RN_NKCountryCode = country.RN_Code;
			activeState.RW_Code = "Brk";
			activeState.RW_Description = "[_MOCK_ACTIVE_STATE_BrkState1]";
			activeState.RW_IsActive = true;

			var states = new OrgCodeLists().State_List(Factory, "SG");

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_ScreeningStatus = "TST";

			var mainAddress = broker.MainAddress;
			mainAddress.OA_Code = "BrkMainAddr";
			mainAddress.Address1 = "Broker Address1";
			mainAddress.Address2 = "Broker Address2";

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			mainAddress.OA_RL_NKRelatedPortCode = "SGSIN";

			mainAddress.OA_City = "Broker City";
			mainAddress.OA_PostCode = "20171207";
			mainAddress.OA_State = "Brk";

			mainAddress.OA_Email = "broker@mail.com";
			mainAddress.OA_Fax = "025";
			mainAddress.OA_Phone = "1800000000";

			var branch = Factory.New<GlbBranch>();
			branch.GB_OH_OrgProxy = broker.PK;

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "TestStaff";
			staff.GS_EmailAddress = "staff@mail.com";
			staff.GS_FaxNum = "030";
			staff.GS_MobilePhone = "18200000000";
			staff.GS_WorkPhone = "18500000000";

			var writer = new BrokerAddressDataObjectWriter(writeManager, Constants.AddressType.CustomsBroker);
			var testDataOBject = writer.GetDataObject(broker);

			CombineAssertions(() =>
			{
				AssertEquals("AddressOverride", false, testDataOBject.AddressOverride);
				AssertEquals("AddressType", "CustomsBroker", testDataOBject.AddressType);
				AssertEquals("Port", "SGSIN", testDataOBject.Port.Code);
				AssertEquals("Country", "SG", testDataOBject.Country.Code);
				AssertEquals("Address1", "Broker Address1", testDataOBject.Address1);
				AssertEquals("Address2", "Broker Address2", testDataOBject.Address2);
				AssertEquals("City", "Broker City", testDataOBject.City);
				AssertEquals("Postcode", "20171207", testDataOBject.Postcode);
				AssertEquals("State", "Brk", testDataOBject.State);

				AssertEquals("Contact", string.Empty, testDataOBject.Contact.GetValueOrDefault());
				AssertEquals("Email", "broker@mail.com", testDataOBject.Email);
				AssertEquals("Fax", "025", testDataOBject.Fax);
				AssertEquals("Phone", "1800000000", testDataOBject.Phone);
				AssertEquals("Mobile", null, testDataOBject.Mobile);
				AssertEquals("ScreeningStatus", "TST", testDataOBject.ScreeningStatus.Code);
			});

			writer = new BrokerAddressDataObjectWriter(writeManager, Constants.AddressType.CustomsBroker, staff);
			testDataOBject = writer.GetDataObject(broker);
			CombineAssertions(() =>
			{
				AssertEquals("AddressOverride", false, testDataOBject.AddressOverride);
				AssertEquals("AddressType", "CustomsBroker", testDataOBject.AddressType);
				AssertEquals("Port", "SGSIN", testDataOBject.Port.Code);
				AssertEquals("Country", "SG", testDataOBject.Country.Code);
				AssertEquals("Address1", "Broker Address1", testDataOBject.Address1);
				AssertEquals("Address2", "Broker Address2", testDataOBject.Address2);
				AssertEquals("City", "Broker City", testDataOBject.City);
				AssertEquals("Postcode", "20171207", testDataOBject.Postcode);
				AssertEquals("State", "Brk", testDataOBject.State);

				AssertEquals("Contact", "TestStaff", testDataOBject.Contact.GetValueOrDefault());
				AssertEquals("Email", "staff@mail.com", testDataOBject.Email);
				AssertEquals("Fax", "030", testDataOBject.Fax);
				AssertEquals("Phone", "18500000000", testDataOBject.Phone);
				AssertEquals("Mobile", "18200000000", testDataOBject.Mobile);
				AssertEquals("ScreeningStatus", "TST", testDataOBject.ScreeningStatus.Code);
			});
		}

		public void TestPopulateContactUsingBranchFallback()
		{
			var broker = Factory.NewWithValidTestData<OrgHeader>();

			var branch = Factory.New<GlbBranch>();
			branch.GB_Fax = "61280012201";
			branch.GB_Phone = "61280012200";

			var staff = Factory.New<GlbStaff>();
			staff.GS_FaxNum = ZString.Empty;
			staff.GS_WorkPhone = ZString.Empty;
			staff.GS_GB_HomeBranch = branch.PK;

			var writer = new BrokerAddressDataObjectWriter(writeManager, Constants.AddressType.CustomsBroker, staff);
			var testDataOBject = writer.GetDataObject(broker);
			AssertEquals(branch.GB_Phone, testDataOBject.Phone);
			AssertEquals(branch.GB_Fax, testDataOBject.Fax);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			writeManager = new DataWritingManager(new ActionInfo(null, declaration));
		}

		DataWritingManager writeManager;
	}
}
