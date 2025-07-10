using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoHouseBill : AddInfo
	{
		public AddInfoHouseBill(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		public new Bill Parent
		{
			get { return (Bill)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		public new AddInfoHouseBillLookups Lookups
		{
			get { return (AddInfoHouseBillLookups)base.Lookups; }
		}

		public new AddInfoHouseBillValidation Validation
		{
			get { return (AddInfoHouseBillValidation)base.Validation; }
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoHouseBillLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			return new AddInfoHouseBillValidation(this);
		}
	}
}
