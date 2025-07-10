using System.Xml.Linq;
using System.Xml.Serialization;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlInclude(typeof(Response_Universal))]
	public abstract class Response
	{
		protected Response()
		{
			EntityInfo = new EntityInfo();
		}

		public static Response New()
		{
			switch (ReferenceDataXMLForSerialize.Instance.NameSpace)
			{
				case NativeXmlInfo.Namespace_2011_11:
					return new Response_Versioned_Native();
				case ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native:
					return new Response_Unversioned_Native();
				default:
					return new Response_Universal();
			}
		}

		[XmlElement(Order = 0)]
		public EntityInfo EntityInfo { get; set; }
		[XmlElement(Order = 1)]
		public NativeResponseStatus Status { get; set; }
		[XmlArray("Information", Order = 2)]
		[XmlArrayItem("Item")]
		public string[] Informations { get; set; }
		[XmlArray("Data", Order = 3)]
		[XmlArrayItem("DataItem")]
		public XElement[] DataItems { get; set; }
	}
}