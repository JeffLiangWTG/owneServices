using CargoWise.Database.Abstractions.Extensions;

namespace Enterprise.Client.EDI
{
	public static class HrSchema
	{
		#region EdiStaffChange

		internal static DatabaseObjectCreateScript EdiStaffChange
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiStaffChange", @"
CREATE TABLE dbo.EdiStaffChange
(
	[ES9_PK] UNIQUEIDENTIFIER NOT NULL,
	[ES9_GS] UNIQUEIDENTIFIER NOT NULL,
	[ES9_IsProfilePhoto] bit NOT NULL default(0),
	CONSTRAINT [PK_UX__ES9_PK] PRIMARY KEY NONCLUSTERED ([ES9_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiStaffChange]
    SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__ES9_GS] ON [EdiStaffChange] ([ES9_GS] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
",
"DROP TABLE EdiStaffChange");
			}
		}

		#endregion

		internal static DatabaseObjectCreateScript EdiGlbStaffEx
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiGlbStaffEx", @"
create table dbo.EdiGlbStaffEx
(
	GS9_PK uniqueidentifier not null,
	GS9_GS uniqueidentifier not null CONSTRAINT [EdiGlbStaffEx_GS9_GS_FK2_GlbStaff] REFERENCES GlbStaff (GS_PK) on delete cascade,
	GS9_FirstName nvarchar(100) not null default (''),
	GS9_MiddleName nvarchar(100) not null default (''),
	GS9_LastName nvarchar(100) not null default (''),
	GS9_DomesticName nvarchar(256) not null default (''),
	GS9_EdiActiveDirectoryObjectGuid uniqueidentifier null,
CONSTRAINT [PK_EdiGlbStaffEx] PRIMARY KEY NONCLUSTERED (GS9_PK ASC) WITH (ALLOW_PAGE_LOCKS = OFF),
);

ALTER TABLE [EdiGlbStaffEx]
    SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__GS9_GS] ON EdiGlbStaffEx (GS9_GS ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE UNIQUE INDEX [NR_UX__GS9_EdiActiveDirectoryObjectGuid] ON EdiGlbStaffEx (GS9_EdiActiveDirectoryObjectGuid ASC)
WHERE GS9_EdiActiveDirectoryObjectGuid IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF)

				", "DROP TABLE EdiGlbStaffEx");
			}
		}

		internal static DatabaseObjectCreateScript GlbStaffHoliday_NR_RX__GA_GS_GA_RecordType_GA_LeaveComment
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__GA_GS_GA_RecordType_GA_LeaveComment",
					"CREATE NONCLUSTERED INDEX NR_RX__GA_GS_GA_RecordType_GA_LeaveComment ON GlbStaffHoliday (GA_GS, GA_RecordType, GA_LeaveComment) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX GlbStaffHoliday.NR_RX__GA_GS_GA_RecordType_GA_LeaveComment");
			}
		}
	}
}
