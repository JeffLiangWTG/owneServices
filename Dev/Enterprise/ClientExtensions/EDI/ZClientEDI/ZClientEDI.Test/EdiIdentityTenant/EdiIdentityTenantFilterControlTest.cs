using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.Module.Testing
{
	[TestedType(typeof(EdiIdentityTenantFilterControl))]
	internal class EdiIdentityTenantFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var collection = new EdiIdentityTenantCollection(Factory);
			using (var filterControl = new EdiIdentityTenantFilterControl(collection, new EdiIdentityTenantFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "IDT_AuthorityUrl");
					AssertHasColumn(columns, "IDT_GraphClientId");
					AssertHasColumn(columns, "IDT_Name");
					AssertHasColumn(columns, "IDT_Onboarding");
					AssertHasColumn(columns, "IDT_TenantId");
					AssertHasColumn(columns, "IDT_OidcClientId");
				});
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
