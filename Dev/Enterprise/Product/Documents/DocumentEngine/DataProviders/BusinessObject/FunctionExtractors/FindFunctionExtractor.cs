using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	class FindFunctionExtractor : BaseFunctionExtractor
	{
		internal FindFunctionExtractor(string parameters)
		{
			this.match = parameters;
		}

		readonly ZString match;

		public override MethodInfoChainLink GetMethodInfoChainLink(Type type)
		{
			MethodInfoChainLink result = null;

			if (typeof(IBODocDataProviderCollection).IsAssignableFrom(type))
			{
				result = new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod(nameof(IBODocDataProviderCollection.Find), new Type[] { typeof(ZString) }), new object[] { match });
			}
			else if (typeof(IBusinessObjectCollection).IsAssignableFrom(type))
			{
				result = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod(nameof(IBODocDataProviderCollection.Find), new Type[] { typeof(ZString) }), new object[] { match });
			}
			if (result != null)
			{
				try
				{
					result.TypeToReflect = BusinessObjectCollection.GetElementTypeFromCollectionType(type);
				}
				catch (ArgumentException)
				{
				}
			}
			return result;
		}

		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);
			}
			if (chainLink?.TypeToReflect == null)
			{
				throw new DataProviderException("The Find function only works on collections. Please check your Find macro and make sure it is used on a collection.", chainLinks);
			}
			return chainLink?.TypeToReflect;
		}
	}
}
