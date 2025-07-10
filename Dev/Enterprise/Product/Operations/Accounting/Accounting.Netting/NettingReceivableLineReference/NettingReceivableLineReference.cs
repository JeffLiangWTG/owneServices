using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableLineReference : AutoNettingReceivableLineReference, INettingTransactionLineReference
	{
		public NettingReceivableLineReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Type
		{
			get { return NR1_Type; }
			set { NR1_Type = value; }
		}

		public ZString Reference
		{
			get { return NR1_Reference; }
			set { NR1_Reference = value; }
		}

		public ZGuid TransactionLinePK
		{
			get { return NR1_NRL_Line; }
		}

		[RelatedBusinessObject("ReceivableLine")]
		public override ZGuid NR1_NRL_Line
		{
			get { return base.NR1_NRL_Line; }
			set { base.NR1_NRL_Line = value; }
		}

		public NettingReceivableTransactionLine ReceivableLine
		{
			get { return Factory.Load<NettingReceivableTransactionLine>(NR1_NRL_Line); }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NR1_NSP_Period; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NR1_Type = "CBR";
			NR1_Reference = "543534";
		}
#endif
	}
}
