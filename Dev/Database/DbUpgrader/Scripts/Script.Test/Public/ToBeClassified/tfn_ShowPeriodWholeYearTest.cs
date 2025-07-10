using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(tfn_ShowPeriodWholeYear))]
	class tfn_ShowPeriodWholeYearTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			//Insert period data for 2015 (CAL), 2016 (1WK), 2017(4WK), 2018(445)
			insertAccPeriodManagementData(TestDbHelper.DefaultCompanyPK);

			AssertPeriods(201512, 201511, 201510, 201509, 201508, 201507, 201506, 201505, 201504, 201503, 201502, 201501);
			AssertPeriods(201602, 201601, 201512, 201511, 201510, 201509, 201508, 201507, 201506, 201505, 201504, 201503);
			AssertPeriods(201652, 201651, 201650, 201649, 201648, 201647, 201646, 201645, 201644, 201643, 201642, 201641);
			AssertPeriods(201703, 201702, 201701, 201652, 201651, 201650, 201649, 201648, 201647, 201646, 201645, 201644);
			AssertPeriods(201713, 201712, 201711, 201710, 201709, 201708, 201707, 201706, 201705, 201704, 201703, 201702);
			AssertPeriods(201802, 201801, 201713, 201712, 201711, 201710, 201709, 201708, 201707, 201706, 201705, 201704);
		}

		void AssertPeriods(int initialPeriod, params int[] expectedperiods)
		{
			var dbHelper = new TestDbHelper(TestConnection);
			var reader = dbHelper.RunTableValuedFunction("tfn_ShowPeriodWholeYear", new { Period = initialPeriod, Company = TestDbHelper.DefaultCompanyPK });
			var dt = new DataTable();
			dt.Load(reader);

			AssertEquals("Number of Rows", 1, dt.Rows.Count);
			for (int i = expectedperiods.Length; i > 0; i--)
			{
				AssertEquals("Period " + i.ToString() + " Value", expectedperiods[11 - i], Convert.ToInt32(dt.Rows[0]["Period" + i.ToString()]));
			}
		}

		void insertAccPeriodManagementData(Guid companyPK)
		{
			var sql = string.Format(
			@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('ACE97F05-FE0E-4B98-964E-702147EE0EFE', 201501, 2015, '2015-01-01', '2015-01-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('845BF5D8-12BF-4E3B-A501-253E7758AAB8', 201502, 2015, '2015-02-01', '2015-02-28 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('151B0D98-9371-48F5-A370-2ED06BEB893A', 201503, 2015, '2015-03-01', '2015-03-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('0E98AB20-7A18-40CC-94B9-CD5EDC31B093', 201504, 2015, '2015-04-01', '2015-04-30 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('F108C457-4E58-484B-9357-328474863675', 201505, 2015, '2015-05-01', '2015-05-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('AC73668B-B59F-4380-84FA-6CE69893F1A4', 201506, 2015, '2015-06-01', '2015-06-30 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('BB77B13F-2AC8-4A25-868C-6DB1AF82997A', 201507, 2015, '2015-07-01', '2015-07-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('A63ED1CF-0659-4C0A-9A0F-EFC7D420FF93', 201508, 2015, '2015-08-01', '2015-08-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('3C83161F-31D4-47F1-9AAE-61ACDABD7821', 201509, 2015, '2015-09-01', '2015-09-30 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('BCF29DFA-10A9-4A4C-8CCF-3DC7BBBB985D', 201510, 2015, '2015-10-01', '2015-10-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('FA384186-0805-4582-9AA0-43985D5011B5', 201511, 2015, '2015-11-01', '2015-11-30 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('69535831-FEC0-4CDF-9B27-B9AA3ED2F3C6', 201512, 2015, '2015-12-01', '2015-12-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('55585E44-FB8F-4295-9034-6B2E25C09F45', 201601, 2016, '2016-01-01', '2016-01-08 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('FA02BE78-8D5A-43ED-BFEA-1EF8DB3FB591', 201602, 2016, '2016-01-09', '2016-01-15 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('EED48582-5529-4638-A3E5-4F82A1766AAC', 201603, 2016, '2016-01-16', '2016-01-22 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('E37B89FA-D97E-4D4C-B05A-9B70A050D994', 201604, 2016, '2016-01-23', '2016-01-29 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('944A68CA-0B7B-46FF-9B10-721E8A9F66C9', 201605, 2016, '2016-01-30', '2016-02-05 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('9FCF2F0A-5DC1-4C41-A176-AC9C9F9BD217', 201606, 2016, '2016-02-06', '2016-02-12 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('486EF84D-B6BC-46DB-8BF5-99D6E3D5DD05', 201607, 2016, '2016-02-13', '2016-02-19 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('2FEBD8A7-3938-4988-85F6-7CE3C46F06C3', 201608, 2016, '2016-02-20', '2016-02-26 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('21FCEB06-FB95-41F9-9F13-6F46E400C7B3', 201609, 2016, '2016-02-27', '2016-03-04 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('973DFDBE-7E26-4C93-A8F7-2977133D1EAC', 201610, 2016, '2016-03-05', '2016-03-11 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('9FCF7BEE-A9D1-4181-AD26-960A2A238196', 201611, 2016, '2016-03-12', '2016-03-18 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('04DC7A51-47E4-4FCF-BC19-0512D049BD4B', 201612, 2016, '2016-03-19', '2016-03-25 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('94B5FF29-C9C2-4892-A993-2269F842351E', 201613, 2016, '2016-03-26', '2016-04-01 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('6F055373-C8BB-422E-8A88-708995EB3BD3', 201614, 2016, '2016-04-02', '2016-04-08 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('DF22AC02-96AD-44A7-ADFC-5AB2437B4BB3', 201615, 2016, '2016-04-09', '2016-04-15 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('99B9962F-7BB0-40C1-B921-3AF9B08CCB7E', 201616, 2016, '2016-04-16', '2016-04-22 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('71EBCE4D-6728-4096-A6E3-B55F129B4ACB', 201617, 2016, '2016-04-23', '2016-04-29 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('7BCBF4B3-6A78-4B8A-8EFC-D2BCEEFE5FF0', 201618, 2016, '2016-04-30', '2016-05-06 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('F729645F-3FB3-4C0E-B154-E5C98B7080F1', 201619, 2016, '2016-05-07', '2016-05-13 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('5C0CA366-E03B-4253-A65E-94A23F3FAEEF', 201620, 2016, '2016-05-14', '2016-05-20 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('2AC70CD9-0E72-4EEA-91F6-F3FAF3781060', 201621, 2016, '2016-05-21', '2016-05-27 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('596363BD-1A61-460B-A353-170616B8BDC0', 201622, 2016, '2016-05-28', '2016-06-03 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('ECF08013-8E9E-402B-B221-C0794591B803', 201623, 2016, '2016-06-04', '2016-06-10 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('CA587FD9-F0C7-4A38-A855-B705D3A1E247', 201624, 2016, '2016-06-11', '2016-06-17 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('9EFF18DE-F766-4CAD-AFC0-3E5D677B5ECF', 201625, 2016, '2016-06-18', '2016-06-24 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('FF1AF24E-19BD-42D5-A2E8-0CC0C4D4EA36', 201626, 2016, '2016-06-25', '2016-07-01 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('06402B56-1817-4A9E-8F31-E976FF4644BF', 201627, 2016, '2016-07-02', '2016-07-08 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('F45F2ED3-F466-433B-852B-93E71A690FD2', 201628, 2016, '2016-07-09', '2016-07-15 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('20270F52-B669-42EF-AACD-89158AD9FF65', 201629, 2016, '2016-07-16', '2016-07-22 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('FDF1938B-437A-4539-90AC-A88FFC7C48F0', 201630, 2016, '2016-07-23', '2016-07-29 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('C955ED95-607D-4716-B2E0-E4FE54F6AD80', 201631, 2016, '2016-07-30', '2016-08-05 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('07F6DB14-7AE9-4145-B088-5893CAAB77B8', 201632, 2016, '2016-08-06', '2016-08-12 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('D1D09CBE-1218-4268-9CCA-73176D09453A', 201633, 2016, '2016-08-13', '2016-08-19 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('4E5C3DE0-F243-4AD6-A91E-223EFD50C46E', 201634, 2016, '2016-08-20', '2016-08-26 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('2B95FAB1-061D-4271-83F9-263D3E652F92', 201635, 2016, '2016-08-27', '2016-09-02 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('DCB845FC-5927-4509-A698-F989C61E4797', 201636, 2016, '2016-09-03', '2016-09-09 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('A391C02C-16F5-418C-A4D4-5E6CF948CF8F', 201637, 2016, '2016-09-10', '2016-09-16 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('BFFB9072-5D1B-4F2E-A8BC-714C031F4EAE', 201638, 2016, '2016-09-17', '2016-09-23 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('6991E86B-FAFF-4C5E-8E32-B8DFA54743D0', 201639, 2016, '2016-09-24', '2016-09-30 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('0359F0E0-0F86-4F9F-B81B-2866518CD97B', 201640, 2016, '2016-10-01', '2016-10-07 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('51E4397C-D872-4855-9626-6F5C3DE0776A', 201641, 2016, '2016-10-08', '2016-10-14 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('5B1A2FB1-CB83-4F06-8B30-9158B2F610E0', 201642, 2016, '2016-10-15', '2016-10-21 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('3041C50C-98E0-4C10-9846-0F7DCAB82B4D', 201643, 2016, '2016-10-22', '2016-10-28 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('F15A7F4F-A468-4A5E-883C-0CB86DBFE560', 201644, 2016, '2016-10-29', '2016-11-04 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('7B17F9D2-5E14-48EE-9272-EDC4AE10FAF6', 201645, 2016, '2016-11-05', '2016-11-11 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('23587072-AC5D-4690-96CB-C91E6FE8F118', 201646, 2016, '2016-11-12', '2016-11-18 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('88018AC7-62BE-4EE4-A49F-61EE529DFDD6', 201647, 2016, '2016-11-19', '2016-11-25 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('A75EE519-043D-427F-9B35-C4C631177A9C', 201648, 2016, '2016-11-26', '2016-12-02 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('37F2B6B4-CD0F-4162-9005-3422F4F22DEF', 201649, 2016, '2016-12-03', '2016-12-09 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('53E7CC6F-0B09-4A9F-A151-FE2B5077DD51', 201650, 2016, '2016-12-10', '2016-12-16 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('B5EEFA09-FC19-427F-A299-EB04D5CA9273', 201651, 2016, '2016-12-17', '2016-12-23 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('67F8F216-A84C-4EF7-9617-913E9FEAB0FC', 201652, 2016, '2016-12-24', '2016-12-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('FDFACBA8-A176-4F4C-AC1F-B514618E73DE', 201701, 2017, '2017-01-01', '2017-01-27 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('0B5E6689-26CE-4977-ACD7-576EA42E41CF', 201702, 2017, '2017-01-28', '2017-02-24 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('F7AED751-FC1A-42C6-AAE7-541825F12324', 201703, 2017, '2017-02-25', '2017-03-24 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('1FCAD6D6-D59E-4058-A308-9B5AAEC0F00E', 201704, 2017, '2017-03-25', '2017-04-21 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('B769A645-72C5-4F99-8CAB-5DEA8FA27D16', 201705, 2017, '2017-04-22', '2017-05-19 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('23F8730B-A1B5-4FE7-8CFC-54749C549F65', 201706, 2017, '2017-05-20', '2017-06-16 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('60B7125E-F319-4D13-8ED3-1D5B7E0B9438', 201707, 2017, '2017-06-17', '2017-07-14 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('68761B41-CA59-4494-8747-F1BB1999D96E', 201708, 2017, '2017-07-15', '2017-08-11 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('91A1B888-F193-4502-8F08-87E1B48D34BA', 201709, 2017, '2017-08-12', '2017-09-08 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('D291128B-3B4E-4734-BFF1-20A128285FAE', 201710, 2017, '2017-09-09', '2017-10-06 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('4F3E3DF6-686A-4CCA-8100-77F6ADFC3B37', 201711, 2017, '2017-10-07', '2017-11-03 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('CB357750-9A02-4B4E-8DFC-CB62C6249760', 201712, 2017, '2017-11-04', '2017-12-01 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('C39B7149-6A4C-484B-810C-97541F7CCED0', 201713, 2017, '2017-12-02', '2017-12-31 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('E2EF9EC6-8F3F-4655-8F9C-4A8AD07F8474', 201801, 2018, '2018-01-01', '2018-01-26 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('13B340F1-AA31-4D30-B64C-E876950B51EF', 201802, 2018, '2018-01-27', '2018-02-23 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('2554EA62-5856-442D-8854-D4A5BEB3E0CC', 201803, 2018, '2018-02-24', '2018-03-30 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('5A47A894-07AD-42D0-B35F-8B4879D4298A', 201804, 2018, '2018-03-31', '2018-04-27 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('280B63BA-FFC2-49F7-8F9B-C9A9AB7A327B', 201805, 2018, '2018-04-28', '2018-05-25 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('421E51CE-1000-4972-88A7-D4520B9C8769', 201806, 2018, '2018-05-26', '2018-06-29 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('68BD5A99-1C5B-43CA-81ED-A8E88D7C9AAC', 201807, 2018, '2018-06-30', '2018-07-27 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('CDE4F578-88F2-4D31-880A-F9B0EB604610', 201808, 2018, '2018-07-28', '2018-08-24 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('C375766E-DB47-42E9-8BB8-5D52653224D4', 201809, 2018, '2018-08-25', '2018-09-28 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('4E1D5160-173C-41B5-8651-4CE10450C2E5', 201810, 2018, '2018-09-29', '2018-10-26 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('D501809C-CA03-4988-A880-6B598B81DEFE', 201811, 2018, '2018-10-27', '2018-11-23 23:59:00', '{0}', 0, 0, 0, 0)


INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
VALUES ('09ED5269-801F-4487-9A52-3055B76FDD92', 201812, 2018, '2018-11-24', '2018-12-31 23:59:00', '{0}', 0, 0, 0, 0)
", companyPK);

			TestConnection.ExecuteNonQuery(sql);
		}
	}
}

