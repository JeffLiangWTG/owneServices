using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

public static class UniqueTransactionIdentifierTextExtractor
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant regex pattern")]
	public static ZString ExtractUniqueTransactionIdentifier(ZString content)
	{
		const string regexPattern = "(?i)<.*IUT>(.+)<\\/.*IUT>";

		var regexMatch = Regex.Match(content, regexPattern);
		return regexMatch.Groups?[1].Value ?? ZString.Empty;
	}
}
