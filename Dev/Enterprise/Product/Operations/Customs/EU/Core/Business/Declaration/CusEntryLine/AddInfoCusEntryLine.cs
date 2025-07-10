using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryLine : AddInfo
	{
		public AddInfoCusEntryLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoCusEntryLineLookups Lookups
		{
			get { return (AddInfoCusEntryLineLookups)base.Lookups; }
		}

		public new AddInfoCusEntryLineValidation Validation
		{
			get { return (AddInfoCusEntryLineValidation)base.Validation; }
		}

		protected override EUAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryLineLookups(this);
		}

		protected override EUAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryLineValidation(this);
		}
	}
}
