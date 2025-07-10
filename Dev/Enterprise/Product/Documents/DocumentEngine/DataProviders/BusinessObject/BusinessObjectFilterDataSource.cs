using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders
{
	class BusinessObjectFilterDataSource : ZExpressionEvaluator
	{
		public BusinessObjectFilterDataSource(BusinessObject dataSource, string functionString)
			: base(functionString)
		{
			DataSource = dataSource;
		}
		readonly BusinessObject DataSource;

		protected override object GetValueFor(string propertyName)
		{
			Type typeOfDataSource = DataSource.GetType();
			var reflector = new BusinessObjectReflector();
			var methodInfoChainLink = reflector.GetMethodInfoChain(typeOfDataSource, DataSource, propertyName);
			if (methodInfoChainLink != null && methodInfoChainLink.Length > 0)
			{
				return getValueFromMethodInfoChainLinks(DataSource, methodInfoChainLink) ?? string.Empty;
			}

			FieldNotFoundException.ReportFieldNotFound(propertyName + (NoResString)" from [" + FunctionString + (NoResString)"]", new DataProviderList(BODocDataProvider.Get(DataSource)));
			return "<" + propertyName + ">";
		}

		object getValueFromMethodInfoChainLinks(object topLevelDataSource, MethodInfoChainLink[] methodInfoChainLinks)
		{
			var result = topLevelDataSource;
			foreach (var methodInfoLink in methodInfoChainLinks)
			{
				if (result == null)
				{
					return null;
				}

				result = BODocDataProvider.GetObject(methodInfoLink.ReflectOutObject(result, DataSource));
			}
			return result;
		}
	}
}
