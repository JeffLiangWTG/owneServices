using Enterprise.Customs.EU.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class EntryFeePaymentPartyUnderstander : EU.Business.Declaration.EntryFeePaymentPartyUnderstander
	{
		public EntryFeePaymentPartyUnderstander(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		public override bool ShouldBrokerPayThisFee(string feeCode, string methodOfPayment, ILogger logger)
		{
			var response = false;
			var logText = (NoResString)" is never paid by broker - excluded from rating";

			var declarationMethodOfPayment = Declaration.JE_PaymentMethod;
			if (declarationMethodOfPayment.IsEmpty || declarationMethodOfPayment == DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14)
			{
				response = true;
				logText = (NoResString)" is always paid by broker - included in rating";
			}

			var mopText = declarationMethodOfPayment.IsEmpty ? (NoResString)"empty" : declarationMethodOfPayment.ToString();
			logger?.Log(LogType.Information, (NoResString)"Fee " + feeCode + (NoResString)" with MoP=" + methodOfPayment + (NoResString)" with deferral payment party " + mopText + logText);
			return response;
		}
	}
}
