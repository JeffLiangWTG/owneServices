using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Client.EDI.Web.Module
{
	public class GlbPersonModule : ZFilterStripGridModule
	{
		public GlbPersonModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.GlbPerson; }
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			ZQuery result = ZQuery.NoResultQuery;
			if (Page.SiteUser.IsLoggedIn)
			{
				OrgHeader currentOrg = Factory.Load<OrgHeader>(((OrgContactWebUser)Page.SiteUser).LoggedInOrganisation.PK);
				if (currentOrg != null && currentOrg.Contacts.Count > 0)
				{
					ZDBOnlyQuery subQuery = new ZDBOnlyQuery(typeof(GlbPerson));
					var filterQuery = ZString.Format($@"
PER_PK in
(
	select OC_PER from dbo.OrgContact
	join dbo.GlbPersonPrimaryRelationship on PPR_PrimaryId = OC_PK and PPR_PrimaryTableCode = 'OC'
	where
	OC_IsActive = 1
	and OC_OH = @OrganisationPK1
)
OR
PER_PK in
(
	select GS_PER from dbo.GlbStaff
	join dbo.GlbPersonPrimaryRelationship on PPR_PrimaryId = GS_PK and PPR_PrimaryTableCode = 'GS'
	join dbo.OrgContact on OC_PER = GS_PER and OC_IsActive = 1 and OC_OH = @OrganisationPK2
	where
	GS_IsActive = 1
	and GS_PER is not null
)
");
					var sqlParams = new ZSqlParameterCollection();
					sqlParams.Add("@OrganisationPK1", currentOrg.PK, OrgContactSchema.OC_OH);
					sqlParams.Add("@OrganisationPK2", currentOrg.PK, GlbBranchSchema.GB_OH_OrgProxy);

					subQuery.AddFilterAndZSQLParameterCollection(filterQuery, sqlParams);
					result = new ZQuery(subQuery);
				}
			}
			return result;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbPersonFilterBusinessObject();
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] result = new DataGridColumn[4];
			result[0] = new ZButtonColumn("Full Name", GlbPerson.Schema.PER_FullName);
			((ZButtonColumn)result[0]).CommandName = "Select";

			result[1] = new ZTextEditColumn("Email Address", GlbPerson.Schema.PER_PrimaryEmail);
			result[2] = new ZTextEditColumn("Primary Workplace", GlbPerson.Schema.PER_PrimaryWorkplace);
			result[3] = new ZTextEditColumn("Working Location", GlbPerson.Schema.PER_WorkingLocation);

			return result;
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return EDIDataRegistry.Instance.DefaultFilterLayoutForWebGlbPerson; }
		}

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(GlbPersonSchema.PER_FullName.Name, DefaultSortOrder) };
		}

		#endregion

		public override Type GridCollectionType
		{
			get { return typeof(GlbPersonCollection); }
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return GlbPersonSchema.PK;
			}
		}
	}
}
