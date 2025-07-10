
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JCJournalLinesCollection : DependentTransactionLineCollection
	{
		public JCJournalLinesCollection(BusinessObject parent, ZQuery query)
			: base(parent, query)
		{
		}

		new public JCJournalLine this[int index]
		{
			get { return (JCJournalLine)Elements[index]; }
		}

		public new JCJournalLine AddNew()
		{
			return (JCJournalLine)base.AddNew();
		}
	}
}
