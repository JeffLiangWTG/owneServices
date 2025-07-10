using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class ProcedureAttributeBasedMethodOfPaymentCalculator : ILineFeeMethodOfPaymentDefaultValueCalculator
{
	ProcedureAttributeBasedMethodOfPaymentCalculator(CusEntryLineFee lineFee)
	{
		this.lineFee = lineFee;
	}

	readonly CusEntryLineFee lineFee;

	public static ILineFeeMethodOfPaymentDefaultValueCalculator GetNew(CusEntryLineFee lineFee)
	{
		Argument.NotNull(lineFee, nameof(lineFee));

		var declaration = lineFee.EntryLine?.Declaration;
		return (declaration?.IsImport ?? false) && IsApplicableChargeType(lineFee)
			? new ProcedureAttributeBasedMethodOfPaymentCalculator(lineFee)
			: new EmptyMethodOfPaymentCalculator();
	}

	class EmptyMethodOfPaymentCalculator : ILineFeeMethodOfPaymentDefaultValueCalculator
	{
		ZString ILineFeeMethodOfPaymentDefaultValueCalculator.GetDefaultValue() => ZString.Empty;
	}

	ZString ILineFeeMethodOfPaymentDefaultValueCalculator.GetDefaultValue()
	{
		var randomLine = lineFee.EntryLine?.RandomLine;
		var procedure = randomLine?.CusProcedure;

		if (procedure is null)
		{
			return ZString.Empty;
		}

		var chargeType = lineFee.CF_ChargeType;
		var procedureCode = procedure.ZZ6_ProcedureCode;
		var previousProcedureCode = procedure.ZZ6_PreviousProcedureCode;
		var factory = procedure.Factory;
		return factory.GetCachedValue($"ILineFeeMethodOfPaymentDefaultValueCalculator_{procedureCode}_{previousProcedureCode}_{chargeType}",
					() => GetAttributeValueForChargeTypeAndProcedure(factory, procedureCode, previousProcedureCode, chargeType));
	}

	static bool IsApplicableChargeType(CusEntryLineFee lineFee)
	{
		return !lineFee.CF_ChargeType.IsEmpty
			&& !lineFee.IsTemporaryAntiDumping
			&& !lineFee.IsTemporaryCountervailing
			&& !lineFee.IsCarTax
			&& !lineFee.IsMiscellaneousContingentRevenueConcerningTax
			&& !lineFee.IsRecoveryOfCourtCostsTax
			&& !lineFee.IsPortTax;
	}

	static ZString GetAttributeValueForChargeTypeAndProcedure(BusinessObjectFactory factory, ZString procedureCode, ZString previousProcedureCode, ZString chargeType)
	{
		var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Italy);
		query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, MessageTypeList.Codes.Import);
		query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);

		var attributeName = UniversalReferenceConstants.RefCusProcedureAttributeNames.OtherFeePaymentMethod;
		if (IsDutyPaymentMethodApplicable(chargeType))
		{
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, ZString.Empty);
			attributeName = UniversalReferenceConstants.RefCusProcedureAttributeNames.DutyPaymentMethod;
		}
		else if (IsVatPaymentMethodApplicable(chargeType))
		{
			attributeName = UniversalReferenceConstants.RefCusProcedureAttributeNames.VatPaymentMethod;
		}

		foreach (var cusProcedure in factory.Load<RefCusProcedure>(query))
		{
			var attributeValue = cusProcedure.Attributes
				.Where(x => x.ZXB_Name == attributeName)
				.Select(x => x.ZXB_Value)
				.FirstOrDefault();

			if (!attributeValue.IsEmpty)
			{
				return attributeValue;
			}
		}
		return ZString.Empty;
	}

	static bool IsVatPaymentMethodApplicable(ZString chargeType)
	{
		return chargeType.StartsWith(UniversalReferenceConstants.RefCusRateCodes.VatChargeTypeStartingCodeB)
			|| chargeType.StartsWith(UniversalReferenceConstants.RefCusRateCodes.VatChargeTypeStartingCode4);
	}

	static bool IsDutyPaymentMethodApplicable(ZString chargeType)
	{
		return chargeType.StartsWith(UniversalReferenceConstants.RefCusRateCodes.DutyChargeTypeStartingCode)
			|| chargeType.StartsWith(UniversalReferenceConstants.RefCusRateCodes.DutyForSanMarinoChargeTypeStartingCode);
	}
}
