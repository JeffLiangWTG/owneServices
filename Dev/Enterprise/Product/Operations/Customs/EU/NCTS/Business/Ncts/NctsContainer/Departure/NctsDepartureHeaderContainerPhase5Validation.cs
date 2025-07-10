using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureHeaderContainerPhase5Validation : NctsDepartureHeaderContainerValidation
	{
		public NctsDepartureHeaderContainerPhase5Validation(NctsDepartureHeaderContainer parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckContainerHasBeenAssigned();
		}

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();

			var parent = Parent;

			if (parent.IsContainerised)
			{
				ContainerNumberValidation.WarnIfInvalid(parent.BC_ContainerNumInfo);
				MandatoryValidation.MessageErrorIfNotEntered(parent.BC_ContainerNumInfo);

				var header = parent.Header;
				if (parent.ValidationDecider is INctsDepartureHeaderContainerPhase5ValidationDecider { IsRuleTR0044Active: true })
				{
					parent.CheckContainerNumberIsUnique(header.DepartureHeaderContainers, $"{ValidationRuleCodeConstants.TR0044.GetRuleCodeMessagePrefix()} ");
				}
			}

			CheckC0055(parent);
		}

		void CheckC0055(NctsDepartureHeaderContainer parent)
		{
			if (parent.ValidationDecider is { IsRuleC0055Active: true }
				&& parent.BC_Mode == Core.Constants.ContainerModes.NonContainerised
				&& !parent.BC_ContainerNum.IsEmpty
				&& !parent.IsNonContainerized)
			{
				parent.BC_ContainerNumInfo.AddWarning(parent.Header.Configuration.ValidationRuleConfiguration.Messages.C0055Message);
			}
		}

		protected override void CheckBC_Mode()
		{
			base.CheckBC_Mode();

			var parent = Parent;
			ListValidation.ErrorIfInvalidCode(parent.BC_ModeInfo);

			if (parent.Header is NctsHeader nctsHeader)
			{
				if (parent.ValidationDecider is INctsDepartureHeaderContainerPhase5ValidationDecider { IsRuleTR0043Active: true })
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.BC_ModeInfo, parent.BC_ModeInfo.Description, $"{ValidationRuleCodeConstants.TR0043.GetRuleCodeMessagePrefix()} ");
				}

				if (parent.ValidationDecider is INctsDepartureHeaderContainerPhase5ValidationDecider { IsRuleTR0046Active: true })
				{
					parent.CheckTR0046AllSameMode(nctsHeader.DepartureHeaderContainers);
				}
			}
		}

		protected override void CheckBC_Seal1()
		{
			base.CheckBC_Seal1();

			var parent = Parent;
			var nctsHeader = parent.Header;

			if (CheckRuleN0003(parent.BC_Seal1))
			{
				parent.BC_Seal1Info.AddMessageError(Parent.Header.Configuration.ValidationRuleConfiguration.Messages.GetN0003Message());
			}

			if (parent.AdditionalSeals.Count > 0)
			{
				var isArrival = nctsHeader?.IsArrivalMovement ?? false;
				if (!isArrival && !(Parent.ValidationDecider?.IsRuleN0003Active ?? false))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.BC_Seal1Info);
				}
			}

			var seal1 = parent.Seal1;
			if (HasDuplicateSeals(seal1, parent))
			{
				parent.BC_Seal1Info.AddWarning($"{ValidationRuleCodeConstants.TR0045.GetRuleCodeMessagePrefix()} " + Res.GetString("8E77814B-4A38-4A31-994C-EE062EC572BA", "Duplicate Seal 1 Number entered."));
			}
		}

		protected override void CheckBC_Seal2()
		{
			base.CheckBC_Seal2();

			var parent = Parent;
			var nctsHeader = parent.Header;

			if (CheckRuleN0003(parent.BC_Seal2))
			{
				parent.BC_Seal2Info.AddMessageError(Parent.Header.Configuration.ValidationRuleConfiguration.Messages.GetN0003Message());
			}

			if (parent.AdditionalSeals.Count > 0)
			{
				var isArrival = nctsHeader?.IsArrivalMovement ?? false;
				if (!isArrival && !(Parent.ValidationDecider?.IsRuleN0003Active ?? false))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.BC_Seal2Info);
				}
			}

			var seal2 = parent.Seal2;
			if (HasDuplicateSeals(seal2, parent))
			{
				parent.BC_Seal2Info.AddWarning($"{ValidationRuleCodeConstants.TR0045.GetRuleCodeMessagePrefix()} " + Res.GetString("B2DB662D-A816-438D-B370-40F78862E780", "Duplicate Seal 2 Number entered."));
			}
		}

		protected override void CheckTotalSealCount()
		{
			var parent = Parent;
			if (parent.BC_ContainerNum.IsEmpty
				&& parent.TotalSealCount.IsEmpty
				&& (parent.ValidationDecider is INctsDepartureHeaderContainerPhase5ValidationDecider { IsRuleR0448Active: true }))
			{
				parent.TotalSealCountInfo.AddMessageError(Res.GetString("7637F4EF-7B26-4A99-9053-4763C41C7371", "[R0448] Number of Seals can't be 0 if Container Identification Number is empty."));
			}
		}

		static bool HasDuplicateSeals(ZString seal, NctsDepartureHeaderContainer parent)
		{
			var nctsHeader = parent.Header;
			return !seal.IsEmpty
					&& (parent.ValidationDecider is INctsDepartureHeaderContainerPhase5ValidationDecider { IsRuleTR0045Active: true })
					&& nctsHeader.HeaderContainersSeals.Where(x => x.Equals(seal)).Skip(1).Any();
		}

		void CheckContainerHasBeenAssigned()
		{
			var nctsDepartureHeaderContainer = Parent;

			if (nctsDepartureHeaderContainer.ValidationDecider is INctsDepartureHeaderContainerPhase5ValidationDecider validationDecider && validationDecider.IsRuleTR0095Active)
			{
				var departureGoodsItems = nctsDepartureHeaderContainer.Header.Bills.SelectMany(x => x.GoodsItems);
				var containerNum = nctsDepartureHeaderContainer.BC_ContainerNum;
				if (departureGoodsItems.All(x => !x.ContainersSelected.Contains(containerNum)))
				{
					nctsDepartureHeaderContainer.AddRowMessageError(ValidationRuleConfiguration.Messages.TR0095Message(nctsDepartureHeaderContainer.BC_SequenceNumber, containerNum));
				}
			}
		}

		bool CheckRuleN0003(ZString seal)
		{
			var parent = Parent;
			var header = parent.Header;
			if (header == null)
			{
				return false;
			}

			return (parent.ValidationDecider?.IsRuleN0003Active ?? false)
				&& !header.IsInPhase5TransitionPeriod
				&& !seal.IsEmpty
				&& header.MovementHeader.SupportingDocuments.Any(x => x.CSI_Code == NctsTypeOfSupportingDocument.Codes._67YY);
		}

		ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header.Configuration.ValidationRuleConfiguration;
	}
}
