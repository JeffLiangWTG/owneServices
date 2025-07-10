using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines an object property.
	/// </summary>
	[XmlRoot("property", Namespace = "http://www.springframework.net")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public sealed class ObjectPropertyDefinition
	{
		public ObjectPropertyDefinition Clone()
		{
			var result = new ObjectPropertyDefinition();
			result.DictionaryValues = DictionaryValues?.Select(x => x?.Clone()).ToArray();
			result.ListValues = ListValues?.Select(x => x?.Clone()).ToArray();
			result.Name = Name;
			result.EnableParallelInit = EnableParallelInit;
			result.IsSubSet = IsSubSet;
			result.ObjectValue = ObjectValue?.Clone();
			result.Value = Value;
			return result;
		}

		#region Properties
		/// <summary>
		/// Gets/Sets the object property values of valueOfType dictionary.
		/// </summary>
		[XmlArray("dictionary")]
		[XmlArrayItem("entry")]
		public ObjectDictionaryValueDefinition[] DictionaryValues { get; set; }

		/// <summary>
		/// Gets/Sets the object property values of valueOfType list.
		/// </summary>
		[XmlArray("list")]
		[XmlArrayItem("ref")]
		public ObjectListValueDefinition[] ListValues { get; set; }

		/// <summary>
		/// Gets/Sets the object property name.
		/// </summary>
		[XmlAttribute("name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets/Sets the object property name.
		/// </summary>
		[XmlAttribute("enableParallelInit")]
		public bool EnableParallelInit { get; set; }

		/// <summary>
		/// Gets/Sets the object subset indicator which indicate whether DictionaryValues or ListValues should be added to other subset.
		/// </summary>
		[XmlAttribute("isSubSet")]
		public bool IsSubSet { get; set; }

		/// <summary>
		/// Gets/Sets the direct object value.
		/// </summary>
		[XmlElement("object")]
		public ObjectDefinition ObjectValue { get; set; }

		/// <summary>
		/// Gets/Sets the object property value.
		/// </summary>
		[XmlAttribute("ref")]
		public string Value { get; set; }

		/// <summary>
		/// Gets/Sets the object property value of valueOfType "Type".
		/// </summary>
		[XmlIgnore]
		public Type ValueOfType
		{
			get
			{
				try
				{
					return valueOfType ?? (!string.IsNullOrEmpty(Value) ? valueOfType = Type.GetType(Value, true, true) : null);
				}
				catch
				{
					throw new CannotLoadObjectTypeException(string.Format(CultureInfo.InvariantCulture, "No type could be found that corresponds with ref attribute value of \"{0}\" specified by property with name \"{1}\"", Value, Name));
				}
			}
		}
		#endregion

		#region Fields
		Type valueOfType;
		#endregion
	}
}
