using System;
using System.Collections.Generic;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface IAttributesAttribute
	{
		bool HasAttributeDefined(string attributeName);

		List<string> GetAttributeDefinitions(Type propertyType, Func<PropertyInfo, string> getXsdType);
	}
}
