using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitContainerValidation : ExitControlBase.Business.CusExitContainerValidation
	{
		public CusExitContainerValidation(AutoCusExitContainer parent)
			: base(parent)
		{
		}

		protected new CusExitContainer Parent => (CusExitContainer)base.Parent;

		protected override void CheckCXN_IsEquipment()
		{
			base.CheckCXN_IsEquipment();
			if (Parent.CXN_IsEquipment && Parent.AllSealNumbers.Count == 0)
			{
				Parent.CXN_IsEquipmentInfo.AddMessageError(Res.GetString("{47A4EA70-5D1A-4D83-81EE-56C509DF9430}", "At least one Seal must be specified for Equipment."));
			}
		}

		protected override void CheckCXN_ContainerNumber()
		{
			base.CheckCXN_ContainerNumber();
			var number = Parent.CXN_ContainerNumber;
			if (number.IsEmpty)
			{
				var info = Parent.CXN_ContainerNumberInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
			else if (!Parent.CXN_IsEquipment)
			{
				string message = ContainerNumberValidation.GetContainerNumberError(number);
				if (message != null)
				{
					Parent.CXN_ContainerNumberInfo.AddWarning(message);
				}
			}
		}

		protected override void CheckCXN_Sequence()
		{
			base.CheckCXN_Sequence();
			var parent = Parent;
			var header = parent.Header;
			if (header.ShouldHaveSeqNumInContainersOrEquipmentsAndSeals)
			{
				var sequence = parent.CXN_Sequence;
				var info = parent.CXN_SequenceInfo;
				if (sequence > 0)
				{
					if ((!parent.IsInDatabase || info.HasChanges)
						&& header.ContainersSequenceDictionary.TryGetValue(sequence, out var count)
						&& count > 1)
					{
						info.AddError(Res.GetString("{517E856C-A3C9-449E-9A5E-08E5674B1AE6}", "{0} ({1}) should not be duplicated", info.Description, sequence));
					}
				}
				else
				{
					MandatoryValidation.CheckNotZero(info);
					MandatoryValidation.CheckNotNegative(info);
				}
			}
		}

		protected override void CheckCXN_Status()
		{
			base.CheckCXN_Status();
			if (IsUcc6RuleActive(Parent, x => x.ValidateCXN_StatusLookups))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CXN_StatusInfo);
			}
		}

		bool IsUcc6RuleActive(CusExitContainer container, Func<ICusExitContainerUcc6ValidationDecider, bool> ruleCheck)
		{
			if (container.ValidationDecider is ICusExitContainerUcc6ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}
	}
}
