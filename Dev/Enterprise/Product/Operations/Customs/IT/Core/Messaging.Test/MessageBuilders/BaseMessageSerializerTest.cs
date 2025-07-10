using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.Testing;

public abstract class BaseMessageSerializerTest : TestCaseWithFactory
{
	public void TestAttributesMatchWithProperties()
	{
		CombineAssertions("Match property type with attribute", () =>
		{
			var testedTypes = new List<Type>();
			CheckProperties(MessageType, testedTypes);
		});
	}

	void CheckProperties(Type classType, List<Type> testedTypes)
	{
		if (!testedTypes.Contains(classType))
		{
			testedTypes.Add(classType);

			var properties = classType.GetProperties();
			foreach (var pInfo in properties)
			{
				CheckProperty(pInfo, testedTypes);
			}
		}
	}

	void CheckProperty(PropertyInfo pInfo, List<Type> testedTypes)
	{
		var propertyType = pInfo.PropertyType;
		var propertyTypeToCheck = IsEnumerableType(propertyType) ? propertyType.GetGenericArguments().First() : propertyType;

		if (IsZType(propertyTypeToCheck))
		{
			CheckLeafLevelProperty(pInfo, testedTypes);
		}
		else
		{
			CheckProperties(propertyTypeToCheck, testedTypes);
		}
	}

	void CheckLeafLevelProperty(PropertyInfo pInfo, List<Type> testedTypes)
	{
		var declaringType = pInfo.DeclaringType;
		if (declaringType.IsInterface)
		{
			CheckAdditionalRuntimeTypes(pInfo, testedTypes, declaringType);
		}
		else
		{
			CheckPropertyWithAttribute(pInfo);
		}
	}

	void CheckAdditionalRuntimeTypes(PropertyInfo pInfo, List<Type> testedTypes, Type declaringType)
	{
		var interfaceAssembly = declaringType.Assembly;
		var additionalRuntimeTypesToCheck = interfaceAssembly.GetTypes().Where(type => pInfo.DeclaringType.IsAssignableFrom(type) && !type.IsInterface);
		foreach (var runtimeType in additionalRuntimeTypesToCheck)
		{
			CheckProperties(runtimeType, testedTypes);
		}
	}

	bool IsZType(Type propertyType)
	{
		var nullablePtyType = Nullable.GetUnderlyingType(propertyType);

		return (nullablePtyType == null)
			? typeof(IZType).IsAssignableFrom(propertyType)
			: typeof(IZType).IsAssignableFrom(nullablePtyType);
	}

	void CheckPropertyWithAttribute(PropertyInfo pInfo)
	{
		var attributeType = pInfo.GetCustomAttribute<MessageFieldRepresentationAttribute>().GetType();
		if (MatchedTypes.ContainsKey(attributeType))
		{
			var expectedPropertyType = MatchedTypes[attributeType];
			var propertyType = pInfo.PropertyType;
			propertyType = IsNullableType(propertyType) || IsEnumerableType(propertyType) ? propertyType.GetGenericArguments().First() : propertyType;

			AssertEquals(FormattableString.Invariant($"Property {pInfo.Name}"), expectedPropertyType, propertyType);
		}
		else
		{
			throw new NotImplementedException(FormattableString.Invariant($"Add the attribute {attributeType.Name} to MatchedTypes dictionary"));
		}
	}

	bool IsEnumerableType(Type propertyType) => propertyType.GetInterfaces().Any(x => x == typeof(IEnumerable));
	bool IsNullableType(Type propertyType) => propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

	Dictionary<Type, Type> MatchedTypes
	{
		get
		{
			if (matchedTypes == null)
			{
				matchedTypes = new Dictionary<Type, Type>
				{
					{ typeof(MessageFieldBoolRepresentationAttribute), typeof(ZBool) },
					{ typeof(MessageFieldDateTimeRepresentationAttribute), typeof(ZDateTime) },
					{ typeof(MessageFieldDateDDMMYYRepresentationAttribute), typeof(ZDate) },
					{ typeof(MessageFieldDateDDMMYYYYRepresentationAttribute), typeof(ZDate) },
					{ typeof(MessageFieldDateYYYYMMDDRepresentationAttribute), typeof(ZDate) },
					{ typeof(MessageFieldDateHHMMSSRepresentationAttribute), typeof(ZDateTime) },
					{ typeof(MessageFieldDateYYYYMMDDHHMMRepresentationAttribute), typeof(ZDateTime) },
					{ typeof(MessageFieldDecimalRepresentationAttribute), typeof(ZDecimal) },
					{ typeof(MessageFieldIntegerRepresentationAttribute), typeof(ZInt) },
					{ typeof(MessageFieldSignedIntegerRepresentationAttribute), typeof(ZInt) },
					{ typeof(MessageFieldStringRepresentationAttribute), typeof(ZString) },
				};
			}
			return matchedTypes;
		}
	}
	Dictionary<Type, Type> matchedTypes;

	protected abstract Type MessageType { get; }
}
