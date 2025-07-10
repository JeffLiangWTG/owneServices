using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Modules.ServiceLevel;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class RefServiceLevelModule : ZFilterGridModule
	{
		public RefServiceLevelModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefServiceLevel; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefServiceLevelFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.ServiceLevel");
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			if (fFilterBusinessObjectDefaults == null)
			{
				fFilterBusinessObjectDefaults = new FilterBusinessObjectDefaults();
			}

			return fFilterBusinessObjectDefaults;
		}
		FilterBusinessObjectDefaults fFilterBusinessObjectDefaults;

		public override Type GridCollectionType
		{
			get { return typeof(WebServiceLevelCollection); }
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(WebRefServiceLevelFilterBusinessObject); }
		}

		public override Type FilterControlType
		{
			get { return typeof(RefServiceLevelFilterControl); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[2];
			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("bb12e3e4-ba84-4a9c-bb99-bcaf0779c69f", "Service Level Code"), RefServiceLevelSchema.Constants.RS_Code);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";
			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("3f6dbcb4-af8a-47a4-b9f6-6d9ef00536a6", "Service Level Description"), RefServiceLevelSchema.Constants.RS_Description);
			return fGridColumnFields;
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefServiceLevelSchema.RS_Code.Name, DefaultSortOrder) };
		}
	}
}
