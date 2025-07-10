using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAImportDeclaredTaxWrapper : IDUAImportDeclaredTax
	{
		public DUAImportDeclaredTaxWrapper(CusEntryLineFee cusEntryLineFee, ZBool isCanary)
		{
			lineFee = Argument.NotNull(cusEntryLineFee, nameof(cusEntryLineFee));
			isCanaryBool = isCanary;
		}
		readonly CusEntryLineFee lineFee;
		readonly ZBool isCanaryBool;
		const string ChargePercentage = "%";
		const string ChargeCalculationMethod = "PVP";

		public ZString TaxClass => lineFee.GetTaxClass(isCanaryBool);

		public ZDecimal TaxableIncome => lineFee.CF_BaseValue;

		public ZDecimal TaxRate => lineFee.CF_Rate;

		public ZString MaxMinIndicator => lineFee.MaxMin;

		public ZString FiscalUnit
		{
			get
			{
				if (fiscalUnit == null)
				{
					fiscalUnit = new CachedProperty<ZString>(lineFee.Factory, () =>
					{
						var calculationMethod = lineFee.CF_MethodOfCalculation;
						if (calculationMethod.IsEmpty || calculationMethod == ChargePercentage || calculationMethod == ChargeCalculationMethod)
						{
							return ChargePercentage;
						}
						else
						{
							return calculationMethod.ConvertCargoWiseToES(lineFee.Factory);
						}
					});
				}
				return fiscalUnit.Value;
			}
		}
		CachedProperty<ZString> fiscalUnit;

		public ZDecimal Fee => lineFee.CF_ChargeAmount;
	}
}
