using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalHeaderContainerPhase5Validation : NctsArrivalHeaderContainerValidation
	{
		public NctsArrivalHeaderContainerPhase5Validation(NctsArrivalHeaderContainer parent) : base(parent)
		{
		}

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();

			var parent = Parent;
			if (parent.IsContainerised)
			{
				var containerNumber = parent.BC_ContainerNum;
				if (parent.ValidationDecider is INctsArrivalHeaderContainerPhase5ValidationDecider { IsRuleTR0044Active: true }
					&& NctsArrival.ArrivalHeaderContainers.Where(x => x.IsContainerised && x.BC_ContainerNum == containerNumber).Skip(1).Any())
				{
					parent.BC_ContainerNumInfo.AddMessageError(Res.GetString("405B8E68-F438-4C55-85A0-E14F94F9B50C", "[TR0044] Duplicate Container Number is entered."));
				}
			}
		}

		protected override void CheckBC_Mode()
		{
			base.CheckBC_Mode();

			if (Parent.ValidationDecider is INctsArrivalHeaderContainerPhase5ValidationDecider validationDecider)
			{
				if (NctsArrival.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
					&& validationDecider.IsRuleTR0043Active
					&& (!Parent.BC_ContainerNum.IsEmpty || Parent.Seals.Cast<CusSeal>().Any(x => !x.BK_SealNumber.IsEmpty)))
				{
					MandatoryValidation.CheckEntered(Parent.BC_ModeInfo, errorNotificationPrefix: "[" + configuration.Messages.TR0043RuleCode + "] ");
				}

				if(validationDecider.IsRuleTR0046Active)
				{
					Parent.CheckTR0046AllSameMode(NctsArrival.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>());
				}
			}
		}

		protected override void CheckBC_UnloadedState()
		{
			base.CheckBC_UnloadedState();
			MandatoryValidation.CheckEntered(Parent.BC_UnloadedStateInfo);

			if (NctsArrival.Configuration.ValidationRuleConfiguration is ValidationRuleConfiguration configuration
				&& Parent.ValidationDecider is INctsArrivalHeaderContainerPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleNR0029Active
				&& Parent.BC_UnloadedState == NctsUnloadedStateList.Codes.NEW
				&& Parent.TotalSealCount == 0)
			{
				Parent.BC_UnloadedStateInfo.AddMessageError(configuration.Messages.GetNR0029aMessage());
			}
		}
	}
}
