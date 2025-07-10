using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public partial class CusEntryLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusEntryLine Clone()
		{
			return (CusEntryLine)base.Clone();
		}

		public new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees
		{
			get { return (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees; }
		}

		public new ConfirmedCusEntryLineFeeCollection ConfirmedFees => (ConfirmedCusEntryLineFeeCollection)base.ConfirmedFees;

		public new CusEntryHeader Header
		{
			get { return (CusEntryHeader)base.Header; }
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new JobComInvoiceLine RandomLine
		{
			get { return base.RandomLine as JobComInvoiceLine; }
		}

		public new CusEntryLineLookups Lookups
		{
			get
			{
				return (CusEntryLineLookups)base.Lookups;
			}
		}

		public new CusEntryLineValidation Validation
		{
			get { return (CusEntryLineValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CW1CusEntryLineFeeCollection(this, Factory);
		}

		protected override Customs.Business.ConfirmedCusEntryLineFeeCollection GetConfirmedCusEntryLineFeeCollection()
		{
			return new ConfirmedCusEntryLineFeeCollection(this);
		}

		protected override Customs.Business.CusEntryLineLookups GetNewLookups()
		{
			return new CusEntryLineLookups(this);
		}

		protected override Customs.Business.CusEntryLineValidation GetNewValidation()
		{
			return new CusEntryLineValidation(this);
		}

		#endregion

		#endregion
	}
}
