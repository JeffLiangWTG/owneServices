using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableTransactionRef : AutoNettingReceivableTransactionRef, INettingTransactionReference
	{
		public NettingReceivableTransactionRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Type
		{
			get { return NRR_Type; }
			set { NRR_Type = value; }
		}

		public ZString Reference
		{
			get { return NRR_Reference; }
			set { NRR_Reference = value; }
		}

		public ZGuid NettingTransactionPK
		{
			get { return NRR_NRT_Transaction; }
		}

		[RelatedBusinessObject("ReceivableTransaction")]
		public override ZGuid NRR_NRT_Transaction
		{
			get { return base.NRR_NRT_Transaction; }
			set { base.NRR_NRT_Transaction = value; }
		}

		public NettingReceivableTransaction ReceivableTransaction
		{
			get { return Factory.Load<NettingReceivableTransaction>(NRR_NRT_Transaction); }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NRR_NSP_Period; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NRR_Type = "CON";
			NRR_Reference = "234324";
		}
#endif

	}
}
