using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableLineReference : AutoNettingPayableLineReference, INettingTransactionLineReference
	{
		public NettingPayableLineReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Type
		{
			get { return NP1_Type; }
			set { NP1_Type = value; }
		}

		public ZString Reference
		{
			get { return NP1_Reference; }
			set { NP1_Reference = value; }
		}

		public ZGuid TransactionLinePK
		{
			get { return NP1_NPL_Line; }
		}

		[RelatedBusinessObject("PayableLine")]
		public override ZGuid NP1_NPL_Line
		{
			get { return base.NP1_NPL_Line; }
			set { base.NP1_NPL_Line = value; }
		}

		public NettingPayableTransactionLine PayableLine
		{
			get { return Factory.Load<NettingPayableTransactionLine>(NP1_NPL_Line); }
		}

		public ZGuid NettingPeriodPK
		{
			get { return NP1_NSP_Period; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NP1_Type = "CBR";
			NP1_Reference = "543534";
		}
#endif

	}
}
