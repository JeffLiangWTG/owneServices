using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationLevelPackageCollection : BaseDeclarationLevelPackageCollection<Package>
	{
		public DeclarationLevelPackageCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Declaration.MarkAsNeedingValidation();
		}
	}
}
