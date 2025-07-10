using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBatchHeaderPrintTask
	{
		public InvoiceBatchHeaderPrintTask(ZGuid pK)
			: this(FilterForPK(pK))
		{
		}

		ZGuid DocBuilderInvoiceMenuPK
		{
			get { return new ZGuid("F91EB783-869C-4A26-8181-689B03405CDF"); }
		}

		public InvoiceBatchHeaderPrintTask(ZQuery filter)
		{
			Task = new PrintTask();
			Task.DeliveryInstructionsDefaultPK = DocBuilderInvoiceMenuPK;

			if (!filter.IsEmpty)
			{
				DocumentPacks = new Dictionary<Guid, DocumentPack>();
				AddInvoicesToPack(filter);

				foreach (DocumentPack pack in DocumentPacks.Values)
				{
					Task.Add(pack);
				}
			}
		}

		public void Run()
		{
			if (Task.Count > 0)
			{
				Task.Run(Env.Security.None);
			}
		}

		public ZString MenuName
		{
			get { return (NoResString)"Invoice Statement Document"; }
		}

		public int TaskCount
		{
			get { return Task.Count; }
		}

		#region Add Invoices

		void AddInvoicesToPack(ZQuery filter)
		{
			foreach (InvoiceBatchHeader transaction in Transactions(filter))
			{
				AddInvoiceToPack(transaction, MenuName);
			}
		}

		void AddInvoiceToPack(InvoiceBatchHeader invoiceBatch, ZString nameOfMenu)
		{
			ZGuid orgForInvoice = invoiceBatch.AH_OH;
			if (DocumentPacks.ContainsKey(orgForInvoice.ToGuid()))
			{
				DocumentPack pack = RetrievePack(orgForInvoice);
				if (pack != null)
				{
					pack.AddReportsToPack(GetInvoicePrintCommand(invoiceBatch, nameOfMenu), null, invoiceBatch, null);
				}
			}
			else
			{
				DocumentPacks.Add(orgForInvoice.ToGuid(), CreatePack(invoiceBatch, nameOfMenu));
			}
		}

		DocumentPack RetrievePack(ZGuid orgForInvoice)
		{
			DocumentPack result = null;
			if (orgForInvoice.IsValid)
			{
				result = DocumentPacks[orgForInvoice.ToGuid()];
			}
			return result;
		}

		DocumentPack CreatePack(InvoiceBatchHeader invoiceBatch, ZString nameOfMenu)
		{
			return new DocumentPack(GetInvoicePrintCommand(invoiceBatch, nameOfMenu), invoiceBatch, null, null);
		}

		DocumentCommand GetInvoicePrintCommand(InvoiceBatchHeader invoiceBatch, ZString nameOfMenu)
		{
			ZQuery invoiceFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, nameOfMenu);
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(invoiceBatch);
			documentCommands.Load();
			BusinessObject[] printInvoiceCommands = documentCommands.Find(invoiceFilter);

			if (printInvoiceCommands.Length < 1)
			{
				throw new ApplicationException("Unable to find Invoice document command for invoice batch header " + invoiceBatch.AH_TransactionNum);
			}

			return (DocumentCommand)printInvoiceCommands[0];
		}

		static TransactionHeaderCollection Transactions(ZQuery filter)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(TransactionHeader));
			query.AddToFilter(filter, JoinCondition.And);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbBranch), AccTransactionHeaderSchema.AH_GB);
			subQuery.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			TransactionHeaderCollection result = new TransactionHeaderCollection(new BusinessObjectFactory(), query);
			result.Load();

			return result;
		}

		readonly PrintTask Task;
		readonly Dictionary<Guid, DocumentPack> DocumentPacks;

		static ZQuery FilterForPK(ZGuid pK)
		{
			return new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, pK);
		}

		#endregion
	}
}
