using System.Data;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class OrganisationMergeSyncWithBorderWiseAction : IMergeAction
	{
		public const string OrgMergeUserCode = "~OM";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void IMergeAction.Merge(OrgHeader oldOrg, OrgHeader newOrg, OrganisationMergerActionOnSave action)
		{
			if ((action == OrganisationMergerActionOnSave.MergeOnly || action == OrganisationMergerActionOnSave.MergeAndDelete) &&
				oldOrg != null && oldOrg.Factory is IDbConnected dbConnected)
			{
				var sql = $@"
UPDATE {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName}
   SET {OrgHeaderSchema.Constants.OH_SystemLastEditUser} = '{OrgMergeUserCode}'
 WHERE {OrgHeaderSchema.Constants.PK} = @OrgPk";

				using (var command = dbConnected.Connection.Command(sql))
				{
					command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, oldOrg.PK.ToGuid());
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
