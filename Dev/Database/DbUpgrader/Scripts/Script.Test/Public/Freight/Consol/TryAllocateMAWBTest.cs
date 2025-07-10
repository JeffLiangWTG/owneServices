using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol
{
	[TestedType(typeof(TryAllocateMAWB))]
	class TryAllocateMAWBTest : DbCreateScriptTest
	{
		public void Test_DefaultMawbStockManagementSetting()
		{
			var branch1Pk = TestDbHelper.BranchBrnPK;
			var branch2Pk = DbHelper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			RecreateMawbs(branch1Pk, branch2Pk);

			// The MAWB stock management setting does not exist by default
			// By default, a branch cannot use global or company mawb
			var mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: false, "STD", CurrentBranch_PRI_MAWB, Guid.Empty);
			AssertEquals("Booking reference match should bypass service match", CurrentBranch_PRI_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "STD", "", Guid.Empty);
			AssertEquals("No booking reference match then match service", CurrentBranch_STD_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("No booking reference match then match service", CurrentBranch_ALL_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("No mawb available to match this service level as they are used by above lines", "", mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: false, "ALL", Company_ALL_MAWB, Guid.Empty);
			AssertEquals("Not allow to use company stock", "", mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: false, "ALL", Global_ALL_MAWB, Guid.Empty);
			AssertEquals("Not allow to use global stock", "", mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: false, "ALL", OtherBranch_ALL_MAWB, Guid.Empty);
			AssertEquals("Not allow to use other branch stock", "", mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, branch1Pk, useOtherBranchMawbRegistryValue: true, "ALL", OtherBranch_PRI_MAWB, Guid.Empty);
			AssertEquals("Allow to use other branch stock because registry value changed to true", OtherBranch_PRI_MAWB, mawb);
		}

		public void Test_DifferentLevelMawbStockManagementSetting()
		{
			var branch1Pk = TestDbHelper.BranchBrnPK;
			var branch2Pk = DbHelper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var carrierPk = CreateAirlineCarrier(Airline_Prefix, "AU");

			RecreateMawbs(branch1Pk, branch2Pk);

			var mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("By default it can use branch mawb", CurrentBranch_ALL_MAWB, mawb);

			UpdateOrgAirlineMAWBStockManagement(carrierPk, TestDbHelper.DefaultCompanyPK, DBNull.Value, allowUseBranchStock: false, allowUseCompanyStock: true, allowUseGlobalStock: false, "N");
			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("Only allow use company mawb", Company_ALL_MAWB, mawb);

			UpdateOrgAirlineMAWBStockManagement(carrierPk, TestDbHelper.DefaultCompanyPK, DBNull.Value, allowUseBranchStock: false, allowUseCompanyStock: false, allowUseGlobalStock: true, "N");
			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("Only allow use global mawb", Global_ALL_MAWB, mawb);

			UpdateOrgAirlineMAWBStockManagement(carrierPk, TestDbHelper.DefaultCompanyPK, DBNull.Value, allowUseBranchStock: false, allowUseCompanyStock: false, allowUseGlobalStock: true, "Y");
			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("Only allow use other branch mawb", OtherBranch_ALL_MAWB, mawb);

			// Allow all levels maswb, to test allocation order
			UpdateOrgAirlineMAWBStockManagement(carrierPk, TestDbHelper.DefaultCompanyPK, DBNull.Value, allowUseBranchStock: true, allowUseCompanyStock: true, allowUseGlobalStock: true, "Y");

			RecreateMawbs(branch1Pk, branch2Pk);
			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "STD", CurrentBranch_PRI_MAWB, Guid.Empty);
			AssertEquals("Get booking reference match from current branch mawb", CurrentBranch_PRI_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "STD", Company_PRI_MAWB, Guid.Empty);
			AssertEquals("Get booking reference match from company mawb", Company_PRI_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "STD", Global_PRI_MAWB, Guid.Empty);
			AssertEquals("Get booking reference match from global mawb", Global_PRI_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("Get service level ALL from current branch mawb", CurrentBranch_ALL_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("Fall back to company mawb", Company_ALL_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("Fall back to global mawb", Global_ALL_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("Fall back to other branch mawb", OtherBranch_PRI_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("Fall back to other branch mawb", OtherBranch_ALL_MAWB, mawb);

			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, TestDbHelper.BranchBrnPK, useOtherBranchMawbRegistryValue: false, "PRI", "", Guid.Empty);
			AssertEquals("All mawb with PRI service level or ALL service level are used up", "", mawb);
		}

		[TestDate(2024, 11, 19, 9, 00, 00)]
		public void Test_MultipleCarrierHaveSameAirlinePrefix()
		{
			// Create 2 carrier with same airline prefix in different country and have different mawb rule
			TestDateAttribute.Date = new DateTime(2024, 11, 19, 9, 01, 00);
			var nzCarrier = CreateAirlineCarrier(Airline_Prefix, "NZ");
			TestDateAttribute.Date = new DateTime(2024, 11, 19, 9, 02, 00);
			var auCarrier = CreateAirlineCarrier(Airline_Prefix, "AU");
			UpdateOrgAirlineMAWBStockManagement(nzCarrier, null, null, true, false, false, "N");
			UpdateOrgAirlineMAWBStockManagement(auCarrier, null, null, true, true, true, "Y");

			// Create branch in different country
			var auBranch = CreateBranch("AUB", TestDbHelper.DefaultCompanyPK, "AUBranch", "AU");
			var nzBranch = CreateBranch("NZB", TestDbHelper.DefaultCompanyPK, "NZBranch", "NZ");
			var usBranch = CreateBranch("USB", TestDbHelper.DefaultCompanyPK, "USBranch", "US");

			// Create Company level mawbs
			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.JobMawb");
			_ = InsertMawb(Airline_Prefix, "11111111", TestDbHelper.DefaultCompanyPK, null, "ALL");
			_ = InsertMawb(Airline_Prefix, "22222222", TestDbHelper.DefaultCompanyPK, null, "ALL");

			// AU branch should be able to allocate company mawb
			var mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, auBranch, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("AU branch should be able to allocate mawb", "11111111", mawb);

			// NZ branch should not be able to allocate company mawb
			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, nzBranch, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("NZ branch should not be able to allocate mawb", "", mawb);

			// US branch should be able to allocate another company mawb at it gets the latest updated rule of auCarrier
			mawb = CallTryAllocateMAWB(Airline_Prefix, TestDbHelper.DefaultCompanyPK, usBranch, useOtherBranchMawbRegistryValue: false, "ALL", "", Guid.Empty);
			AssertEquals("US branch should be able to allocate mawb", "22222222", mawb);
		}

		const string Airline_Prefix = "618";
		const string CurrentBranch_PRI_MAWB = "00000001";
		const string CurrentBranch_STD_MAWB = "00000002";
		const string CurrentBranch_ALL_MAWB = "00000003";
		const string Company_PRI_MAWB = "10000001";
		const string Company_STD_MAWB = "10000002";
		const string Company_ALL_MAWB = "10000003";
		const string Global_PRI_MAWB = "20000001";
		const string Global_STD_MAWB = "20000002";
		const string Global_ALL_MAWB = "20000003";
		const string OtherBranch_PRI_MAWB = "30000001";
		const string OtherBranch_STD_MAWB = "30000002";
		const string OtherBranch_ALL_MAWB = "30000003";

		Guid CreateBranch(string code, Guid companyPK, string branchName, string countryCode)
		{
			var pk = Guid.NewGuid();
			DbHelper.Insert(GlbBranchSchema.Constants.TableName, new
			{
				GB_PK = pk,
				GB_Code = code,
				GB_GC = companyPK,
				GB_RN_NKCountryCode = countryCode,
				GB_BranchName = branchName
			});
			return pk;
		}

		void RecreateMawbs(Guid branch1Pk, Guid branch2Pk)
		{
			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.JobMawb");

			// Current branch mawb
			_ = InsertMawb(Airline_Prefix, CurrentBranch_PRI_MAWB, TestDbHelper.DefaultCompanyPK, branch1Pk, "PRI");
			_ = InsertMawb(Airline_Prefix, CurrentBranch_STD_MAWB, TestDbHelper.DefaultCompanyPK, branch1Pk, "STD");
			_ = InsertMawb(Airline_Prefix, CurrentBranch_ALL_MAWB, TestDbHelper.DefaultCompanyPK, branch1Pk, "ALL");

			// Company mawb
			_ = InsertMawb(Airline_Prefix, Company_PRI_MAWB, TestDbHelper.DefaultCompanyPK, null, "PRI");
			_ = InsertMawb(Airline_Prefix, Company_STD_MAWB, TestDbHelper.DefaultCompanyPK, null, "STD");
			_ = InsertMawb(Airline_Prefix, Company_ALL_MAWB, TestDbHelper.DefaultCompanyPK, null, "ALL");

			// Global mawb
			_ = InsertMawb(Airline_Prefix, Global_PRI_MAWB, null, null, "PRI");
			_ = InsertMawb(Airline_Prefix, Global_STD_MAWB, null, null, "STD");
			_ = InsertMawb(Airline_Prefix, Global_ALL_MAWB, null, null, "ALL");

			// Other branch mawb
			_ = InsertMawb(Airline_Prefix, OtherBranch_PRI_MAWB, TestDbHelper.DefaultCompanyPK, branch2Pk, "PRI");
			_ = InsertMawb(Airline_Prefix, OtherBranch_STD_MAWB, TestDbHelper.DefaultCompanyPK, branch2Pk, "STD");
			_ = InsertMawb(Airline_Prefix, OtherBranch_ALL_MAWB, TestDbHelper.DefaultCompanyPK, branch2Pk, "ALL");
		}

		string CallTryAllocateMAWB(string airlinePrefix, Guid companyID, Guid branchID, bool useOtherBranchMawbRegistryValue, string serviceLevel, string mawbBookingReference, Guid exceptMawbID)
		{
			var allocatedMawb = "";
			using (var command = TestConnection.Command("TryAllocateMAWB"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@CompanyID", SqlDbType.UniqueIdentifier, companyID);
				command.AddParameter("@BranchID", SqlDbType.UniqueIdentifier, branchID);
				command.AddParameter("@UseOtherBranchMawbRegistryValue", SqlDbType.Bit, useOtherBranchMawbRegistryValue);
				command.AddParameter("@AirlinePrefix", SqlDbType.VarChar, 3, airlinePrefix);
				command.AddParameter("@ServiceLevel", SqlDbType.VarChar, 3, serviceLevel);
				command.AddParameter("@BookingReference", SqlDbType.VarChar, JobMawbSchema.JM_MAWB.MaxLength, mawbBookingReference);
				command.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, 3, "JK");
				command.AddParameter("@ExceptMawbID", SqlDbType.UniqueIdentifier, exceptMawbID);
				command.AddParameter("@UserCode", SqlDbType.VarChar, 3, "~BP");
				command.AddOutputParameter("@JobMAWBId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);

				Guid allocatedMAWBId;
				try
				{
					command.ExecuteNonQuery();
					allocatedMAWBId = (Guid)command.GetParameterValue("@JobMAWBId");
					using (var queryCommand = TestConnection.Command($"SELECT JM_MAWB FROM dbo.JobMawb WHERE JM_PK = '{allocatedMAWBId}'"))
					{
						allocatedMawb = queryCommand.ExecuteScalar().ToString();
					}
				}
				catch (Exception ex)
				{
					if (ex.Message != "[=NO_MAWBS_AVAILABLE=]")
					{
						throw;
					}
				}
			}

			return allocatedMawb;
		}

		void UpdateOrgAirlineMAWBStockManagement(Guid carrierId, Object companyId, Object branchId, bool allowUseBranchStock, bool allowUseCompanyStock, bool allowUseGlobalStock, string allowUseOtherBranchStock)
		{
			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.OrgAirlineMAWBStockManagement WHERE OHM_OH_Carrier = '{carrierId}'");

			var pk = Guid.NewGuid();
			DbHelper.Insert(OrgAirlineMAWBStockManagementSchema.Constants.TableName, new
			{
				OHM_PK = pk,
				OHM_OH_Carrier = carrierId,
				OHM_GC_Company = companyId,
				OHM_GB_Branch = branchId,
				OHM_AllowUseBranchStock = allowUseBranchStock,
				OHM_AllowUseCompanyStock = allowUseCompanyStock,
				OHM_AllowUseGlobalStock = allowUseGlobalStock,
				OHM_AllowUseOtherBranchStock = allowUseOtherBranchStock,
			});
		}

		Guid CreateAirlineCarrier(string airlinePrefix, string countryCode)
		{
			var carrierOrgPk = Guid.NewGuid();
			DbHelper.Insert(OrgHeaderSchema.Constants.TableName, new
			{
				OH_PK = carrierOrgPk,
				OH_Code = carrierOrgPk.ToString().Substring(0, 9),
				OH_FullName = "Airline Organization",
				OH_IsShippingLine = true,
				OH_IsAirLine = true
			});

			var airlinePK = new Guid(TestConnection.ExecuteScalar(
				$"SELECT TOP 1 RM_PK FROM [dbo].[RefAirline] WHERE RM_EagleAddedAirlinePrefixOrAccountingCode = '{airlinePrefix}'").ToString());

			var orgMiscServPk = Guid.NewGuid();
			DbHelper.Insert(OrgMiscServSchema.Constants.TableName, new
			{
				OM_PK = orgMiscServPk,
				OM_RM_Airline = airlinePK,
				OM_RN_NKEXDefaultCntryOfOrigin = countryCode,
				OM_OH = carrierOrgPk
			});

			return carrierOrgPk;
		}

		Guid InsertMawb(string airLinePrefix, string mawb, object companyId, object branchId, string serviceLevel)
		{
			var mawbPK = Guid.NewGuid();

			DbHelper.Insert(JobMawbSchema.Constants.TableName, new
			{
				JM_PK = mawbPK,
				JM_Airline3DigitPrefix = airLinePrefix,
				JM_MAWB = mawb,
				JM_ServiceLevel = serviceLevel,
				JM_GB = branchId,
				JM_GC_Company = companyId,
				JM_SystemLastEditUser = "E",
				JM_SystemLastEditTimeUtc = DateTime.UtcNow,
			});

			return mawbPK;
		}
	}
}

