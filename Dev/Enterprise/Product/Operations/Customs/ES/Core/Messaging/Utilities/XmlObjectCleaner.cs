using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.ES.Messaging
{
	public class XmlObjectCleaner
	{
		public void RemoveEmptyXmlElements(object msgObject)
		{
			Argument.NotNull(msgObject, nameof(msgObject));
			SetEmptyChildXmlPropertiesAsNull(msgObject, out _);
		}

		void SetEmptyChildXmlPropertiesAsNull(object msgObject, out bool areAllChildPropertiesEmpty)
		{
			areAllChildPropertiesEmpty = true;

			var xmlProperties = msgObject
				.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => !p.IsDefined(typeof(XmlIgnoreAttribute)));

			foreach (var xmlPtyInfo in xmlProperties.Where(x => !x.Name.Equals((NoResString)"Date")))
			{
				var isXmlPropertyEmpty = SetEmptyXmlPropertyAsNull(msgObject, xmlPtyInfo);

				if (!isXmlPropertyEmpty)
				{
					areAllChildPropertiesEmpty = false;
				}
			}
		}

		bool SetEmptyXmlPropertyAsNull(object parentObject, PropertyInfo xmlPtyInfo)
		{
			var propertyIsNullOrEmpty = true;
			var xmlObject = xmlPtyInfo.GetValue(parentObject);

			if (xmlObject != null)
			{
				if (typeof(System.Collections.ICollection).IsAssignableFrom(xmlPtyInfo.PropertyType))
				{
					propertyIsNullOrEmpty = SetEmptyXmlCollectionPropertyAsNull(parentObject, xmlPtyInfo, xmlObject);
				}
				else
				{
					propertyIsNullOrEmpty = SetEmptyXmlIndividualPropertyAsNull(parentObject, xmlPtyInfo, xmlObject);
				}
			}

			return propertyIsNullOrEmpty;
		}

		bool SetEmptyXmlCollectionPropertyAsNull(object parentObject, PropertyInfo xmlPtyInfo, object xmlObject)
		{
			var isListNullOrEmpty = true;
			var xmlList = (System.Collections.IList)xmlObject;

			var type = xmlObject.GetType().GetTypeInfo().GetElementType()
						?? xmlObject.GetType().GetGenericArguments().Single();
			var isSimpleType = IsSimpleType(type);

			for (int i = 0; i < xmlList.Count; i++)
			{
				var isItemNullOrEmpty = SetEmptyXmlArrayItemAsNull(xmlList, i, isSimpleType);

				if (!isItemNullOrEmpty)
				{
					isListNullOrEmpty = false;
				}
			}

			if (isListNullOrEmpty)
			{
				xmlPtyInfo.SetValue(parentObject, null);
			}

			return isListNullOrEmpty;
		}

		bool SetEmptyXmlArrayItemAsNull(System.Collections.IList xmlList, int itemIndex, bool isSimpleType)
		{
			var listItem = xmlList[itemIndex];

			if (listItem == null)
			{
				return true;
			}

			var nullifyReference = new Action(() => { });
			if (!xmlList.IsReadOnly)
			{
				nullifyReference = new Action(() => xmlList[itemIndex] = null);
			}

			return (isSimpleType)
				? SetEmptySimpleObjectAsNull(listItem, nullifyReference)
				: SetEmptyComplexObjectAsNull(listItem, nullifyReference);
		}

		bool SetEmptyXmlIndividualPropertyAsNull(object parentObject, PropertyInfo xmlPtyInfo, object xmlObject)
		{
			if (IsPropertyMandatory(xmlPtyInfo))
			{
				return false;
			}
			else
			{
				var isSimpleType = IsSimpleType(xmlPtyInfo.PropertyType);
				var nullifyReference = new Action(() => xmlPtyInfo.SetValue(parentObject, null));

				return (isSimpleType)
					? SetEmptySimpleObjectAsNull(xmlObject, nullifyReference)
					: SetEmptyComplexObjectAsNull(xmlObject, nullifyReference);
			}
		}

		/// <summary>
		/// Checks if a complex object is empty, setting it to null if so.
		/// Calls SetEmptyChildXmlPropertiesAsNull recursively to check its subtree of XML objects, setting empty ones to null.
		/// </summary>
		/// <param name="xmlObject">Actual complex object</param>
		/// <param name="nullifyReference">Action to set the reference to this object in the parent as null</param>
		/// <returns>
		/// True if complex object was empty and therefore set to null.
		/// Otherwise false.
		/// </returns>
		bool SetEmptyComplexObjectAsNull(object xmlObject, Action nullifyReference)
		{
			SetEmptyChildXmlPropertiesAsNull(xmlObject, out bool complexObjectIsEmpty);

			if (complexObjectIsEmpty)
			{
				nullifyReference();
			}

			return complexObjectIsEmpty;
		}

		/// <summary>
		/// Checks if a simple object is empty, setting it to null if so.
		/// </summary>
		/// <param name="objectValue">Simple object value</param>
		/// <param name="nullifyReference">Action to set the reference to this object in the parent as null</param>
		/// <returns>
		/// True if simple object was empty/null and therefore set to null.
		/// Otherwise false.
		/// </returns>
		bool SetEmptySimpleObjectAsNull(object objectValue, Action nullifyReference)
		{
			var objectIsNullOrEmpty = false;

			if (
				objectValue.GetType().Equals(typeof(string))
				&& string.IsNullOrWhiteSpace((string)objectValue)
			)
			{
				nullifyReference();
				objectIsNullOrEmpty = true;
			}

			return objectIsNullOrEmpty;
		}

		bool IsSimpleType(Type type)
		{
			return
				type.IsPrimitive
				|| type.IsEnum
				|| type.Equals(typeof(string))
				|| type.Equals(typeof(decimal));
		}

		bool IsPropertyMandatory(PropertyInfo xmlPtyInfo) => MandatoryProperties.Contains(xmlPtyInfo.Name);

		IEnumerable<string> MandatoryProperties => new List<string> { "C47TributoIndicadorMaxMinNor", "NifDeclarante", "NombreDeclarante" };
	}
}
