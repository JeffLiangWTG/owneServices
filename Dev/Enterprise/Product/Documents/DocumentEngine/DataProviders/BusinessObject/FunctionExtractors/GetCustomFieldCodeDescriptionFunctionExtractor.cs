using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors
{
	sealed class GetCustomFieldCodeDescriptionFunctionExtractor : BaseFunctionExtractor
	{
		readonly CustomFieldCodeDescriptionFunctionExtractorCommon commonCustomFieldCodeDescriptionFunctionExtractor;

		internal GetCustomFieldCodeDescriptionFunctionExtractor(string parameters)
		{
			this.commonCustomFieldCodeDescriptionFunctionExtractor =
				new CustomFieldCodeDescriptionFunctionExtractorCommon(parameters, null);
		}
		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected) => this.commonCustomFieldCodeDescriptionFunctionExtractor.GetMethodInfoChainLink(typeBeingReflected);
		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks) => this.commonCustomFieldCodeDescriptionFunctionExtractor.AddMethodInfoChainLinks(typeToReflect, chainLinks);
	}

	sealed class GetCustomFieldCodeDescriptionWithTypeFunctionExtractor : BaseFunctionExtractor
	{
		readonly CustomFieldCodeDescriptionFunctionExtractorCommon commonCustomFieldCodeDescriptionFunctionExtractor;

		internal GetCustomFieldCodeDescriptionWithTypeFunctionExtractor(string parameters)
		{
			var parsedParams = GetCustomFieldParameterParser.ExtractParameters(parameters);
			this.commonCustomFieldCodeDescriptionFunctionExtractor =
				new CustomFieldCodeDescriptionFunctionExtractorCommon(parsedParams.FieldName, parsedParams.TypeName);
		}
		public override MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected) => this.commonCustomFieldCodeDescriptionFunctionExtractor.GetMethodInfoChainLink(typeBeingReflected);
		internal override Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks) => this.commonCustomFieldCodeDescriptionFunctionExtractor.AddMethodInfoChainLinks(typeToReflect, chainLinks);
	}

	struct CustomFieldCodeDescriptionFunctionExtractorCommon
	{
		readonly string fieldName;
		readonly string fieldType;

		internal CustomFieldCodeDescriptionFunctionExtractorCommon(string fieldName, string fieldType)
		{
			this.fieldName = fieldName;
			this.fieldType = fieldType;
		}

		internal MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected)
		{
			MethodInfoChainLink result = null;

			if (BODocDataProvider.IsBODocDataProvider(typeBeingReflected))
			{
				var methodInfo = typeof(BODocDataProvider).GetMethod("GetCustomFieldCodeDescription", new Type[] { typeof(BusinessObject), typeof(string), typeof(string) });
				result = new MethodInfoChainLink(methodInfo, new object[] { fieldName, fieldType })
				{
					TypeToReflect = methodInfo.ReturnType
				};
			}

			return result;
		}

		internal Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks)
		{
			if (typeof(IBODocDataProviderCollection).IsAssignableFrom(typeToReflect) || typeof(IBusinessObjectCollection).IsAssignableFrom(typeToReflect))
			{
				var propertyInfo = typeToReflect.GetProperty("Item", new Type[] { typeof(int) });

				chainLinks.Add(new MethodInfoChainLink(propertyInfo.GetGetMethod()));
				typeToReflect = propertyInfo.PropertyType;
			}

			var chainLink = GetMethodInfoChainLink(typeToReflect);
			if (chainLink != null)
			{
				chainLinks.Add(chainLink);

				return chainLink.TypeToReflect;
			}

			return null;
		}
	}
}
