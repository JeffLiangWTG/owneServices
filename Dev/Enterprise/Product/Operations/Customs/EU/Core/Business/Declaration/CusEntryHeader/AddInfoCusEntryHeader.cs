using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryHeader : AddInfo
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

		public new AddInfoCusEntryHeaderLookups Lookups => (AddInfoCusEntryHeaderLookups)base.Lookups;

		public new AddInfoCusEntryHeaderValidation Validation => (AddInfoCusEntryHeaderValidation)base.Validation;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryHeaderLookups(this);

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryHeaderValidation(this);
	}
}
