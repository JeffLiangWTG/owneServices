using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;
using CustomsAidaXml = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class FeeWrapper : CustomsAidaXml.IFee
{
	public FeeWrapper(CusEntryLineFee entryLineFee)
	{
		this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
	}

	readonly CusEntryLineFee entryLineFee;

	string CustomsAidaXml.IFee.ChargeType => entryLineFee.CF_ChargeType;

	string CustomsAidaXml.IFee.MethodOfPayment => entryLineFee.CF_MethodOfPayment;

	decimal? CustomsAidaXml.IFee.BaseValue
	{
		get
		{
			if (IsPercentageMethodOfCalculation)
			{
				return entryLineFee.CF_BaseValue;
			}

			return null;
		}
	}

	decimal CustomsAidaXml.IFee.Rate => entryLineFee.CF_Rate;

	decimal CustomsAidaXml.IFee.ChargeAmount => entryLineFee.CF_ChargeAmount;

	decimal? CustomsAidaXml.IFee.Quantity
	{
		get
		{
			if (IsPercentageMethodOfCalculation)
			{
				return null;
			}

			return entryLineFee.CF_BaseValue;
		}
	}

	string CustomsAidaXml.IFee.UnitOfMeasure
	{
		get
		{
			if (IsPercentageMethodOfCalculation)
			{
				return null;
			}

			return entryLineFee.CF_MethodOfCalculation;
		}
	}

	bool IsPercentageMethodOfCalculation => entryLineFee.CF_MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
}
