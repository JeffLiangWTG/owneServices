using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class ForeignExportDeclarationProvider : IForeignExportDeclaration
	{
		ForeignExportDeclarationProvider(MercosulForeignDeclaration mercosulForeign)
		{
			this.mercosulForeign = Argument.NotNull(mercosulForeign, nameof(mercosulForeign));
		}
		readonly MercosulForeignDeclaration mercosulForeign;

		public static ForeignExportDeclarationProvider New(MercosulForeignDeclaration mercosulForeign) => mercosulForeign == null ? null : new ForeignExportDeclarationProvider(mercosulForeign);

		public string DeclarationNumber => mercosulForeign.CSI_Description;

		public string InitialRangeNumber => mercosulForeign.CSI_ReferenceNumber;

		public string FinalRangeNumber => mercosulForeign.CSI_ReferenceNumber2;
	}
}
