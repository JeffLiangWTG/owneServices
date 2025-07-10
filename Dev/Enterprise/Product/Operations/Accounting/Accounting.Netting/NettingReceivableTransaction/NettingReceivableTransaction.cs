using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Netting
{
	[UniversalDataContext(DataContextType.NettingTransaction)]
	public class NettingReceivableTransaction : AutoNettingReceivableTransaction, INettingTransaction
	{
		public NettingReceivableTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public ZGuid NettingSystemPK
		{
			get { return NRT_NS_NettingSystem; }
			set { NRT_NS_NettingSystem = value; }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NRT_NSP_Period; }
			set { NRT_NSP_Period = value; }
		}

		public ZGuid IssuerPK
		{
			get { return NRT_NSO_Issuer; }
			set { NRT_NSO_Issuer = value; }
		}

		[RelatedBusinessObject("Issuer")]
		public override ZGuid NRT_NSO_Issuer
		{
			get { return base.NRT_NSO_Issuer; }
			set { base.NRT_NSO_Issuer = value; }
		}

		public NettingOrganisation Issuer
		{
			get { return Factory.Load<NettingOrganisation>(NRT_NSO_Issuer); }
		}

		public ZGuid RecipientPK
		{
			get { return NRT_NSO_Recipient; }
			set { NRT_NSO_Recipient = value; }
		}

		[RelatedBusinessObject("Recipient")]
		public override ZGuid NRT_NSO_Recipient
		{
			get { return base.NRT_NSO_Recipient; }
			set { base.NRT_NSO_Recipient = value; }
		}

		public NettingOrganisation Recipient
		{
			get { return Factory.Load<NettingOrganisation>(NRT_NSO_Recipient); }
		}

		public ZString Currency
		{
			get { return NRT_RX_NKInvoiceCurrency; }
			set { NRT_RX_NKInvoiceCurrency = value; }
		}

		public ZDecimal Amount
		{
			get { return NRT_Amount; }
			set { NRT_Amount = value; }
		}

		public ZString Reference
		{
			get { return NRT_Reference; }
			set { NRT_Reference = value; }
		}

		public ZGuid OriginalTransaction
		{
			get { return NRT_NRT_OriginalTransaction; }
			set { NRT_NRT_OriginalTransaction = value; }
		}

		public ZDateTime Date
		{
			get { return NRT_Date; }
			set { NRT_Date = value; }
		}

		public ZDateTime DueDate
		{
			get { return NRT_DueDate; }
			set { NRT_DueDate = value; }
		}

		public ZString ApprovalStatus
		{
			get { return NRT_ApprovalStatus; }
			set { NRT_ApprovalStatus = value; }
		}

		public ZString TransactionType
		{
			get { return NRT_TransactionType; }
			set { NRT_TransactionType = value; }
		}

		[ChildEditable(true)]
		public NettingReceivableLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new NettingReceivableLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		NettingReceivableLineCollection lines;

		[ChildEditable(true)]
		public NettingReceivableTransactionReferenceCollection References
		{
			get
			{
				if (references == null)
				{
					references = new NettingReceivableTransactionReferenceCollection(this);
					RegisterEditableChildObject(references);
				}
				return references;
			}
		}
		NettingReceivableTransactionReferenceCollection references;

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

			NRT_Amount = 1;
			NRT_ApprovalStatus = "ADD";
			NRT_Reference = "Something is better than nothing!";
			NRT_RX_NKInvoiceCurrency = "XXX";
			NRT_SystemCreateUser = "~BP";
			NRT_TransactionType = "INV";
		}
#endif

	}
}
