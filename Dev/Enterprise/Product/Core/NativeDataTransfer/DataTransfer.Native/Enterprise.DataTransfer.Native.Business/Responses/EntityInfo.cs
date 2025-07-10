using System;
using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	public class EntityInfo
	{
		public string Name { get; set; }
		public string TableName { get; set; }
		public Guid? PrimaryKey { get; set; }
		public string LocalCode { get; set; }
		public string ExternalCode { get; set; }
	}
}
