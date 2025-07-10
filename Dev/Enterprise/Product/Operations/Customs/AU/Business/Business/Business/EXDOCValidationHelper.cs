using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class EXDOCValidationHelper
	{
		public static void CheckForProduceTypeIsHorticultureOrGrainsAndPlants(ZString produceType, ZPropertyInfo zPropertyInfo, ZString labelName)
		{
			if (UniversalReferenceHelper.Errata53Enabled() && EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(produceType))
			{
				zPropertyInfo.AddMessageError(ZString.Format("{0} must not be present when Produce Type is Horticulture or Grains and Seeds", labelName));
			}
		}
	}
}
