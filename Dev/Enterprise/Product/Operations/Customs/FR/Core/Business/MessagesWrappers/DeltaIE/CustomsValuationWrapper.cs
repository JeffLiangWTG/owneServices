using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CustomsValuationWrapper : ICustomsValuation
	{
		CustomsValuationWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public static CustomsValuationWrapper New(CusEntryLine entryLine) => entryLine == null ? null : new CustomsValuationWrapper(entryLine);

		public ICollection<IAdditionsAndDeductions> AdditionsAndDeductions => additionsAndDeductions ?? (additionsAndDeductions = GetAdditionsAndDeductions());
		ICollection<IAdditionsAndDeductions> additionsAndDeductions;

		ICollection<IAdditionsAndDeductions> GetAdditionsAndDeductions()
		{
			var result = new Collection<IAdditionsAndDeductions>();
			foreach (CusEntryLineCalculatedFee charge in entryLine.CusEntryLineCalculatedFees)
			{
				if (charge.IsLineLevel && !charge.CustomsCode.IsEmpty)
				{
					result.Add(AdditionsAndDeductionWrapper.New(charge.CustomsCode, charge.Amount, charge.Currency));
				}
			}
			return result;
		}

		public string ValuationMethod => valuationMethod ?? (valuationMethod = entryLine.RandomLine.JI_ValuationCode);
		string valuationMethod;
	}
}
