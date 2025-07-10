using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.ArchiveManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions
{
	class PeriodArchiveCommencedAction : IArchiveAction
	{
		public void Add(Guid jobHeaderPK)
			=> jobHeaderPKList.Add(jobHeaderPK);

		readonly List<Guid> jobHeaderPKList = new();

		#region IArchiveAction Members

		public ITransactionManager BeginTransactionWithManager()
			=> Db.Connection.BeginTransactionWithManager();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "faster to do one single database hit.")]
		public void Execute()
		{
			if (jobHeaderPKList.Count == 0)
			{
				return;
			}

			var builder = new StringBuilder(jobHeaderPKList.Count * 39);
			foreach (var pk in jobHeaderPKList)
			{
				if (builder.Length > 0)
				{
					_ = builder.Append(",");
				}

				_ = builder.AppendFormat("'{0}'", pk);
			}

			var sql = string.Format(@"UPDATE dbo.AccPeriodManagement SET AM_ArchiveCommenced = 1,
																		AM_SystemLastEditTimeUtc = GETUTCDATE(),
																		AM_SystemLastEditUser = @SystemLastEditUser WHERE AM_ArchiveCommenced <> 1 AND AM_PK IN 
										(SELECT DISTINCT AM_PK FROM dbo.AccTransactionLines  
										JOIN dbo.GlbBranch  ON AL_GB = GB_PK 
										JOIN dbo.GlbCompany  ON GB_GC = GC_PK 
										JOIN dbo.AccPeriodManagement  ON AM_GC_Company = GB_GC AND (AL_PostDate BETWEEN AM_StartDate AND AM_EndDate)
										WHERE AL_JH IN ({0}))", builder.ToString());
			using var command = Db.Connection.Command(sql);
			_ = command.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
			_ = command.ExecuteNonQuery();
		}

		#endregion
	}
}
