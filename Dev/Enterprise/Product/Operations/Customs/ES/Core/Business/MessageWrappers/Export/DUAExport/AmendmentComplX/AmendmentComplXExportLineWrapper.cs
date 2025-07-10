using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AmendmentComplXExportLineWrapper : DUAExportLineWrapper
	{
		public AmendmentComplXExportLineWrapper(CusEntryLine cusEntryLine) : base(cusEntryLine) { }

		const string ConcessionCode9VA = "9VA";
		const string ConcessionCode9PV = "9PV";

		protected override ZString GetConcessionCodeFromProcedure()
		{
			var concession = base.GetConcessionCodeFromProcedure();
			if (concession != ConcessionCode9VA && concession != ConcessionCode9PV)
			{
				return concession;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override IEnumerable<EU.Business.AdditionalProcedureCode> GetAdditionalProcedureCodes() => randomLine.AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>()
																																				.Where(x => x.CY_Code.Right(3) != ConcessionCode9VA
																																							&& x.CY_Code.Right(3) != ConcessionCode9PV);
	}
}
