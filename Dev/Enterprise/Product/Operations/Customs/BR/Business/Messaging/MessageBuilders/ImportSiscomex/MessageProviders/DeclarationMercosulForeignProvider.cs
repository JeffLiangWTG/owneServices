using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationMercosulForeignProvider : IDeclarationMercosulForeign
	{
		public DeclarationMercosulForeignProvider(MercosulForeignDeclaration mercosulForeign)
		{
			this.mercosulForeign = Argument.NotNull(mercosulForeign, nameof(mercosulForeign));
		}

		public static DeclarationMercosulForeignProvider New(MercosulForeignDeclaration mercosulForeign) => mercosulForeign == null ? null : new DeclarationMercosulForeignProvider(mercosulForeign);

		readonly MercosulForeignDeclaration mercosulForeign;

		public string DeclarationMercosulForeignNumber => mercosulForeign.CSI_Description;

		public string InicialNumber => mercosulForeign.CSI_ReferenceNumber;

		public string FinalNumber => mercosulForeign.CSI_ReferenceNumber2;
	}
}

