using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NctsMovementInConsolController : NctsMovementController
	{
		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var consol = businessEntity as ForwardingConsol;
			var nctsHeader = businessEntity as NctsHeader;
			if (nctsHeader != null && nctsHeader.Consol != null)
			{
				consol = nctsHeader.Consol;
			}
			if (consol != null)
			{
				ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);
				var consolForm = new ConsolForm(consol);
				consolForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.EU.NctsMovementController;
				return consolForm;
			}
			return base.GetForm(businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.EU.NctsMovementInConsolController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NctsHeader); }
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var result = factory.Load(TypeOfTopLevelBusinessObject, sourceEntityPK) ?? factory.Load(typeof(ForwardingConsol), sourceEntityPK);
			return result;
		}

		public override IZForm ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			var newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
			var deleteForm = controller.ShowDeleteForm(sourceEntity);
			LastShownForm = controller.LastShownForm;
			return deleteForm;
		}
	}
}