using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Customs.Testing
{
	sealed class ZZEntryChargeTypeListTest : TestCaseWithFactory
	{
		public void TestZAList()
		{
			var list = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(typeof(ZZEntryChargeTypeList), list.GetType());
		}

		public void TestGetEntryChargeTypeForZZAndCusRateCode()
		{
			var prepareTestDataSql = @"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZZ')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'ZZ', 'Common', NULL)

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CG')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'CG', 'Congo', NULL)

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'ZA', 'South Africa', NULL)

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'NA')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'NA', 'Namibia', NULL)

INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping) VALUES(NEWID(), 'SMTAR', 'Countries that self manage Tariffs', 'ZZ')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'SMTAR', 'CG', 'Congo', '2021-06-01 00:00:00', '2076-06-01 00:00:00', 'ZZ')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(NEWID(), 'SMTAR', 'NA', 'Namibia', '2021-06-01 00:00:00', '2076-06-01 00:00:00', 'ZZ')

INSERT INTO RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
VALUES(NEWID(), 'AAA', 'AAA Record', 1, 'ZA', '')

INSERT INTO RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
VALUES(NEWID(), 'BBB', 'BBB Record', 1, 'NA','')

INSERT INTO RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
VALUES(NEWID(), 'DTY', 'DUTY', 1, 'NA','')

INSERT INTO RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
VALUES(NEWID(), 'VAT', 'VAT Record', 1, 'NA', '')
";
			Db.Connection.ExecuteNonQuery(prepareTestDataSql);

			var list = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("ZZ data", "AAA, VAT", list.CodesAsString);
			list = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.Congo);
			AssertEquals("Cus data", "DTY, EXC, ADD, LEV, CVD, OTH, VAT", list.CodesAsString);
			list = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.Namibia);
			AssertContainsExactElementsInAnyOrder("Cus and ZZ data and no duplicate DTY/VAT", new[] { "BBB", "DTY", "EXC", "ADD", "LEV", "CVD", "OTH", "VAT" }, list.GetAllCodes());
		}

		public void TestGetEntryChargeTypeIncludeParentDataGrouping()
		{
			var parentPK = ZGuid.NewZGuid().ToString();
			var insertParentDataGroupingData = System.FormattableString.Invariant(
						$@"Insert into {RefDataGroupingSchema.Constants.TableName}
					({RefDataGroupingSchema.Constants.PK}, {RefDataGroupingSchema.Constants.ZZZ_DataGrouping}, {RefDataGroupingSchema.Constants.ZZZ_Description}, {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping})
			Values ('{parentPK}', 'ZA2', 'Parent ZA', null)");

			using (var command = Db.Connection.Command(insertParentDataGroupingData))
			{
				command.ExecuteNonQuery();
			}

			string zaGroupingCode = (string)Db.Connection.ExecuteScalar($"select {RefDataGroupingSchema.Constants.PK} from {RefDataGroupingSchema.Constants.TableName} where ZZZ_DataGrouping = 'ZA' ");
			if (zaGroupingCode.IsNullOrEmpty())
			{
				zaGroupingCode = ZGuid.NewZGuid().ToString();
				var insertDataGroupingData = System.FormattableString.Invariant(
						$@"Insert into {RefDataGroupingSchema.Constants.TableName}
					({RefDataGroupingSchema.Constants.PK}, {RefDataGroupingSchema.Constants.ZZZ_DataGrouping}, {RefDataGroupingSchema.Constants.ZZZ_Description}, {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping})
			Values (newid(), 'ZA', 'South Africa', '{parentPK}')");

				using (var command = Db.Connection.Command(insertDataGroupingData))
				{
					command.ExecuteNonQuery();
				}
			}
			else
			{
				var updateDataGroupingData = System.FormattableString.Invariant(
				   $@"update {RefDataGroupingSchema.Constants.TableName}
					set {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping} = '{parentPK}' where ZZZ_DataGrouping = 'ZA'");
				using (var command = Db.Connection.Command(updateDataGroupingData))
				{
					command.ExecuteNonQuery();
				}
			}

			var insertCusRateTypeData = System.FormattableString.Invariant(
					$@"Insert into {RefCusRateTypeSchema.Constants.TableName}
					({RefCusRateTypeSchema.Constants.PK}, {RefCusRateTypeSchema.Constants.ZZR_RateType}, {RefCusRateTypeSchema.Constants.ZZR_Description}, {RefCusRateTypeSchema.Constants.ZZR_IsPayable}, {RefCusRateTypeSchema.Constants.ZZR_ZZZ_NKDataGrouping}, {RefCusRateTypeSchema.Constants.ZZR_CustomsValueFormula})
			Values (newid(), 'TP1', 'Anti-Dumping', 1, 'ZA', ''),
					(newid(), 'PEN', 'Penalties', 1, 'ZA2', ''),
					(newid(), 'TP2', 'Anti-Dumping', 0, 'ZA', '')");

			using (var command = Db.Connection.Command(insertCusRateTypeData))
			{
				command.ExecuteNonQuery();
			}

			var insertCusTaxOrFeeType = System.FormattableString.Invariant(
					$@"Insert into {RefCusTaxOrFeeTypeSchema.Constants.TableName}
					({RefCusTaxOrFeeTypeSchema.Constants.PK}, {RefCusTaxOrFeeTypeSchema.Constants.ZX0_TaxOrFeeType}, {RefCusTaxOrFeeTypeSchema.Constants.ZX0_Description})
			Values (newid(), 'OTH', 'Others')");

			using (var command = Db.Connection.Command(insertCusTaxOrFeeType))
			{
				command.ExecuteNonQuery();
			}

			var insertCusTaxOrFee = System.FormattableString.Invariant(
					$@"Insert into {RefCusTaxOrFeeSchema.Constants.TableName}
					({RefCusTaxOrFeeSchema.Constants.PK}, {RefCusTaxOrFeeSchema.Constants.ZZF_ZX0_NKTaxOrFeeType}, {RefCusTaxOrFeeSchema.Constants.ZZF_ZZZ_NKDataGrouping}, {RefCusTaxOrFeeSchema.Constants.ZZF_Code}, {RefCusTaxOrFeeSchema.Constants.ZZF_Description},{RefCusTaxOrFeeSchema.Constants.ZZF_Value})
					Values (newid(), 'OTH', 'ZA', 'MIN', 'Minima', 0.8)");

			using (var command = Db.Connection.Command(insertCusTaxOrFee))
			{
				command.ExecuteNonQuery();
			}

			Factory.Save();

			var list = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode("TP1"));
			Assert(!list.ContainsCode("TP2"));
			Assert(list.ContainsCode("PEN"));
			Assert(list.ContainsCode(Core.Constants.Customs.CusEntryFeeTypes.VAT));
			Assert(list.ContainsCode("OTH"));
		}
	}
}
