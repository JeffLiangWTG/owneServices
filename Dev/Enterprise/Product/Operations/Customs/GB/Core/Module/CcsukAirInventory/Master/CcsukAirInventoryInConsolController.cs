using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryInConsolController : CcsukAirInventoryController
	{
		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var consol = businessEntity as ForwardingConsol;
			var mawb = businessEntity as CusMAWB;

			if (mawb != null && mawb.Consol != null)
			{
				consol = mawb.Consol;
			}

			if (consol != null)
			{
				var consolForm = new ConsolForm(consol);
				consolForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.GB.CcsukAirInventory;
				return consolForm;
			}
			return base.GetForm(businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventoryInConsol; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusMAWB); }
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
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventory);
			var deleteForm = controller.ShowDeleteForm(sourceEntity);
			LastShownForm = controller.LastShownForm;
			return deleteForm;
		}
	}
}
