using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderPhase5Validation : NctsHeaderValidation
	{
		public NctsHeaderPhase5Validation(NctsHeader parent)
			: base(parent)
		{
		}

		protected sealed override void CheckArrivalMrnFromUser()
		{
			if (ValidationEnabled)
			{
				CheckArrivalMrnFromUserCore();
			}
		}

		protected virtual void CheckArrivalMrnFromUserCore()
		{
			var parent = Parent;
			base.CheckArrivalMrnFromUser();
			if (parent.IsArrivalMovement)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ArrivalMrnFromUserInfo);

				if (parent.ValidationDecider is INctsHeaderArrivalPhase5ValidationDecider decider)
				{
					if (decider.IsRuleTR0047Active)
					{
						var (duplicate, declarationReferenceWithDuplicateMRN) = NctsHelper.RetrieveLRNOfDeclarationWithMatchingMRN(parent.PK, parent.ArrivalMrnFromUser, parent.Factory, NctsMovementType.Codes.Arrival);
						if (duplicate)
						{
							parent.ArrivalMrnFromUserInfo.AddMessageError(Res.GetString("D4B2BAC0-93DB-434F-98F2-4F3191FFAFE6", "[TR0047] An NCTS Arrival declaration with the same MRN number already exists on Job '{0}'. Please check the MRN number to ensure that it is correct.", declarationReferenceWithDuplicateMRN));
						}
					}

					if (!parent.ArrivalMrnFromUser.IsEmpty && decider.IsRuleTR0035Active && parent.ArrivalMrnFromUser.Length < 18)
					{
						parent.ArrivalMrnFromUserInfo.AddMessageError(Res.GetString("3791a3c3-07a8-44bf-ac7a-de4294461e7f", "[TR0035] Invalid MRN Number. MRN Number should be exactly 18 Character Long."));
					}
				}
			}
		}

		protected override void CheckLocalReferenceNumber_Length()
		{
			var parent = Parent;
			if (parent.IsArrivalMovement && parent.ArrivalMovementHeader.ValidationDecider is INctsArrivalMovementHeaderPhase5ValidationDecider decider)
			{
				if (parent.LocalReferenceNumber.Length <= 4 && decider.IsRuleTR0034Active)
				{
					parent.LocalReferenceNumberInfo.AddMessageError(Res.GetString("CE27221A-750F-4884-A0C5-730A88996A2A", "[TR0034] This field must have a value and the value must be longer than 4 characters."));
				}
			}
		}

		protected override void CheckBH_ExportFlag()
		{
			base.CheckBH_ExportFlag();
			var header = Parent;

			if (header.IsInPhase5TransitionPeriod &&
				header.ValidationDecider is INctsHeaderArrivalPhase5ValidationDecider { IsRuleNR0015Active: true } &&
				header.BH_ExportFlag == EventFlagList.Codes.Yes &&
				header.EnRouteIncidents.Count == 0)
			{
				header.BH_ExportFlagInfo.AddMessageError(Res.GetString("9eeee3af-5796-456f-b8fc-f4a182b036c0", "[NR0015] At least one incident record is required."));
			}
		}

		protected override void CheckBH_OverrideFreightDefaults()
		{
			base.CheckBH_OverrideFreightDefaults();

			var parent = Parent;

			if (parent.IsDepartureMovement && !parent.BH_OverrideFreightDefaults && parent.IsPluggedIn && parent.Bills.Any(b => !b.B0_ReferenceIDInfo.ReadOnly || b.GoodsItems.Any(g => !g.BY_DescriptionInfo.ReadOnly || g.Packages.Cast<NctsPackage>().Any(p => !p.B5_MarksAndNumbersInfo.ReadOnly))))
			{
				parent.BH_OverrideFreightDefaultsInfo.AddError(Res.GetString("1B1BD847-2148-48A8-980E-A8CBC5C18AFA", "The 'Override Freight Defaults' must be ticked if you want to add a new house consignment, goods item, or package line."));
			}
		}
	}
}
