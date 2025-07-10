using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Modules.Vessel;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class RefVesselModule : ZFilterGridModule
	{
		public RefVesselModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefVessel; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefVesselFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Vessel");
		}

		public override Type FilterControlType
		{
			get { return typeof(RefVesselFilterControl); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults fFilterBusinessObjectDefaults = new FilterBusinessObjectDefaults();
			return fFilterBusinessObjectDefaults;
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(WebRefVesselFilterBusinessObject); }
		}

		public override Type GridCollectionType
		{
			get { return typeof(RefVesselCollection); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefVesselSchema.RV_Code.Name, DefaultSortOrder) };
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[1];
			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("0e863ea2-15a6-4455-be35-1912b1851f8c", "Vessel Code"), RefVesselSchema.Constants.RV_Code);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";
			return fGridColumnFields;
		}
	}
}
