using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDTaxWrapper : IDeclarationDVDTax
	{
		public DeclarationDVDTaxWrapper(ZString baseUnit, ZDecimal baseQuantity, ZDecimal baseAmount)
		{
			BaseUnit = baseUnit;
			BaseQuantity = baseQuantity;
			BaseAmount = baseAmount;
		}

		const string ChargePercentage = "%";

		public ZString BaseUnit { get; }

		public ZDecimal BaseQuantity { get; }

		public ZDecimal BaseAmount { get; }

		public static IReadOnlyCollection<DeclarationDVDTaxWrapper> GetTaxesList(CusEntryLine cusEntryLine)
		{
			var taxes = new List<DeclarationDVDTaxWrapper>();

			taxes.Add(new DeclarationDVDTaxWrapper(ZString.Empty, ZDecimal.Zero, cusEntryLine.CL_CustomsValue));

			cusEntryLine.Fees.Cast<CusEntryLineFee>()
								.Where(lf => lf.CF_MethodOfCalculation != ChargePercentage)
								.ForEach(fee => taxes.Add(new DeclarationDVDTaxWrapper(fee.CF_MethodOfCalculation, fee.CF_BaseValue, ZDecimal.Zero)));

			return taxes.AsReadOnly();
		}
	}
}
