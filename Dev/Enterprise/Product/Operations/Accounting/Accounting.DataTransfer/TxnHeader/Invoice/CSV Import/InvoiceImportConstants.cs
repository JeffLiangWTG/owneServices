namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public static class DataTransferInvoiceConstants
	{
		public static class InvHeaderPos
		{
			public const int RowType			        = 0;
			public const int Ledger				        = 1;
			public const int OrganisationCode	        = 2;
			public const int OrganisationName	        = 3;
			public const int TxnType			        = 4;
			public const int TxnNumber			        = 5;
			public const int TxnDescription		        = 6;
			public const int InvoiceDate		        = 7;
			public const int DueDate			        = 8;
			public const int Currency			        = 9;
			public const int InvoiceTotal		        = 10;
			public const int DefaultBranch		        = 11;
			public const int DefaultDepartment	        = 12;
			public const int PaymentReference	        = 13;
			public const int IsDisbursement             = 14;
			public const int PostDate					= 15;
			public const int OverrideSystemExchangeRate = 16;
		}

		public static class InvLinePos
		{
			public const int RowType 			= 0;
			public const int ChargeCode			= 1;
			public const int GLAccount			= 2;
			public const int Description		= 3;
			public const int Branch				= 4;
			public const int Department			= 5;
			public const int InvoiceAmtExclTax	= 6;
			public const int InvoiceAmtIncTax	= 7;
			public const int TaxCode			= 8;
			public const int TaxAmount			= 9;
			public const int IsFinalCharge		= 10;
			public const int LocalInvoiceAmtExclTax = 11;
			public const int SubAccountType		= 12;
			public const int SubAccountCode		= 13;
		}

		public static class InvoiceLineSubAccountPos
		{
			public const int RowType = 0;
			public const int SubAccountType = 1;
			public const int SubAccountCode = 2;
		}

		public static class JobInfoPos
		{
			public const int RowType			 = 0;
			public const int JobType			 = 1;
			public const int JobNumber			 = 2;
			public const int Reference1			 = 3;
			public const int Reference2			 = 4;
			public const int ApportionmentMethod = 5;
		}

		public static class AttachmentPos
		{
			public const int RowType = 0;
			public const int FileName = 1;
			public const int FilePath = 2;
			public const int DocumentType = 3;
		}

		public static class RowTypes
		{
			public const string JobInformation	= "JOBINFO";
			public const string InvoiceHeader	= "INVHEAD";
			public const string InvoiceLine		= "INVLINE";
			public const string InvoiceLineSubAccount = "INVLINESUBACCOUNT";
			public const string Attachment		= "ATTACHMENT";
		}
	}
}