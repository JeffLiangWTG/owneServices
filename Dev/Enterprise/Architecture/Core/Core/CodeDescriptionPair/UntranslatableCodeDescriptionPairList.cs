using Enterprise.Integration.Accounting;

namespace Enterprise.ZArchitecture.Core
{
	public class UntranslatableCodeDescriptionPairList : CodeDescriptionPairList, ICustomsTRInvoicePaymentCodeList
	{
		public UntranslatableCodeDescriptionPairList(string untranslatableReason)
		{
			UntranslatableReason = untranslatableReason;
		}

		public UntranslatableCodeDescriptionPairList(string untranslatableReason, OLookUpEditType lookupEditType)
			: base(lookupEditType)
		{
			UntranslatableReason = untranslatableReason;
		}

		public string UntranslatableReason
		{
			get;
			private set;
		}
	}
}
