using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	static class ICSInterpretationPrettier
	{
		public static ZString CreatePrettyInterpretation(ZString response)
		{
			var document = new XmlDocument();
			try
			{
				document.LoadXml(InterpretationHelper.RemoveAllNamespaces(response));
			}
			catch (XmlException)
			{
				return "An error occurred when attempting to interpret the error response";
			}

			var htmlFriendlyError = new ZStringBuilder(MessagePrettierCss.CSS);
			var errors = document.SelectNodes(ErrorResponseErrorsXPath);

			if (errors.Count > 0)
			{
				_ = htmlFriendlyError.Append("<h3>Response Errors</h3>");
				InterpretationHelper.CreateTableOfErrorsFromNodeList(htmlFriendlyError, errors, new[]
				{
					("Raised By", RaisedByXPath),
					("Error Number", NumberXPath),
					("Error Type", TypeXPath),
					("Error Text", TextXPath),
					("Location", LocationXPath),
				});
			}

			return htmlFriendlyError.ToString();
		}

		const string ErrorResponseErrorsXPath = "//ErrorResponse/Error";
		const string RaisedByXPath = "RaisedBy";
		const string NumberXPath = "Number";
		const string TypeXPath = "Type";
		const string TextXPath = "Text";
		const string LocationXPath = "Location";
	}
}
