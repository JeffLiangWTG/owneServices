using System.Xml.Serialization;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines an object value of list type.
	/// </summary>
	[XmlRoot("ref", Namespace = "http://www.springframework.net")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public class ObjectListValueDefinition
	{
		public ObjectListValueDefinition Clone() => new ObjectListValueDefinition() { ObjectName = this.ObjectName };

		/// <summary>
		/// Gets/Sets the referred object name.
		/// </summary>
		[XmlAttribute("object")]
		public string ObjectName { get; set; }
	}
}
