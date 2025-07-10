using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class NctsDataProviderHelper
{
	public static bool? ToOptionalSealIsValid(this ZString status)
	{
		switch (status)
		{
			case YesNoList.Codes.Yes:
				return true;
			case YesNoList.Codes.No:
				return false;
			default:
				return null;
		}
	}

	public static bool IsSendDestinationCountryAtConsignmentLevelV4(NctsHeader nctsHeader) => !nctsHeader.IsLinkedExport && nctsHeader.MovementHeader.IsUniformCountryOfDestination;
}
