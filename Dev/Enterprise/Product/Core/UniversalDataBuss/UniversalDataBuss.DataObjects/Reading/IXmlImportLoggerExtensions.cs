using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	internal static class IXmlImportLoggerExtensions
	{
		internal static void LogAddOrUpdate(this IXmlImportLogger logger, BusinessObject targetBO, bool newBOAdded, Type typeOfDataObjectUsedForUpdate)
		{
			var rootElementAttribute = typeOfDataObjectUsedForUpdate.GetAttribute<RootElementAttribute>();
			var rootElementTypeName = rootElementAttribute != null ? rootElementAttribute.RootElementName : targetBO.GetType().Name;
			if (newBOAdded)
			{
				logger.LogBoth(LogType.Information, Res.GetString("a82b66ee-a81e-4edb-8d56-8fc4826685e9", "Added {0} from {1}.", targetBO.HumanReadableName, rootElementTypeName));
			}
			else
			{
				logger.LogBoth(LogType.Information, Res.GetString("675c9f3d-54fb-4970-aa57-d5e4121c0612", "Updated {0} from {1}.", targetBO.HumanReadableName, rootElementTypeName));
			}
		}
	}
}
