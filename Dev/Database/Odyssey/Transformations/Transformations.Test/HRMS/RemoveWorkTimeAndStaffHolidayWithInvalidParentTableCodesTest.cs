using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.HRMS;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.HRMS
{
	[TestedType(typeof(RemoveWorkTimeAndStaffHolidayWithInvalidParentTableCodes))]
	public class RemoveWorkTimeAndStaffHolidayWithInvalidParentTableCodesTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert(!Db.Connection.Exists(@"FROM dbo.GlbWorkTime WHERE GW_ParentTableCode NOT IN ('GE', 'GS', 'GWP', 'GWT', 'WW', 'GD')"));
			Assert(!Db.Connection.Exists(@"FROM dbo.GlbStaffHoliday WHERE GA_ParentTableCode NOT IN ('GS', 'FC', 'GD', '')"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveWorkTimeAndStaffHolidayWithInvalidParentTableCodes();

		protected override void PrepareTestData()
		{
			var gA_PK = new Guid();
			var gW_PK = new Guid();
			Db.Connection.ExecuteNonQuery(@$"
declare @staffID uniqueidentifier;
select top 1 @staffID = GS_PK from GlbStaff;

alter table GlbWorkTIme drop constraint if exists Constraint_GW_ParentTableCode;
alter table GlbStaffHoliday drop constraint if exists Constraint_GA_ParentTableCode;

INSERT INTO GlbStaffHoliday (
		[GA_PK]
      ,[GA_WorkHolidayType]
      ,[GA_ApprovalStatus]
      ,[GA_StartTime]
      ,[GA_EndTime]
      ,[GA_DaysLeaveTaken]
      ,[GA_GS]
      ,[GA_RecordType]
      ,[GA_ParentID]
      ,[GA_ParentTableCode]
      ,[GA_IsValid]
      ,[GA_IsWorkingAway]
      ,[GA_AvailabilityPercentage]
      ,[GA_LeaveComment]
      ,[GA_SystemCreateTimeUtc]
      ,[GA_SystemCreateUser]
      ,[GA_SystemLastEditTimeUtc]
      ,[GA_SystemLastEditUser]
      ,[GA_AutoVersion]
      ,[GA_OverrideLeaveTaken]
  ) VALUES
  (
	 '{gA_PK}',
     'ANN',
     'APP',
     '2024-09-17 00:00:00',
     '2024-09-17 00:00:00',
     2.00,
     @staffID,
     'LEV',
     null,
	 'BD',
	0,
	0,
	0,
	'',
     '2024-09-17 00:00:00',
      'DC3',
     '2024-09-17 00:00:00',
    'DC3',
	1,
	null);");

			Db.Connection.ExecuteNonQuery(@$"
declare @staffID uniqueidentifier;
select top 1 @staffID = GS_PK from GlbStaff;

INSERT INTO GlbWorkTime ( [GW_PK]
      ,[GW_ParentID]
      ,[GW_ParentTableCode]
      ,[GW_IsValid]
      ,[GW_DayOfWeek]
      ,[GW_StartTime]
      ,[GW_EndTime]
      ,[GW_AutoVersion]
      ,[GW_SystemCreateTimeUtc]
      ,[GW_SystemCreateUser]
      ,[GW_SystemLastEditTimeUtc]
      ,[GW_SystemLastEditUser]
	  ) VALUES (
	   '{gW_PK}',
	   @staffID,
	   'BD',
	   1,
	   'MON',
	   '1900-01-01 08:30:00',
	   '1900-01-01 18:00:00',
	   0,
	   '2024-09-17 00:00:00',
	   'DC3',
	   '2024-09-17 00:00:00',
	   'DC3'
	  )");
			Assert(Db.Connection.Exists(@$"FROM dbo.GlbStaffHoliday WHERE GA_PK = '{gA_PK}'"));
			Assert(Db.Connection.Exists(@$"FROM dbo.GlbWorkTime WHERE GW_PK = '{gW_PK}'"));
		}
	}
}
