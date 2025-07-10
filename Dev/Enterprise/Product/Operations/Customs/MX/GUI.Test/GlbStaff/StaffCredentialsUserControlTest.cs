using System.Linq;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(StaffCredentialsUserControl))]
	sealed class StaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestGridColumns()
		{
			using (var form = GetFormToBash())
			{
				var grid = (ZGrid)form.Controls.Find("CredentialsUserGrid", true).SingleOrDefault();
				AssertNotNull("Grid should not be null", grid);
				AssertEquals("Columns Count", 2, grid.ColumnStyles.Count);
				CombineAssertions("Column Styles", () =>
				{
					AssertNotNull($"'{GlbExternalPassword_MXL.Schema.GP_CertificateAuthority}' not null", grid.GetColumnStyle(GlbExternalPassword_MXL.Schema.GP_CertificateAuthority));
					AssertNotNull($"'{GlbExternalPassword_MXL.Schema.GP_UserID}' not null", grid.GetColumnStyle(GlbExternalPassword_MXL.Schema.GP_UserID));
				});
			}
		}
	}
}
