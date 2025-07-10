using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.CIN;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.FR.Module
{
	public class CINTemporaryStorageConsolController : TemporaryStorageController
	{
		public CINTemporaryStorageConsolController()
			: base()
		{
		}
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new CINTemporaryStoragePlugin(businessEntity);

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.FR.CINTemporaryStorageConsolController; }
		}
		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|CINTemporaryStorageConsolController", "CIN"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var consol = businessEntity as ForwardingConsol;
			var storageHeader = businessEntity as CusTempStorageJobHeader;

			if (storageHeader != null && storageHeader.RelatedBusinessObject != null)
			{
				consol = storageHeader.RelatedBusinessObject as ForwardingConsol;
			}

			if (consol != null)
			{
				ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);
				var result = new ConsolForm(consol);
				result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.FR.CINTemporaryStorageConsolController;
				return result;
			}
			return base.GetForm(businessEntity);
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			=> factory.Load(TypeOfTopLevelBusinessObject, sourceEntityPK)
				?? factory.Load(typeof(ForwardingConsol), sourceEntityPK);
	}
}
