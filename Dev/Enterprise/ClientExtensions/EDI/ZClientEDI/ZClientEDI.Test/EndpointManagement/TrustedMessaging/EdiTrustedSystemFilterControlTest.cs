using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	class EdiTrustedSystemFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var collection = new EdiTrustedSystemCollection(Factory);
			using (var filterControl = new EdiTrustedSystemFilterControl(collection, new EdiTrustedSystemFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "ETS_Product");
				AssertHasColumn(columns, "ETS_SystemID");
				AssertHasColumn(columns, "ETS_Description");
				AssertHasColumn(columns, "ETS_SystemNumber");
				AssertHasColumn(columns, "HasSecretKey");
				AssertHasColumn(columns, "HasTSCCertificate");
				AssertHasColumn(columns, "ETS_AccessTokenExpiryOverride");
				AssertHasColumn(columns, "ETS_IssueRefreshToken");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
