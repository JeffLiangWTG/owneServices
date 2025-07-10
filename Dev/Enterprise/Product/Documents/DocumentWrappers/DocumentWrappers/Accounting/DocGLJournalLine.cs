using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.DocumentWrappers
{
	public class DocGLJournalLine : DocTransactionLine
	{
		DocGLJournalLine(TransactionLine transactionLine, BusinessObjectFactory factoryToWrap)
			: base(transactionLine, factoryToWrap)
		{
		}

		public new static DocTransactionLine New(TransactionLine transactionLine, BusinessObjectFactory factoryToWrap)
		{
			return (transactionLine != null) ? new DocGLJournalLine(transactionLine, factoryToWrap) : null;
		}

		public override ZDecimal ExchangeRate
		{
			get { return TransactionLine.AL_ExchangeRate; }
		}
	}
}
