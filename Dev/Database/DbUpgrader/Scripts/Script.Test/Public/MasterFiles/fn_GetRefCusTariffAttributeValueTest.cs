using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Testing.Public.MasterFiles
{
	[TestedType(typeof(fn_GetRefCusTariffAttributeValue))]
	class fn_GetRefCusTariffAttributeValueTest : DbCreateScriptTest
	{
		public void Testfn_GetRefCusTariffAttributeValue()
		{
			var tariffTypePK = CreateRefCusTariffType();
			var tariffItems1 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "2402201000", "Filter tip cigarettes", new DateTime(2015, 01, 01), new DateTime(2079, 01, 01) });
			var tariffPK = CreateRefCusTariff(tariffTypePK, tariffItems1);
			CreateRefCusTariffAttribute(tariffPK, "InvoiceQuantity in CU1");

			var tariffItems2 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "25010010", "Rock salt and sea salt made by the heat of the sun, not refined", new DateTime(2016, 01, 01), new DateTime(2079, 01, 01) });
			CreateRefCusTariff(tariffTypePK, tariffItems2);

			var tariffItems3 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "2402209000", "Other", new DateTime(2015, 01, 01), new DateTime(2020, 12, 31) });
			var tariffPK3 = CreateRefCusTariff(tariffTypePK, tariffItems3);
			CreateRefCusTariffAttribute(tariffPK3, "InvoiceQuantity in CU1");

			var sqlQuery = "SELECT * FROM fn_GetRefCusTariffAttributeValue('KR', @TariffCode, @valuationDate, 'HSN', 'InvoiceQuantity in CU1')";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@TariffCode", System.Data.SqlDbType.VarChar, "2402201000");
			command.AddParameter("@valuationDate", System.Data.SqlDbType.DateTime, new DateTime(2023, 11, 01));

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Y", result.Rows[0]["Value"]);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@TariffCode", System.Data.SqlDbType.VarChar, "25010010");
			command.AddParameter("@valuationDate", System.Data.SqlDbType.DateTime, new DateTime(2023, 11, 01));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No Tariff Attribute Data", 0, result.Rows.Count);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@TariffCode", System.Data.SqlDbType.VarChar, "2402209000");
			command.AddParameter("@valuationDate", System.Data.SqlDbType.DateTime, new DateTime(2023, 11, 01));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No Tariff Attribute Data", 0, result.Rows.Count);
		}
	}
}
