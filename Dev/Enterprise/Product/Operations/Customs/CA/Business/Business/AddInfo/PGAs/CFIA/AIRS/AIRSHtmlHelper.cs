using System;
using System.IO;
using System.Net;
using System.Text;
#if NET
using System.Text.Encodings.Web;
#endif
#if NETFRAMEWORK
using System.Web.UI;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class AIRSHtmlHelper
	{
		public static ZString GetHtmlFormattedText(ZString htmlBody)
		{
			return HtmlTemplates.HtmlTemplate.Replace("(*HtmlBody*)", htmlBody);
		}

		public static class Constant
		{
			public const string ViewState = "__VIEWSTATE";
			public const string ViewStateGenerator = "__VIEWSTATEGENERATOR";
			public const string ViewStateEncrypted = "__VIEWSTATEENCRYPTED";
			public const string EventValidation = "__EVENTVALIDATION";
			public const string TariffToSearch = "TariffToSearch";

			public const string NoDataSetMessage = "No Documentation and Registration Requirements";
			public const string PostContentType = "Content-Type:application/x-www-form-urlencoded";
			public const string FailedLoadConfigurationErrorMessage = "Failed to load AIRS website setting.";
		}

		public static ZString WriteMessageErrorHtml(ZString message)
		{
			return ZString.Format(HtmlTemplates.EmptyHtml, message);
		}

		public static class HtmlTemplates
		{
			public const string EmptyHtml = "<html>{0}</Html>";
			public const string HtmlTemplate = @"<html>
<head>
	<style type='text/css'>
		th {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 12px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: lightgray;
		}
		td {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 11px;
			border: 1px solid gray;
			border-collapse: collapse;
			background-color: white;
		}
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF' width='100%' height='100%'>
		(*HtmlBody*)
	</table>
</body>
</html>";
		}

		public static byte[] GetPostData(AIRSWebpageConfiguration configuration, ZString tariffPassedIn, Func<ZString, ZString> getInitialRequest)
		{
			string srcString = getInitialRequest(configuration.Url);

			var postDataBuilder = new ZStringBuilder();
			postDataBuilder.Append($"{Constant.ViewState}={GetRequestHeaderDetails(srcString, Constant.ViewState)}");
			postDataBuilder.Append($"{Constant.ViewStateGenerator}={GetRequestHeaderDetails(srcString, Constant.ViewStateGenerator)}");
			postDataBuilder.Append($"{Constant.ViewStateEncrypted}={GetRequestHeaderDetails(srcString, Constant.ViewStateEncrypted)}");
			postDataBuilder.Append($"{Constant.EventValidation}={GetRequestHeaderDetails(srcString, Constant.EventValidation)}");
			postDataBuilder.Append(configuration.PostDataTemplate.Replace(Constant.TariffToSearch, tariffPassedIn));
			var postString = postDataBuilder.ToStringWithDelimiterBetweenAppends("&");
			return Encoding.UTF8.GetBytes(postString);
		}

		public static ZString GetInitialRequest(ZString url)
		{
			try
			{
#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
				var request = WebRequest.Create(new Uri(url)) as HttpWebRequest;
#pragma warning restore SYSLIB0014
				request.Method = "GET";
				request.KeepAlive = true;
				request.CookieContainer = new CookieContainer();

				var firstResponse = string.Empty;
				using (var response = request.GetResponse())
				{
					using (var responseStream = response.GetResponseStream())
					{
						using (var reader = new StreamReader(responseStream, Encoding.UTF8))
						{
							firstResponse = reader.ReadToEnd();
						}
					}
				}
				return firstResponse;
			}
			catch
			{
				throw new WebException(Res.GetString("A3D26E41-EA36-4D6C-B0F8-67049638C635", "Cannot get access to {0}", url));
			}
		}

		static ZString GetRequestHeaderDetails(string responseStr, string name)
		{
			var viewStateStr = $"id=\"{name}\" value=\"";
			var len1 = responseStr.IndexOf(viewStateStr) + viewStateStr.Length;
			var len2 = responseStr.IndexOf("\"", len1);
			return WebUtility.UrlEncode(responseStr.Substring(len1, len2 - len1));
		}

		public static ZString BuildHtmlFromDataSet(BusinessObjectFactory factory, CodeDescriptionPairList materializedLPCOs, CodeDescriptionPairList deMaterializedLPCOs, CodeDescriptionPairList aIRSRegistrations, bool isChildGroup)
		{
			var result = ZString.Empty;
			if (materializedLPCOs.Count == 0 && deMaterializedLPCOs.Count == 0 && aIRSRegistrations.Count == 0)
			{
				result = AIRSHtmlHelper.WriteMessageErrorHtml(AIRSHtmlHelper.Constant.NoDataSetMessage);
			}
			else
			{
				var configuration = new AIRSWebpageConfiguration(factory);
				var htmlBuilder = new StringBuilder();
				var captionWriter = new StringWriter();

#if NETFRAMEWORK
				using (HtmlTextWriter writer = new HtmlTextWriter(captionWriter))
				{
					if (isChildGroup)
					{
						writer.RenderBeginTag(HtmlTextWriterTag.Tr);
						writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
						writer.RenderBeginTag(HtmlTextWriterTag.Th);
						writer.Write("OR");
						writer.RenderEndTag();
						writer.RenderEndTag();
					}
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
					writer.RenderBeginTag(HtmlTextWriterTag.Th);
					writer.Write(configuration.CodeText);
					writer.RenderEndTag();
					writer.RenderBeginTag(HtmlTextWriterTag.Th);
					writer.Write(configuration.RegistrationText);
					writer.RenderEndTag();
					writer.RenderEndTag();
				}
#else
				if (isChildGroup)
				{
					captionWriter.WriteLine("<tr>");
					captionWriter.WriteLine("<th colspan=\"2\">OR</th>");
					captionWriter.WriteLine("</tr>");
				}
				captionWriter.WriteLine("<tr>");
				captionWriter.Write("<th>");
				captionWriter.Write(HtmlEncoder.Default.Encode(configuration.CodeText));
				captionWriter.WriteLine("</th>");
				captionWriter.Write("<th>");
				captionWriter.Write(HtmlEncoder.Default.Encode(configuration.RegistrationText));
				captionWriter.WriteLine("</th>");
				captionWriter.WriteLine("</tr>");
#endif

				htmlBuilder.Append(captionWriter.ToString());
				htmlBuilder.Append(BuildSingleHtmlBody(materializedLPCOs, configuration.MaterializedGridTitle, configuration));
				htmlBuilder.Append(BuildSingleHtmlBody(deMaterializedLPCOs, configuration.DeMaterializedGridTitle, configuration));
				htmlBuilder.Append(BuildSingleHtmlBody(aIRSRegistrations, configuration.AIRSRegistrationGridTitle, configuration));
				result = AIRSHtmlHelper.GetHtmlFormattedText(htmlBuilder.ToString());
			}

			return result;
		}

		static ZString BuildSingleHtmlBody(CodeDescriptionPairList keyValuePairs, ZString title, AIRSWebpageConfiguration configuration)
		{
			StringWriter stringWriter = new StringWriter();

#if NETFRAMEWORK
			using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
			{
				if (keyValuePairs.Count > 0)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
					writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
					writer.RenderBeginTag(HtmlTextWriterTag.Th);
					writer.Write(title);
					writer.RenderEndTag();
					writer.RenderEndTag();
					foreach (CodeDescriptionPair pair in keyValuePairs)
					{
						writer.RenderBeginTag(HtmlTextWriterTag.Tr);
						writer.RenderBeginTag(HtmlTextWriterTag.Td);
						writer.Write(pair.Code);
						writer.RenderEndTag();
						writer.RenderBeginTag(HtmlTextWriterTag.Td);
						writer.Write(pair.Description);
						writer.RenderEndTag();
						writer.RenderEndTag();
					}
				}
			}
#else
			if (keyValuePairs.Count > 0)
			{
				stringWriter.WriteLine("<tr>");
				stringWriter.WriteLine($"<th colspan=\"2\">{HtmlEncoder.Default.Encode(title)}</th>");
				stringWriter.WriteLine("</tr>");
				foreach (CodeDescriptionPair pair in keyValuePairs)
				{
					stringWriter.WriteLine("<tr>");
					stringWriter.WriteLine($"  <td>{HtmlEncoder.Default.Encode(pair.Code)}</td>");
					stringWriter.WriteLine($"  <td>{HtmlEncoder.Default.Encode(pair.Description)}</td>");
					stringWriter.WriteLine("</tr>");
				}
			}
#endif

			return stringWriter.ToString();
		}
	}
}
