using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public static class NctsDepartureHeaderContainerValidationHelper
	{
		public static void CheckInvalidRC(this NctsDepartureHeaderContainer container)
		{
			ListValidation.ErrorIfInvalidPK(container.BC_RCInfo, ResString.GetMultilingualString("8FDC9E6F-8C1D-4ECF-B7DC-AB2C55D48D77", "Please enter a valid Container Type."));
		}

		public static void CheckInvalidMode(this NctsDepartureHeaderContainer container)
		{
			ListValidation.ErrorIfInvalidCode(container.BC_ModeInfo);
		}

		public static void CheckMandatoryContainerNum(this NctsDepartureHeaderContainer container)
		{
			var header = container.Header;
			if (header != null && header.MovementHeader is NctsDepartureMovementHeader moveHeader && moveHeader.BM_SealType == SealTypeList.Codes.ContainerSeal)
			{
				if (header.DepartureHeaderContainers.Cast<FRNctsDepartureHeaderContainer>().All(x => x.BC_ContainerNum.IsEmpty))
				{
					container.BC_ContainerNumInfo.AddMessageError(ResString.GetMultilingualString("D8FDDE7D-2727-4855-8F31-CA8E875A086D", "You have not entered a container number."));
				}
			}
		}
	}
}
