using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class MercosulCertificateProvider : IMercosulCertificate
	{
		MercosulCertificateProvider(MercosulForeignDeclaration mercosulForeignDeclaration)
		{
			this.mercosulForeignDeclaration = Argument.NotNull(mercosulForeignDeclaration, nameof(mercosulForeignDeclaration));
		}
		readonly MercosulForeignDeclaration mercosulForeignDeclaration;

		public static MercosulCertificateProvider New(MercosulForeignDeclaration mercosulForeignDeclaration) => mercosulForeignDeclaration == null ? null : new MercosulCertificateProvider(mercosulForeignDeclaration);

		public string Type => mercosulForeignDeclaration.CSI_SubType;

		public string Number => mercosulForeignDeclaration.CSI_Code;

		public string Quantity => mercosulForeignDeclaration.CSI_Quantity3.ToString();
	}
}
