using System;
using System.Reflection;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	/// <summary>
	/// Filters and classifies types found when reflecting through properties and methods of a class.
	/// E.g., when looking for properties to use for doc macros, will filter out properties that
	/// return generic base classes like BusinessObjectCollection.
	/// </summary>
	public interface IDataReflectorFilter
	{
		string NamespacePrefix { get; }

		bool IsAllowed(PropertyInfo property);
		bool IsAllowed(MethodInfo method);
		bool CanHaveChildMembers(Type propertyType);
		bool IsCollection(Type type);
		bool IsRelatedObject(Type returnType);
	}
}
