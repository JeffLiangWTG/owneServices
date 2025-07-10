using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS
{
	public class InlandTransportValidation : Customs.Business.CusCodeDataValidation
	{
		public InlandTransportValidation(InlandTransport parent) : base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var parent = Parent;
			ValidateIfNotEntered(parent.CY_DataInfo, parent.CY_CodeInfo);
		}

		protected override void CheckCY_CodeIsNotEmpty()
		{
			var parent = Parent;
			ValidateIfNotEntered(parent.CY_CodeInfo, parent.CY_DataInfo);
		}

		protected override void CheckCY_CodeList()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);
		}

		void ValidateIfNotEntered(ZPropertyInfo targetInfo, ZPropertyInfo dependentInfo)
		{
			var parent = Parent;

			if (parent.Parent is NctsDepartureMovementHeader movementHeader
				&& movementHeader.Header is NctsHeader header
				&& !header.IsInPhase5TransitionPeriod
				&& header.Configuration is NctsConfiguration configuration
				&& movementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleB2101Active
				&& movementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport)
			{
				if (!dependentInfo.Value.IsEmpty)
				{
					var rulePrefix = configuration.ValidationRuleConfiguration.Messages.B2101RuleCode.GetRuleCodeMessagePrefix(true);
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: rulePrefix);
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		new InlandTransport Parent => (InlandTransport)base.Parent;
	}
}
