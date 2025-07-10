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
	public class RefUNLOCOModule : ZFilterGridModule
	{
		public RefUNLOCOModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefUNLOCO; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefUNLOCOFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Location");
		}

		public override Type FilterControlType
		{
			get { return typeof(GUI.Modules.Location.RefUNLOCOFilterControl); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(WebRefUNLOCOFilterBusinessObject); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefUNLOCOSchema.RL_Code.Name, DefaultSortOrder) };
		}

		public override Type GridCollectionType
		{
			get { return typeof(RefUNLOCOCollection); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[3];

			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("9d814efa-ca94-4fcd-9e33-43175e24f34f", "Code"), RefUNLOCOSchema.RL_Code.Name);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";

			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("f7a42d6a-efa6-4c3b-9fb8-12d356303e7b", "Port Name"), RefUNLOCOSchema.RL_PortName.Name);
			fGridColumnFields[2] = new ZTextEditColumn(Res.GetString("46ef85dd-783c-408f-938f-2fa40fbb5150", "IATA"), RefUNLOCOSchema.RL_IATA.Name);
			return fGridColumnFields;
		}
	}
}
