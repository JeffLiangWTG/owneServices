using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class GetEventLastDateTimeFunctionExtractor : BaseFunctionExtractor
	{
		internal GetEventLastDateTimeFunctionExtractor(string eventCode)
		{
			this.eventCode = eventCode;
		}

		readonly string eventCode;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			MethodInfoChainLink result = null;
			if (BODocDataProvider.IsBODocDataProvider(typeBeingReflected))
			{
				var methodInfo = typeof(BODocDataProvider).GetMethod("GetEventLastDateTime", new[] { typeof(BusinessObject), typeof(string) });
				result = new MethodInfoChainLink(methodInfo, new object[] { eventCode })
				{
					TypeToReflect = methodInfo.ReturnType
				};
			}
			return result;
		}

		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			if (typeof(IBODocDataProviderCollection).IsAssignableFrom(typeToReflect) || typeof(IBusinessObjectCollection).IsAssignableFrom(typeToReflect))
			{
				var propertyInfo = typeToReflect.GetProperty("Item", new[] { typeof(int) });
				chainLinks.Add(new MethodInfoChainLink(propertyInfo.GetGetMethod()));
				typeToReflect = propertyInfo.PropertyType;
			}

			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);

				return chainLink.MethodInfo.ReturnType;
			}

			return null;
		}
	}
}
