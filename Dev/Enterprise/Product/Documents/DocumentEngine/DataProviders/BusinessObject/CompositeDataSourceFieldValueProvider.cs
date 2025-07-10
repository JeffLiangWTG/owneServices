using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.DocumentEngine.DataProviders;

class CompositeDataSourceFieldValueProvider
{
	public CompositeDataSourceFieldValueProvider(string fieldNameWithoutPrefix, IEnumerable<(MethodInfoChainLink[] methodInfoChain, object dataSource)> fieldValueSources, Func<string, object, MethodInfoChainLink[], object> getFieldValue)
	{
		this.fieldNameWithoutPrefix = fieldNameWithoutPrefix;
		this.fieldValueSources = fieldValueSources.ToList();
		this.getFieldValue = getFieldValue;
	}

	public object GetFieldValue()
	{
		object result = null;

		foreach (var fieldValueSource in fieldValueSources)
		{
			result = getFieldValue(fieldNameWithoutPrefix, fieldValueSource.dataSource, fieldValueSource.methodInfoChain);
			if (result != null && !result.Equals(null))
			{
				return result;
			}
		}

		return result;
	}

	public MethodInfoChainLink[] GetNestedMethodChain()
	{
		return fieldValueSources[0].methodInfoChain;
	}

	readonly string fieldNameWithoutPrefix;
	readonly List<(MethodInfoChainLink[] methodInfoChain, object dataSource)> fieldValueSources;
	readonly Func<string, object, MethodInfoChainLink[], object> getFieldValue;

	public static readonly MethodInfo GetFieldValueMethod = typeof(CompositeDataSourceFieldValueProvider).GetMethod(nameof(GetFieldValue));
}
