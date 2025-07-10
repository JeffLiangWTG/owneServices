using System.IO;
using System.Xml.Linq;

namespace Enterprise.Messaging.Integration
{
	public interface IXmlEDIMessage : IEDIMessage
	{
		XElement Content { get; set; }
		void SetMessageTypeFromStream(Stream stream);
	}
}
