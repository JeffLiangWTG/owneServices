using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class LocalCartageUpgradeTask : EmbeddedUpgradeTask
	{
		public LocalCartageUpgradeTask() : base(new LocalCartageDataFile())
		{
		}

		#region Overrides

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			DeleteCartageTypeAndItsAffiliates(sourceRow);
			base.DoInsert(sourceRow, targetTable, ref targetIndex);
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			DeleteCartageTypeAndItsAffiliates(targetRow);

			// Walk to next row on the target DataSet
			targetIndex++;
		}

		#endregion

		#region Implementation

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName == LocalCartageJobTypeSchema.E3_GE.Name)
			{
				return;
			}

			if (columnName == LocalCartageJobTypeSchema.E3_Description.Name)
			{
				return;
			}

			if (columnName == LocalCartageJobTypeSchema.E3_IsHidden.Name)
			{
				return;
			}

			if (columnName == LocalCartageJobOrgSchema.E5_IsBillToParty.Name)
			{
				return;
			}

			if (columnName == LocalCartageJobOrgSchema.E5_UsageComment.Name)
			{
				return;
			}

			if (sourceRow.Table.TableName == LocalCartageJobLegTypeSchema.Constants.TableName && ((bool)sourceRow["E4_IsBooking"]))
			{
				if (columnName == LocalCartageJobLegTypeSchema.E4_E5_FromOrg.Name)
				{
					return;
				}

				if (columnName == LocalCartageJobLegTypeSchema.E4_E5_WaitPointOrg.Name)
				{
					return;
				}

				if (columnName == LocalCartageJobLegTypeSchema.E4_E5_ToOrg.Name)
				{
					return;
				}

				if (columnName == LocalCartageJobLegTypeSchema.E4_EquipmentGroup.Name)
				{
					return;
				}
			}

			base.UpdateColumn(columnName, targetRow, sourceRow);
		}

		/// <summary>
		/// Attempt to delete a LocalCartageJobType based on the given PK by deleting its affiliates frist
		/// </summary>
		/// <param name="Pk">Primary Key</param>
		/// <returns>
		/// TRUE : Delete succeeded
		/// FALSE: Delete failed
		/// </returns>
		internal bool DeleteCartageTypeAndItsAffiliates(DataRow rowSource)
		{
			if (rowSource.Table.TableName.ToUpper() == LocalCartageJobTypeSchema.Constants.TableName.ToUpper())
			{
				return DeleteCartageTypeAndItsAffiliates((string)rowSource[LocalCartageJobTypeSchema.Constants.E3_JobType]);
			}
			else if (rowSource.Table.TableName.ToUpper() == LocalCartageJobOrgSchema.Constants.TableName.ToUpper())
			{
				return DeleteCartageTypeAndItsAffiliates((Guid)rowSource[LocalCartageJobOrgSchema.Constants.E5_E3]);
			}
			else if (rowSource.Table.TableName.ToUpper() == LocalCartageJobLegTypeSchema.Constants.TableName.ToUpper())
			{
				return DeleteCartageTypeAndItsAffiliates((Guid)rowSource[LocalCartageJobLegTypeSchema.Constants.E4_E3]);
			}
			else
			{
				throw new ArgumentException("Specified Table is not accepted: " + rowSource.Table.TableName);
			}
		}

		internal bool DeleteCartageTypeAndItsAffiliates(Guid pK)
		{
			bool result = false;
			try
			{
				string delLegs = "DELETE dbo.LocalCartageJobLegType WHERE E4_E3 = @PK";
				DbCommand cmdLegs = Db.Connection.Command(delLegs);
				cmdLegs.AddParameterBasedOnDbColumn("@Pk", pK, LocalCartageJobLegTypeSchema.E4_E3);
				cmdLegs.ExecuteNonQuery();

				string delOrgs = "DELETE dbo.LocalCartageJobOrg WHERE E5_E3 = @PK";
				DbCommand cmdOrgs = Db.Connection.Command(delOrgs);
				cmdOrgs.AddParameterBasedOnDbColumn("@Pk", pK, LocalCartageJobOrgSchema.E5_E3);
				cmdOrgs.ExecuteNonQuery();

				string delTypes = "DELETE dbo.LocalCartageJobType WHERE E3_PK = @PK";
				DbCommand cmdTypes = Db.Connection.Command(delTypes);
				cmdTypes.AddParameterBasedOnDbColumn("@Pk", pK, LocalCartageJobTypeSchema.PK);
				cmdTypes.ExecuteNonQuery();

				result = true;
			}
			catch (SqlException)
			{
				result = false;
			}

			return result;
		}

		internal bool DeleteCartageTypeAndItsAffiliates(string jobTypeCode)
		{
			string findJobType = "SELECT E3_PK From dbo.LocalCartageJobType WHERE E3_JobType = @JobType";
			DbCommand cmdFindJobType = Db.Connection.Command(findJobType);
			cmdFindJobType.AddParameterBasedOnDbColumn("@JobType", jobTypeCode, LocalCartageJobTypeSchema.E3_JobType);
			Guid? pK = (Guid?)cmdFindJobType.ExecuteScalar();

			if (pK.HasValue)
			{
				return DeleteCartageTypeAndItsAffiliates(pK.Value);
			}
			return false;
		}

		#endregion
	}
}
