using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusEntryHeader : AddInfo
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		public new AddInfoCusEntryHeaderLookups Lookups
		{
			get { return (AddInfoCusEntryHeaderLookups)base.Lookups; }
		}

		public new AddInfoCusEntryHeaderValidation Validation
		{
			get { return (AddInfoCusEntryHeaderValidation)base.Validation; }
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryHeaderLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryHeaderValidation(this);
		}
	}
}
