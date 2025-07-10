using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
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
		}

		public new AddInfoCusContainerLookups Lookups
		{
			get { return (AddInfoCusContainerLookups)base.Lookups; }
		}

		public new AddInfoCusContainerValidation Validation => (AddInfoCusContainerValidation)base.Validation;

		protected override EUAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusContainerLookups(this);
		}

		protected override EUAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusContainerValidation(this);
		}
	}
}
