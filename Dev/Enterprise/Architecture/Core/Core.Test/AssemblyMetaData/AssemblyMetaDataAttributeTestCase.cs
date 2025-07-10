using System;
using System.Linq;
using System.Reflection;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	[TestsSubclassesOf(typeof(AssemblyMetaDataAttribute))]
	public abstract class AssemblyMetaDataAttributeTestCase<TAssemblyMetaDataAttribute> : TestCase
		where TAssemblyMetaDataAttribute : AssemblyMetaDataAttribute, new()
	{
		public void TestEquals_SameInstance_ReturnsTrue()
		{
			var instance = GetAssemblyMetaDataAttributeForTesting();
			var result = instance.Equals(instance);

			Assert("Same instance should be true", result);
		}

		public void TestEquals_EquivalentInstances_ReturnsTrue()
		{
			var instance1 = GetAssemblyMetaDataAttributeForTesting();
			var instance2 = GetAssemblyMetaDataAttributeForTesting();

			var isReferenceEquals = object.ReferenceEquals(instance1, instance2);
			Assert("[PreCondition]: Should be using different instances", !isReferenceEquals);

			var result = instance1.Equals(instance2);
			Assert("Different instance with the same value should be true", result);
		}

		public void TestEquals_NullInstance_ReturnsFalse()
		{
			var instance = GetAssemblyMetaDataAttributeForTesting();

			var result = instance.Equals(null);

			Assert("Equals check against null should return false", !result);
		}

		public void TestImplementsIEquatableOrNoPublicProperties()
		{
			var type = typeof(TAssemblyMetaDataAttribute);

			var implementsIEquatable = type.GetInterfaces()
			.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>) && i.GetGenericArguments()[0] == type);

			var hasNoPublicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).All(p => p.DeclaringType != type);

			Assert($"Type {type.Name} must implement IEquatable<T> or have no public properties; implementsIEquatable: {implementsIEquatable}, hasNoPublicProperties: {hasNoPublicProperties}. This is needed to prevent possible duplicate result on AssemblyMetaDataReader.", implementsIEquatable || hasNoPublicProperties);
		}

		protected virtual TAssemblyMetaDataAttribute GetAssemblyMetaDataAttributeForTesting() =>
			new TAssemblyMetaDataAttribute();
	}
}
