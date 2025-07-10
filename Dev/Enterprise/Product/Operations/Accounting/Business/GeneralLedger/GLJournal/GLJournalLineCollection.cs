using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalLineCollection : DependentTransactionLineCollection
	{
		public GLJournalLineCollection(GLJournal @object, ZQuery query)
			: base(@object, query)
		{
		}

		public GLJournalLineCollection(GLJournal @object)
			: this(@object, new ZQuery())
		{
		}

		public new GLJournalLine this[int index]
		{
			get { return (GLJournalLine)Elements[index]; }
		}

		public new GLJournalLine AddNew()
		{
			return (GLJournalLine)base.AddNew();
		}

		public void SetPostDateForAllLines()
		{
			foreach (GLJournalLine line in this)
			{
				if (MasterGLJournal != null)
				{
					line.AL_PostDate = MasterGLJournal.AH_PostDate;
				}
			}
		}

		public void SetReverseDateForAllLines()
		{
			foreach (GLJournalLine line in this)
			{
				if (MasterGLJournal != null)
				{
					line.AL_ReverseDate = MasterGLJournal.AH_DueDate;
				}
			}
		}

		public void SetLineTypeForAllLines()
		{
			foreach (GLJournalLine line in this)
			{
				if (MasterGLJournal != null)
				{
					line.AL_LineType = MasterGLJournal.AH_TransactionType;
				}
			}
		}

		bool CheckIsSetBalancing()
		{
			return Count > 0 && !((IBusinessObjectCollectionInternals)this).IsListChangedSuspended && !MasterGLJournal.IsNoteJournal;
		}

		#region Implementation

		protected override bool AllowSort
		{
			get { return false; }
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (bizO.IsInDatabase)
			{
				MasterGLJournal.AH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
		}

		protected override void SetDefaultsForNewChildCore(BusinessObject child)
		{
			base.SetDefaultsForNewChildCore(child);

			ZDecimal newLineAmount = 0m;
			var newLine = child as GLJournalLine;

			if (CheckIsSetBalancing())
			{
				var previousItem = GetSecondLastItemInCollection();
				var signedJournalBalance = JournalBalance;
				newLine.UnsignedOSLineAmount = Math.Abs(signedJournalBalance);

				if (signedJournalBalance > 0)
				{
					newLine.DebitCreditSign = nameof(DebitCredit.CR);
				}
				else
				{
					newLine.DebitCreditSign = nameof(DebitCredit.DR);
				}

				newLine.AL_GB = previousItem.AL_GB;
				newLine.AL_GE = previousItem.AL_GE;
				newLine.AL_Desc = previousItem.AL_Desc;
				newLineAmount = newLine.AL_OSExTaxAmount;
			}
			else
			{
				newLine.AL_Desc = MasterGLJournal.AH_Desc;
			}

			if (!MasterGLJournal.IsHeaderAmountsUpdateSuspended)
			{
				MasterGLJournal.UpdateAH_OSExTaxAmount(newLineAmount);
			}
		}

		protected ZDecimal JournalBalance
		{
			get { return MasterGLJournal.AH_OSExTaxAmount; }
		}

		protected GLJournal MasterGLJournal
		{
			get { return (GLJournal)Master; }
		}

		protected GLJournalLine GetSecondLastItemInCollection()
		{
			GLJournalLine result = null;
			int secondLastItemIndex = Count - 1;

			if (secondLastItemIndex >= 0)
			{
				result = this[secondLastItemIndex];
			}

			return result;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, MasterGLJournal.AH_GC);
			return query;
		}

		protected override void SetDefaultExchangeRate(DependentTransactionLine line)
		{
			//do nothing - don't set ex rate from header
		}

		#endregion
	}
}
