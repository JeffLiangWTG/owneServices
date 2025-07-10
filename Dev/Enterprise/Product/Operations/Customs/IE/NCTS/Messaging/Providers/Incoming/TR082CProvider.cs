using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR082C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR082CProvider
	{
		Tr082C XmlObject { get; }

		public TR082CProvider(Tr082C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;

		public ZString LRN => XmlObject.Declaration?.Lrn ?? ZString.Empty;

		public ZDateTime RequestDate => (XmlObject.Declaration?.RequestDate).ConvertToZDateTime();

		public ZDateTime DateLimit => (XmlObject.Declaration?.DateLimit).ConvertToZDateTime();

		public IEnumerable<(ZString DocumentType, ZString DocumentComplementaryInformation)> AdditionalInformations => XmlObject.AdditionalInformation?.Cast<AdditionalInformationType101>().Select(x =>
		{
			return (new ZString(x.DocumentType), new ZString(x.DocumentComplementaryInformation));
		}) ?? Enumerable.Empty<(ZString DocumentType, ZString DocumentComplementaryInformation)>();
	}
}
