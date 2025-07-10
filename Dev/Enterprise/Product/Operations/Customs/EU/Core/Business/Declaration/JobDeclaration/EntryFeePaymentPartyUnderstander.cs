using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EntryFeePaymentPartyUnderstander
	{
		public EntryFeePaymentPartyUnderstander(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration");
			this.Declaration = declaration;
		}

		protected JobDeclaration Declaration
		{
			get;
			set;
		}

		public virtual bool ShouldBrokerPayThisFee(string feeCode, string methodOfPayment, ILogger logger)
		{
			return true;
		}
	}
}
