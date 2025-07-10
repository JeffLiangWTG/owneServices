using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public static class BRRefCusProcedure
	{
		public static CodeDescriptionPairList GetRefCusProcedureList(BusinessObjectFactory factory, ZString shipmentType)
		{
			if (shipmentType == BRJobMessageTypeList.Codes.Export)
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, $"BR_RefCusProcedures_{shipmentType}");// Cache Key
				return factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();
					var procedures = new RefCusProcedure.Loader(factory).LoadForShipmentTypeAndZzzDataGrouping(shipmentType, Core.Constants.CountryCodes.Brazil);
					foreach (var procedure in procedures)
					{
						result.AddPair(procedure.ZZ6_ProcedureCode, procedure.ZZ6_Description);
					}
					result.Sort();
					return result;
				});
			}
			return new CodeDescriptionPairList();
		}

		public static RefCusProcedureCollection GetRefCusProcedureList(BusinessObjectFactory factory, ZString category, ZString messageType, ZString messageSubType, ZDateTime date)
		{
			return factory.GetCachedValue($"BRRefCusProcedures_{category}_{messageType}_{messageSubType}_{date:yyyy/dd/MM}",
				() => RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(factory, Core.Constants.CountryCodes.Brazil, messageType, messageSubType, date, false, category));
		}

		public static RefCusProcedure GetRefCusProcedure(BusinessObjectFactory factory, ZString category, ZString messageType, ZString messageSubType, ZString taxRegime, ZDateTime date)
		{
			return GetRefCusProcedureList(factory, category, messageType, messageSubType, date).FirstOrDefault(x => x.ZZ6_PreviousProcedureCode == taxRegime);
		}
	}
}
