using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusSealPhase5Validation : CusSealValidation
	{
		public CusSealPhase5Validation(AutoCusSeal parent) : base(parent)
		{
		}

		protected override void CheckBK_UnloadingState()
		{
			base.CheckBK_UnloadingState();
			if (Parent.Header is NctsHeader header && header.IsArrivalMovement)
			{
				MandatoryValidation.CheckEntered(Parent.BK_UnloadingStateInfo);
			}
		}

		protected override bool CheckDuplicateSealNumber => Parent.ValidationDecider is ICusSealPhase5ValidationDecider decider && decider.IsRuleTR0045Active;

		protected override string DuplicateSealNumberWarningPrefix => $"{ValidationRuleCodeConstants.TR0045.GetRuleCodeMessagePrefix()} ";
	}
}
