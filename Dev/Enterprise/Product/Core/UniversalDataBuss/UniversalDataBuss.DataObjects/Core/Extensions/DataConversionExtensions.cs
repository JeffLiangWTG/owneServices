using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class DataConversionExtensions
	{
		public static ZString GetUNLocoFromSuppliedLocationCode(this BusinessObjectFactory factory, ZString locationCode)
		{
			if (locationCode.Length == 3)
			{
				var refUnlocoLoader = ObjectFactory.Get<IRefUNLOCOLoader>();
				var refUnloco = refUnlocoLoader.LoadFromIATA(factory, locationCode);
				if (refUnloco != null)
				{
					locationCode = refUnloco.RL_Code;
				}
			}

			return locationCode;
		}

		public static ZString? GetLloydsIMO(this IRefVessel vessel)
		{
			return vessel != null ? vessel.RV_LloydsNumber : null;
		}
	}
}
