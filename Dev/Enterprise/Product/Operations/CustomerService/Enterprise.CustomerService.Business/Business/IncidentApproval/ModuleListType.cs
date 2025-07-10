using CargoWiseOne.ResourceStrings;
namespace Enterprise.CustomerService.Business
{
	public enum ModuleListType
	{
		Unspecified,

		[ResourceStringData("ModuleListType|MenuSection", Caption = "Menu Section")]
		MenuSection,

		[ResourceStringData("ModuleListType|Cr8", Caption = "Requirement")]
		Cr8,

		[ResourceStringData("ModuleListType|Cr9", Caption = "Service")]
		Cr9,

		[ResourceStringData("ModuleListType|DetectedMenuItem", Caption = "Detected Menu Item")]
		DetectedMenuItem
	}
}
