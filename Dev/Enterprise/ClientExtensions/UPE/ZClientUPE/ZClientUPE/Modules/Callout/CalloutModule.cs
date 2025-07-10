
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class CalloutModule : UPEAirCargoCalloutBaseModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.Callout; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CalloutCollection(Factory);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.Callout);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CalloutFilterControl(GridCollection, (CalloutFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CalloutFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}
	}
}
