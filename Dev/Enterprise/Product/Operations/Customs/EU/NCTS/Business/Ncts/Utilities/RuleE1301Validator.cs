using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class RuleE1301Validator
	{
		public RuleE1301Validator(NctsHeader header)
		{
			this.header = header;
		}

		public void Validate(ZPropertyInfo propertyInfo, string documentDescription, bool isRuleActive)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo));

			if (!propertyInfo.Value.IsEmpty
				&& header != null
				&& header.IsPhase5
				&& header.IsInPhase5TransitionPeriod
				&& isRuleActive)
			{
				var errorMessage = header.Configuration.ValidationRuleConfiguration.Messages.E1301Message(documentDescription);
				propertyInfo.AddMessageError(errorMessage);
			}
		}

		readonly NctsHeader header;
	}
}
