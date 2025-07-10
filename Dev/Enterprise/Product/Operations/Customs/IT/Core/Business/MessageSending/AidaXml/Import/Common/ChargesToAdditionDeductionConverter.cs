using System;
using System.Collections.Generic;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class ChargesToAdditionDeductionConverter
{
	public IReadOnlyCollection<AdditionDeductionCodeBucket> GetAdditionsAndDeductions(IReadOnlyCollection<IInvoiceLineChargeWrapper> lineCharges)
	{
		Argument.NotNull(lineCharges, nameof(lineCharges));

		var additionDeductionCodeBags = new Dictionary<string, AdditionDeductionCodeBucket>();
		var codeMapper = new ChargeCodeToAdditionDeductionCodeMapper();

		foreach (var invoiceLineCharge in lineCharges)
		{
			if (IsApplicableForAddition(invoiceLineCharge))
			{
				MapAndAddToDictionary(invoiceLineCharge,
					mapperFunction: codeMapper.MapChargeCodeToCodeOfTypeAddition);
			}
			else if (IsApplicableForDeduction(invoiceLineCharge))
			{
				MapAndAddToDictionary(invoiceLineCharge,
					mapperFunction: codeMapper.MapChargeCodeToCodeOfTypeDeduction);
			}
		}

		return additionDeductionCodeBags.Values.ToCollection();

		void MapAndAddToDictionary(IInvoiceLineChargeWrapper invoiceLineCharge, Func<string, string> mapperFunction)
		{
			var mappedCode = mapperFunction(invoiceLineCharge.ChargeCode);
			if (!additionDeductionCodeBags.ContainsKey(mappedCode))
			{
				var codeBucket = new AdditionDeductionCodeBucket(mappedCode);
				additionDeductionCodeBags[mappedCode] = codeBucket;
			}

			additionDeductionCodeBags[mappedCode].AddCharge(invoiceLineCharge);
		}
	}

	bool IsApplicableForAddition(IInvoiceLineChargeWrapper invoiceLineCharge)
		=> invoiceLineCharge.IsDutiable && !invoiceLineCharge.IsIncludedInLine;

	bool IsApplicableForDeduction(IInvoiceLineChargeWrapper invoiceLineCharge)
	{
		var isNonDutiableAndIncludedInInvLine = !invoiceLineCharge.IsDutiable && invoiceLineCharge.IsIncludedInLine;
		return isNonDutiableAndIncludedInInvLine || IsAnExceptionForDeduction(invoiceLineCharge);
	}

	bool IsAnExceptionForDeduction(IInvoiceLineChargeWrapper invoiceLineCharge)
		=> string.Equals(invoiceLineCharge.ChargeCode, UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge, StringComparison.OrdinalIgnoreCase);
}
