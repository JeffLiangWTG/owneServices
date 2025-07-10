using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(fn_GetRefCusTariffView))]
	class fn_GetRefCusTariffViewTest : DbCreateScriptTest
	{
		public void Testfn_GetRefCusTariffView()
		{
			var tariffTypePK = CreateRefCusTariffType();

			var tariffItems1 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "28415090900", "Other chromates and dichromates; peroxochromates", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems1);

			var tariffItems2 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "63064010009", "Pneumatic mattresses, of cotton", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems2);

			var sqlQuery = "SELECT * FROM fn_GetRefCusTariffView('KR', @TariffCode, @valuationDate, 'HSN')";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@TariffCode", System.Data.SqlDbType.VarChar, "28415090900");
			command.AddParameter("@valuationDate", System.Data.SqlDbType.DateTime, new DateTime(2023, 2, 1));

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Other chromates and dichromates; peroxochromates", result.Rows[0]["Value"]);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@TariffCode", System.Data.SqlDbType.VarChar, "28415090900");
			command.AddParameter("@valuationDate", System.Data.SqlDbType.DateTime, new DateTime(2022, 12, 1));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No Tariff Data", 0, result.Rows.Count);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@TariffCode", System.Data.SqlDbType.VarChar, "63064010009");
			command.AddParameter("@valuationDate", System.Data.SqlDbType.DateTime, new DateTime(2023, 2, 1));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Pneumatic mattresses, of cotton", result.Rows[0]["Value"]);
		}
	}
}

