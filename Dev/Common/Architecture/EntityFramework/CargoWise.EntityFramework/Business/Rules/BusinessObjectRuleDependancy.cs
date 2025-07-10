using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WTG.Rules.Engine;

namespace CargoWise.EntityFramework.Business.Rules
{
	interface IBusinessObjectRuleDependancy
	{
		Type ResultType { get; }
	}

	class BusinessObjectRuleDependancy<T> : IRuleDependency<T>, IBusinessObjectRuleDependancy
	{
		readonly IEnumerable<PropertyInfo> propertyChain;

		public Type ResultType => propertyChain.Last().PropertyType;

		public BusinessObjectRuleDependancy(string propertyPath)
		{
			var properties = new List<PropertyInfo>();

			var previousType = typeof(T);
			foreach (var propertyName in propertyPath.Split('.'))
			{
				var prop = previousType.GetProperty(propertyName)
					?? throw new ArgumentException("'" + propertyPath + "' is not valid for " + typeof(T).FullName);

				previousType = prop.PropertyType;
				properties.Add(prop);
			}

			propertyChain = properties.AsReadOnly();
		}

		public Task<RuleDependencyResult<TResult>> GetValueAsync<TResult>(T obj)
		{
			RuleDependencyResult<TResult> result;
			if (TryGetValue(obj, out result))
			{
				return Task.FromResult(result);
			}

			throw new ArgumentException("The property could not be retrieved");
		}

		public bool TryGetValue<TResult>(T obj, out RuleDependencyResult<TResult> result)
		{
			object value = obj;
			foreach (var prop in propertyChain)
			{
				if (ReferenceEquals(null, value))
				{
					result = RuleDependencyResult<TResult>.NotAvailable;
					return false;
				}

				value = prop.GetValue(value);
			}

			result = new RuleDependencyResult<TResult>((TResult)value);
			return true;
		}
	}
}
