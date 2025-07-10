using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.ZClientAPL.Testing
{
	public class APLClientOverride_DbUpgradeTest : TestCaseWithFactory
	{
		public void TestClient_sfn_APLCartageImportDeliveryReportFunction()
		{
			string companyPKString = GlbCompany.CurrentCompany.PK.ToString();
			DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_sfn_APLCartageImportDeliveryReport('AU', '" + companyPKString + "', 'All', 'All', 'Container')");
			AssertEquals("Client Function table should have been created OK", 0, table.Rows.Count);
		}

#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}
#endregion
	}
}
