using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class UnmatchedOrganisationTest : CreateSystemOrgScriptsTest
	{
		protected override CreateSystemOrgScripts Scripts
		{
			get { return new CreateUNMATCHEDOrgScripts(); }
		}

		protected override void DoAdditionalAssertions()
		{
			base.DoAdditionalAssertions();
			AssertRecordInDB(
				"SELECT COUNT(*) FROM dbo.StmNote JOIN dbo.OrgHeader ON ST_ParentID = OH_PK AND ST_Table = @table WHERE OH_Code = @code",
				Scripts.OrgCode + " Org Note exists in system",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@code", Scripts.OrgCode, OrgHeaderSchema.OH_Code);
					cmd.AddParameterBasedOnDbColumn("@table", "OrgHeader", StmNoteSchema.ST_Table);
				});
		}
	}
}
