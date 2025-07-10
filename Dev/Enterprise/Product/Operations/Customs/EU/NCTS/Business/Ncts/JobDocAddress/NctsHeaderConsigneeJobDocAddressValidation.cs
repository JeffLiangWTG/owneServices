using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderConsigneeJobDocAddressValidation : ConsigneeJobDocAddressValidation
	{
		public NctsHeaderConsigneeJobDocAddressValidation(AutoJobDocAddress parent, NctsHeader nctsHeader)
			: base(parent, nctsHeader)
		{
		}

		protected override bool IsRelevantConsigneeEmpty() => NctsHeader.Bills.Count == 0 || NctsHeader.Bills.Any(x => x.Consignee.IsEmpty);

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			CheckRuleB1823();
			CheckRuleC0001_4();
			CheckRuleC0001_6();
			CheckRuleG0001_1();
		}

		void CheckRuleB1823()
		{
			var isRuleB1823Applicable = ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validation
				&& validation.IsRuleB1823Active
				&& IsInPhase5TransitionPeriod;

			if (!isRuleB1823Applicable)
			{
				return;
			}

			var parent = Parent;
			var codesOfC0009 = UniversalLookupsHelper.GetCountryC0009List(parent.Factory, RefDataGroupingCodes.EuropeanUnionEUN);
			var organisationPKIsEmpty = parent.OrganisationPK.IsEmpty;
			var organisationPKInfo = parent.OrganisationPKInfo;
			var movementHeader = MovementHeader;
			var validationRuleConfigurationMessages = ValidationRuleConfiguration.Messages;

			if (codesOfC0009.ContainsCode(movementHeader.BM_RL_NKDestinationPort))
			{
				var isGoodsItemsConsigneeOrganisationPkPresent = NctsHeader.Bills.Any(x => x.GoodsItems.Any(x => !x.Consignee.OrganisationPK.IsEmpty));
				if (!organisationPKIsEmpty && isGoodsItemsConsigneeOrganisationPkPresent)
				{
					organisationPKInfo.AddMessageError(validationRuleConfigurationMessages.B1823aMessage);
				}
				if (organisationPKIsEmpty && !isGoodsItemsConsigneeOrganisationPkPresent)
				{
					organisationPKInfo.AddMessageError(validationRuleConfigurationMessages.B1823bMessage);
				}
			}
			else if (!organisationPKIsEmpty
				&& movementHeader.IsSecurityTypeBTHOrEXI
				&& movementHeader.Has30600AdditionalInformation)
			{
				organisationPKInfo.AddMessageError(validationRuleConfigurationMessages.B1823aMessage);
			}
		}

		void CheckRuleC0001_4()
		{
			var header = NctsHeader;
			var parent = Parent;

			var isRuleC0001_4Applicable = ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validation && validation.IsRuleC0001_4Active
				&& !IsInPhase5TransitionPeriod;

			if (!isRuleC0001_4Applicable)
			{
				return;
			}

			if (!parent.OrganisationPK.IsEmpty
				&& Has30600AdditionalInformationAtHeaderOrBills())
			{
				parent.OrganisationPKInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0001_4Message);
			}

			bool Has30600AdditionalInformationAtHeaderOrBills() => header.Has30600AdditionalInformation
				|| header.Bills.Any(x => x.Has30600AdditionalInformation);
		}

		void CheckRuleC0001_6()
		{
			var header = NctsHeader;
			var movementHeader = header.MovementHeader;
			var parent = Parent;
			var targetPropertyInfo = parent.OrganisationPKInfo;

			var isRuleC0001_6Applicable = ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validation && validation.IsRuleC0001_6Active
				&& !IsInPhase5TransitionPeriod;

			if (!isRuleC0001_6Applicable)
			{
				return;
			}

			var isDestinationPortExcludedFromC0009 = !header.GetC0009CountryCodes().Contains(movementHeader.BM_RL_NKDestinationPort);

			if (isDestinationPortExcludedFromC0009)
			{
				if (movementHeader.IsSecurityTypeNONOrENT)
				{
					return;
				}

				if (header.Has30600AdditionalInformation || (isHouseConsignmentDocType30600Present() && Has30600InfoAtBillsAndNoBillWithEmptyConsigneeWithout30600Info()))
				{
					if (!parent.OrganisationPK.IsEmpty)
					{
						targetPropertyInfo.AddWarning(ValidationRuleConfiguration.Messages.C0001_6Message);
					}
					return;
				}
			}
			new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => parent.OrganisationPK,
			lineValuesProvider: () => header.Bills.Select(x => x.Consignee.OrganisationPK),
			ruleCode: ValidationRuleConfiguration.Messages.C0001_6RuleCode)
			{
				IsEmptyFunc = x => x.IsEmpty,
				EmptyHeaderAndLinesMessageProvider = () => ValidationCaptions.EmptyHeaderAndHouseValue,
				IgnoredHeaderSameLinesMessageProvider = () => ValidationCaptions.HeaderLevelWillBeIgnoredHousesHaveSameValue,
				IgnoredHeaderWithLinesValuesMessageProvider = () => ValidationCaptions.HeaderLevelWillBeIgnoredHousesHaveDifferentValues,
			}.ValidateHeader(parent.OrganisationPKInfo);

			bool isHouseConsignmentDocType30600Present() => header.Bills.Any(x => x.Has30600AdditionalInformation);
			bool Has30600InfoAtBillsAndNoBillWithEmptyConsigneeWithout30600Info() => !header.Bills.Any(x => x.Consignee.IsEmpty && !x.Has30600AdditionalInformation);
		}

		void CheckRuleG0001_1()
		{
			var nctsHeader = NctsHeader;
			var parent = Parent;
			var isRuleG0001_1Applicable = ValidationDecider is INctsHeaderDeparturePhase5ValidationDecider validation && validation.IsRuleG0001_1Active;

			if (!isRuleG0001_1Applicable || parent.OrganisationPK.IsEmpty)
			{
				return;
			}

			if (!IsInPhase5TransitionPeriod
				&& nctsHeader.Bills.Any(x => x.Has30600AdditionalInformation))
			{
				parent.OrganisationPKInfo.AddMessageError(ValidationRuleConfiguration.Messages.G0001_1Message);
			}

			if (IsInPhase5TransitionPeriod
				&& nctsHeader.Bills.SelectMany(b => b.GoodsItems).Any(x => x.Has30600AdditionalInformation))
			{
				parent.OrganisationPKInfo.AddMessageError(ValidationRuleConfiguration.Messages.G0001_1Message);
			}
		}

		bool IsInPhase5TransitionPeriod => NctsHeader.IsInPhase5TransitionPeriod;

		NctsDepartureMovementHeader MovementHeader => NctsHeader.MovementHeader;

		ValidationRuleConfiguration ValidationRuleConfiguration => NctsHeader.Configuration.ValidationRuleConfiguration;

		INctsHeaderValidationDecider ValidationDecider => NctsHeader.ValidationDecider;
	}
}
