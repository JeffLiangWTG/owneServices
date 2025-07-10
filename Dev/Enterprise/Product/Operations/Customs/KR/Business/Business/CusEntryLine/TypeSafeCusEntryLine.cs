namespace Enterprise.Customs.KR.Business
{
	public partial class CusEntryLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusEntryLine Clone()
		{
			return (CusEntryLine)base.Clone();
		}

		public new CusEntryHeader Header
		{
			get { return (CusEntryHeader)base.Header; }
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

		//For PID, it returns a validation class which inherits from Customs.Business.CusEntryLineValidation
		//public new CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

		#endregion

		#region Implementation

		#region protected override

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override Customs.Business.CusEntryLineLookups GetNewLookups()
		{
			return new CusEntryLineLookups(this);
		}

		protected override Customs.Business.CusEntryLineValidation GetNewValidation()
		{
			Customs.Business.CusEntryLineValidation result = null;
			if (Header.IsMisc)
			{
				result = new MiscCusEntryLineValidation(this);
			}
			else
			{
				result = new CusEntryLineValidation(this);
			}
			return result;
		}

		#endregion

		#endregion
	}
}
