using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransactionRef : AutoNettingPayableTransactionRef, INettingTransactionReference
	{
		public NettingPayableTransactionRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Type
		{
			get { return NPR_Type; }
			set { NPR_Type = value; }
		}

		public ZString Reference
		{
			get { return NPR_Reference; }
			set { NPR_Reference = value; }
		}

		public ZGuid NettingTransactionPK
		{
			get { return NPR_NPT_Transaction; }
			set { NPR_NPT_Transaction = value; }
		}

		[RelatedBusinessObject("PayabaleTransaction")]
		public override ZGuid NPR_NPT_Transaction
		{
			get { return base.NPR_NPT_Transaction; }
		}

		public NettingPayableTransaction PayabaleTransaction
		{
			get { return Factory.Load<NettingPayableTransaction>(NPR_NPT_Transaction); }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NPR_NSP_Period; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NPR_Type = "CON";
			NPR_Reference = "234324";
		}
#endif

	}
}
