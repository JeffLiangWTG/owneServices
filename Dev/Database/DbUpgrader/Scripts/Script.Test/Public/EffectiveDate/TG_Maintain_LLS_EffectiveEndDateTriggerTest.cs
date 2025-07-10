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
	[TestedType(typeof(TG_Maintain_LLS_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_LLS_EffectiveEndDateTriggerTest : TG_GenericEndDateTriggerTest<HrlStaffPolicySchema, HrlStaffPolicy>
	{
		protected override void SetUp()
		{
			base.SetUp();
			var sql = new SqlQueryBuilder();
			policy = new HrlPolicy("Mine", "AU");
			policy.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		HrlPolicy policy;
		protected override HrlStaffPolicy Create(GlbStaff parent, DateTimeOffset effective)
			=> new HrlStaffPolicy(parent.PK, effective, policy);

		protected override IEnumerable<(string column, string sqlValue, object assertValue)> GetValidEdits()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("YYA");
			var newPolicy = new HrlPolicy("IDC", "US");

			staff.AppendInsertAndReturnObject(sql);
			newPolicy.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			return new (string, string, object)[] {
				("LLS_GS_Staff", $"'{staff.PK}'", staff.PK),
				("LLS_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("LLS_LLP_Policy", $"'{newPolicy.PK}'", newPolicy.PK),
				("LLS_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("LLS_SystemLastEditUser", "'XYZ'", "XYZ"),
			};
		}

		public override void TestUpdateColumnOneByOne()
		{
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };
			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var sql = new SqlQueryBuilder();
			var newPolicy = new HrlPolicy("Whatev", "US");
			newPolicy.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"LLS_PK", // We won't ever update the primary key of a row
				"LLS_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"LLS_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
				"LLS_SystemCreateTimeUtc", "LLS_SystemCreateUser" // Audit columns never updated
			};

			var original = new Dictionary<string, string>
			{
				{ "LLS_PK", $"'{pk}'" },
				{ "LLS_AutoVersion", "0" },
				{ "LLS_GS_Staff", $"'{staffs[0].PK}'" },
				{ "LLS_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "LLS_LLP_Policy", $"'{policy.PK}'" },
				{ "LLS_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "LLS_SystemCreateUser", "'XYZ'" },
				{ "LLS_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "LLS_SystemLastEditUser", "'YZX'" },
			};

			UpdateRowsOneByOneAndAssertChanged(pk, original, shouldNeverBeUpdatedDirectly);
		}
	}

	class HrlStaffPolicy : SQLDataObject<HrlStaffPolicy>, IContiguousHistory
	{
		public HrlStaffPolicy(Guid staff, DateTimeOffset effectiveDate, HrlPolicy policy)
		{
			LLS_GS_Staff = staff;
			LLS_EffectiveDate = effectiveDate;
			LLS_LLP_Policy = policy.PK;

			LLS_SystemCreateTimeUtc = DateTime.UtcNow;
			LLS_SystemLastEditTimeUtc = DateTime.UtcNow;
			LLS_SystemCreateUser = "E";
			LLS_SystemLastEditUser = "E";
		}

		public Guid LLS_GS_Staff { get; }
		public DateTimeOffset LLS_EffectiveDate { get; }
		public DateTimeOffset? LLS_AutoEffectiveEndDate { get; }
		public Guid? LLS_LLP_Policy { get; }

		public string LLS_SystemCreateUser { get; }
		public string LLS_SystemLastEditUser { get; }
		public DateTime LLS_SystemCreateTimeUtc { get; }
		public DateTime LLS_SystemLastEditTimeUtc { get; }

		DateTimeOffset IContiguousHistory.EffectiveDate => LLS_EffectiveDate;
		DateTimeOffset? IContiguousHistory.AutoEffectiveEndDate => LLS_AutoEffectiveEndDate;
	}

	class HrlPolicy : SQLDataObject<HrlPolicy>
	{
		public HrlPolicy(string name, string country)
		{
			LLP_Name = name;
			LLP_RN_NKCountry = country;

			LLP_SystemCreateTimeUtc = DateTime.UtcNow;
			LLP_SystemLastEditTimeUtc = DateTime.UtcNow;
			LLP_SystemCreateUser = "E";
			LLP_SystemLastEditUser = "E";
		}

		public string LLP_Name { get; }
		public string LLP_RN_NKCountry { get; }

		public string LLP_SystemCreateUser { get; }
		public string LLP_SystemLastEditUser { get; }
		public DateTime LLP_SystemCreateTimeUtc { get; }
		public DateTime LLP_SystemLastEditTimeUtc { get; }
	}
}
