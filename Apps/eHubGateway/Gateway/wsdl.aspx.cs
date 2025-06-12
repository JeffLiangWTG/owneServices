using System;
using System.Net;
using System.Xml;

namespace CargoWise.eHub.Gateway.Host
{
	public partial class wsdl : System.Web.UI.Page
	{
		private const string ServicePath = "eHubStreamedService.svc"; 

		protected void Page_Load(object sender, EventArgs e)
		{
			var query = Request.QueryString.ToString();
			if (string.IsNullOrEmpty(query) || !(query.ToLower().Contains("wsdl") || query.ToLower().Contains("xsd") || query.ToLower().Contains("disco")))
			{
				Response.Redirect(ServicePath);
			}

			var serviceUri = new UriBuilder(Request.Url);
			serviceUri.Path = serviceUri.Path.Replace("wsdl.aspx", ServicePath);
			var request = WebRequest.Create(serviceUri.Uri.AbsoluteUri);
			request.Headers["x-request-original"] = "true";

			var doc = new XmlDocument();
			try
			{
				var response = request.GetResponse();

				doc.Load(response.GetResponseStream());

				ChangeAttributeScheme(doc, "location");
				ChangeAttributeScheme(doc, "schemaLocation");
				ChangeAttributeScheme(doc, "ref");
				ChangeAttributeScheme(doc, "docRef");

				Response.ContentType = response.ContentType;
				doc.Save(Response.OutputStream);
				Cache[Request.Url.ToString()] = new Tuple<string, string>(response.ContentType, doc.OuterXml);
			}
			catch (Exception ex)
			{
				if (Cache[Request.Url.ToString()] is Tuple<string, string> cached)
				{
					Response.ContentType = cached.Item1;
					Response.Write(cached.Item2);
				}
				else
				{
					Response.StatusCode = 500;
					Response.StatusDescription = ex.Message;
				}
			}
		}

		private void ChangeAttributeScheme(XmlDocument document, string attribute)
		{
			var nodes = document.SelectNodes($"//*[@{attribute}]");
			for (var index = 0; index < nodes?.Count; index++)
			{
				var node = nodes[index];
				node.Attributes[attribute].Value = node.Attributes[attribute].Value.Replace("http://", "https://");
			}
		}
	}
}
