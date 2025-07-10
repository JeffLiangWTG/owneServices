using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public partial class DirectPaymentLine : DirectTransactionLineBase
	{
		public DirectPaymentLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return true; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return Env.Security.NewCashBookDirectPaymentAllowOverrideOfVATRecoverable; }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionTypes.DirectPayment; }
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new DirectPaymentLineValidation(this);
		}
	}
}
