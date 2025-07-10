using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.JP.AFR.Business
{
	public enum BLLFunctionCode
	{
		[ResourceStringData("BLLFunctionCode|RegisterSplit", Caption = "Register Split")]
		RegisterSplit = 1,
		[ResourceStringData("BLLFunctionCode|RegisterSwitch", Caption = "Register Switch")]
		RegisterSwitch = 2,
		[ResourceStringData("BLLFunctionCode|RegisterMerge", Caption = "Register Merge")]
		RegisterMerge = 3,
		[ResourceStringData("BLLFunctionCode|CancelSplit", Caption = "Cancel Split")]
		CancelSplit = 4,
		[ResourceStringData("BLLFunctionCode|CancelSwitch", Caption = "Cancel Switch")]
		CancelSwitch = 5,
		[ResourceStringData("BLLFunctionCode|CancelMerge", Caption = "Cancel Merge")]
		CancelMerge = 6
	}
}
