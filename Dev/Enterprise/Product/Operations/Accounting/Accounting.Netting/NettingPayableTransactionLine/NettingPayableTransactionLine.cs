using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransactionLine : AutoNettingPayableTransactionLine, INettingTransactionLine
	{
		public NettingPayableTransactionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid NettingTransactionPK
		{
			get { return NPL_NPT_Transaction; }
		}

		[RelatedBusinessObject("PayableTransaction")]
		public override ZGuid NPL_NPT_Transaction
		{
			get { return base.NPL_NPT_Transaction; }
			set { base.NPL_NPT_Transaction = value; }
		}

		public NettingPayableTransaction PayableTransaction
		{
			get { return Factory.Load<NettingPayableTransaction>(NPL_NPT_Transaction); }
		}

		public ZString JobReference
		{
			get { return NPL_PrimaryJobReference; }
			set { NPL_PrimaryJobReference = value; }
		}

		public ZDecimal Amount
		{
			get { return NPL_Amount; }
			set { NPL_Amount = value; }
		}

		public ZString TransactionCurrency
		{
			get { return NPL_RX_NKCurrency; }
			set { NPL_RX_NKCurrency = value; }
		}

		public ZBool IsApproved
		{
			get { return NPL_IsApproved; }
			set { NPL_IsApproved = value; }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NPL_NSP_Period; }
		}

		[ChildEditable(true)]
		public NettingPayableLineReferenceCollection References
		{
			get
			{
				if (references == null)
				{
					references = new NettingPayableLineReferenceCollection(this);
					RegisterEditableChildObject(references);
				}

				return references;
			}
		}
		NettingPayableLineReferenceCollection references;

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

			NPL_Amount = 1;
			NPL_PrimaryJobReference = "something";
			NPL_RX_NKCurrency = "XXX";
		}
#endif

	}
}
