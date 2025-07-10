using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportOnlineOrder : IImportOnlineOrder
	{
		public string OnlineOrderNo { get; set; }
		public int SequenceNo { get; set; }

		ZString IImportOnlineOrder.OnlineOrderNo => OnlineOrderNo;
		ZInt IImportOnlineOrder.SequenceNo => SequenceNo;
	}
}
