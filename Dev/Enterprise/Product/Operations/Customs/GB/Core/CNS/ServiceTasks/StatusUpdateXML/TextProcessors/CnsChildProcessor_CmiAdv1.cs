
using System.Text.RegularExpressions;
using CargoWise.Types;
namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// MANIFEST AMENDMENT AFTER UNIT ARRIVAL

	class CnsChildProcessor_CmiAdv1 : CnsChildProcessor
	{
		protected override Match RegexMatchForUcn
		{
			get
			{
				// Base expects the UCN to be a single (the first) group matched in a simple string. But ths CMI-ADV-1 message splits its UCNs up into parts spread over several lines. 
				// Find these parts, smush them together into a new simple string, and return a match made on THAT string. 

				var sb = new ZStringBuilder();
				sb.Append("UCN: ");
				var firstTwoPartsPattern = @"ARRIVAL TIME/DATE STATUS 
  (....)  (.....)";
				var secondTwoPartsPattern = "UGI (...) UCI (..)";
				foreach (var pattern in new string[] { firstTwoPartsPattern, secondTwoPartsPattern })
				{
					var regex = new Regex(pattern).Match(receivedEdiMessage.EM_MessageText);
					sb.Append(regex.Groups[1].Value);
					sb.Append(regex.Groups[2].Value);
				}
				sb.Append(" ");
				return new Regex(RegexPatternForUcn).Match(sb.ToString());
			}
		}
	}
}
