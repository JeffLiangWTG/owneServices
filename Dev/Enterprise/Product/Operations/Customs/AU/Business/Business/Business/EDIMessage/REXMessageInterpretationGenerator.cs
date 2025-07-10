using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class REXMessageInterpretationGenerator
	{
		public static ZString GetInterpretedHTML(REXMessage message)
		{
			var xml = message.EM_MessageText;
			var sb = new StringBuilder();

			if (IsRexReadOwnershipMessage(xml))
			{
				var formattedMessage = FormatRexReadMessage(xml);
				sb.Append(formattedMessage);
			}
			else if (IsRexForwardOwnershipMessage(xml))
			{
				sb.AppendCaption("Forward Request sent.");

				AppendRexNumberAndClientGroup(sb, xml);

				var requiresAcceptance = FindNodeValue(xml, "NST1:requiresAcceptance");
				sb.AppendKeyValuePair("Requires Acceptance", requiresAcceptance);

				var holdUntilStatus = FindNodeValue(xml, "NST1:holdUntilStatus");
				sb.AppendKeyValuePair("Hold Until Status", holdUntilStatus, appendWhenEmpty: false);
			}
			else if (IsRexForwardOwnershipResponseMessage(xml))
			{
				sb.AppendCaption("Forward Ownership Response received.");

				var outcome = FindNodeValue(xml, "rex:outcome");
				sb.AppendKeyValuePair("Outcome", outcome);
			}
			else if (IsRexTransferOwnershipMessage(xml))
			{
				sb.AppendCaption("Transfer Request sent.");

				AppendRexNumberAndClientGroup(sb, xml);

				var exporter = FindNodeValue(xml, "NST1:exporter");
				sb.AppendKeyValuePair("Exporter", exporter);
			}
			else if (IsRexTransferOwnershipResponseMessage(xml))
			{
				sb.AppendCaption("Transfer Ownership Response received.");

				var outcome = FindNodeValue(xml, "rex:outcome");
				sb.AppendKeyValuePair("Outcome", outcome);
			}

			return WrapIntoBody(sb.ToString());
		}

		static void AppendRexNumberAndClientGroup(StringBuilder sb, ZString xml)
		{
			var rexNumber = FindNodeValue(xml, "NST2:rexNumber");
			sb.AppendKeyValuePair("REX Number", rexNumber);

			var clientGroup = FindNodeValue(xml, "NST1:clientGroup");
			sb.AppendKeyValuePair("Client Group", clientGroup);
		}

		#region Implementation

		static bool IsRexForwardOwnershipMessage(string xml)
		{
			return Regex.IsMatch(xml, @"^<NST1:RexForwardOwnership.*<\/NST1:RexForwardOwnership>$", MatchOptions);
		}

		static bool IsRexForwardOwnershipResponseMessage(string xml)
		{
			return Regex.IsMatch(xml, @"^<rex:RexForwardOwnershipResponse.*<\/rex:RexForwardOwnershipResponse>$", MatchOptions);
		}

		static bool IsRexTransferOwnershipMessage(string xml)
		{
			return Regex.IsMatch(xml, @"^<NST1:RexTransferOwnership.*<\/NST1:RexTransferOwnership>$", MatchOptions);
		}

		static bool IsRexTransferOwnershipResponseMessage(string xml)
		{
			return Regex.IsMatch(xml, @"^<rex:RexTransferOwnershipResponse.*<\/rex:RexTransferOwnershipResponse>$", MatchOptions);
		}

		static bool IsRexReadOwnershipMessage(string xml)
		{
			return Regex.IsMatch(xml, @"^<ns1:ReadRexResponse.*<\/ns1:ReadRexResponse>$", MatchOptions);
		}

		static StringBuilder FormatRexReadMessage(ZString xml)
		{
			var sb = new StringBuilder();
			sb.AppendCaption("Rex Update received.");

			ZString rexStatus = FindNodeValue(xml, "ns2:complianceStatus");
			if (!rexStatus.IsEmpty)
			{
				sb.AppendKeyValuePair("REX Status", rexStatus);
			}

			ZString permitNumber = FindNodeValue(xml, "ns2:permitNumber");
			if (!permitNumber.IsEmpty)
			{
				sb.AppendKeyValuePair("Export Permit Number", permitNumber);
			}

			ZString edn = FindNodeValue(xml, "ns2:edn");
			if (!edn.IsEmpty)
			{
				sb.AppendKeyValuePair("EDN", edn);
			}

			ZString lastAmendDate = FindNodeValue(xml, "ns2:lastAmendDateTime");
			if (!lastAmendDate.IsEmpty)
			{
				sb.AppendKeyValuePair("Last Amended", lastAmendDate);
			}

			return sb;
		}

		static string FindNodeValue(string xml, string nodeName)
		{
			var pattern = $@"<{nodeName}>(.*)<\/{nodeName}>";
			var match = Regex.Match(xml, pattern);
			return match.Success ? match.Groups[1].Value : string.Empty;
		}

		static void AppendCaption(this StringBuilder sb, string caption)
		{
			sb.Append($"<b>{caption}</b><br />");
		}

		static void AppendKeyValuePair(this StringBuilder sb, string key, string value, bool appendWhenEmpty = true)
		{
			if (!string.IsNullOrEmpty(value) || appendWhenEmpty)
			{
				sb.Append($"<b>{key}: {value}</b><br />");
			}
		}

		static string WrapIntoBody(string content)
		{
			var tableCreator = new HtmlTableCreator(new NameValueCollection
			{
				{ "border", "1" },
				{ "cellpadding", "1" },
				{ "cellspacing", "0" },
				{ "width", "100%" },
				{ "class", "table" }
			})
			{
				EnableHTMLEncoding = false
			};
			tableCreator.WriteRowWithFormatting(new CellWithFormatting(content));
			return $@"<html xmlns=""http://www.w3.org/1999/xhtml"">
	<head>
		<title>Message Detail</title>
		<style type=""text/css"">{styleSheet}</style>
	</head>
	<body>{tableCreator.ToHtml()}</body>
</html>";
		}

		const RegexOptions MatchOptions = RegexOptions.Multiline | RegexOptions.Singleline;

		static readonly string styleSheet = SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value;

		#endregion
	}
}
