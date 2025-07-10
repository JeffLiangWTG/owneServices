using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusSealValidation : Customs.Business.CusSealValidation
	{
		public CusSealValidation(AutoCusSeal parent) : base(parent)
		{
		}

		protected new CusSeal Parent => (CusSeal)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			CheckRuleN0003();
		}

		protected override void CheckBK_UnloadingState()
		{
			base.CheckBK_UnloadingState();
			if (!NctsHelper.UnloadedStateInitiallyNew(Parent.BK_UnloadingStateInfo))
			{
				ListValidation.ErrorIfInvalidCode(Parent.BK_UnloadingStateInfo);
			}
		}

		protected override void CheckBK_SealNumber()
		{
			base.CheckBK_SealNumber();

			var parent = Parent;
			var sealNumber = parent.BK_SealNumber;
			if (CheckDuplicateSealNumber && !sealNumber.IsEmpty && parent.Parent is BusinessObject sealParent)
			{
				var hasDuplicates = false;
				switch (sealParent)
				{
					case NctsDepartureHeaderContainer departureContainer:
						var parentHeaderSeals = departureContainer.Header?.HeaderContainersSeals ?? new List<ZString>();
						hasDuplicates = parentHeaderSeals.Where(x => x == sealNumber).Skip(1).Any();
						break;
					case NctsContainer nctsContainer:
						hasDuplicates = nctsContainer.Parent is EnRouteIncident incident && incident.IncidentContainers.HasDuplicatesForSealNumber(sealNumber);
						break;
					case NctsArrivalHeaderContainer parentArrivalHeaderContainer:
						var containerMode = parentArrivalHeaderContainer.BC_Mode;
						if (containerMode == Core.Constants.ContainerModes.Containerised || containerMode == Core.Constants.ContainerModes.NonContainerised)
						{
							hasDuplicates = parentArrivalHeaderContainer.Seals.Where(x => x.BK_SealNumber == sealNumber).Skip(1).Any();
						}
						break;
				}
				if (hasDuplicates)
				{
					Parent.BK_SealNumberInfo.AddWarning(DuplicateSealNumberWarningPrefix + Res.GetString("F50B659C-6304-4E41-99B8-03828C28D2D5", "Duplicate Seal Number entered."));
				}
			}

			if (Parent.Header?.Configuration.CusSealConfiguration.GetValidationDecider(Parent.Header) is ICusSealValidationDecider { IsRuleNR0029Active: true }
				&& Parent.BK_UnloadingState == NctsUnloadedStateList.Codes.NEW
				&& Parent.BK_SealNumber.IsEmpty)
			{
				Parent.BK_SealNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.GetNR0029bMessage());
			}
		}

		ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header?.Configuration.ValidationRuleConfiguration;

		protected virtual bool CheckDuplicateSealNumber => true;

		protected virtual string DuplicateSealNumberWarningPrefix => string.Empty;

		void CheckRuleN0003()
		{
			var rowMessageError = ValidationRuleConfiguration?.Messages?.GetN0003Message();

			var parent = Parent;

			if (rowMessageError == null)
			{
				return;
			}

			parent.RemoveRowMessageError(rowMessageError);

			if (parent.ValidationDecider is ICusSealPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleN0003Active
				&& parent?.Header is NctsHeader header
				&& !header.IsInPhase5TransitionPeriod
				&& header.MovementHeader.SupportingDocuments.Any(x => x.CSI_Code == NctsTypeOfSupportingDocument.Codes._67YY))
			{
				parent.AddRowMessageError(rowMessageError);
			}
		}
	}
}
