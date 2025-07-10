using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class DeferredPaymentProvider : IDeferredPayment
	{
		public DeferredPaymentProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public string DeferredPayment => declaration.JE_DefermentAccountNumber;

		public string CcQualifier => null;
	}
}
