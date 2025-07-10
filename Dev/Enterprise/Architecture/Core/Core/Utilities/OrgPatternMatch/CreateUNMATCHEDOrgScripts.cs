using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public class CreateUNMATCHEDOrgScripts : CreateSystemOrgScripts
	{
		#region SuppressResourceStringsCheckRegion

		protected override void AdditionalActions()
		{
			base.AdditionalActions();
			CreateUnmatchedOrgNote();
		}

		#region Create Note On Unmatched Org

		void CreateUnmatchedOrgNote()
		{
			string noteDescription = SourceGenerated.Res.GetString("873d104b-c768-4c89-91bc-bf3117cecd77", "This organization is a system-generated default organization, used for the purposes of matching when an organization cannot be found in the system during a data import. This organization will be used as a place holder for a consignee, consignor or broker, instead of creating temporary organizations, when an organization match is not available. This feature may be disabled in the system registry by your administrator.");
			string sQL = @"INSERT INTO dbo.StmNote (" +
									StmNoteSchema.PK.Name + ", " +
									StmNoteSchema.ST_ParentID.Name + ", " +
									StmNoteSchema.ST_Table.Name + ", " +
									StmNoteSchema.ST_Description.Name + ", " +
									StmNoteSchema.ST_NoteText.Name + "," +
									StmNoteSchema.ST_NoteType.Name +
									") VALUES ('" + Guid.NewGuid().ToString() + "', '" + OrgPK.ToString() + "', 'OrgHeader', 'Unmatched Org Details', '" + noteDescription + "', 'PRV')";
			Db.Connection.ExecuteNonQuery(sQL);
		}

		#endregion

		protected internal override string OrgCode
		{
			get { return "UNMATCHED"; }
		}

		protected internal override string OrgName
		{
			get { return "UNMATCHED ORGANISATION"; }
		}

		protected internal override string OA_Address2
		{
			get { return "PLEASE SEE ATTACHED NOTE"; }
		}

		protected internal override string OS_FullCompanyName
		{
			get { return "UNMATCHED ORGANISATION"; }
		}

		protected internal override string OS_CompanyName1
		{
			get { return "U532"; }
		}

		protected internal override string OS_CompanyName2
		{
			get { return "O625"; }
		}

		protected internal override string OS_CompanyName3
		{
			get { return ""; }
		}

		protected internal override string OS_CompanyName4
		{
			get { return ""; }
		}

		protected internal override string OS_Address1
		{
			get { return "A362"; }
		}

		protected internal override string OS_Address2
		{
			get { return "S121"; }
		}

		protected internal override string OS_Address3
		{
			get { return ""; }
		}

		protected internal override string OS_Address4
		{
			get { return ""; }
		}

		protected internal override string OS_City
		{
			get { return "N000"; }
		}

		protected internal override string OS_State
		{
			get { return "N200"; }
		}

		#endregion
	}
}
