using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class ValidationMessage
	{
		#region BR3181

		public string BR3181RuleCode = "BR3181";

		public string GetBR3181RuleEUMemberStateMessage() => FormatValidationMessageWithRuleCodePrefix(BR3181RuleCode, Res.GetString("14d1755f-9b7a-43b1-a800-c1f92dc77d1a", "The first 2 characters must correspond to a member state of the EU."));

		#endregion

		#region InvalidValue

		public string InvalidValueRuleCode => GetInvalidValueRuleCodeCore();
		protected virtual string GetInvalidValueRuleCodeCore() => ZString.Empty;

		public IMultilingualString InvalidValueRuleMessage => invalidValueRuleMessage ??= FormatValidationMessageWithRuleCodePrefix(InvalidValueRuleCode, GetInvalidValueRuleMessageCore());
		IMultilingualString invalidValueRuleMessage;

		protected virtual IMultilingualString GetInvalidValueRuleMessageCore() => ListValidation.InvalidCodeMessageError;

		public ZString GetMandatoryValueRuleMessage(ZPropertyInfo info) => MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info));

		#endregion

		public static IMultilingualString FormatValidationMessageWithRuleCodePrefix(string prefix, IMultilingualString message)
		{
			if (!string.IsNullOrEmpty(prefix))
			{
				return MultilingualString.Join(" ", (NoResString)$"[{prefix}]", (MultilingualString)message);
			}
			else
			{
				return message;
			}
		}

		public static string FormatValidationMessageWithRuleCodePrefix(string prefix, string message)
		{
			if (!string.IsNullOrEmpty(prefix))
			{
				return string.Join(" ", $"[{prefix}]", message);
			}
			else
			{
				return message;
			}
		}
	}
}
