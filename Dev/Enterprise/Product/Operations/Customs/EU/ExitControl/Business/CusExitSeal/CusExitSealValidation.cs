using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitSealValidation : Customs.Business.CusSealValidation
	{
		public CusExitSealValidation(CusExitSeal parent)
			: base(parent)
		{
		}

		protected CusExitContainer Container => Parent.Container;

		protected new CusExitSeal Parent => (CusExitSeal)base.Parent;

		protected override void CheckBK_SealNumber()
		{
			base.CheckBK_SealNumber();
			var sealNumberInfo = Parent.BK_SealNumberInfo;
			var number = Parent.BK_SealNumber;
			if (!number.IsEmpty && !Parent.BK_SequenceNumber.IsEmpty
				&& (!Parent.IsInDatabase || sealNumberInfo.HasChanges)
				&& Container is CusExitContainer container
				&& container.AdditionalSealNumbersNumberDictionary.TryGetValue(number, out var count)
				&& count > 1)
			{
				Parent.BK_SealNumberInfo.AddMessageError(Res.GetString("{02F976A6-B1A4-493B-BB3B-CCEC879EF79B}", "This seal number ({0}) is specified more than once.", number));
			}
		}

		protected override void CheckBK_SequenceNumber()
		{
			base.CheckBK_SequenceNumber();
			var parent = Parent;
			var header = Container.Header;
			if (header.ShouldHaveSeqNumInContainersOrEquipmentsAndSeals)
			{
				var sequence = parent.BK_SequenceNumber;
				var info = parent.BK_SequenceNumberInfo;
				if (sequence > 0)
				{
					if ((!parent.IsInDatabase || info.HasChanges)
						&& Container.AdditionalSealNumbersSequenceNumberDictionary.TryGetValue(sequence, out var count)
						&& count > 1)
					{
						info.AddError(Res.GetString("{DA874A0D-3592-41F5-B1B6-021872AF0262}", "{0} ({1}) should not be duplicated", info.Description, sequence));
					}
				}
				else
				{
					MandatoryValidation.CheckNotZero(info);
				}
			}
		}

		protected override void CheckBK_UnloadingState()
		{
			base.CheckBK_UnloadingState();

			if (IsUcc6RuleActive(Parent, x => x.ValidateBK_UnloadingStateLookups))
			{
				ListValidation.ErrorIfInvalidCode(Parent.BK_UnloadingStateInfo, Parent.Lookups.StatusList);
			}
		}

		bool IsUcc6RuleActive(CusExitSeal seal, Func<ICusExitSealUcc6ValidationDecider, bool> ruleCheck)
		{
			if (seal.ValidationDecider is ICusExitSealUcc6ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}
	}
}
