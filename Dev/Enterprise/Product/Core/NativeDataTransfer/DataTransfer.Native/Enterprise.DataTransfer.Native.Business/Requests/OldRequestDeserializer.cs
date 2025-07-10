using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class OldRequestDeserializer : BaseRequestDeserializer
	{
		public override Request Deserialize(XElement element)
		{
			var entitySets = new List<XElement>();
			var settings = new HeaderData_Unversioned_Native();

			var rootName = element.Name;

			foreach (var node in element.Elements())
			{
				if (node.Name.LocalName == "OwnerOrg")
				{
					settings.OwnerCode = node.Value;
				}
				else
				{
					var entitySet = new XElement(rootName, node);
					entitySets.Add(entitySet);
				}
			}

			if (!entitySets.Any())
			{
				throw new NativeXMLUserVisibleException("No valid elements were included.");
			}

			return new Request { EntitySets = entitySets, Settings = settings };
		}

		public override Request Deserialize(Stream stream)
		{
			stream.Position = 0;
			using (var reader = XmlReader.Create(stream))
			{
				var root = XElement.Load(reader);
				return Deserialize(root);
			}
		}

		public override IXmlSerializer GetResponseSerializer()
		{
			return new ObjectXmlSerializer<Response_Unversioned_Native>();
		}

		protected override Response GetNewResponse()
		{
			return new Response_Unversioned_Native();
		}

		protected override string NameSpace
		{
			get { return ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native; }
		}
	}
}
