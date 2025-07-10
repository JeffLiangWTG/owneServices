using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.MessageDelivery
{
	class InterchangeSerializer : IBusinessObjectSerializer
	{
		public InterchangeSerializer(ProcessTaskNotification action, XmlInterchange xmlInterchange)
		{
			this.action = action;
			this.xmlInterchange = xmlInterchange;
		}
		readonly ProcessTaskNotification action;
		readonly XmlInterchange xmlInterchange;
		public IValueObjectExportContext Context { get; set; }
		public IEDICommunicationsMode Mode { get; set; }

		public SubStreamableStream SerializeToStream(BusinessObject businessObject)
		{
			ModifyInterchangeInfo(Mode);

			return GetStreamForInterchange();
		}

		public SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObject)
		{
			ModifyInterchangeInfo(Mode);

			return GetStreamForInterchange();
		}

		void ModifyInterchangeInfo(IEDICommunicationsMode mode)
		{
			if (mode != null)
			{
				if (xmlInterchange.InterchangeInfo.Source.SenderCode.IsEmpty)
				{
					xmlInterchange.InterchangeInfo.Source.SenderCode = mode.EK_LocalPartyVanID; //SenderID;
				}

				if (xmlInterchange.InterchangeInfo.Target.ReceiverCode.IsEmpty)
				{
					xmlInterchange.InterchangeInfo.Target.ReceiverCode = mode.EK_RelatedPartyVanID; //ReceiverID;					
				}
			}

			if (action != null)
			{
				if (xmlInterchange.InterchangeInfo.Source.Purpose.IsEmpty)
				{
					xmlInterchange.InterchangeInfo.Source.Purpose = action.PQ_MessagePurpose;
				}
			}
		}

		SubStreamableStream GetStreamForInterchange()
		{
			var interchangeSerialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
			var stream = (SubStreamableStream)new MemoryStream();
			interchangeSerialiser.Serialize(stream, xmlInterchange);
			stream.Position = 0;
			return stream;
		}
	}
}
