using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(PeriodEndDateEditForm))]
	public class PeriodEndDateEditFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PeriodEndDateEditForm(new PeriodManager(Factory));
		}

		public void TestFileNewMenuBaseMenuItemNotEnabled()
		{
			using (PeriodEndDateEditForm form = (PeriodEndDateEditForm)GetFormToBash())
			{
				form.Show();
				MenuItem fileNewMenuItem = form.FileMenuItem_ForTestOnly.MenuItems.FindByName(ZFormMenuStrategy.FileNewMenuItemName);
				Assert("File -> New menu item should not be enabled", fileNewMenuItem == null || !fileNewMenuItem.Enabled);
			}
		}

		public void TestNewPartitionBoundariesAreCreated()
		{
			CreatePartitions();

			AssertPartitionKeyResult(false);

			//Creating initial set of Periods
			NewYearPeriodSettings newYearSettings = new NewYearPeriodSettings();
			newYearSettings.StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			newYearSettings.PeriodFormat = newYearSettings.PeriodFormat;

			ZShort newYear = (ZShort)newYearSettings.EndDate.Year;
			NewYearPeriodSettings priorYearSettings = new NewYearPeriodSettings();
			priorYearSettings.StartDate = newYearSettings.StartDate.AddYears(-1);
			priorYearSettings.PeriodFormat = newYearSettings.PeriodFormat;

			PeriodManager newPeriodManager = new PeriodManager(Factory);
			newPeriodManager.CreatePeriodData(priorYearSettings, Factory, newYear - 1);
			newPeriodManager.CreatePeriodData(newYearSettings, Factory, newYear);
			newPeriodManager.FinancialYear = newYear;

			using (var form = new PeriodEndDateEditForm(newPeriodManager))
			{
				form.Show();
				form.FireSaveButton();
			}

			AssertPartitionKeyResult(true);
		}

		public void TestNoExceptionIsThrownDuringPeriodCreationIfPartitionDoesNotExist()
		{
			DropPartitions();

			AssertNoExceptionThrown(() =>
			{
					//Creating initial set of Periods
					NewYearPeriodSettings newYearSettings = new NewYearPeriodSettings();
				newYearSettings.StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
				newYearSettings.PeriodFormat = newYearSettings.PeriodFormat;

				ZShort newYear = (ZShort)newYearSettings.EndDate.Year;
				NewYearPeriodSettings priorYearSettings = new NewYearPeriodSettings();
				priorYearSettings.StartDate = newYearSettings.StartDate.AddYears(-1);
				priorYearSettings.PeriodFormat = newYearSettings.PeriodFormat;

				PeriodManager newPeriodManager = new PeriodManager(Factory);
				newPeriodManager.CreatePeriodData(priorYearSettings, Factory, newYear - 1);
				newPeriodManager.CreatePeriodData(newYearSettings, Factory, newYear);
				newPeriodManager.FinancialYear = newYear;

				using (var form = new PeriodEndDateEditForm(newPeriodManager))
				{
					form.Show();
					form.FireSaveButton();

					AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			});
		}

		void CreatePartitions()
		{
			var sql = @"IF NOT EXISTS(SELECT null FROM sys.partition_functions WHERE  NAME = 'PF_AccountingPeriodCompany')
						BEGIN
							DECLARE @sqlcmd nvarchar(max), @ids varchar(max);
							SELECT @ids = coalesce(@ids + ', ', '') +  a.AM_Key  FROM (SELECT '''' + convert(char(6), AM_Period) + convert(char(36), AM_GC_Company) + '''' as AM_Key from dbo.AccPeriodManagement) a

							SET @ids = '''99999900000000-0000-0000-0000-000000000000''' +  IIF(@ids IS NULL, '', ', ' + @ids) ;

							SET @sqlcmd = N'CREATE PARTITION FUNCTION PF_AccountingPeriodCompany(char(42)) AS RANGE LEFT FOR VALUES (' + @ids + N')' ;

							EXEC SP_EXECUTESQL @sqlcmd;
						END
						
						IF NOT EXISTS(SELECT * FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany')
						BEGIN
							CREATE PARTITION SCHEME PS_AccountingPeriodCompany AS PARTITION PF_AccountingPeriodCompany ALL TO ([REPORTGROUP]);
						END";

			((IDbConnected)Factory).Connection.ExecuteNonQuery(sql);
		}

		void DropPartitions()
		{
			var sql = @"--Creating partition Schema and Function
						IF OBJECT_ID(N'RptDtJobCostingData', N'U') IS NOT NULL
						BEGIN 
						
							IF EXISTS(SELECT * FROM sys.indexes WHERE name='CI_JCD_PostDate' AND object_id = OBJECT_ID('RptDtJobCostingData'))
							BEGIN
								DROP INDEX CI_JCD_PostDate ON dbo.RptDtJobCostingData
							END
						
							CREATE CLUSTERED INDEX X_CI_JCD_PostDate ON dbo.RptDtJobCostingData(JCD_PostDate) ON[REPORTGROUP]
						END

						IF EXISTS(SELECT * FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany')
						BEGIN
							DROP PARTITION SCHEME PS_AccountingPeriodCompany
						END

						IF EXISTS(SELECT null FROM sys.partition_functions WHERE  NAME = 'PF_AccountingPeriodCompany')
						BEGIN
							DROP PARTITION FUNCTION PF_AccountingPeriodCompany
						END
						
						IF OBJECT_ID(N'RptDtJobCostingData', N'U') IS NOT NULL
						BEGIN 
							IF EXISTS(SELECT * FROM sys.indexes WHERE name='X_CI_JCD_PostDate' AND object_id = OBJECT_ID('RptDtJobCostingData'))
							BEGIN
								DROP INDEX X_CI_JCD_PostDate ON dbo.RptDtJobCostingData
							END
						END";

			((IDbConnected)Factory).Connection.ExecuteNonQuery(sql);
		}

		void AssertPartitionKeyResult(bool loadPeriod = true)
		{
			var expectedPeriodKeys = new List<string>() { "99999900000000-0000-0000-0000-000000000000" };

			var sql = @"SELECT	CAST(value as char(42)) as PeriodKey
						FROM	sys.partition_functions AS pf
								INNER JOIN sys.partition_range_values AS prv ON prv.function_id = pf.function_id
						WHERE	pf.name = 'PF_AccountingPeriodCompany'";

			var dt = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			var actualPeriodKeys = new List<string>();
			foreach (DataRow row in dt.Rows)
			{
				actualPeriodKeys.Add(Convert.ToString(row["PeriodKey"]));
			}
			AssertEquals("Partition Key Count", expectedPeriodKeys.Count, dt.Rows.Count);
			AssertContainsExactElementsInAnyOrder(expectedPeriodKeys, actualPeriodKeys);
		}
	}
}
