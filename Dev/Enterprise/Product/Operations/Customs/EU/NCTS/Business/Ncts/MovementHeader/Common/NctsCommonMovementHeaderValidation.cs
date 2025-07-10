using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsCommonMovementHeaderValidation : CusInBondMoveHeaderValidation
	{
		public NctsCommonMovementHeaderValidation(NctsCommonMovementHeader parent)
			: base(parent)
		{
		}

		protected new NctsCommonMovementHeader Parent => (NctsCommonMovementHeader)base.Parent;

		protected NctsHeader NctsHeader => Parent.Header;

		protected INctsMovementHeaderValidationDecider ValidationDecider => Parent.ValidationDecider;

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			if (ValidationDecider?.IsInBondEntryTypeListValidationActive ?? true)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BM_InBondEntryTypeInfo);
			}
		}

		protected override void CheckBM_PlaceOfUnloading()
		{
			var parent = Parent;
			var info = parent.BM_PlaceOfUnloadingInfo;
			base.CheckBM_PlaceOfUnloading();
			if (ShouldListValidatePlaceOfUnloading)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.BM_PlaceOfUnloadingInfo);
			}
			CheckRuleC0191(parent, info);
			CheckRuleB1858(parent, info);
		}

		protected bool IsBM_PlaceOfUnloadingMandatory(ZString typeOfSecurity) => typeOfSecurity == NctsTypeOfSecurityList.Codes.ENT || typeOfSecurity == NctsTypeOfSecurityList.Codes.BTH;

		protected virtual bool ShouldListValidatePlaceOfUnloading => true;

		protected override void CheckBM_SealQty()
		{
			base.CheckBM_SealQty();
			MandatoryValidation.CheckNotNegative(Parent.BM_SealQtyInfo);
		}

		protected void ValidateCustomsOfficeCode(ICustomsOffice customsOffice, ZPropertyInfo customsOfficeInfo)
		{
			if (customsOffice is EuOfficeCode office)
			{
				office.Validation.ValidateCY_Data();
				customsOfficeInfo.AddAllNotificationsFrom(office.CY_DataInfo);
			}
		}

		bool IsRuleB1858Applicable => (ValidationDecider?.IsRuleB1858Active ?? false) && (NctsHeader?.IsInPhase5TransitionPeriod ?? false);

		void CheckRuleB1858(NctsCommonMovementHeader parent, ZPropertyInfo info)
		{
			var isB1858 = false;
			var isB1858_2 = false;

			if (IsRuleB1858Applicable)
			{
				isB1858 = true;
			}
			else if (ValidationDecider is IRuleB1858_2Decider decider && decider.IsActive)
			{
				isB1858_2 = parent.IsInPhase5TransitionPeriod;
			}

			if (isB1858 || isB1858_2)
			{
				if (parent.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON)
				{
					if (!parent.BM_ForeignDestPortKCode.IsEmpty || !parent.BM_PlaceOfUnloading.IsEmpty)
					{
						if (isB1858_2)
						{
							info.AddWarning(parent.Header.Configuration.ValidationRuleConfiguration.Messages.B1858_2aMessage);
						}
						else
						{
							info.AddMessageError(parent.Header.Configuration.ValidationRuleConfiguration.Messages.B1858aMessage);
						}
					}
				}
				else
				{
					if (parent.BM_SpecificCircumstance != NctsSpecificCircumstanceIndicatorList.Codes.XXX && (parent.BM_ForeignDestPortKCode.IsEmpty || (parent.BM_PlaceOfUnloading.IsEmpty && parent.BM_ForeignDestPortKCode.Length != 5)))
					{
						if (isB1858_2)
						{
							info.AddMessageError(parent.Header.Configuration.ValidationRuleConfiguration.Messages.B1858_2bMessage);
						}
						else
						{
							info.AddMessageError(parent.Header.Configuration.ValidationRuleConfiguration.Messages.B1858bMessage);
						}
					}
				}
			}
		}

		void CheckRuleC0191(NctsCommonMovementHeader parent, ZPropertyInfo info)
		{
			if (ValidationDecider is { IsRuleC0191Active: true }
				&& !parent.Header.IsInPhase5TransitionPeriod
				&& IsBM_PlaceOfUnloadingMandatory(parent.BM_TypeOfSecurity))
			{
				MandatoryValidation.MessageErrorIfNotEntered(info, messagePrefix: NctsConstants.ValidationRuleMessagePrefixes.C0191);
			}
		}
	}
}
