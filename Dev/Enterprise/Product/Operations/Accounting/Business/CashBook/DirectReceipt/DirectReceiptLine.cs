using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt
{
	public partial class DirectReceiptLine : DirectTransactionLineBase
	{
		public DirectReceiptLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString LineType
		{
			get { return ZArchitecture.Core.TransactionTypes.DirectReceipt; }
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new DirectReceiptLineValidation(this);
		}
	}
}