using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.DocumentEngine.ReflectiveFieldMap;

namespace Enterprise.DocumentVisualizer.Business.Reflection
{
	sealed class VisualizerDataReflectorFilter : IDataReflectorFilter
	{
		public VisualizerDataReflectorFilter()
		{
			internalDataReflectorFilter = new DocDataReflectorFilter();
		}

		readonly IDataReflectorFilter internalDataReflectorFilter;

		public string NamespacePrefix => internalDataReflectorFilter.NamespacePrefix;

		public bool IsAllowed(PropertyInfo property) => internalDataReflectorFilter.IsAllowed(property);

		public bool IsAllowed(MethodInfo method) => internalDataReflectorFilter.IsAllowed(method);

		public bool CanHaveChildMembers(Type propertyType) => internalDataReflectorFilter.CanHaveChildMembers(propertyType);

		public bool IsCollection(Type type) => internalDataReflectorFilter.IsCollection(type) || IsReadOnlyCollection(type);

		public bool IsRelatedObject(Type returnType) => internalDataReflectorFilter.IsRelatedObject(returnType);

		public bool IsReadOnlyCollection(Type type) => type.GetInterfaces().Concat(new Type[] { type }).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
	}
}
