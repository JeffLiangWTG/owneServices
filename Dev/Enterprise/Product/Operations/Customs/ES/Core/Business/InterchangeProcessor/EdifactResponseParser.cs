using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Customs.ES.MessageDefinitions.EdifactResponse;
using Enterprise.ZArchitecture.Core;
using HtmlAgilityPack;

namespace Enterprise.Customs.ES.Business
{
	public class EdifactResponseParser
	{
		public EdifactResponseParser(string responseContent)
		{
			this.responseContent = responseContent;
		}

		readonly string responseContent;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string ServerError500Title = "ERROR 500";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string ServerErrorProcesoEDITitle = "ERROR PROCESO EDI";

		public MessageProcessingResult Parse()
		{
			var htmlContent = WebUtility.HtmlDecode(responseContent);

			var document = new HtmlDocument();
			document.LoadHtml(htmlContent);

			var documentNode = document.DocumentNode;

			var titleNode = documentNode.SelectSingleNode(@"//head/title");
			var title = titleNode?.InnerText?.ToUpperInvariant() ?? string.Empty;

			if (title.Contains(ServerError500Title))
			{
				return MessageProcessingResult.RemoteServerError(ServerError500Title);
			}

			var textarea = documentNode.SelectSingleNode((NoResString)@"//body//div[starts-with(@class, ""AEAT_aplicacion"")]//textarea");

			if (textarea != null)
			{
				var content = textarea.InnerText;

				var declarationStatusValue = GetLabel(content, "cod-ret");
				var errorStatusValue = GetLabel(content, "des-cod");
				var respuestaValue = GetLabel(content, "RESPUESTA");

				if (declarationStatusValue != null
					|| errorStatusValue != null
					|| respuestaValue != null)
				{
					if ((declarationStatusValue?.StartsWith("6") ?? false) || (declarationStatusValue?.StartsWith("7") ?? false))
					{
						return MessageProcessingResult.RemoteServerError($"Declaration Status is {declarationStatusValue}.");
					}
					else
					{
						var response = XmlUtils.SerializeToString
						(
							new Response()
							{
								DeclarationStatus = declarationStatusValue,
								ErrorStatus = errorStatusValue,
								Respuesta = respuestaValue
							}
						);

						if (title.Contains(ServerErrorProcesoEDITitle))
						{
							var message = $"{ServerErrorProcesoEDITitle}{System.Environment.NewLine}{response}";
							return MessageProcessingResult.RemoteServerError(message);
						}
						else
						{
							return MessageProcessingResult.Success(Encoding.UTF8.GetBytes(response));
						}
					}
				}
			}

			var errorNode = documentNode.SelectSingleNode((NoResString)@"//body//div[starts-with(@class, ""AEAT_aplicacion"")]/div[@class=""AEAT_bloque_errores""]");
			if (errorNode != null)
			{
				var errorDetails = errorNode.InnerText.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				var message = string.Join(System.Environment.NewLine, errorDetails.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));

				return MessageProcessingResult.BadRequest(message);
			}

			return MessageProcessingResult.RemoteServerError($@"Can not parse the response content:{System.Environment.NewLine}{responseContent}");
		}

		string GetLabel(string htmlConetnt, string label)
		{
			var reg = new Regex($@"(?<=(\[|<){label}(\]|>))(.|\n)*?(?=(\[|<)\/{label}(\]|>))", RegexOptions.Multiline | RegexOptions.IgnoreCase);
			var match = reg.Match(htmlConetnt);

			return match != null && match.Success ? match.Value?.Trim() : null;
		}
	}
}
