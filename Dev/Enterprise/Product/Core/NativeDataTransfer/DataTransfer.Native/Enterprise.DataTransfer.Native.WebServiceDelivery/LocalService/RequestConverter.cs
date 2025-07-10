using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService
{
	public class RequestConverter : IConverter<IRequestMessage, XElement>
	{
		public XElement Convert(IRequestMessage source)
		{
			var xmlString = source.Message;
			return XElement.Parse(xmlString);
		}
	}
}
