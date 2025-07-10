using System;
using System.Reflection;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class ConstantsDescriptionsGlobalizationTest : TestCase
	{
		public void TestAllDescriptionsAreMultlingualStrings()
		{
			CombineAssertions(delegate
			{
				TestAllDescriptionsAreMultlingualStrings(typeof(Constants));
			});
		}

		void TestAllDescriptionsAreMultlingualStrings(Type type)
		{
			if (!IsTypeNotIncotermsDescription(type))
			{
				if (type.Name.IndexOf("Description", StringComparison.InvariantCultureIgnoreCase) > -1 && type.BaseType != typeof(Attribute))
				{
					Assert("Description properties should be typed MultilingualString: " + type.FullName, type.GetFields().Length == 0);
					foreach (var property in type.GetProperties())
					{
						AssertNotEquals("Description properties should be typed MultilingualString: " + type.FullName + "." + property.Name, typeof(string), property.PropertyType);
						AssertNotEquals("Description properties should be typed MultilingualString: " + type.FullName + "." + property.Name, typeof(ZString), property.PropertyType);
					}
				}
				foreach (var nestedType in type.GetNestedTypes())
				{
					TestAllDescriptionsAreMultlingualStrings(nestedType);
				}
			}
		}

		bool IsTypeNotIncotermsDescription(Type type)
		{
			var typeNameToCheck = type.DeclaringType != null
				? $"{type.DeclaringType.Name}.{type.Name}"
				: type.Name;

			return typeNameToCheck == $"{nameof(Constants.IncoTerms)}.{nameof(Constants.IncoTerms.Descriptions)}";
		}

		public void TestAllUnitDescriptions()
		{
			TestUnitDescriptions(typeof(Constants.Length), Constants.Length.GetDescription, Constants.PluralState.Plural, Constants.PluralState.PluralOrNonPlural);
			TestUnitDescriptions(typeof(Constants.Volume), Constants.Volume.GetDescription, Constants.PluralState.Plural, Constants.PluralState.PluralOrNonPlural, Constants.PluralState.NonPlural);
			TestUnitDescriptions(typeof(Constants.Weight), Constants.Weight.GetDescription, Constants.PluralState.Plural, Constants.PluralState.PluralOrNonPlural, Constants.PluralState.NonPlural);
			TestUnitDescriptions(typeof(Constants.Area), Constants.Area.GetDescription, Constants.PluralState.Plural, Constants.PluralState.PluralOrNonPlural);
		}

		void TestUnitDescriptions(Type unitType, Constants.UnitDescriptionCallback unitDescription, params Constants.PluralState[] pluralStates)
		{
			foreach (var pluralState in pluralStates)
			{
				TestUnitDescriptions(unitType, unitDescription, pluralState);
			}
		}

		void TestUnitDescriptions(Type unitType, Constants.UnitDescriptionCallback unitDescription, Constants.PluralState pluralState)
		{
			foreach (var field in unitType.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.FieldType == typeof(string))
				{
					string code = (string)field.GetValue(null);
					AssertNotEquals(unitType.FullName + ": " + code + "(" + pluralState + ")", "", unitDescription(code, pluralState));
				}
			}
		}
	}
}
