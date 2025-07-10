using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBatchHeaderValidation : TransactionHeaderValidation
	{
		public InvoiceBatchHeaderValidation(InvoiceBatchHeader parent)
			: base(parent)
		{
		}

		protected new InvoiceBatchHeader Parent
		{
			get { return (InvoiceBatchHeader)base.Parent; }
		}

		#region AH_OSTotal

		protected override void CheckAH_OSTotal()
		{
			base.CheckAH_OSTotal();
			if (Parent.Line.SelectedCount == 0 && !Parent.AH_IsCancelled)
			{
				Parent.AH_OSTotalInfo.AddError(CheckAH_OSTotalErrMsg);
			}
		}

		static string CheckAH_OSTotalErrMsg
		{
			get { return Res.GetString("c6e7b4f1-7b53-4639-8b3c-2929eed74f23", "Invoice Batch amount cannot be 0. Please select transactions in order to generate Invoice Batch."); }
		}

		#endregion

		#region AH_OH

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();
			if (Parent.AH_OH.IsEmpty)
			{
				Parent.AH_OHInfo.AddError(Res.GetString("a2dccf50-135c-4353-9014-1b5cc26d81b4", "Invoice Batch function is restricted to a single debtor. Please enter an organization."));
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
			}
			if (SelectedJobTypesHaveDifferentLayouts)
			{
				Parent.AH_OHInfo.AddError(Res.GetString("77cb0bfd-2dee-408a-b43a-743dd8db0ec8", "This debtor has different layout setup information for the selected job types. Review the 'Invoice Batching' configuration for this debtor"));
			}
		}

		#endregion

		#region AH_RX_NKTransactionCurrency

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			if (Parent.AH_RX_NKTransactionCurrency.IsEmpty)
			{
				Parent.AH_RX_NKTransactionCurrencyInfo.AddError(Res.GetString("5f3fedd1-b56b-4340-a7fe-1a44e17f2711", "Invoice Batch function is restricted to a single currency. Please enter a value."));
			}
		}

		#endregion

		#region Job Type

		bool SelectedJobTypesHaveDifferentLayouts
		{
			get
			{
				bool selectedJobTypesHaveDifferentLayouts = false;
				if (Parent.Header != null && Parent.SelectedJobTypeCodes.Count > 1 && Parent.Header.CompanyData.InvoiceTypes.Count > 1)
				{
					List<ZString> jobTypes =  new List<ZString>();
					Parent.SelectedJobTypeCodes.ForEach(x => jobTypes.AddRange(PeriodicInvoiceModuleDecider.GetJobTypesByInvoiceModule(x)));
					var invoiceTypes = Parent.Header.CompanyData.GetApplicableInvoiceTypes(ZString.Empty, ZString.Empty, ZString.Empty, true, jobTypes.ToArray());
					if (invoiceTypes != null && invoiceTypes.Count > 1)
					{
						foreach (OrgInvoiceType invoiceType in invoiceTypes.Values)
						{
							if (invoiceType.PI_Type != invoiceTypes.Values.First().PI_Type)
							{
								selectedJobTypesHaveDifferentLayouts = true;
								break;
							}
						}
					}
				}
				return selectedJobTypesHaveDifferentLayouts;
			}
		}

		//void EnsureLinesAreNotAlreadyBatched()
		//{

		//    Dictionary<string, List<InvoicingBase>> BatchedTransactions = new Dictionary<string, List<InvoicingBase>>(); 
		//    foreach (InvoicingBase Line in Parent.Line)
		//    {
		//        if (Line.IsInDatabase)
		//        {
		//            Line.Reload();
		//        }
		//        if (!Line.AH_AH_InvoiceStatement.IsEmpty)
		//        {
		//            InvoiceBatchHeader Batch = Parent.Factory.Load<InvoiceBatchHeader>(Line.AH_AH_InvoiceStatement);
		//            if(!BatchedTransactions.ContainsKey(Batch.AH_TransactionNum))
		//            {
		//                BatchedTransactions.Add(Batch.AH_TransactionNum, new List<InvoicingBase>());
		//            }
		//            BatchedTransactions[Batch.AH_TransactionNum].Add(Line);
		//        }
		//    }

		//    if (BatchedTransactions.Count > 0)
		//    {
		//        List<string> BatchMessages = new List<string>();
		//        bool MultipleInvoices = BatchedTransactions.Count > 1;
		//        foreach (string Batch in BatchedTransactions.Keys)
		//        {
		//            List<InvoicingBase> Invoices = BatchedTransactions[Batch];
		//            string InvoicesInBatch = "";
		//            for(int x = 0;x < Invoices.Count; x++)
		//            {
		//                InvoicingBase Line  = Invoices[x];
		//                Line.AddRowError("This invoice is already part of batch " +  Batch + ".\r\nRedo search for invoces to batch or exclude this invoice from the batch");
		//                InvoicesInBatch += Line.AH_TransactionNum + (Invoices.Count==1? "" : (x==Invoices.Count-2 ? " and " : ", "));
		//            }
		//            BatchMessages.Add((Invoices.Count > 1 ? "Invoices " : "Invoice ") + InvoicesInBatch.TrimEnd(", ".ToCharArray()) + (Invoices.Count > 1 ? " are " : " is ") + "already part of batch " + Batch + ".");
		//            MultipleInvoices = MultipleInvoices || Invoices.Count > 1;
		//        }

		//        string ParentMessage = "";
		//        foreach(string BatchMessage in BatchMessages)
		//        {
		//            ParentMessage += BatchMessage + "\r\n";
		//        }
		//        ParentMessage += "Redo search for invoices to batch or exclude " + (MultipleInvoices ? "these invoices" : "this invoice") + " from the batch.";
		//        Parent.AddRowError(ParentMessage);
		//    }

		//}

		#endregion

		protected override void CheckAH_InvoiceTerm()
		{
			base.CheckAH_InvoiceTerm();

			string error = Parent.TermsAndDueDateCalculationProvider.CanInvoiceTermBeSelectedError;
			if (!string.IsNullOrEmpty(error))
			{
				Parent.AH_InvoiceTermInfo.AddError(error);
			}
		}
	}
}
