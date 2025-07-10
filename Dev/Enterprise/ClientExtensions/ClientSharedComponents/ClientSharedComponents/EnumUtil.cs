using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Types;

namespace Enterprise.ClientSharedComponents
{
	/// <summary>
	/// Utility class for working with 'extended' enums using <see cref="StringValueAttribute"/> attributes.
	/// </summary>
	public class EnumUtil
	{
		/// <summary>
		/// Creates a new <see cref="StringEnum"/> instance.
		/// </summary>
		/// <param name="enumType">Enum type.</param>
		public EnumUtil(Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(string.Format("Supplied type must be an Enum.  Type was {0}", enumType.ToString()));
			}

			this.enumType = enumType;
		}

		/// <summary>
		/// Gets the string value associated with the given enum value.
		/// </summary>
		/// <param name="valueName">Name of the enum value.</param>
		/// <returns>String Value</returns>
		public string GetStringValue(string valueName)
		{
			Enum lEnumType;
			string lStringValue = null;
			try
			{
				lEnumType = (Enum)Enum.Parse(enumType, valueName);
				lStringValue = GetDescription(lEnumType);
			}
			catch (Exception ex) when (!ex.IsCriticalException()) //Swallow!
			{
			}

			return lStringValue;
		}

		/// <summary>
		/// Gets the string values associated with the enum.
		/// </summary>
		/// <returns>String value array</returns>
		public Array GetStringValues()
		{
			ArrayList values = new ArrayList();
			//Look for our string value associated with fields in this enum
			foreach (FieldInfo fieldInfo in enumType.GetFields().Where(f => f.IsStatic).OrderBy(f => f.MetadataToken))
			{
				//Check for our custom attribute
				EnumDescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
				if (attributes.Length > 0)
				{
					values.Add(attributes[0].Value);
				}
			}
			return values.ToArray();
		}

		/// <summary>
		/// Gets the values as a 'bindable' list datasource.
		/// </summary>
		/// <returns>IList for data binding</returns>
		[SuppressWeaklyTypedCollectionMessage]
		public IList GetListValues()
		{
			Type lUnderlyingType = Enum.GetUnderlyingType(enumType);
			ArrayList lValues = new ArrayList();
			//Look for our string value associated with fields in this enum
			foreach (FieldInfo lFieldInfo in enumType.GetFields())
			{
				//Check for our custom attribute
				EnumDescriptionAttribute[] lAttributes = lFieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
				if (lAttributes.Length > 0)
				{
					lValues.Add(new DictionaryEntry(Convert.ChangeType(Enum.Parse(enumType, lFieldInfo.Name), lUnderlyingType), lAttributes[0].Value));
				}
			}
			return lValues;
		}

		/// <summary>
		/// Return the existence of the given string value within the enum.
		/// </summary>
		/// <param name="stringValue">String value.</param>
		/// <returns>Existence of the string value</returns>
		public bool IsStringDefined(string stringValue)
		{
			return Parse(enumType, stringValue) != null;
		}

		/// <summary>
		/// Return the existence of the given string value within the enum.
		/// </summary>
		/// <param name="stringValue">String value.</param>
		/// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
		/// <returns>Existence of the string value</returns>
		public bool IsStringDefined(string stringValue, bool ignoreCase)
		{
			return Parse(enumType, stringValue, ignoreCase) != null;
		}

		/// <summary>
		/// Gets the underlying enum type for this instance.
		/// </summary>
		/// <value></value>
		public Type EnumType
		{
			get { return enumType; }
		}
		readonly Type enumType;

		#region Static implementation
		/// <summary>
		/// Gets a string value for a particular enum value.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>String Value associated via a <see cref="StringValueAttribute"/> attribute, or null if not found.</returns>
		public static string GetDescription(Enum value)
		{
			string lOutput = null;
			Type lType = value.GetType();

			if (StringValues.ContainsKey(value))
			{
				lOutput = (StringValues[value] as EnumDescriptionAttribute).Value;
			}
			else
			{
				//Look for our 'StringValueAttribute' in the field's custom attributes
				FieldInfo lFieldInfo = lType.GetField(value.ToString());
				EnumDescriptionAttribute[] lAttributes = lFieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
				if (lAttributes.Length > 0)
				{
					StringValues.Add(value, lAttributes[0]);
					lOutput = lAttributes[0].Value;
				}
			}
			return lOutput;
		}

		/// <summary>
		/// Gets the string values associated with the enum as a single delimited string.
		/// </summary>
		/// <returns></returns>
		public static string GetDescriptions(Type enumType)
		{
			ZStringBuilder result = new ZStringBuilder();
			EnumUtil lEnum = new EnumUtil(enumType);
			foreach (string value in lEnum.GetStringValues())
			{
				result.Append(value);
			}
			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		/// <summary>
		/// Parses the supplied enum and string value to find an associated enum value (case sensitive).
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="stringValue">String value.</param>
		/// <returns>Enum value associated with the string value, or null if not found.</returns>
		public static object Parse(Type type, string stringValue)
		{
			return Parse(type, stringValue, false);
		}

		/// <summary>
		/// Parses the supplied enum and string value to find an associated enum value.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="stringValue">String value.</param>
		/// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
		/// <returns>Enum value associated with the string value, or null if not found.</returns>
		public static object Parse(Type type, string stringValue, bool ignoreCase)
		{
			object output = null;
			string lEnumStringValue = null;

			if (!type.IsEnum)
			{
				throw new ArgumentException(string.Format("Supplied type must be an Enum.  Type was {0}", type.ToString()));
			}

			//Look for our string value associated with fields in this enum
			foreach (FieldInfo fieldInfo in type.GetFields())
			{
				//Check for our custom attribute
				EnumDescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
				if (attributes.Length > 0)
				{
					lEnumStringValue = attributes[0].Value;
				}

				//Check for equality then select actual enum value.
				if (string.Compare(lEnumStringValue, stringValue, ignoreCase) == 0)
				{
					output = Enum.Parse(type, fieldInfo.Name);
					break;
				}
			}
			return output;
		}

		/// <summary>
		/// Return the existence of the given string value within the enum.
		/// </summary>
		/// <param name="stringValue">String value.</param>
		/// <param name="enumType">Type of enum</param>
		/// <returns>Existence of the string value</returns>
		public static bool IsStringDefined(Type enumType, string stringValue)
		{
			return Parse(enumType, stringValue) != null;
		}

		/// <summary>
		/// Return the existence of the given string value within the enum.
		/// </summary>
		/// <param name="stringValue">String value.</param>
		/// <param name="enumType">Type of enum</param>
		/// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
		/// <returns>Existence of the string value</returns>
		public static bool IsStringDefined(Type enumType, string stringValue, bool ignoreCase)
		{
			return Parse(enumType, stringValue, ignoreCase) != null;
		}
		#endregion

		static Hashtable StringValues
		{
			get { return stringValues ?? (stringValues = new Hashtable()); }
		}
		[ThreadStatic]
		static Hashtable stringValues;
	}

	#region Class StringValueAttribute
	/// <summary>
	/// Simple attribute class for storing String Values
	/// </summary>
	public class EnumDescriptionAttribute : Attribute
	{
		/// <summary>
		/// Creates a new <see cref="StringValueAttribute"/> instance.
		/// </summary>
		/// <param name="value">Value.</param>
		public EnumDescriptionAttribute(string value)
		{
			this.value = value;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value></value>
		public string Value
		{
			get { return value; }
		}
		readonly string value;
	}
	#endregion
}
