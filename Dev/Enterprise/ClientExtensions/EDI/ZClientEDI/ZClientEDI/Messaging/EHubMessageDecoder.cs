using System.Text.RegularExpressions;
using Enterprise.eHubMessaging.Business;

namespace Enterprise.Client.EDI.Messaging
{
	class EHubMessageDecoder
	{
		const string base64Pattern = @"([a-zA-Z0-9+/\s]+={0,2})";

		public static string UnpackMessage(string text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				string input = GetCustomerServiceMessage(text);

				if (input != null)
				{
					string output = SystemMessage.Unpack(input);

					if (string.IsNullOrEmpty(output))
					{
						output = SystemMessage.Unpack(input, "request.tmp");
					}
					if (string.IsNullOrEmpty(output))
					{
						output = SystemMessage.Unpack(input, "response.tmp");
					}
					return Regex.Replace(output, string.Concat("<Data>", base64Pattern, "</Data>"), "<Data>This data has been removed</Data>");
				}
			}
			return "This message cannot be decoded or is empty. Customer service message with content is expected.";
		}

		public static string GetCustomerServiceMessage(string text)
		{
			string requestPattern = string.Concat(@"<CustomerServiceRequest compressed=""1"" xmlns=""http://www.cargowise.com/Schemas/System"">", base64Pattern, "</CustomerServiceRequest>");
			string responsePattern = string.Concat(@"<CustomerServiceResponse compressed=""1"">", base64Pattern, "</CustomerServiceResponse>");

			string output = null;

			if (Regex.IsMatch(text, requestPattern))
			{
				output = Regex.Match(text, requestPattern).Groups[1].Value;
			}
			if (output == null && Regex.IsMatch(text, responsePattern))
			{
				output = Regex.Match(text, responsePattern).Groups[1].Value;
			}
			return output;
		}
	}
}
