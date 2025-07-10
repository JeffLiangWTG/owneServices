using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class ComprehensiveValuationCollection : CusReferenceCollection<ComprehensiveValuation>
	{
		public ComprehensiveValuationCollection(JobComInvoiceHeader parent) : base(parent, ComprehensiveValuation.ComprehensiveValuationSchema.Type)
		{
			MaxCountValidationEnable(MaxRowCount);
		}

		public static int MaxRowCount => 3;

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxRowCount;
	}
}
