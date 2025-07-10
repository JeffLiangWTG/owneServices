using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class RefCommodityCodeModule : ZFilterGridModule
	{
		public RefCommodityCodeModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefCommodityCode; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefCommodityCodeFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Commodity");
		}

		public override Type FilterControlType
		{
			get { return typeof(GUI.Modules.Commodity.RefCommodityCodeFilterControl); }
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

		public override Type FilterBusinessObjectType
		{
			get { return typeof(WebRefCommodityCodeFilterBusinessObject); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefCommodityCodeSchema.RH_Code.Name, DefaultSortOrder) };
		}

		public override Type GridCollectionType
		{
			get { return typeof(RefCommodityCodeCollection); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[2];

			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("cd07ee1c-e125-4863-a8a3-bf65970f0109", "Code"), RefCommodityCodeSchema.RH_Code.Name);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";

			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("6fad663e-084d-4f9b-94e2-0a8870c01d75", "Description"), RefCommodityCodeSchema.RH_Description.Name);
			return fGridColumnFields;
		}
	}
}
