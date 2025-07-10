using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.US.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	public class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestIsFeeType()
		{
			AssertEquals(false, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Beef));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Blueberry));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals(false, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.DutiableMail));
			AssertEquals(false, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Duty));
			AssertEquals(false, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred));
			AssertEquals(false, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Avocado));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal));
			AssertEquals(false, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.FreshLimes));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Mango));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Sorghum));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.DairyFee));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Mushroom));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Raspberry));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Pork));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Potato));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Sugar));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.Watermelon));
			AssertEquals(true, EntryChargeTypeList.IsFeeType(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new EntryChargeTypeList();
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.UnitedStates;

		public void TestIsOtherRevenueAmountChargeCode()
		{
			Assert(EntryChargeTypeList.IsOtherRevenueAmountChargeCode(Core.Constants.USCustoms.FeeCodes.Coffee));
			Assert(!EntryChargeTypeList.IsOtherRevenueAmountChargeCode(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
		}
	}

	class ZZEntryChargeTypeListTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var prepareTestDataSql = @"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'US')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'US', 'United States', NULL)

INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', 'Accounting Class Fee Code', 'US')

INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '107', '107 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '053', '053 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '106', '106 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '672', '672 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '056', '056 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '110', '110 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '016', '016 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '496', '496 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '102', '102 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '501', '501 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '055', '055 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '108', '108 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '311', '311 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '499', '499 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '500', '500 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '103', '103 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '058', '058 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '022', '022 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '054', '054 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '090', '090 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '057', '057 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '105', '105 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '109', '109 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '079', '079 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '018', '018 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '104', '104 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '017', '017 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '124', '124 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', '125', '125 DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', 'DTY', 'DTY DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', 'ARS', 'ARS DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'ACFEE', 'MPC', 'MPC DESC FROM DB', '2000-01-01 00:00:00', '2076-06-01 00:00:00', 'US')
";
			Db.Connection.ExecuteNonQuery(prepareTestDataSql);
			var list = new EntryChargeTypeList();

			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Beef, Core.Constants.USCustoms.FeeCodes.Beef + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Pork, Core.Constants.USCustoms.FeeCodes.Pork + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Honey, Core.Constants.USCustoms.FeeCodes.Honey + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Cotton, Core.Constants.USCustoms.FeeCodes.Cotton + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Sugar, Core.Constants.USCustoms.FeeCodes.Sugar + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Potato, Core.Constants.USCustoms.FeeCodes.Potato + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.FreshLimes, Core.Constants.USCustoms.FeeCodes.FreshLimes + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Mushroom, Core.Constants.USCustoms.FeeCodes.Mushroom + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Watermelon, Core.Constants.USCustoms.FeeCodes.Watermelon + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, Core.Constants.USCustoms.FeeCodes.SoftwoodLumber + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Raspberry, Core.Constants.USCustoms.FeeCodes.Raspberry + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Blueberry, Core.Constants.USCustoms.FeeCodes.Blueberry + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Avocado, Core.Constants.USCustoms.FeeCodes.Avocado + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Mango, Core.Constants.USCustoms.FeeCodes.Mango + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Sorghum, Core.Constants.USCustoms.FeeCodes.Sorghum + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.DairyFee, Core.Constants.USCustoms.FeeCodes.DairyFee + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.HMF, Core.Constants.USCustoms.FeeCodes.HMF + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, Core.Constants.USCustoms.FeeCodes.MerchandiseInformal + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.DutiableMail, Core.Constants.USCustoms.FeeCodes.DutiableMail + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, "Antidumping Duty");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.CountervailingDuty, "Countervailing Duty");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.Duty, Core.Constants.USCustoms.FeeCodes.Duty + " DESC FROM DB");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, "Excise Tax Payable");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, "Excise Tax Deferred");
			AssertListCodeDescription(list, Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, Core.Constants.USCustoms.FeeCodes.ReconciliationInterest + " DESC FROM DB");
			AssertListNotCpntainsCode(list, Core.Constants.USCustoms.FeeCodes.DistilledSpirits);
			AssertListNotCpntainsCode(list, Core.Constants.USCustoms.FeeCodes.Tobacco);
			AssertListNotCpntainsCode(list, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertListNotCpntainsCode(list, Core.Constants.USCustoms.FeeCodes.OtherExcise);
			AssertListNotCpntainsCode(list, Core.Constants.USCustoms.FeeCodes.OtherAgencies);
		}

		void AssertListCodeDescription(Customs.EntryChargeTypeList list, ZString code, ZString desc)
		{
			Assert(list.ContainsCode(code));
			AssertEquals(desc, list.GetDescriptionFromCode(code));
		}

		void AssertListNotCpntainsCode(Customs.EntryChargeTypeList list, ZString code)
		{
			Assert(!list.ContainsCode(code));
		}
	}
}
