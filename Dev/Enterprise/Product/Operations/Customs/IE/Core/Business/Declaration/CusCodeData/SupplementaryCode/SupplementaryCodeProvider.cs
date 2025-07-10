using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
	{
		public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
		{
			parent = master;
		}
		readonly ISupplementaryCodeSupporter parent;

		public override ZShort NumberOfCodes => parent is JobComInvoiceLine line && line.Declaration is JobDeclaration declaration && declaration.IsExport && !declaration.IsTransitionPeriodAES30 ? (short)97 : base.NumberOfCodes;
	}
}
