using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaPackedItemValidation : EU.H7.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(ASYCUDA.Business.AsycudaPackedItem parent)
			: base(parent)
		{
		}

		public new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePackedItemHaveTransportDocument();
			ValidatePackedItemContainsMandatoryCode();
			ValidatePackedItemPreviousDocuments();
		}

		protected override void CheckAPI_GrossWeight()
		{
			base.CheckAPI_GrossWeight();
			MandatoryValidation.MessageErrorIfNotEntered(base.Parent.API_GrossWeightInfo);
		}

		protected override bool CheckTotalIntrinsicValueOfGoodsIsExceed(out string messageOfValueExceed)
		{
			var bill = Parent.Bill;
			var intrinsicValue = bill.SumOfGoodsValue;

			if (bill.AdditionalProcedureContainsC07() && intrinsicValue > 150m)
			{
				messageOfValueExceed = Parent.Header.ValidationConfiguration.ValidationMessage.GetBR600005RuleValuesMustNotExceedEUR150Message();
				return true;
			}

			if (bill.ABL_Procedure == EUH7AdditionalProcedureCodeList.Codes.C08)
			{
				if (intrinsicValue <= 45m)
				{
					intrinsicValue += ConvertToRequiredCurrencyIfNeeded(bill.ABL_TransportValue, bill.ABL_RX_NKTransportValueCurrency, bill.RequiredCurrencyCode);
					intrinsicValue += ConvertToRequiredCurrencyIfNeeded(bill.ABL_InsuranceValue, bill.ABL_RX_NKInsuranceValueCurrency, bill.RequiredCurrencyCode);
				}

				if (intrinsicValue > 45m)
				{
					messageOfValueExceed = Parent.Header.ValidationConfiguration.ValidationMessage.GetBR600005RuleValuesMustNotExceedEUR45Message();
					return true;
				}
			}

			messageOfValueExceed = string.Empty;
			return false;
		}

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			if (IE.Business.Constants.TariffCodes.H7InvalidTariffCodes.Contains(Parent.API_Tariff))
			{
				Parent.API_TariffInfo.AddMessageError(Parent.Header.ValidationConfiguration.ValidationMessage.GetCD0185RuleMessage());
			}
		}

		void ValidatePackedItemPreviousDocuments()
		{
			var errMessage = Parent.ValidationMessage.GetBR2011RuleMessage();
			Parent.RemoveRowMessageError(errMessage);

			if (Parent.Bill.Header.AMA_ApplicationCode == SubmitTypeList.Codes.V1
					&& Parent.Bill.ABL_ShipmentType == SubStyleCodeList.Codes.NormalDeclaration
					&& Parent.PreviousDocuments.IsNullOrEmpty())
			{
				Parent.AddRowMessageError(errMessage);
			}
		}

		void ValidatePackedItemContainsMandatoryCode()
		{
			var br2037Message = Parent.ValidationMessage.GetBR2037RuleMessage();
			var br2032Message = Parent.ValidationMessage.GetBR2032RuleMessage();
			Parent.RemoveRowMessageError(br2037Message);
			Parent.RemoveRowMessageError(br2032Message);

			if (Parent.Bill.ABL_Procedure != EUH7AdditionalProcedureCodeList.Codes.C08)
			{
				var itemSupportingDocCodes = Parent.SupportingDocuments.Select(document => document.CSI_Code);
				if (!itemSupportingDocCodes.Intersect(IE.Business.Constants.SupportingDocumentCodes.H7NonC08MandatoryCodes).Any())
				{
					if (Parent.Bill.Header.AMA_ApplicationCode == SubmitTypeList.Codes.V1)
					{
						Parent.AddRowMessageError(br2037Message);
					}
					else if (Parent.Bill.Header.AMA_ApplicationCode == SubmitTypeList.Codes.V2)
					{
						Parent.AddRowMessageError(br2032Message);
					}
				}
			}
		}

		void ValidatePackedItemHaveTransportDocument()
		{
			var errMessage = Parent.ValidationMessage.GetC0634RuleMessage();
			Parent.RemoveRowMessageError(errMessage);

			if (!Parent.BillHasTransportDocument && !Parent.AdditionalDocumentsContainTransportDocument)
			{
				Parent.AddRowMessageError(errMessage);
			}
		}

		ZDecimal ConvertToRequiredCurrencyIfNeeded(ZDecimal sourceValue, ZString sourceCurrencyCode, ZString destinationCurrencyCode)
		{
			if (sourceCurrencyCode != destinationCurrencyCode)
			{
				var sourceCurrency = RefCurrency.LoadFromCurrencyCode(Parent.Factory, sourceCurrencyCode);
				var destinationCurrency = RefCurrency.LoadFromCurrencyCode(Parent.Factory, destinationCurrencyCode);
				var converter = CurrencyConverter.New(Parent.Factory, ZDateTime.Today, ExchangeRateType.Customs, roundToTargetCurrencyDecimals: true);
				var convertedTransportValue = converter.ConvertRounded(new Money(sourceValue, sourceCurrency), destinationCurrency);

				return convertedTransportValue.Amount;
			}
			else
			{
				return sourceValue;
			}
		}

		protected override string TariffListValidationErrorMessage => Parent.Header.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage.ToString();
	}
}
