using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.MessageDelivery
{
	public class ExlBusinessObjectSerializer : IBusinessObjectSerializer
	{
		public ExlBusinessObjectSerializer(IValueObjectDataAdapter dataAdapter, ZString messagePurpose)
		{
			DataAdapter = dataAdapter;
			MessagePurpose = messagePurpose;
		}

		IValueObjectDataAdapter DataAdapter { get; set; }
		ZString MessagePurpose { get; set; }

		public IValueObjectExportContext Context { get; set; }
		public IEDICommunicationsMode Mode { get; set; }

		#region IBusinessObjectSerializer Members

		public SubStreamableStream SerializeToStream(BusinessObject businessObject)
		{
			return SerializeToStream(new[] { businessObject });
		}

		public SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObjectsToSerialize)
		{
			var result = (SubStreamableStream)new MemoryStream();

			var interchange = DataAdapter.ToXmlInterchange(businessObjectsToSerialize.ToArray(), Context) as Xsd.XmlInterchange;
			interchange.InterchangeInfo.Source.SenderCode = Mode.EK_LocalPartyVanID; //SenderID;
			interchange.InterchangeInfo.Target.ReceiverCode = Mode.EK_RelatedPartyVanID; //ReceiverID;
			interchange.InterchangeInfo.Source.Purpose = MessagePurpose;
			var serialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			serialiser.Serialize(result, interchange);

			result.Position = 0;

			return result;
		}

		#endregion

	}
}
