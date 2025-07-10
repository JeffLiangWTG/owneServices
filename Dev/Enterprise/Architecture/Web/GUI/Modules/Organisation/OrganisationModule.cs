using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrganisationModule : ZFilterGridModule
	{
		public OrganisationModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.Organisation; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "OrganisationFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Organisation");
		}

		public override Type FilterControlType
		{
			get { return typeof(OrganisationFilterControl); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults fFilterBusinessObjectDefaults = new FilterBusinessObjectDefaults();
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault((ZString)OrganisationFilterBusinessObject.Schema.OH_DetailsFilter, (ZString)OrgConstants.FilterControl.OrgDetails.Common));
			return fFilterBusinessObjectDefaults;
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(OrganisationFilterBusinessObject); }
		}

		public override Type GridCollectionType
		{
			get { return typeof(OrgHeaderCollection); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(OrgHeaderSchema.OH_Code.Name, DefaultSortOrder) };
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[3];
			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("47c76dec-a191-4caf-b25c-143677c71436", "Company Code"), OrgHeaderSchema.Constants.OH_Code);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";
			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("e6f18647-7e1e-4f9d-a2ca-b4cef44bad10", "Company Name"), OrgHeaderSchema.Constants.OH_FullName);
			fGridColumnFields[2] = new ZTextEditColumn(Res.GetString("7552e66d-3823-49fa-a8ac-0ea3c75236ba", "Closest Port"), OrgHeaderSchema.Constants.OH_RL_NKClosestPort);
			return fGridColumnFields;
		}
	}
}
