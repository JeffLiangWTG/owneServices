using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillValidationForMasterChild : AsycudaBillValidation
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
		}

		protected AsycudaManifestHeader Header => Parent.Header;

		protected override void CheckABL_BillNumber()
		{
			CheckABL_BillNumberCore();
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();

			var parent = Parent;
			if (!parent.ABL_GrossWeight.IsEmpty && parent.ABL_GrossWeight < Header.TotalHouseBillsGrossWeight)
			{
				parent.ABL_GrossWeightInfo.AddMessageError(Res.GetString("BE33119E-9BBB-4261-AF27-16A7D09D2BEF", "Total Gross weight is less than the gross weight entered under bills."));
			}
		}

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();

			var parent = Parent;
			if (!parent.ABL_ManifestQty.IsEmpty && parent.ABL_ManifestQty < Header.TotalHouseBillsPackages)
			{
				parent.ABL_ManifestQtyInfo.AddMessageError(Res.GetString("89F71023-9976-44C5-A1D5-1B54FFE3B6D5", "Total Packages are less than the packages entered under bills."));
			}
		}

		protected virtual void CheckABL_BillNumberCore()
		{
			var header = Header;
			if (header != null)
			{
				var info = Parent.ABL_BillNumberInfo;
				if (Parent.ABL_BillNumber.IsEmpty && header.IsConsolidator)
				{
					info.AddMessageError(ValidationConstants.ManifestNumberIsRequired(header.MasterBillLabel.Caption));
				}
			}
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();
			ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_BillIssueDateInfo, ManifestValidationRuleCodes.BillIssueDate);
		}

		protected override void CheckABL_E_ARV()
		{
			base.CheckABL_E_ARV();
			CheckMandatoryABL_E_ARV();
		}

		protected virtual void CheckMandatoryABL_E_ARV()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_ARVInfo, Res.GetString("f57f836c-893a-4432-b160-bd717f0b291a", "Estimated Time of Arrival at Border"));
		}

		protected override void CheckABL_E_DEP()
		{
			base.CheckABL_E_DEP();
			if (IsABL_E_DEPRequired)
			{
				CheckMandatoryABL_E_DEP();
			}
		}

		protected virtual bool IsABL_E_DEPRequired => true;

		protected virtual void CheckMandatoryABL_E_DEP()
		{
			var header = Header;
			if (header != null)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(Parent.ABL_E_DEPInfo, ManifestValidationRuleCodes.EstimatedDepartureTime, header.AMA_TransportMode);
			}
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();
			var portInfo = Parent.ABL_RL_NKPortOfDischargeInfo;
			if (ShouldCheckHasAsycudaCountry)
			{
				CheckHasAsycudaCountry(portInfo);
			}
			if (!portInfo.HasErrors())
			{
				if (Parent.ABL_RL_NKPortOfDischarge.IsEmpty)
				{
					var messageError = ZZValidationHeaderHelper.IsMandatoryWhenTransportModeMatches(ManifestValidationRuleCodes.PortOfDischarge, Header.AMA_TransportMode);
					if (!messageError.IsEmpty)
					{
						portInfo.AddMessageError(messageError);
					}
					else if (IsPortOfDischargesRequired)
					{
						portInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(portInfo)));
					}
				}
				else
				{
					CheckValidUNLOCOCode(portInfo, Parent.PortOfDischarge, ManifestValidationRuleCodes.IATAPortOfDischarge);
				}
			}

			ValidateABL_RL_NKPortOfLoading();
		}

		protected virtual bool IsPortOfDischargesRequired => true;

		protected virtual bool ShouldCheckHasAsycudaCountry => true;

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			base.CheckABL_RL_NKPortOfLoading();
			var portInfo = Parent.ABL_RL_NKPortOfLoadingInfo;
			if (ShouldCheckHasAsycudaCountry)
			{
				CheckHasAsycudaCountry(portInfo);
			}
			if (!portInfo.HasErrors())
			{
				if (IsPortOfLoadingRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(portInfo);
				}
				CheckValidUNLOCOCode(portInfo, Parent.PortOfLoading, ManifestValidationRuleCodes.IATAPortOfLoading);
			}

			ValidateABL_RL_NKPortOfDischarge();
		}

		protected virtual bool IsPortOfLoadingRequired => true;

		void CheckValidUNLOCOCode(ZPropertyInfo portCodeInfo, RefUNLOCO refUNLOCO, string validationRule)
		{
			ListValidation.ErrorIfInvalidCode(portCodeInfo);
			if (refUNLOCO != null)
			{
				var header = Header;
				if (header.IsRoad && !refUNLOCO.RL_HasRoad ||
					header.IsAir && !refUNLOCO.RL_HasAirport ||
					header.IsSea && !refUNLOCO.RL_HasSeaport ||
					header.IsMail && !refUNLOCO.RL_HasPost)
				{
					portCodeInfo.AddWarning(string.Format(CultureInfo.InvariantCulture, "Selected {0} cannot be used for {1} transport", portCodeInfo.HumanReadableName, header.AMA_TransportMode));
				}

				if (Parent.IsAir && refUNLOCO.RL_IATA.IsEmpty)
				{
					ZZValidationHeaderHelper.CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(portCodeInfo, validationRule);
				}
			}
		}

		protected bool NoSupportedManifestCountryCode => CountryHelper.GetPrimaryCountryCode(Parent.ABL_RL_NKPortOfDischarge, Parent.ABL_RL_NKPortOfLoading, Header.Consol, Parent.Factory).IsEmpty;

		void CheckHasAsycudaCountry(ZPropertyInfo zPropertyInfo)
		{
			if (NoSupportedManifestCountryCode)
			{
				zPropertyInfo.AddError(ValidationConstants.ManifestMustGoThruSupportedCountries);
			}
		}
		protected override void CheckABL_CustomsDischargePort()
		{
			var header = Header;
			if (header.FeatureProvider?.SupportsCustomsPorts(header) ?? false)
			{
				ValidationHelper.CheckCustomsPort(Parent.ABL_CustomsDischargePortInfo, Parent.ABL_RL_NKPortOfDischargeInfo, Parent.PortOfDischarge, header);
			}
		}

		protected override void CheckABL_CustomsLoadPort()
		{
			var header = Header;
			if (header.FeatureProvider?.SupportsCustomsPorts(header) ?? false)
			{
				ValidationHelper.CheckCustomsPort(Parent.ABL_CustomsLoadPortInfo, Parent.ABL_RL_NKPortOfLoadingInfo, Parent.PortOfLoading, header);
			}
		}
	}
}
