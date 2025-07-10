using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using UniversalReferenceConstants = Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaBillValidationForRegularBill : EU.H7.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override IMultilingualString InvalidCodeErrorMessage => Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateABL_ShipmentType();
			ValidateHasAtLeastOnePack();
			AddMessageErrorForMissingAdditionalDocuments();
			AddMessageErrorForMissingPreviousDocuments();
			AddMessageErrorForMissingSupportingDocuments();
		}

		void ValidateHasAtLeastOnePack()
		{
			if (Parent.Packs.Count == 0)
			{
				Parent.AddRowError(Res.GetString("d6c23962-7ff8-4021-b7ed-c6a9126a5d19", "At least one Pack is required."));
			}
		}

		void AddMessageErrorForMissingAdditionalDocuments()
		{
			if (Parent.Header.IsLV2)
			{
				ValidateRequiredTypeForTransportDocument();
				Validate1D24TypeRequiredForAdditionalReference();
			}

			ValidateRequiredTypeForAdditionalReference();
		}

		public void ValidateRequiredTypeForTransportDocument()
		{
			if (!Parent.AdditionalDocuments.Any(a => a.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument) && AdditionalDocumentValidation.ItemsDoNotHaveAValidTransportDocument(Parent))
			{
				Parent.AddRowMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR1106RuleMessage());
			}
		}

		void Validate1D24TypeRequiredForAdditionalReference()
		{
			if (!Parent.AdditionalDocuments.Any(a => a.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference))
			{
				Parent.AddRowMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR20319Rule1D24REFRequiredMessage());
			}
		}

		void ValidateRequiredTypeForAdditionalReference()
		{
			if (Parent.ABL_Procedure == EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49 && !Parent.AdditionalDocuments.Any(a => a.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference))
			{
				Parent.AddRowMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR600008RuleMessage());
			}
		}

		void AddMessageErrorForMissingPreviousDocuments()
		{
			if (Parent.Header.IsLV1 && Parent.ABL_ShipmentType == SubStyleCodeList.Codes.NormalDeclaration && !Parent.PreviousDocuments.Any())
			{
				Parent.AddRowMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR2011RuleMessage());
			}

			if (Parent.Header.IsLV2 && !Parent.PreviousDocuments.Any())
			{
				Parent.AddRowMessageError(Parent.ValidationConfiguration.ValidationMessage.GetC0614RuleMessage());
			}
		}

		void AddMessageErrorForMissingSupportingDocuments()
		{
			if (!Parent.SupportingDocuments.Any())
			{
				SupportingDocumentValidationHelper.ValidateRequiredCodesForProcedureNonC08(Parent, Parent.AddRowMessageError);
				if (Parent.Header.IsLV1)
				{
					SupportingDocumentValidationHelper.Validate1D24CodeRequired(Parent, Parent.AddRowMessageError);
				}
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			if (Parent.Header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.EuH7V1)
			{
				if (Parent.ABL_ConsigneeRegNo.IsEmpty &&
					(Parent.ABL_ConsigneeName.IsEmpty
					|| (Parent.ABL_ConsigneeStreet1.IsEmpty && Parent.ABL_ConsigneeStreet2.IsEmpty)
					|| Parent.ABL_ConsigneePostcode.IsEmpty
					|| Parent.ABL_ConsigneeCity.IsEmpty
					|| Parent.ABL_RN_NKConsigneeCountry.IsEmpty))
				{
					Parent.ABL_ConsigneeRegNoInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetC0617RuleImporterIdentificationNumberMessage());
				}

				var importerIDTypes = new List<string>
				{
					ImporterIdentificationTypes.Codes.PYE,
					ImporterIdentificationTypes.Codes.ITX,
					ImporterIdentificationTypes.Codes.CGT,
				};

				if (importerIDTypes.Contains(Parent.ABL_ConsigneeRegNoType))
				{
					if (Parent.ABL_ConsigneeRegNo.Length > 9 || !Parent.ABL_ConsigneeRegNo.IsLettersAndNumbersOnlyOrEmpty)
					{
						switch (Parent.ABL_ConsigneeRegNoType)
						{
							case ImporterIdentificationTypes.Codes.PYE:
								Parent.ABL_ConsigneeRegNoInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR3181RulePYEFormatMessage());
								break;

							case ImporterIdentificationTypes.Codes.ITX:
								Parent.ABL_ConsigneeRegNoInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR3181RuleITXFormatMessage());
								break;

							case ImporterIdentificationTypes.Codes.CGT:
								Parent.ABL_ConsigneeRegNoInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR3181RuleCGTFormatMessage());
								break;
						}
					}
				}
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			ValidateABL_ConsigneeRegNo();
		}

		protected override void CheckABL_ConsigneeStreet1()
		{
			ValidateABL_ConsigneeRegNo();
		}

		protected override void CheckABL_ConsigneeStreet2()
		{
			ValidateABL_ConsigneeRegNo();
		}

		protected override void CheckABL_ConsigneeCity()
		{
			ValidateABL_ConsigneeRegNo();
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			ValidateABL_ConsigneeRegNo();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKConsigneeCountryInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
		}

		protected override void CheckABL_ConsigneePostcode()
		{
			ValidateABL_ConsigneeRegNo();
		}

		protected override void CheckABL_Procedure()
		{
			base.CheckABL_Procedure();

			if (Parent.ABL_Procedure.IsEmpty)
			{
				Parent.ABL_ProcedureInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR600002RuleMessage());
			}

			if (!Parent.ABL_Procedure.IsEmpty
				&& !Parent.ABL_Procedure.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.F49)
				&& Parent.AdditionalDocuments != null
				&& Parent.AdditionalDocuments.Any(v => v.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
					&& v.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference
					&& v.CSI_Code == IE.Business.Constants.AdditionalReferenceCodes._1A06))
			{
				Parent.ABL_ProcedureInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR600014RuleMessage());
			}

			if (!Parent.ABL_Procedure.IsEmpty && !Parent.ABL_Procedure.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.F48) && !Parent.ABL_SellerRegNo.IsEmpty)
			{
				Parent.ABL_ProcedureInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR600009RuleMessage());
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_ProcedureInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
		}

		protected override void CheckABL_ShipmentType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ABL_ShipmentTypeInfo);
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			ValidateABL_ShipperRegNo();
		}

		protected override void CheckABL_ShipperPostcode()
		{
			base.CheckABL_ShipperPostcode();
			ValidateABL_ShipperRegNo();
		}

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();
			ValidateABL_ShipperRegNo();
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKShipperCountryInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
			ValidateABL_ShipperRegNo();
			if (Parent.Header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.EuH7V1)
			{
				if (Parent.ABL_RN_NKShipperCountry.IsEmpty && (!Parent.ABL_ShipperRegNo.IsEmpty || !Parent.ABL_ShipperName.IsEmpty))
				{
					Parent.ABL_RN_NKShipperCountryInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetC0617RuleExporterCountryMessage());
				}
			}
		}

		protected override void CheckABL_ShipperStreet1()
		{
			base.CheckABL_ShipperStreet1();
			CheckABL_ShipperStreets(Parent.ABL_ShipperStreet1Info);
			ValidateABL_ShipperRegNo();
		}

		protected override void CheckABL_ShipperStreet2()
		{
			base.CheckABL_ShipperStreet2();
			CheckABL_ShipperStreets(Parent.ABL_ShipperStreet2Info);
			ValidateABL_ShipperRegNo();
		}

		void CheckABL_ShipperStreets(ZPropertyInfo propertyInfo)
		{
			if (Parent.Header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.EuH7V1)
			{
				if (Parent.ABL_ShipperStreet1.IsEmpty && Parent.ABL_ShipperStreet2.IsEmpty && !Parent.ABL_ShipperName.IsEmpty)
				{
					propertyInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetC0617RuleExporterStreetMessage());
				}
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();

			if (Parent.Header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.EuH7V2)
			{
				if (Parent.ABL_ShipperRegNo.IsEmpty
					&& (Parent.ABL_ShipperName.IsEmpty
					|| Parent.ABL_ShipperPostcode.IsEmpty
					|| Parent.ABL_ShipperCity.IsEmpty
					|| Parent.ABL_RN_NKShipperCountry.IsEmpty
					|| (Parent.ABL_ShipperStreet1.IsEmpty && Parent.ABL_ShipperStreet2.IsEmpty)))
				{
					Parent.ABL_ShipperRegNoInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR3006RuleMessage());
				}
			}
		}

		protected override void CheckABL_RX_NKTransportValueCurrency()
		{
			base.CheckABL_RX_NKTransportValueCurrency();
			var transportCurrencyInfo = Parent.ABL_RX_NKTransportValueCurrencyInfo;

			if (!Parent.ABL_TransportValue.IsEmpty)
			{
				MandatoryValidation.CheckEntered(transportCurrencyInfo);
			}

			if (!string.Equals(Parent.ABL_RX_NKTransportValueCurrency, Parent.ABL_RX_NKInsuranceValueCurrency,
					StringComparison.OrdinalIgnoreCase))
			{
				ValidateABL_RX_NKInsuranceValueCurrency();
			}
		}

		protected override void CheckABL_RX_NKInsuranceValueCurrency()
		{
			base.CheckABL_RX_NKInsuranceValueCurrency();
			var info = Parent.ABL_RX_NKInsuranceValueCurrencyInfo;

			if (!Parent.ABL_InsuranceValue.IsEmpty)
			{
				MandatoryValidation.CheckEntered(info);
			}

			if (!string.Equals(Parent.ABL_RX_NKTransportValueCurrency, Parent.ABL_RX_NKInsuranceValueCurrency,
					StringComparison.OrdinalIgnoreCase))
			{
				string warning = Res.GetString("4a3aebd3-8494-4c13-8967-e96d5674c71b",
					"Insurance Value Currency is different from Transport Value Currency which will be converted upon save.");
				Parent.ABL_RX_NKInsuranceValueCurrencyInfo.AddWarning(warning);
			}
		}

		protected override void CheckABL_TransportValue()
		{
			base.CheckABL_TransportValue();

			if (Parent.ABL_Procedure.Equals(UniversalReferenceConstants.ProcedureCodes.Concession.C08) && IsTotalAmountExceedsLimit())
			{
				Parent.ABL_TransportValueInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR600005RuleValuesMustNotExceedEUR45Message());
			}
		}

		protected override void CheckABL_InsuranceValue()
		{
			base.CheckABL_InsuranceValue();

			if (Parent.ABL_Procedure.Equals(UniversalReferenceConstants.ProcedureCodes.Concession.C08) && IsTotalAmountExceedsLimit())
			{
				Parent.ABL_InsuranceValueInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR600005RuleValuesMustNotExceedEUR45Message());
			}
		}

		bool IsTotalAmountExceedsLimit()
		{
			var totalAmount = Parent.SumOfGoodsValue;
			var targetCurrency = new Currency(Parent.RequiredCurrencyCode);

			if (Parent.ABL_TransportValue != 0 && !Parent.ABL_RX_NKTransportValueCurrency.IsEmpty)
			{
				var transportMoney = new Money(Parent.ABL_TransportValue, new Currency(Parent.ABL_RX_NKTransportValueCurrency));
				var convertedTransportMoney = Parent.Header.CurrencyConverter.ConvertRounded(transportMoney, targetCurrency);
				totalAmount += convertedTransportMoney.Amount;
			}

			if (Parent.ABL_InsuranceValue != 0 && !Parent.ABL_RX_NKInsuranceValueCurrency.IsEmpty)
			{
				var insuranceMoney = new Money(Parent.ABL_InsuranceValue, new Currency(Parent.ABL_RX_NKInsuranceValueCurrency));
				var convertedInsuranceMoney = Parent.Header.CurrencyConverter.ConvertRounded(insuranceMoney, targetCurrency);
				totalAmount += convertedInsuranceMoney.Amount;
			}

			return totalAmount > 45;
		}
	}
}
