using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR084C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR084CProvider
	{
		Tr084C XmlObject { get; }
		public TR084CProvider(Tr084C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn;
		public ZString LRN => XmlObject.Declaration?.Lrn;
		public ZDateTime RequestDate => (XmlObject.Declaration?.RequestDate).ConvertToZDateTime();
		public ZDateTime DateLimit => (XmlObject.Declaration?.DateLimit).ConvertToZDateTime();
		public IEnumerable<(ZString DocumentType, ZString DocumentComplementaryInformation)> AdditionalInformations => XmlObject.AdditionalInformation?.Cast<AdditionalInformationType101>()
			.Select(p => (new ZString(p.DocumentType), new ZString(p.DocumentComplementaryInformation))) ?? Enumerable.Empty<(ZString DocumentType, ZString DocumentComplementaryInformation)>();
	}
}
