using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class AccAmbiguousCommission : AutoAccAmbiguousCommission
	{
		public AccAmbiguousCommission(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AC0_CA0_SelectedAgreement), ConcurrencyPolicy.Strict);
		}

		#region Properties

		public override AccTransactionHeader Source
		{
			get { return Factory.Load<TransactionHeader>(AC0_AH_Source); }
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (AC0_AH_Source.IsEmpty)
			{
				AC0_AH_Source = Factory.New<Accounting.Business.ARAP.Invoicing.ARInvoice>().PK;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}

