using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	class AddressEditRecordSQLTest : TestCaseWithFactory
	{
		public void TestAddressEditRecordQuery()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			orgAddress.Header.OH_FullName = "Test Company";
			orgAddress.OA_Code = "PICKUP";
			orgAddress.Address1 = "Test line 1";
			orgAddress.Address2 = "Test line 2";
			orgAddress.OA_City = "Test City";
			orgAddress.OA_PostCode = "0000";
			orgAddress.OA_State = "Test State";
			Factory.Save();

			var sqlcontext = $"SELECT * FROM dbo.AddressEditRecord('{ZDate.Today.ToShortDateString()}', '{ZDate.Today.AddDays(1).ToShortDateString()}', 0,0)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlcontext);

			AssertEquals(result.Rows.Count, 1);
			AssertEquals(result.Rows[0]["OrgName"], "Test Company");
			AssertEquals(result.Rows[0]["OrgCode"], orgAddress.Header.OH_Code);
			AssertEquals(result.Rows[0]["AddressCode"], "PICKUP");
			AssertEquals(result.Rows[0]["Address1"], "Test line 1");
			AssertEquals(result.Rows[0]["Address2"], "Test line 2");
			AssertEquals(result.Rows[0]["City"], "Test City");
			AssertEquals(result.Rows[0]["PostCode"], "0000");
			AssertEquals(result.Rows[0]["State"], "Test State");
		}

		public void TestTSAKnownAddressEditRecordQuery()
		{
			var notKnownAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			notKnownAddress.Header.OH_FullName = "Test Company";
			notKnownAddress.OA_Code = "DELIVERY";
			notKnownAddress.Address1 = "Test line 1";
			notKnownAddress.Address2 = "Test line 2";
			notKnownAddress.OA_City = "Test City";
			notKnownAddress.OA_PostCode = "0000";
			notKnownAddress.OA_State = "Test State";
			Factory.Save();

			var knownAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgCountryDataTSARecord = Factory.New<OrgCountryData>();
			orgCountryDataTSARecord.OV_OA_ApprovedLocation = knownAddress.PK;
			orgCountryDataTSARecord.OV_EXApprovedOrMajorExporter = "Yes";
			orgCountryDataTSARecord.OV_EXApprovalNumber = "1234";
			orgCountryDataTSARecord.OV_RN_NKClientCountryRelation = "US";
			orgCountryDataTSARecord.OV_OH_OrgHeader = knownAddress.OA_OH;

			Factory.Save();

			knownAddress.Header.OH_FullName = "Test Company";
			knownAddress.OA_Code = "PICKUP";
			knownAddress.Address1 = "Test line 1";
			knownAddress.Address2 = "Test line 2";
			knownAddress.OA_City = "Test City";
			knownAddress.OA_PostCode = "0000";
			knownAddress.OA_State = "Test State";
			Factory.Save();

			var sqlcontext = $"SELECT * FROM dbo.AddressEditRecord('{ZDate.Today.ToShortDateString()}', '{ZDate.Today.AddDays(1).ToShortDateString()}', 1, 0)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlcontext);

			AssertEquals(result.Rows.Count, 1);
			AssertEquals(result.Rows[0]["OrgName"], "Test Company");
			AssertEquals(result.Rows[0]["OrgCode"], knownAddress.Header.OH_Code);
			AssertEquals(result.Rows[0]["AddressCode"], "PICKUP");
			AssertEquals(result.Rows[0]["Address1"], "Test line 1");
			AssertEquals(result.Rows[0]["Address2"], "Test line 2");
			AssertEquals(result.Rows[0]["City"], "Test City");
			AssertEquals(result.Rows[0]["PostCode"], "0000");
			AssertEquals(result.Rows[0]["State"], "Test State");
		}

		public void TestMIDAddressEditRecordQuery()
		{
			var notKnownAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			notKnownAddress.Header.OH_FullName = "Test Company";
			notKnownAddress.OA_Code = "DELIVERY";
			notKnownAddress.Address1 = "Test line 1";
			notKnownAddress.Address2 = "Test line 2";
			notKnownAddress.OA_City = "Test City";
			notKnownAddress.OA_PostCode = "0000";
			notKnownAddress.OA_State = "Test State";
			Factory.Save();

			var knownAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgCusCodeMIDRecord = Factory.New<OrgCusCode>();
			orgCusCodeMIDRecord.OK_OA_PremisesAddress = knownAddress.PK;
			orgCusCodeMIDRecord.OK_CodeType = "MID";
			orgCusCodeMIDRecord.OK_CustomsRegNo = "abcd1234";
			orgCusCodeMIDRecord.OK_RN_NKCodeCountry = "US";
			orgCusCodeMIDRecord.OK_OH = knownAddress.OA_OH;

			Factory.Save();

			knownAddress.Header.OH_FullName = "Test Company";
			knownAddress.OA_Code = "PICKUP";
			knownAddress.Address1 = "Test line 1";
			knownAddress.Address2 = "Test line 2";
			knownAddress.OA_City = "Test City";
			knownAddress.OA_PostCode = "0000";
			knownAddress.OA_State = "Test State";
			Factory.Save();

			var sqlcontext = $"SELECT * FROM dbo.AddressEditRecord('{ZDate.Today.ToShortDateString()}', '{ZDate.Today.AddDays(1).ToShortDateString()}', 0, 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlcontext);

			AssertEquals(result.Rows.Count, 1);
			AssertEquals(result.Rows[0]["OrgName"], "Test Company");
			AssertEquals(result.Rows[0]["OrgCode"], knownAddress.Header.OH_Code);
			AssertEquals(result.Rows[0]["AddressCode"], "PICKUP");
			AssertEquals(result.Rows[0]["Address1"], "Test line 1");
			AssertEquals(result.Rows[0]["Address2"], "Test line 2");
			AssertEquals(result.Rows[0]["City"], "Test City");
			AssertEquals(result.Rows[0]["PostCode"], "0000");
			AssertEquals(result.Rows[0]["State"], "Test State");
		}
	}
}
