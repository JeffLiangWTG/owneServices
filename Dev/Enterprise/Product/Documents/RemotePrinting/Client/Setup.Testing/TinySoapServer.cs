using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Enterprise.RemotePrinting.Client.Setup.Testing
{
	public abstract class TinySoapServer : IDisposable
	{
		protected TinySoapServer(string url)
		{
			if (url == null)
			{
				throw new ArgumentNullException(nameof(url));
			}

			listener = new HttpListener();
			listener.Prefixes.Add(url);
		}

		readonly HttpListener listener;

		#region Start/Stop

		public async void Start()
		{
			listener.Start();
			do
			{
				if (!listener.IsListening)
				{
					break;
				}

				var context = await listener.GetContextAsync().ConfigureAwait(false);
				ProcessRequest(context);
			}
			while (true);
		}

		public void Stop()
		{
			listener.Stop();
		}

		#endregion

		#region Process Request

		public const string SoapNsUri = "http://schemas.xmlsoap.org/soap/envelope/";

		void ProcessRequest(HttpListenerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			using (var stream = context.Response.OutputStream)
			{
				ProcessSoapRequest(context, stream);
			}
		}

		void ProcessSoapRequest(HttpListenerContext context, Stream outputStream)
		{
			var ns = new XmlNamespaceManager(new NameTable());
			ns.AddNamespace("soap", SoapNsUri);

			var handled = false;

			var input = new XmlDocument();
			input.Load(context.Request.InputStream);
			var inputBody = input.SelectSingleNode("//soap:Body", ns);

			var requestElement = inputBody?.ChildNodes.OfType<XmlElement>().FirstOrDefault();
			if (requestElement != null)
			{
				var output = new XmlDocument();
				output.LoadXml("<?xml version='1.0' encoding='utf-8'?><soap:Envelope xmlns:soap='" + SoapNsUri + "' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><soap:Body/></soap:Envelope>");
				var responseBody = output.SelectSingleNode("//soap:Body", ns) ?? throw new InvalidOperationException("Cannot find Body element in Soap response message.");

				var responseElement = output.CreateElement(requestElement.LocalName + "Response", ResultNamespace);
				responseBody.AppendChild(responseElement);

				if (HandleSoapMethod(input, output, requestElement, responseElement))
				{
					context.Response.ContentType = "application/soap+xml; charset=utf-8";
					context.Response.StatusCode = (int)HttpStatusCode.OK;
					var writer = new XmlTextWriter(outputStream, Encoding.UTF8);
					output.WriteTo(writer);
					writer.Flush();
					handled = true;
				}
			}

			if (!handled)
			{
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			}
		}

		protected abstract bool HandleSoapMethod(XmlDocument inputDocument, XmlDocument outputDocument, XmlElement requestMethodElement, XmlElement responseMethodElement);

		protected virtual string ResultNamespace => "http://www.cargowise.com/";

		protected void AddSoapResult(XmlDocument outputDocument, XmlElement requestMethodElement, XmlElement responseMethodElement, string innerXml)
		{
			if (outputDocument == null)
			{
				throw new ArgumentNullException(nameof(outputDocument));
			}
			if (requestMethodElement == null)
			{
				throw new ArgumentNullException(nameof(requestMethodElement));
			}
			if (responseMethodElement == null)
			{
				throw new ArgumentNullException(nameof(responseMethodElement));
			}

			var result = outputDocument.CreateElement(requestMethodElement.LocalName + "Result", ResultNamespace);
			responseMethodElement.AppendChild(result);

			result.InnerXml = innerXml ?? string.Empty;
		}

		#endregion

		#region IDisposable Support

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Stop();
					listener.Close();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
