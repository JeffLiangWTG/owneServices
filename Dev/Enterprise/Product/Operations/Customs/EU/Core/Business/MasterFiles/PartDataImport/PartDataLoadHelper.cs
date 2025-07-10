using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public static class PartDataLoadHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
		public static void SetSupplementCodes(CusClassPartPivot cusClassPartPivot, ZString[] supplementCodes)
		{
			var provider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(cusClassPartPivot);
			var codes = supplementCodes.Where(x => !x.IsEmpty).ToArray();
			var length = Math.Min(codes.Length, provider.NumberOfCodes + provider.CodesStartingOrder - 1);

			short order = 1;
			var supplementaryCodeLoader = new SupplementaryCode.Loader(cusClassPartPivot.Factory);
			for (var i = 0; i < length; i++)
			{
				var supplementaryCode = supplementaryCodeLoader.LoadOrCreate<SupplementaryCode, CusClassPartPivot>(cusClassPartPivot, order++);
				supplementaryCode.CY_Code = codes[i].Left(Customs.EU.Business.SupplementaryCode.Schema.CY_CodeMaxLength);
			}
		}
	}
}
