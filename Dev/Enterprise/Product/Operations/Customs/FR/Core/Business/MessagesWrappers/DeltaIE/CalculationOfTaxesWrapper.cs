using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CalculationOfTaxesWrapper : ICalculationOfTaxes
	{
		CalculationOfTaxesWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			this.customsOffice = entryLine.Declaration?.JE_CustomsOffice ?? string.Empty;
		}

		readonly CusEntryLine entryLine;
		readonly string customsOffice;

		public static CalculationOfTaxesWrapper New(CusEntryLine entryLine) => entryLine == null ? null : new CalculationOfTaxesWrapper(entryLine);

		public ICollection<IDutiesAndTaxes> DutiesAndTaxe => dutiesAndTaxe ?? (dutiesAndTaxe = GetDutiesAndTaxeCollection());
		ICollection<IDutiesAndTaxes> GetDutiesAndTaxeCollection()
		{
			var result = new List<IDutiesAndTaxes>();

			entryLine.Fees.Cast<CusEntryLineFee>().ForEach(f => result.Add(DutiesAndTaxesWrapper.New(f, customsOffice)));

			if (entryLine.CL_LineNumber == 1)
			{
				foreach (var charge in entryLine.Header.Charges)
				{
					result.Add(DutiesAndTaxesWrapper.New(charge, customsOffice));
				}
			}

			return result.Any() ? result : null;
		}
		ICollection<IDutiesAndTaxes> dutiesAndTaxe;

		public string Preference => preference ?? (preference = entryLine.PreferenceCode);
		string preference;

		public double TotalDutiesAndTaxesAmount => entryLine.Fees.Count > 0 || entryLine.Header.Charges.Count > 0 ? DutiesAndTaxe.Sum(fee => fee.PayableTaxAmount) : 0d;
	}
}
