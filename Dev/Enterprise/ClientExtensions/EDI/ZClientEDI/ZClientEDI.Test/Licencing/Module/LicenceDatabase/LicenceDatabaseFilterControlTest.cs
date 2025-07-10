using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace ZClientEDI.Test.Licencing.Module.LicenceDatabase
{
	[TestedType(typeof(LicenceDatabaseFilterControl))]
	class LicenceDatabaseFilterControlTest : TestCaseWithFactory
	{
		public void TestContainsTokenAuthenticationEnabledColumn()
		{
			var collection = new LicenceDatabaseCollection(Factory);
			using (var filterControl = new LicenceDatabaseFilterControl(collection, new LicenceDatabaseFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "LD_TokenAuthenticationEnabled");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
