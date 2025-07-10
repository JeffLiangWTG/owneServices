using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.MessageDelivery
{
	class XmlBusinessObjectSerializer : IBusinessObjectSerializer
	{
		public XmlBusinessObjectSerializer(IValueObjectDataAdapter dataAdapter, ZString messagePurpose)
		{
			this.dataAdapter = dataAdapter;
			this.messagePurpose = messagePurpose;
		}

		readonly ZString messagePurpose;
		readonly IValueObjectDataAdapter dataAdapter;

		#region IBusinessObjectSerializer Members

		public SubStreamableStream SerializeToStream(BusinessObject businessObject)
		{
			return SerializeToStream(new BusinessObject[] { businessObject });
		}

		public SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObjects)
		{
			var result = (SubStreamableStream)new MemoryStream();

			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
			var bos = businessObjects.ToArray();
			if (Mode == null)
			{
				serializer.ExportXmlData(result, dataAdapter, bos, Context, "", "", messagePurpose);
			}
			else
			{
				dataAdapter.SetAdditionalRequirementsForDataExportEvent(
					() =>
					{
						return (Mode.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface &&
							Mode.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
					});
				serializer.ExportXmlData(result, dataAdapter, bos, Context, Mode.EK_LocalPartyVanID, Mode.EK_RelatedPartyVanID, messagePurpose);
			}

			result.Position = 0;

			return result;
		}

		#endregion

		public IValueObjectExportContext Context { get; set; }
		public IEDICommunicationsMode Mode { get; set; }
	}
}
