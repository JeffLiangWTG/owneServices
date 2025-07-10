
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class CFXJournalReverser
	{
		public void ReverseJournal(InvoicingBase invoiceOrCreditNote)
		{
			ZQuery filterForRelatedCharges = new ZQuery();
			BusinessObjectFactory factory = invoiceOrCreditNote.Factory;
			if (invoiceOrCreditNote.Lines.Count > 0)
			{
				ZGuid[] guids = new ZGuid[invoiceOrCreditNote.Lines.Count];
				for (int i = 0; i < invoiceOrCreditNote.Lines.Count; i++)
				{
					guids[i] = invoiceOrCreditNote.Lines[i].PK;
				}
				filterForRelatedCharges.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AL_ARLine, SQLComparisonOperator.Equal, guids);
				Charge[] chargesForInvoice = (Charge[])factory.Load(typeof(Charge), filterForRelatedCharges);

				if (chargesForInvoice.Length > 0)
				{
					ZQuery cFXLineFilter = new ZQuery(AccTransactionLinesSchema.PK, Array.ConvertAll(chargesForInvoice, charge => charge.JR_AL_CFXLine));
					AccTransactionLinesCollection cFXLines = new AccTransactionLinesCollection(factory, cFXLineFilter);
					cFXLines.Load();

					JCJournalHeader journal = null;
					foreach (AccTransactionLines line in cFXLines)
					{
						journal = factory.Load<JCJournalHeader>(line.AL_AH);
						if (journal != null)
						{
							break;
						}
					}

					if (journal != null)
					{
						bool oKToReverse = true;
						foreach (JCJournalLine cFXLine in journal.Lines)
						{
							bool foundInInnerForeach = false;
							foreach (Charge chargeWithCFX in chargesForInvoice)
							{
								if (cFXLine.PK == chargeWithCFX.JR_AL_CFXLine)
								{
									foundInInnerForeach = true;
									break;
								}
							}
							oKToReverse = foundInInnerForeach && oKToReverse;
						}

						if (oKToReverse)
						{
							ReversingBase transaction  = new ReversingFactory().NewReversing(journal);
							transaction.Reverse();

							JCJournalHeader reverseJournal = transaction.ReverseTransaction as JCJournalHeader;
							InvoicingBase reverseInvoiceOrCreditNote = invoiceOrCreditNote.ReverseTransaction as InvoicingBase;
							if (reverseJournal != null && reverseInvoiceOrCreditNote != null && reverseJournal.AH_PostDate != reverseInvoiceOrCreditNote.AH_PostDate)
							{
								reverseJournal.AH_PostDate = reverseInvoiceOrCreditNote.AH_PostDate;
							}

							foreach (Charge charge1 in chargesForInvoice)
							{
								charge1.JR_AL_CFXLine = ZGuid.Empty;
							}
						}
					}
				}
			}
		}
	}
}
