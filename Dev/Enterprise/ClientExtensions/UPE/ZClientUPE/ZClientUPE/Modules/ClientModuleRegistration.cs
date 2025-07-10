using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public abstract class ClientModuleRegistration
	{
		public static readonly ModuleIdentifier JobAirSailing = new ClientModuleIdentifier(UPEModuleId.UPEJobAirSailing, "Flight Schedule");
		public static readonly ModuleIdentifier AirCargo = new ClientModuleIdentifier(UPEModuleId.UPEAirCargo, "Consol Reporting");
		public static readonly ModuleIdentifier HouseAirCargo = new ClientModuleIdentifier(UPEModuleId.UPEHouseAirCargo, "Cargo Reporting");
		public static readonly ModuleIdentifier Organisation = new ClientModuleIdentifier(UPEModuleId.UPEOrganisation, "Organisation");
		public static readonly ModuleIdentifier OrgMatchApproval = new ClientModuleIdentifier(UPEModuleId.UPEOrgMatchApproval, "Organisation Matching");
		public static readonly ModuleIdentifier ImportClassification = new ClientModuleIdentifier(UPEModuleId.UPEImportClassification, "Import Class. Lookup");
		public static readonly ModuleIdentifier SupplierPart = new ClientModuleIdentifier(UPEModuleId.UPESupplierPart, "Products");

		public static readonly ModuleIdentifier JobDeclaration = new ClientModuleIdentifier(UPEModuleId.UPEJobDeclaration, "Customs Declaration");
		public static readonly ModuleIdentifier Callout = new ClientModuleIdentifier(UPEModuleId.UPECallout, "Finance");
		public static readonly ModuleIdentifier Enquiry = new ClientModuleIdentifier(UPEModuleId.UPEEnquiry, "Enquiry");
		public static readonly ModuleIdentifier Checkout = new ClientModuleIdentifier(UPEModuleId.UPECheckout, "Operations");
		public static readonly ModuleIdentifier DogHitXRay = new ClientModuleIdentifier(UPEModuleId.UPEDogHitXRay, "Dog Hit Or X-Ray");
		public static readonly ModuleIdentifier Allocation = new ClientModuleIdentifier(UPEModuleId.UPEAllocation, "Classifier Allocation");
		public static readonly ModuleIdentifier Dashboard = new ClientModuleIdentifier(UPEModuleId.UPEDashboard, "Dashboard");
		public static readonly ModuleIdentifier BatchPrinting = new ClientModuleIdentifier(UPEModuleId.UPEBatchPrinting, "Batch Printing");
		public static readonly ModuleIdentifier Reports = new ClientModuleIdentifier(UPEModuleId.UPEReports, "Reports", "Reports (UPE)");

		public static class Subcategory
		{
			public static ModuleTreeLoaderConstant.Entry UPE
			{
				get { return new ModuleTreeLoaderConstant.Entry("UPE", (NoResString)"UPS Express", "U"); }
			}
		}
	}
}
