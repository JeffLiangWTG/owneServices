using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class citf_FindTaxRateByTypeAndRateTest : ScriptTest
	{
		public void Testcitf_FindTaxRateByTypeAndRateTest()
		{
			TestObjectCreator.CreateTaxRateWithoutZZ("SER3", "Standard", 11, type: "SER");
			TestObjectCreator.CreateTaxRateWithoutZZ("SVC", "Standard", 5, type: "NOT");
			TestObjectCreator.CreateTaxRateWithoutZZ("NOTROD1", "Standard", 0, type: "NOT");
			TestObjectCreator.CreateTaxRateWithoutZZ("EXEMPT", "Standard", 0, type: "EXT");
			TestObjectCreator.CreateTaxRateWithoutZZ("NOTRT9", "Standard", 4, type: "NOT");
			TestObjectCreator.CreateTaxRateWithoutZZ("AST", "Standard", 12, type: "SER");
			TestObjectCreator.CreateTaxRateWithoutZZ("AST5a", "Standard", 6, type: "SER");
			TestObjectCreator.CreateTaxRateWithoutZZ("AST10", "Standard", 10, type: "SER");

			var result = RunScript("SER", "M").FirstOrDefault();
			AssertNotNull(result);
			AssertDataRow(result, header, valuesForSER);

			result = RunScript("NOT", "L").FirstOrDefault();
			AssertNotNull(result);
			AssertDataRow(result, header, valuesForNOT);
		}

		static EnumerableRowCollection<DataRow> RunScript(string type, string rate)
		{
			var sql = $"SELECT * FROM citf_FindTaxRateByTypeAndRate (@CurrentCompanyPK, @Type, @ExtraTaxRateType, @Rate, @TaxDate)";
			var command = Db.Connection.Command(sql);

			command.AddParameter("@CurrentCompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			command.AddParameter("@Type", SqlDbType.VarChar, type);
			command.AddParameter("@ExtraTaxRateType", SqlDbType.VarChar, string.Empty);
			command.AddParameter("@Rate", SqlDbType.VarChar, rate);
			command.AddParameter("@TaxDate", SqlDbType.SmallDateTime, DateTime.Now);
			var result = DataUtils.GetDataTableFromCommand(command).AsEnumerable();
			return result;
		}

		readonly string[] header = {
"ATV_Code",
"ATV_Description",
"ATV_Rate",
"ATV_Type",
"ATV_ExtraTaxRateType",
"ATV_RN_NKCountry",
"ATV_A9_DefaultVatClass",
"ATV_IsActive",
"RowNumberLow",
"RowNumberHigh" };

		readonly object[] valuesForSER = { "SER3", "Standard", 11.000M, "SER", "", "AU", null, true, (long)3, (long)2 };
		readonly object[] valuesForNOT = { "NOTRT9", "Standard", 4.000M, "NOT", "", "AU", null, true, (long)1, (long)2 };
	}
}
