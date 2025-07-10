using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	/// <summary>
	/// Provides information about Related Business Objects related to a RatingHeader.
	/// Used for eDocs.
	/// </summary>
	public class DirectDebitBatchHeaderDocManagerInfo : AccountingDocManagerInfo
	{
		public DirectDebitBatchHeaderDocManagerInfo(DirectDebitBatchHeader bizO, string docManagerCode)
			: base(bizO, docManagerCode)
		{
		}

		protected DirectDebitBatchHeader DirectDebitBatchHeader
		{
			get { return (DirectDebitBatchHeader)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			return DirectDebitBatchHeader.Lines.ToArray();
		}
	}
}