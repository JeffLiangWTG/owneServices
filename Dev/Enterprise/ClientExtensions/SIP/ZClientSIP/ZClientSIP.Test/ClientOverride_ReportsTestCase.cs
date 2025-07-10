using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SIP.Testing
{
	public class ClientOverride_ReportsTestCase : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestClientProfitAndLossReportStockWell()
		{
			ZQuery filter = new ZQuery();
			filter.OrderBy = AccGLHeaderSchema.AG_AccountNum.Name;
			AccGLHeader glHeader = (AccGLHeader)Factory.LoadTop1(typeof(AccGLHeader), filter);
			AccGLAggregate gLAggregate = Factory.New<AccGLAggregate>();
			gLAggregate.AA_AG = glHeader.PK;
			gLAggregate.AA_GB = GlbBranch.CurrentBranch.PK;
			gLAggregate.AA_GC = GlbCompany.CurrentCompany.PK;
			gLAggregate.AA_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			filter.AddToFilter(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.NotEqual, glHeader.AG_AccountNum);
			AccGLHeader grossProfitTotalAccount = (AccGLHeader)Factory.LoadTop1(typeof(AccGLHeader), filter);
			Db.Connection.ExecuteNonQuery(@"INSERT dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled) VALUES (newid(), 'GL_GROSS_PROFIT_TOTAL_ACCOUNT', NULL, NULL, 'BIN', 1, convert(varbinary(8000), N'" + grossProfitTotalAccount.PK.ToString() + "'), NULL, 0)");
			DbCommand command = Db.Connection.Command("EXEC ClientProfitAndLossReportStockWell '', '" + GlbCompany.CurrentCompany.PK.ToString() + "', '', '', '', ''");
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals("Department code", GlbDepartment.CurrentDepartment.GE_Code, reader.GetString(1).Trim());
				while (reader.Read())
				{
					AssertEquals("Account number should be greater than or equal to the registry item", true, grossProfitTotalAccount.AG_AccountNum.CompareTo(reader.GetString(1)) <= 0);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}
	}
}
