using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Common.ModuleRegistration;

public static class CustomsControllerIDs
{
	public static readonly ControllerID JobDeclaration = ControllerIDs.Customs.JobDeclaration;
	public static readonly ControllerID JobDeclarationPluggedIntoShipment = ControllerIDs.Customs.JobDeclarationPluggedIntoShipment;
	public static readonly ControllerID SingleTariffClassification = ControllerIDs.Customs.SingleTariffClassification;
	public static readonly ControllerID SupplierPart = ControllerIDs.Customs.SupplierPart;
	public static readonly ControllerID CustomsStatement = ControllerIDs.Customs.CustomsStatement;
}
