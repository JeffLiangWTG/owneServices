using System.Linq;
using System.Xml.Serialization;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines a constructor argument for instantiating an object.
	/// </summary>
	[XmlRoot("constructor-arg")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public class ObjectConstructorArgumentDefinition
	{
		public ObjectConstructorArgumentDefinition Clone()
		{
			var result = new ObjectConstructorArgumentDefinition();
			result.Index = Index;
			result.ObjectName = ObjectName;
			result.ListValues = ListValues?.Select(x => x?.Clone()).ToArray();
			return result;
		}

		/// <summary>
		/// Gets/Sets the argument index.
		/// </summary>
		[XmlAttribute("index")]
		public int Index { get; set; }

		/// <summary>
		/// Gets/Sets the referred object name.
		/// </summary>
		[XmlAttribute("ref")]
		public string ObjectName { get; set; }

		/// <summary>
		/// Gets/Sets the argument value of type list.
		/// </summary>
		[XmlArray("list")]
		[XmlArrayItem("ref")]
		public ObjectListValueDefinition[] ListValues { get; set; }
	}
}
