using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class WriteOffTransactionCreator : ES.Business.WriteOffTransactionCreator
	{
		public ZString AddGuaranteesWriteOffTransactionsNcts(NctsHeader departureHeader, ZDateTime admissionDate, LoggingInformation logger = null)
		{
			var hasPositiveBalance = false;
			var hasCreatedTransaction = false;
			var guarantees = departureHeader.GetEffectiveGuarantees();

			foreach (NctsGuarantee guarantee in guarantees)
			{
				var reference = guarantee.PW_BondNumber;

				(hasPositiveBalance, hasCreatedTransaction) = AddGuaranteeWriteOffTransaction(departureHeader.Factory, departureHeader.MovementReferenceNumber, departureHeader.LocalReferenceNumber, true, EUGuaranteeTypeList.Codes.TRA,
																								reference, departureHeader.CountryCode, admissionDate, hasPositiveBalance, hasCreatedTransaction, logger);
			}

			return hasPositiveBalance ? NotWrittenOffText : (hasCreatedTransaction ? WrittenOffText : ExcludedText);
		}
	}
}
