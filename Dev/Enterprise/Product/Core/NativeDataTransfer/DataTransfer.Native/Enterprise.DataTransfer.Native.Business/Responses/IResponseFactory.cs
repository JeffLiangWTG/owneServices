using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	public interface IResponseFactory
	{
		XNamespace NameSpace { get; }
		Response GetNewResponse();
		IXmlSerializer GetResponseSerializer();
	}
}
