using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Common.ModuleRegistration;

public static class CustomsModuleIDs
{
	public static readonly ModuleIdentifier JobDeclaration = ModuleIDs.Customs.JobDeclaration;
	public static readonly ModuleIdentifier EntryHeader = ModuleIDs.Customs.EntryHeader;
	public static readonly ModuleIdentifier Permits = ModuleIDs.Customs.Permits;

	public static class Universal
	{
		public static readonly ModuleIdentifier RefCusTariff = ModuleIDs.Customs.Universal.RefCusTariff;
		public static readonly ModuleIdentifier RefDataGrouping = ModuleIDs.Customs.Universal.RefDataGrouping;
		public static readonly ModuleIdentifier ZZRefCusCodeList = ModuleIDs.Customs.Universal.ZZRefCusCodeList;
		public static readonly ModuleIdentifier ZZRefCusProcedure = ModuleIDs.Customs.Universal.ZZRefCusProcedure;
	}
}
