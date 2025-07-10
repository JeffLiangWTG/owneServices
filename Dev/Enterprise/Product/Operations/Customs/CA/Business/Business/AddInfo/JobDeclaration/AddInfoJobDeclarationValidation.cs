using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobDeclarationValidation : CAAddInfoValidation
	{
		public AddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected new AddInfoJobDeclaration Parent
		{
			get { return (AddInfoJobDeclaration)base.Parent; }
		}

		protected AddInfoJobDeclarationLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckCA_EstReleaseDate()
		{
			base.CheckCA_EstReleaseDate();
			new DateRangeValidation().ValidateDateValueHasChanged(Parent.CA_EstReleaseDateInfo, true);
		}
	}
}
