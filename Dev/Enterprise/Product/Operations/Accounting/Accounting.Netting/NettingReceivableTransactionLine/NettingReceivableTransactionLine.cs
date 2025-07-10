using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableTransactionLine : AutoNettingReceivableTransactionLine, INettingTransactionLine
	{
		public NettingReceivableTransactionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid NettingTransactionPK
		{
			get { return NRL_NRT_Transaction; }
		}

		[RelatedBusinessObject("ReceivableTransaction")]
		public override ZGuid NRL_NRT_Transaction
		{
			get { return base.NRL_NRT_Transaction; }
			set { base.NRL_NRT_Transaction = value; }
		}

		public NettingReceivableTransaction ReceivableTransaction
		{
			get { return Factory.Load<NettingReceivableTransaction>(NRL_NRT_Transaction); }
		}

		public ZString JobReference
		{
			get { return NRL_PrimaryJobReference; }
			set { NRL_PrimaryJobReference = value; }
		}

		public ZDecimal Amount
		{
			get { return NRL_Amount; }
			set { NRL_Amount = value; }
		}

		public ZString TransactionCurrency
		{
			get { return NRL_RX_NKCurrency; }
			set { NRL_RX_NKCurrency = value; }
		}

		public ZBool IsApproved
		{
			get { return NRL_IsApproved; }
			set { NRL_IsApproved = value; }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NRL_NSP_Period; }
		}

		[ChildEditable(true)]
		public NettingReceivableLineReferenceCollection References
		{
			get
			{
				if (references == null)
				{
					references = new NettingReceivableLineReferenceCollection(this);
					RegisterEditableChildObject(references);
				}

				return references;
			}
		}
		NettingReceivableLineReferenceCollection references;

		public INettingTransactionLineReference AddNewLineReference()
		{
			return References.AddNew();
		}

		public override void Delete()
		{
			References.DeleteAll();

			base.Delete();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NRL_Amount = 1;
			NRL_PrimaryJobReference = "something";
			NRL_RX_NKCurrency = "XXX";
		}
#endif

	}
}
