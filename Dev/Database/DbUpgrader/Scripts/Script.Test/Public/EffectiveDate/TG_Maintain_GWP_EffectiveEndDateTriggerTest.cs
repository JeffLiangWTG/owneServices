using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.EffectiveDate;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	[TestedType(typeof(TG_Maintain_GWP_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GWP_EffectiveEndDateTriggerTest : TG_GenericEndDateTriggerTest<GlbWorkPatternSchema, GlbWorkPattern>
	{
		protected override GlbWorkPattern Create(GlbStaff parent, DateTimeOffset effective)
			=> new(parent.PK, "WP TEST", "This is a test workpattern.", effective, new DateTime(1900, 1, 1), true, null);

		protected override IEnumerable<(string column, string sqlValue, object assertValue)> GetValidEdits()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("YYA");

			var template = new GlbStaffChangeRequestTemplate("Code1");
			var changeRequest = new GlbStaffChangeRequest(template);

			_ = staff.AppendInsertAndReturnObject(sql);

			_ = template.AppendInsertAndReturnObject(sql);
			_ = changeRequest.AppendInsertAndReturnObject(sql);

			_ = TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			return [
				("GWP_GS_Staff", $"'{staff.PK}'", staff.PK),
				("GWP_Name", "'WorkPattern 1'", "WorkPattern 1"),
				("GWP_Comment", "'Comment 1'", "Comment 1"),
				("GWP_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GWP_StandardDuration", "'1900-1-1 20:00:00'", new DateTime(1900, 1, 1, 20, 0, 0)),
				("GWP_GCR_ChangeRequest", $"'{changeRequest.PK}'", changeRequest.PK),
				("GWP_IsApproved", "0", false),
				("GWP_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GWP_SystemLastEditUser", "'XYZ'", "XYZ"),
			];
		}

		public override void TestUpdateColumnOneByOne()
		{
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };
			_ = TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GWP_PK", // We won't ever update the primary key of a row
				"GWP_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GWP_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
				"GWP_SystemCreateTimeUtc", "GWP_SystemCreateUser" // Audit columns never updated
			};

			var original = new Dictionary<string, string>
			{
				{ "GWP_PK", $"'{pk}'" },
				{ "GWP_AutoVersion", "0" },
				{ "GWP_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GWP_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GWP_StandardDuration", "'1900-1-1 20:00:00'" },
				{ "GWP_Name", "'New Name'" },
				{ "GWP_Comment", "'New Comment'" },
				{ "GWP_GCR_ChangeRequest", "NULL" },
				{ "GWP_IsApproved", "1" },
				{ "GWP_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GWP_SystemCreateUser", "'XYZ'" },
				{ "GWP_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GWP_SystemLastEditUser", "'YZX'" },
			};

			UpdateRowsOneByOneAndAssertChanged(pk, original, shouldNeverBeUpdatedDirectly);
		}
	}

	class GlbWorkPattern : SQLDataObject<GlbWorkPattern>, IContiguousHistory
	{
		public GlbWorkPattern(Guid staff, string name, string comment, DateTimeOffset effectiveDate, DateTime standardDuration, bool isApproved, GlbStaffChangeRequest request)
		{
			GWP_GS_Staff = staff;

			GWP_Name = name;
			GWP_Comment = comment;

			GWP_EffectiveDate = effectiveDate;
			GWP_StandardDuration = standardDuration;

			GWP_IsApproved = isApproved;
			GWP_GCR_ChangeRequest = request?.PK;

			GWP_SystemCreateTimeUtc = DateTime.UtcNow;
			GWP_SystemLastEditTimeUtc = DateTime.UtcNow;
			GWP_SystemCreateUser = "E";
			GWP_SystemLastEditUser = "E";
		}

		public Guid GWP_GS_Staff { get; }
		public DateTimeOffset GWP_EffectiveDate { get; }
		public DateTimeOffset? GWP_AutoEffectiveEndDate { get; }

		public string GWP_Name { get; }
		public string GWP_Comment { get; }

		public DateTime GWP_StandardDuration { get; }

		public bool GWP_IsApproved { get; }
		public Guid? GWP_GCR_ChangeRequest { get; }

		public string GWP_SystemCreateUser { get; }
		public string GWP_SystemLastEditUser { get; }
		public DateTime GWP_SystemCreateTimeUtc { get; }
		public DateTime GWP_SystemLastEditTimeUtc { get; }

		DateTimeOffset IContiguousHistory.EffectiveDate => GWP_EffectiveDate;
		DateTimeOffset? IContiguousHistory.AutoEffectiveEndDate => GWP_AutoEffectiveEndDate;
	}

	class GlbStaffChangeRequest : SQLDataObject<GlbStaffChangeRequest>
	{
		public GlbStaffChangeRequest(GlbStaffChangeRequestTemplate template)
		{
			GCR_GSG_Template = template.PK;

			GCR_SystemCreateTimeUtc = DateTime.UtcNow;
			GCR_SystemLastEditTimeUtc = DateTime.UtcNow;
			GCR_SystemCreateUser = "E";
			GCR_SystemLastEditUser = "E";
		}

		public Guid GCR_GSG_Template { get; }

		public DateTime GCR_SystemCreateTimeUtc { get; }
		public string GCR_SystemCreateUser { get; }
		public DateTime GCR_SystemLastEditTimeUtc { get; }
		public string GCR_SystemLastEditUser { get; }
	}

	class GlbStaffChangeRequestTemplate : SQLDataObject<GlbStaffChangeRequestTemplate>
	{
		public GlbStaffChangeRequestTemplate(string code)
		{
			GSG_TemplateName = "Template " + Guid.NewGuid();

			GSG_Code = code;

			GSG_SystemCreateTimeUtc = DateTime.UtcNow;
			GSG_SystemLastEditTimeUtc = DateTime.UtcNow;
			GSG_SystemCreateUser = "E";
			GSG_SystemLastEditUser = "E";
		}

		public string GSG_TemplateName { get; }
		public string GSG_Code { get; }

		public DateTime GSG_SystemCreateTimeUtc { get; }
		public string GSG_SystemCreateUser { get; }
		public DateTime GSG_SystemLastEditTimeUtc { get; }
		public string GSG_SystemLastEditUser { get; }
	}
}
