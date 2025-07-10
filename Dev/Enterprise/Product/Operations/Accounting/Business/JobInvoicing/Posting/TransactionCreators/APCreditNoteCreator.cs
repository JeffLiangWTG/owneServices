using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Only used for AP Credit Notes.
	/// </summary>
	public class APCreditNoteCreator
	{
		public APCreditNoteCreator(Job job)
		{
			this.Factory = job.Factory;
			Jobs = new[] { job };
		}

		public APCreditNoteCreator(IEnumerable<Job> jobs, BusinessObjectFactory fallbackFactory)
		{
			this.Factory = jobs.Any() ? jobs.First().Factory : fallbackFactory;
			this.Jobs = jobs;
		}

		public virtual bool CreateTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			foreach (string key in GetKeys(transactions))
			{
				TransactionHeader value = transactions[key];

				if (ShouldCreateAPCreditNote(value))
				{
					APInvoice invoice = (APInvoice)value;
					APCreditNote creditNote = CreateAPCreditNote(transactions, invoice, key);
					RemoveAndDeleteInvoice(transactions, key, invoice);

					result = true;
				}
			}

			return result;
		}

		#region Implementation

		readonly BusinessObjectFactory Factory;
		readonly IEnumerable<Job> Jobs;

		/// <summary>
		/// Returns collection of keys existed before creating credit notes
		/// As we create CreditNotes, original collection Transactions.Keys will be modified 
		/// so we need a snapshot of it before any change is made
		/// </summary>
		/// <param name="transactions">Hashtable that contains transactions</param>
		/// <returns>List of currently contained keys</returns>
		object[] GetKeys(TransactionCreatorHashtable transactions)
		{
			ArrayList keys = new ArrayList();
			foreach (object key in transactions.Keys)
			{
				keys.Add(key);
			}
			return (object[])keys.ToArray(typeof(object));
		}

		bool ShouldCreateAPCreditNote(object value)
		{
			return value is APInvoice && ((APInvoice)value).AH_InvoiceAmount > 0;
		}

		APCreditNote CreateAPCreditNote(TransactionCreatorHashtable transactions, APInvoice invoice, string key)
		{
			APCreditNote creditNote = (APCreditNote)CreateCreditNote(invoice, typeof(APCreditNote), typeof(APCreditNoteLine), JobChargeSchema.JR_AL_APLine);
			AddAPCreditNote(transactions, key, creditNote);
			return creditNote;
		}

		void AddAPCreditNote(TransactionCreatorHashtable transactions, string key, APCreditNote creditNote)
		{
			CreditNoteKey cNKey = GetCreditNoteKey(key);
			transactions.AddAPCreditNote(creditNote, cNKey.ClientCode, cNKey.JobDescription);
		}
		
		public struct CreditNoteKey
		{
			public string ClientCode;
			public string JobDescription;

			public CreditNoteKey(string clientCode, string jobDescription)
			{
				this.ClientCode = clientCode;
				this.JobDescription = jobDescription;
			}
		}

		CreditNoteKey GetCreditNoteKey(string key)
		{
			string[] keyComponents = key.Split(new char[] { ':' }, 3);
			string clientCode = keyComponents[1];
			string jobDescription = (keyComponents.Length > 2) ? keyComponents[2] : "";
			return new CreditNoteKey(clientCode, jobDescription);
		}

		CreditNote CreateCreditNote(Invoice invoice, Type creditNoteType, Type creditNoteLineType, SchemaColumn chargeLinkColumn)
		{
			CreditNote creditNote = (CreditNote)Factory.New(creditNoteType);

#if DEBUG
			if (Globals.IsTest && NewCreditNoteAction_ForTestOnly != null)
			{
				NewCreditNoteAction_ForTestOnly(creditNote);
			}
#endif

			creditNote.CopyValuesFrom(invoice);
			SetCreditNoteValues(creditNote, invoice);

			JobConsolCost[] costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, invoice.PK));
			foreach (JobConsolCost cost in costs)
			{
				cost.E6_AH_APInvoice = creditNote.PK;
			}

			using (creditNote.Lines.SuspendListChanged())
			{
				foreach (InvoiceLine invoiceLine in invoice.Lines)
				{
					CreditNoteLine creditNoteLine = (CreditNoteLine)Factory.New(creditNoteLineType);
					creditNoteLine.CopyValuesFrom(invoiceLine);
					SetCreditNoteLineValues(creditNoteLine, invoiceLine);
					ResetChargeLink(chargeLinkColumn, invoiceLine, creditNoteLine);
					creditNote.Lines.Add(creditNoteLine);
				}
			}
			return creditNote;
		}

