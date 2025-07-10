using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefPostCodeSchema))]
[assembly: UsesConstants(typeof(RefCityTownSchema))]
[assembly: UsesConstants(typeof(RefCityPCodePivotSchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class RefCityPCodeDataFile : EmbeddedDataFile, IFixReferencesAndDuplicates
	{
		public RefCityPCodeDataFile() : base(DataFileRelativePath, DataFileTables)
		{
		}

		internal RefCityPCodeDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"Public\RefCityPCode\RefCityPCode.xml.gz";
		public override string ResourceRelativeName => "RefCityPCode.RefCityPCode.xml.gz";

		protected static readonly string[] DataFileTables = new string[3]
		{
			RefPostCodeSchema.Constants.TableName,
			RefCityTownSchema.Constants.TableName,
			RefCityPCodePivotSchema.Constants.TableName
		};

		protected override string SelectQuery
		{
			get
			{
				return @"
                    SELECT * FROM dbo.RefPostCode ORDER BY 1;
                    SELECT * FROM dbo.RefCityTown ORDER BY 1;
                    SELECT * FROM dbo.RefCityPCodePivot ORDER BY 1";
			}
		}

		#region PerformExtraDataManipulationBeforeEnableFks

		void IFixReferencesAndDuplicates.PerformExtraDataManipulationBeforeEnablingConstraints()
		{
			FixFkReferencesToConflictedPostCodePks();
			DeleteOrphanPivotRecords();
		}

		void DeleteOrphanPivotRecords()
		{
			string sqlText = "DELETE dbo.RefCityPCodePivot WHERE R0_RK = @orphanFK OR R0_R9 = @orphanFK";

			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@orphanFK", SqlDbType.UniqueIdentifier, DBNull.Value);
			foreach (Guid orphanFK in pivotOrphanFKs)
			{
				cmd.SetParameterValue("@orphanFK", orphanFK);
				cmd.ExecuteNonQuery();
			}
		}

		void FixFkReferencesToConflictedPks(string fixSqlText, Dictionary<Guid, Guid> conflictedPks)
		{
			DbCommand cmd = Db.Connection.Command(fixSqlText);
			cmd.AddParameter("@OldPk", SqlDbType.UniqueIdentifier, DBNull.Value);
			cmd.AddParameter("@NewPk", SqlDbType.UniqueIdentifier, DBNull.Value);

			foreach (Guid oldPk in conflictedPks.Keys)
			{
				cmd.SetParameterValue("@OldPk", oldPk);
				cmd.SetParameterValue("@NewPk", conflictedPks[oldPk]);
				cmd.ExecuteNonQuery();
			}
		}

		#region RefPostCode PK conflicts

		void FixFkReferencesToConflictedPostCodePks()
		{
			string sqlText = @"UPDATE dbo.RefCityPCodePivot SET R0_RK = @NewPk WHERE R0_RK = @OldPk";
			FixFkReferencesToConflictedPks(sqlText, postcodePKConflicts);

			sqlText = @"UPDATE dbo.RefCityPCodePivot SET R0_R9 = @NewPk WHERE R0_R9 = @OldPk";
			FixFkReferencesToConflictedPks(sqlText, citytownPKConflicts);
		}

		public void AddPostCodePkConflict(Guid oldPk, Guid newPk)
		{
			postcodePKConflicts.Add(oldPk, newPk);
		}

		public void AddCityTownPkConflict(Guid oldPk, Guid newPk)
		{
			citytownPKConflicts.Add(oldPk, newPk);
		}

		public void AddPivotOrphanFK(Guid pivotOrphanFk)
		{
			pivotOrphanFKs.Add(pivotOrphanFk);
		}

		protected Dictionary<Guid, Guid> postcodePKConflicts = new Dictionary<Guid, Guid>();
		protected Dictionary<Guid, Guid> citytownPKConflicts = new Dictionary<Guid, Guid>();
		protected List<Guid> pivotOrphanFKs = new List<Guid>();

		#endregion

		#endregion
	}
}
