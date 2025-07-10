using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class InventoryDataProviderHelper
{
	internal static bool IsUnloadingStateMISorDIF(this ZString unloadedState) 
	{
		switch (unloadedState)
		{
			case NctsUnloadedStateList.Codes.MIS:
			case NctsUnloadedStateList.Codes.DIF:
				return true;
		}
		return false;
	}

	internal static bool IsUnloadingStateNEWorDIF(this ZString unloadedState)
	{
		switch (unloadedState)
		{
			case NctsUnloadedStateList.Codes.NEW:
			case NctsUnloadedStateList.Codes.DIF:
				return true;
		}
		return false;
	}

	internal static bool IsUnloadingStateNEWorMISorDIF(this ZString unloadedState)
	{
		switch (unloadedState)
		{
			case NctsUnloadedStateList.Codes.NEW:
			case NctsUnloadedStateList.Codes.MIS:
			case NctsUnloadedStateList.Codes.DIF:
				return true;
		}
		return false;
	}

	internal static bool IsUnloadingStateDECorMISorDIF(this ZString unloadedState)
	{
		switch (unloadedState)
		{
			case NctsUnloadedStateList.Codes.DEC:
			case NctsUnloadedStateList.Codes.MIS:
			case NctsUnloadedStateList.Codes.DIF:
				return true;
		}
		return false;
	}

	internal static bool IsUnloadingStateDECorNEWorDIForDAM(this ZString unloadedState)
	{
		switch (unloadedState)
		{
			case NctsUnloadedStateList.Codes.NEW:
			case NctsUnloadedStateList.Codes.DEC:
			case NctsUnloadedStateList.Codes.DIF:
			case NctsUnloadedStateList.Codes.DAM:
				return true;
		}
		return false;
	}

	internal static bool IsUnloadingStateNEWorMIS(this ZString unloadedState)
	{
		switch (unloadedState)
		{
			case NctsUnloadedStateList.Codes.NEW:
			case NctsUnloadedStateList.Codes.MIS:
				return true;
		}
		return false;
	}

	internal static bool IsDifferent(IZType declared, IZType unloaded) => !unloaded.IsEmpty && !unloaded.Equals(declared);
}
