using System.Xml.Serialization;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines the key of a object property value of type dictionary.
	/// </summary>
	[XmlRoot("key", Namespace = "http://www.springframework.net")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public class ObjectDictionaryValueKeyDefinition
	{
		public ObjectDictionaryValueKeyDefinition Clone() => new ObjectDictionaryValueKeyDefinition() { Value = this.Value };

		/// <summary>
		/// Gets/Sets the key value.
		/// </summary>
		[XmlElement("value")]
		public string Value { get; set; }
	}
}
