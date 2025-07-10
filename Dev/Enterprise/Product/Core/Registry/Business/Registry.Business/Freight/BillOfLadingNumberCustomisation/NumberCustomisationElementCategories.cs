using System;

namespace Enterprise.Registry.Business
{
	[Flags]
	public enum NumberCustomisationElementCategories
	{
		None = 0,
		Standard = 1,
		LinerAgency = 2,
		FreightStandard = 4,
		SundryCharges = 8,
		Consol = 16,
		Domestic = 32,
		PackageID = 64,
		SupplierBooking = 128,
		ClientContract = 256,
		EMCSDeclaration = 512,
		ConsolidatedDeclaration = 1024,
		WarehouseJob = 2048,
		WarehouseOrder = 4096,
		WarehouseReceive = 8192,
		OceanCarrier = 16384,

		Default = Standard | FreightStandard,
	}
}
