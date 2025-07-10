using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	static class NctsPreviousDocumentPhase5RuleNRWithMrnValidationHelper
	{
		public const int MRNLength = 18;

		public static ZString GetMrnValidationMessage(string mrn, string ruleCode = null)
		{
			if (mrn.Length != MRNLength)
			{
				return GetRuleMessage(ruleCode, GetLengthMustBe18CharactersMessage());
			}

			if (mrn.Any(char.IsWhiteSpace))
			{
				return GetRuleMessage(ruleCode, GetSpacesAreNotAllowedMessage());
			}

			return ZString.Empty;
		}

		static string GetRuleMessage(string ruleCode, string messageError)
			=> ruleCode == null
			? messageError
			: ValidationRuleMessages.FormatMessage(ruleCode, messageError);

		static string GetLengthMustBe18CharactersMessage() => Res.GetString("CF671594-FF9A-445B-96A2-820543BBF341", "{0} Length must be {1} characters.", BaseMassage, MRNLength);

		static string GetSpacesAreNotAllowedMessage() => Res.GetString("E5E03795-707A-4DF4-A3E9-626157BCB738", "{0} Spaces are not allowed.", BaseMassage);

		static string BaseMassage =>
			Res.GetString("1EC5FE4B-37FF-4232-AFA1-C9218C234878", "This type of document must contain an MRN Number.");
	}
}
