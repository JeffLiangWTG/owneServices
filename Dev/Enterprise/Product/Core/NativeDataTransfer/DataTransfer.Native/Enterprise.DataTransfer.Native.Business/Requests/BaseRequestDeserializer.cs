using System.IO;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public abstract class BaseRequestDeserializer : IResponseFactory
	{
		public abstract Request Deserialize(XElement element);
		public abstract Request Deserialize(Stream stream);

		public abstract IXmlSerializer GetResponseSerializer();

		#region IResponseFactory Members

		Response IResponseFactory.GetNewResponse()
		{
			return GetNewResponse();
		}

		XNamespace IResponseFactory.NameSpace
		{
			get { return NameSpace; }
		}

		#endregion

		protected abstract Response GetNewResponse();
		protected abstract string NameSpace { get; }
	}
}