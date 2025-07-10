using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Netting
{
	public class NettingMatchPivot : AutoNettingMatchPivot
	{
		public NettingMatchPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var nettingReceivableTransaction = Factory.NewWithValidTestData<NettingReceivableTransaction>();
			var nettingPayableTransaction = Factory.NewWithValidTestData<NettingPayableTransaction>();

			NMP_NRT_ReceivableTransaction = nettingReceivableTransaction.PK;
			NMP_NPT_PayableTransaction = nettingPayableTransaction.PK;
		}
#endif
	}
}