#if DEBUG
		internal Action<InvoicingBase> NewCreditNoteAction_ForTestOnly;
#endif

		void RemoveAndDeleteInvoice(TransactionCreatorHashtable transactions, string key, Invoice invoice)
		{
			transactions.Remove(key);
			using (invoice.Lines.SuspendListChanged())
			{
				for (int i = invoice.Lines.Count - 1; i >= 0; i--)
				{
					invoice.Lines.RemoveAndDelete(invoice.Lines[i]);
				}
			}
			//HACK: need to refactor the whole thing about creating credit notes
			//we should make a decision about whether it's an invoice or a credit note in the beginning, not 
			//at the latest stage and then delete the Invoice business object like it does now.
			//Delete on Invoice wouldn't work because of the parent class, so we need to load a very basic object and delete it. A full-on hack.
			MasterFiles.Business.AccTransactionHeader header = Factory.Load<MasterFiles.Business.AccTransactionHeader>(invoice.PK);
			if (header != null)
			{
				header.Delete();
			}
		}

		void ResetChargeLink(SchemaColumn chargeLinkColumn, InvoiceLine invoiceLine, CreditNoteLine creditNoteLine)
		{
			Charge linkedCharge = FindLinkedCharge(chargeLinkColumn, invoiceLine);
			if (linkedCharge != null)
			{
				if (chargeLinkColumn == JobChargeSchema.JR_AL_ARLine)
				{
					linkedCharge.ClearRevenueLink();
				}
				else if (chargeLinkColumn == JobChargeSchema.JR_AL_APLine)
				{
					linkedCharge.ClearCostLink();
				}

				linkedCharge[chargeLinkColumn.Name] = creditNoteLine.PK;
			}
		}

		Charge FindLinkedCharge(SchemaColumn chargeLinkColumn, InvoiceLine invoiceLine)
		{
			ZQuery filter = new ZQuery(chargeLinkColumn, invoiceLine.PK);
			foreach (Job job in Jobs)
			{
				if (!job.IsWorkOnHold)
				{
					BusinessObject[] charge = job.Charges.Find(filter);
					if (charge.Length == 1)
					{
						return (Charge)charge[0];
					}
				}
			}
			return null;
		}

		void SetCreditNoteValues(CreditNote creditNote, Invoice invoice)
		{
			creditNote.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			creditNote.AH_InvoiceDate = invoice.AH_InvoiceDate;
			creditNote.AH_DueDate = invoice.AH_DueDate;
			creditNote.SetDefaultComplianceSubTypeBasedOnTransactionTypeAndCountry();
		}

		void SetCreditNoteLineValues(CreditNoteLine creditNoteLine, InvoiceLine invoiceLine)
		{
			creditNoteLine.AL_OSExTaxAmount = -invoiceLine.AL_OSExTaxAmount;
			creditNoteLine.AL_OSTaxAmount = -invoiceLine.AL_OSTaxAmount;
			creditNoteLine.AL_OSWHTAmount = -invoiceLine.AL_OSWHTAmount;
			creditNoteLine.AL_OverseasTotal = -invoiceLine.AL_OverseasTotal;
			creditNoteLine.AL_A9_VATClass = invoiceLine.AL_A9_VATClass;

			((AccountingSuspenders.IRunMethodSuspending)creditNoteLine).RunMethodSuspended = true;
			try
			{
				creditNoteLine.AL_LocalExTaxAmount = -invoiceLine.AL_LocalExTaxAmount;
				creditNoteLine.AL_LocalTaxAmount = -invoiceLine.AL_LocalTaxAmount;
				creditNoteLine.AL_LocalWHTAmount = -invoiceLine.AL_LocalWHTAmount;
			}
			finally
			{
				((AccountingSuspenders.IRunMethodSuspending)creditNoteLine).RunMethodSuspended = false;
			}
		}

		#endregion
	}
}
