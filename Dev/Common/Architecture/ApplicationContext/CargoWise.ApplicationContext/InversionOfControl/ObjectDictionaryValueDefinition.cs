using System.Xml.Serialization;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines an object value of type dictionary.
	/// </summary>
	[XmlRoot("entry", Namespace = "http://www.springframework.net")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public class ObjectDictionaryValueDefinition
	{
		public ObjectDictionaryValueDefinition Clone()
		{
			var result = new ObjectDictionaryValueDefinition();
			result.Key = this.Key?.Clone();
			result.Value = this.Value;
			return result;
		}

		/// <summary>
		/// Gets/Sets the key of the object property value of type dictionary.
		/// </summary>
		[XmlElement("key")]
		public ObjectDictionaryValueKeyDefinition Key { get; set; }

		/// <summary>
		/// Gets/Sets the value of the object property value of type dictionary.
		/// </summary>
		[XmlElement("value")]
		public string Value { get; set; }
	}
}
