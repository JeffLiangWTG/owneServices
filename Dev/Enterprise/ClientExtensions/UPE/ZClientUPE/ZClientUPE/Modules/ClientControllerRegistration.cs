using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public abstract class ClientControllerRegistration
	{
		public static readonly ControllerID AirCargo = new ClientControllerID("UPEAirCargo");
		public static readonly ControllerID AirCargoConsol = new ClientControllerID("UPEAirCargoConsol");
		public static readonly ControllerID JobDeclaration = new ClientControllerID("UPEJobDeclaration");
		public static readonly ControllerID Callout = new ClientControllerID("UPECallout");
		public static readonly ControllerID Enquiry = new ClientControllerID("UPEEnquiry");
		public static readonly ControllerID Checkout = new ClientControllerID("UPECheckout");
		public static readonly ControllerID DogHitXRay = new ClientControllerID("UPEDogHitXRay");
		public static readonly ControllerID Allocation = new ClientControllerID("UPEAllocation");
		public static readonly ControllerID Dashboard = new ClientControllerID("UPEDashboard");
	}
}
