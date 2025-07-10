using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusContainer : AddInfo
	{
		public AddInfoCusContainer(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		public new AddInfoCusContainerLookups Lookups
		{
			get { return (AddInfoCusContainerLookups)base.Lookups; }
		}

		public new AddInfoCusContainerValidation Validation
		{
			get { return (AddInfoCusContainerValidation)base.Validation; }
		}

		protected override CAAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusContainerLookups(this);
		}

		protected override CAAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusContainerValidation(this);
		}
	}
}
