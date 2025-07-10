using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.UniversalDataBuss.Testing.DataObjectCreators
{
	public static class ValidationRuleCreator
	{
		public static ValidationRule Create(ZString code, ZInt sequence, ZString messageLog, ZString result)
		{
			var rule = new ValidationRule();
			rule.Code = code;
			rule.Sequence = sequence;
			rule.MessageLog = messageLog;
			rule.Result = result;
			return rule;
		}

		public static ValidationRule Create(ZString code)
		{
			var rule = new ValidationRule();
			rule.Code = code;
			return rule;
		}
	}
}
