using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.EndpointManagement.Module.Testing
{
	class EdiTrustedMessagingConfigFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var collection = new EdiTrustedMessagingConfigGlobalCollection(Factory);
			using (var filterControl = new EdiTrustedMessagingConfigFilterControl(collection, new EdiTrustedMessagingConfigFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "ETM_Product");
				AssertHasColumn(columns, "ETM_CertificateType");
				AssertHasColumn(columns, "ETM_CertificateThumbprint");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
