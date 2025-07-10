using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.Module.Testing
{
	[TestedType(typeof(EdiIdentityApplicationFilterControl))]
	class EdiIdentityApplicationFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var collection = new EdiIdentityApplicationCollection(Factory);
			using (var filterControl = new EdiIdentityApplicationFilterControl(collection, new EdiIdentityApplicationFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "IDA_ApplicationName");
					AssertHasColumn(columns, "IDA_ClientID");
					AssertHasColumn(columns, "IDA_IsActive");
					AssertHasColumn(columns, "IDA_IsRollback");
					AssertHasColumn(columns, "IDA_RedirectUrlStatus");
					AssertHasColumn(columns, "IDA_ApplicationModule");
					AssertHasColumn(columns, "IsLicenceActive");
					AssertHasColumn(columns, "LicenceDatabase+EnterpriseID");
					AssertHasColumn(columns, "LicenceDatabase+EnterpriseCode");
					AssertHasColumn(columns, "LicenceDatabase+CompanyName");
					AssertHasColumn(columns, "LicenceDatabase+CompanyCode");
					AssertHasColumn(columns, "LicenceProductCode");
					AssertHasColumn(columns, "LicenceType");
					AssertHasColumn(columns, "LicenceDatabase+LD_ServerCode");
					AssertHasColumn(columns, "Tenant+IDT_Name");
					AssertHasColumn(columns, "TenantID");
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
