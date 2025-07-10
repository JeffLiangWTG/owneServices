using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public class FriendlyCodeWithPointers : EU.Business.WorldCustomsOrganisation.FriendlyCodeWithPointers
	{
		public FriendlyCodeWithPointers(CargoWise.Customs.GB.MessageDefinitions.CDS.PointerParser pointerParser, ICodeWithPointers codeWithPointers, XElement requestXml) : base(pointerParser, codeWithPointers, requestXml)
		{
		}
	}
}
