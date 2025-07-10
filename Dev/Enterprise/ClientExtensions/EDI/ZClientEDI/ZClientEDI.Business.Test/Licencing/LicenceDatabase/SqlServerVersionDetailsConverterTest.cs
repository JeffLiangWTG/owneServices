using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class SqlServerVersionDetailsConverterTest : TestCase
	{
		public void TestEditionToCode()
		{
			AssertEquals("ToCode(DbConnection.SqlServerEdition.Desktop)", SqlServerEditionList.Codes.Desktop, SqlServerVersionDetailsConverter.ToCode(DbConnection.SqlServerEdition.Express));
			AssertEquals("ToCode(DbConnection.SqlServerEdition.Enterprise)", SqlServerEditionList.Codes.Enterprise, SqlServerVersionDetailsConverter.ToCode(DbConnection.SqlServerEdition.EnterpriseDeveloper));
			AssertEquals("ToCode(DbConnection.SqlServerEdition.Standard)", SqlServerEditionList.Codes.Standard, SqlServerVersionDetailsConverter.ToCode(DbConnection.SqlServerEdition.StandardWorkgroup));
			AssertEquals("ToCode(DbConnection.SqlServerEdition.Standard)", SqlServerEditionList.Codes.Other, SqlServerVersionDetailsConverter.ToCode(DbConnection.SqlServerEdition.Other));
		}
	}
}