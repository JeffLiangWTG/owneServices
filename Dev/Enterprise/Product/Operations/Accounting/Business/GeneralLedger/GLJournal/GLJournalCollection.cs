using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalCollection : AccTransactionHeaderCollection
	{
		public GLJournalCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public GLJournalCollection(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public new GLJournal this[int index] => (GLJournal)Elements[index];

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(GLJournal);
		}

		public new GLJournal AddNew()
		{
			return (GLJournal)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		#region Implementation

		protected override ZQuery CreateRelationshipFilter()
		{
			var transactionTypes = new[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLAutoJournal, TransactionTypes.GLNoteJournal };

			ZQuery result = new ZQuery();
			result.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.General);
			result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionTypes);
			return result;
		}

		#endregion
	}
}