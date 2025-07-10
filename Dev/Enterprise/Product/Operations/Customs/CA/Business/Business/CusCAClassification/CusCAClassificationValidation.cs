using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAClassificationValidation : AutoCusCAClassificationValidation
	{
		public CusCAClassificationValidation(AutoCusCAClassification parent)
			: base(parent)
		{
		}

		protected new CusCAClassification Parent
		{
			get { return (CusCAClassification)base.Parent; }
		}

		protected CusCAClassificationLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		#region CheckCCA_RN_NKOrigin

		protected override void CheckCCA_RN_NKOrigin()
		{
			base.CheckCCA_RN_NKOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_RN_NKOriginInfo, Parent.Lookups.Origins);
		}

		#endregion

		#region CheckCCA_ProvinceOfOrigin

		protected override void CheckCCA_ProvinceOfOrigin()
		{
			base.CheckCCA_ProvinceOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_ProvinceOfOriginInfo, Parent.Lookups.StatesOfOrigin);
		}

		#endregion

		#region CheckCCA_99TariffCode

		protected override void CheckCCA_99TariffCode()
		{
			base.CheckCCA_99TariffCode();
			new TariffValidator(Parent.Factory).ValidateFourDigitTariff(Parent.CCA_99TariffCodeInfo);
		}

		#endregion

		#region CheckCCA_ValueForDutyCode

		protected override void CheckCCA_ValueForDutyCode()
		{
			base.CheckCCA_ValueForDutyCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_ValueForDutyCodeInfo, Parent.Lookups.ValueForDutyCodes);
		}

		#endregion

		#region CheckCCA_DestinationProvince

		protected override void CheckCCA_DestinationProvince()
		{
			base.CheckCCA_DestinationProvince();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_DestinationProvinceInfo, Parent.Lookups.CanadianProvinces);
		}

		#endregion

		#region CheckCCA_RN_NKCFIAOrigin

		protected override void CheckCCA_RN_NKCFIAOrigin()
		{
			base.CheckCCA_RN_NKCFIAOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_RN_NKCFIAOriginInfo, Parent.Lookups.CFIAOrigins);
		}

		#endregion

		#region CheckCCA_CFIAUSStateOfOrigin

		protected override void CheckCCA_CFIAUSStateOfOrigin()
		{
			base.CheckCCA_CFIAUSStateOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_CFIAUSStateOfOriginInfo, Parent.Lookups.CFIAStatesOfOrigin);
		}

		#endregion

		#region CheckCCA_ImportReasonCode

		protected override void CheckCCA_ImportReasonCode()
		{
			base.CheckCCA_ImportReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_ImportReasonCodeInfo, Parent.Lookups.ImportReasonCodes);
		}

		#endregion

		#region CheckCCA_EndUse

		protected override void CheckCCA_EndUse()
		{
			base.CheckCCA_EndUse();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_EndUseInfo, Parent.Lookups.CFIAEndUseCodes);
		}

		#endregion

		#region CheckCCA_MiscID

		protected override void CheckCCA_MiscID()
		{
			base.CheckCCA_MiscID();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_MiscIDInfo, (ICodeDescriptionPairList)Parent.Lookups.CFIAMiscIDCodes);
		}

		#endregion

		#region CheckCCA_GSTStatusCode

		protected override void CheckCCA_GSTStatusCode()
		{
			base.CheckCCA_GSTStatusCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_GSTStatusCodeInfo, Parent.Lookups.GSTStatusCodes);
		}

		#endregion

		#region CheckCCA_ETExemption

		protected override void CheckCCA_ETExemption()
		{
			base.CheckCCA_ETExemption();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_ETExemptionInfo, Parent.Lookups.ETExemptionCodes);
		}

		#endregion

		#region CheckCCA_ETRateCode

		protected override void CheckCCA_ETRateCode()
		{
			base.CheckCCA_ETRateCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_ETRateCodeInfo, Parent.Lookups.ExciseTaxRateCodes);
		}

		#endregion

		#region CheckCCA_TreatmentCode

		protected override void CheckCCA_TreatmentCode()
		{
			base.CheckCCA_TreatmentCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CCA_TreatmentCodeInfo, Parent.Lookups.TreatmentCodes);
		}

		#endregion

		#region CheckCCA_AuthorityNumber

		protected override void CheckCCA_AuthorityNumber()
		{
			base.CheckCCA_AuthorityNumber();

			if (Parent.Parent is CusClassPartPivot pivot)
			{
				var owners = pivot?.Part?.RelatedOrganisations?.BuyerRelations;
				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Parent.Factory, Parent.CCA_AuthorityNumberInfo, owners?.Select(o => o.OU_OH).ToArray());
			}
			else
			{
				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Parent.Factory, Parent.CCA_AuthorityNumberInfo);
			}
		}

		#endregion
	}
}
