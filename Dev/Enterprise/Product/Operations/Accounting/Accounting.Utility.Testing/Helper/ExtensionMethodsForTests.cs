using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class ExtensionMethodsForTests
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1166:Do not extend NUnit", Justification = "Baseline")]
		public static void AssertBusinessObjectFields(this TestCase test,
			string message, BusinessObject bizo, Dictionary<string, IZType> fieldNamesAndValues, params string[] notEqualFieldNames)
		{
			var notEqualFieldNamesHashSet = new HashSet<string>(notEqualFieldNames);

			Assertion.AssertContainsExactElementsInAnyOrder(message + " notEqualFieldNames must not contain duplications.", notEqualFieldNames, notEqualFieldNamesHashSet);

			Assertion.CombineAssertions(() =>
			{
				foreach (var field in fieldNamesAndValues)
				{
					var fieldName = field.Key;
					var fieldValue = field.Value;
					var messagePrefix = message + string.Format(" Field name '{0}' ", fieldName);
					var propertyInfo = bizo.ZPropertyInfoHash.GetPropertySafe(fieldName);
					Assertion.AssertNotNull(messagePrefix + "not found", propertyInfo);
					if (propertyInfo != null)
					{
						if (notEqualFieldNamesHashSet.Contains(fieldName))
						{
							Assertion.AssertNotEquals(messagePrefix, fieldValue, propertyInfo.Value);
						}
						else
						{
							Assertion.AssertEquals(messagePrefix, fieldValue, propertyInfo.Value);
						}
					}
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1166:Do not extend NUnit", Justification = "Baseline")]
		public static void AssertCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder<T>(this TestCase test,
			T destinationObject, T sourceObject, string[] includedFields, string[] excludedFields, Dictionary<string, IZType> expectedFieldValuesByNames,
			Action<T, T> copyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder, string[] fieldsCannotBeSetDirectly = null)
			where T : BusinessObject
		{
			PrepareSourceObject(test, sourceObject, includedFields, excludedFields, expectedFieldValuesByNames, fieldsCannotBeSetDirectly);
			copyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(destinationObject, sourceObject);
			test.AssertBusinessObjectFields("Destination should copy all fields from source object.", destinationObject, expectedFieldValuesByNames, excludedFields);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1166:Do not extend NUnit", Justification = "Baseline")]
		public static void AssertCopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder<T>(this TestCase test,
			T[] sourceObjectArray, string[] includedFields, string[] excludedFields, Dictionary<string, IZType>[] expectedFieldValuesByNames,
			Func<T[], T[]> copyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder)
			where T : BusinessObject
		{
			for (int i = 0; i < sourceObjectArray.Length; i++)
			{
				PrepareSourceObject(test, sourceObjectArray[i], includedFields, excludedFields, expectedFieldValuesByNames[i]);
			}

			T[] destinationObjectArray = copyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceObjectArray);
			Assertion.AssertEquals(sourceObjectArray.Length, destinationObjectArray.Length);

			for (int i = 0; i < sourceObjectArray.Length; i++)
			{
				test.AssertBusinessObjectFields("Destination should copy all fields from source object.", destinationObjectArray[i], expectedFieldValuesByNames[i], excludedFields);
			}
		}

		static void PrepareSourceObject<T>(TestCase test, T sourceObject, string[] includedFields, string[] excludedFields, Dictionary<string, IZType> expectedFieldValuesByNames, string[] fieldsCannotBeSetDirectly = null)
			where T : BusinessObject
		{
			var excludedFieldsHashSet = new HashSet<string>(excludedFields);
			Assertion.AssertContainsExactElementsInAnyOrder("excludedFields must not contain duplications", excludedFields, excludedFieldsHashSet);

			var includedFieldsHashSet = new HashSet<string>(includedFields);
			Assertion.AssertContainsExactElementsInAnyOrder("includedFields must not contain duplications", includedFields, includedFieldsHashSet);

			var allUsedFieldsHashSet = new HashSet<string>(includedFieldsHashSet);
			allUsedFieldsHashSet.UnionWith(excludedFieldsHashSet);
			var combinedFieldsList = new List<string>(includedFields);
			combinedFieldsList.AddRange(excludedFields);
			Assertion.AssertContainsExactElementsInAnyOrder("includedFields must not contain duplications in excludedFields", allUsedFieldsHashSet, combinedFieldsList);

			var messageForNewFields =
@"If the field should be copied it must be added to bizo PersistentFieldsToCopyAndInValidOrder list in correct place to set its value in correct order according to other fildes in the list. 
If the field should not be copied it must be added to excluded fields list in this test.";

			var persistentFieldNames = from ZPropertyInfo property in sourceObject.ZPropertyInfoHash
									   where property.IsPersistent
									   select property.Name;
			Assertion.AssertContainsExactElementsInAnyOrder(messageForNewFields, persistentFieldNames, allUsedFieldsHashSet);

			Assertion.AssertContainsExactElementsInAnyOrder("expectedFieldValuesByNames must contain values for all fields.", allUsedFieldsHashSet, expectedFieldValuesByNames.Keys);

			var excludedFieldsThatCanBeSet = fieldsCannotBeSetDirectly != null ? excludedFields.Except(fieldsCannotBeSetDirectly).ToArray() : excludedFields;
			sourceObject.SetFieldsInParticularOrder(expectedFieldValuesByNames, excludedFieldsThatCanBeSet );
			sourceObject.SetFieldsInParticularOrder(expectedFieldValuesByNames, includedFields);

			test.AssertBusinessObjectFields("Precondition: source must have all fields set correctly.", sourceObject, expectedFieldValuesByNames);
		}
	}
}
