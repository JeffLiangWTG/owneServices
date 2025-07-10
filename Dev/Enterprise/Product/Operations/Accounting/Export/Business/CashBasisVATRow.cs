using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Export.Business
{
	public class CashBasisVATRow
	{
		public Guid LinePK { get; set; }
		public ZDateTime PostDate { get; set; }
		public ZDecimal TaxAmount { get; set; }

		public override string ToString()
		{
			return string.Format((NoResString)"LinePK={0} PostDate={1} TaxAmount={2}",
						LinePK, PostDate, TaxAmount);
		}
	}
}
