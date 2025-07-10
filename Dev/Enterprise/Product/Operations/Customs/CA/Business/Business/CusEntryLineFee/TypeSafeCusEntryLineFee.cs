
namespace Enterprise.Customs.CA.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusEntryLineFee Clone()
		{
			return (CusEntryLineFee)base.Clone();
		}

		public new CusEntryLineFeeLookups Lookups
		{
			get
			{
				return (CusEntryLineFeeLookups)base.Lookups;
			}
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups()
		{
			return new CusEntryLineFeeLookups(this);
		}

		#endregion

		#endregion
	}
}
