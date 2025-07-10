using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransaction : AutoNettingPayableTransaction, INettingTransaction
	{
		public NettingPayableTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid NettingSystemPK
		{
			get { return NPT_NS_NettingSystem; }
			set { NPT_NS_NettingSystem = value; }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NPT_NSP_Period; }
			set { NPT_NSP_Period = value; }
		}

		public ZGuid IssuerPK
		{
			get { return NPT_NSO_Issuer; }
			set { NPT_NSO_Issuer = value; }
		}

		[RelatedBusinessObject("Issuer")]
		public override ZGuid NPT_NSO_Issuer
		{
			get { return base.NPT_NSO_Issuer; }
			set { base.NPT_NSO_Issuer = value; }
		}

		public NettingOrganisation Issuer
		{
			get { return Factory.Load<NettingOrganisation>(NPT_NSO_Issuer); }
		}

		public ZGuid RecipientPK
		{
			get { return NPT_NSO_Recipient; }
			set { NPT_NSO_Recipient = value; }
		}

		[RelatedBusinessObject("Recipient")]
		public override ZGuid NPT_NSO_Recipient
		{
			get { return base.NPT_NSO_Recipient; }
			set { base.NPT_NSO_Recipient = value; }
		}

		public NettingOrganisation Recipient
		{
			get { return Factory.Load<NettingOrganisation>(NPT_NSO_Recipient); }
		}

		public ZString Currency
		{
			get { return NPT_RX_NKInvoiceCurrency; }
			set { NPT_RX_NKInvoiceCurrency = value; }
		}

		public ZDecimal Amount
		{
			get { return NPT_Amount; }
			set { NPT_Amount = value; }
		}

		public ZString Reference
		{
			get { return NPT_Reference; }
			set { NPT_Reference = value; }
		}

		public ZGuid OriginalTransaction
		{
			get { return NPT_NPT_OriginalTransaction; }
			set { NPT_NPT_OriginalTransaction = value; }
		}

		public ZDateTime Date
		{
			get { return NPT_Date; }
			set { NPT_Date = value; }
		}

		public ZDateTime DueDate
		{
			get { return NPT_DueDate; }
			set { NPT_DueDate = value; }
		}

		public ZString ApprovalStatus
		{
			get { return NPT_ApprovalStatus; }
			set { NPT_ApprovalStatus = value; }
		}

		public ZString TransactionType
		{
			get { return NPT_TransactionType; }
			set { NPT_TransactionType = value; }
		}

		[ChildEditable(true)]
		public NettingPayableLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new NettingPayableLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		NettingPayableLineCollection lines;

		[ChildEditable(true)]
		public NettingPayableTransactionReferenceCollection References
		{
			get
			{
				if (references == null)
				{
					references = new NettingPayableTransactionReferenceCollection(this);
					RegisterEditableChildObject(references);
				}
				return references;
			}
		}
		NettingPayableTransactionReferenceCollection references;

		public INettingTransactionLine AddNewLine()
		{
			return Lines.AddNew();
		}

		public INettingTransactionReference AddNewTransactionReference()
		{
			return References.AddNew();
		}

		public void DeleteLines()
		{
			Lines.DeleteAll();
		}

		public void DeleteTransactionReferences()
		{
			References.DeleteAll();
		}

		public override void Delete()
		{
			References.DeleteAll();
			Lines.DeleteAll();

			base.Delete();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NPT_Amount = 1;
			NPT_ApprovalStatus = "ADD";
			NPT_Reference = "Something is better than nothing!";
			NPT_RX_NKInvoiceCurrency = "XXX";
			NPT_SystemCreateUser = "~BP";
			NPT_TransactionType = "INV";
		}
#endif

	}
}
