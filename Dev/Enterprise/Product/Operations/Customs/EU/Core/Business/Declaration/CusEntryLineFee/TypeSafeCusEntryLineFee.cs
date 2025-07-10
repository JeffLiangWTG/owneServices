using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeCusEntryLineFee : AutoCusEntryLineFee
	{
		#region Constructor

		protected TypeSafeCusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

		public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		CusEntryLineFee EntryLineFee => (CusEntryLineFee)this;

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(EntryLineFee);

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation()
		{
			if (EntryLineFee.EntryLine?.Declaration?.Configuration?.UseUniversalFeeCalculation(EntryLineFee.EntryLine.Declaration) ?? false)
			{
				return new EUUniversalCusEntryLineFeeValidation(EntryLineFee);
			}
			else
			{
				return new CusEntryLineFeeValidation(EntryLineFee);
			}
		}

		#endregion

		#endregion
	}
}
