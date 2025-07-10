using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusEntryLine : AddInfo
	{
		public AddInfoCusEntryLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		public new AddInfoCusEntryLineLookups Lookups
		{
			get { return (AddInfoCusEntryLineLookups)base.Lookups; }
		}

		public new AddInfoCusEntryLineValidation Validation
		{
			get { return (AddInfoCusEntryLineValidation)base.Validation; }
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryLineLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryLineValidation(this);
		}
	}
}
