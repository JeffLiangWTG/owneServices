using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class GetDescriptionFromCodeFunctionExtractor : BaseFunctionExtractor
	{
		internal GetDescriptionFromCodeFunctionExtractor(string parameters)
		{
			this.match = parameters;
		}

		readonly string match;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type type)
		{
			return typeof(ICodeDescriptionBoolList).IsAssignableFrom(type)
				? new MethodInfoChainLink(type.GetMethod(nameof(ICodeDescriptionBoolList.GetDescriptionFromCode), new[] { typeof(string) }), new object[] { match }) { TypeToReflect = typeof(ZString) }
				: null;
		}

		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			if (chainLinks.Count == 0)
			{
				if (IsIBODocDataProviderCollection(typeToReflect) || IsIBusinessObjectCollection(typeToReflect))
				{
					var itemPropertyInfo = typeToReflect.GetProperty("Item", new Type[] { typeof(int) });
					if (itemPropertyInfo != null)
					{
						chainLinks.Add(new MethodInfoChainLink(itemPropertyInfo.GetGetMethod()));
						typeToReflect = itemPropertyInfo.PropertyType;
					}
				}
			}

			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);
			}

			return typeof(ZString);
		}

		static bool IsIBusinessObjectCollection(Type type)
		{
			return typeof(IBusinessObjectCollection).IsAssignableFrom(type);
		}

		static bool IsIBODocDataProviderCollection(Type type)
		{
			return typeof(IBODocDataProviderCollection).IsAssignableFrom(type);
		}
	}
}
