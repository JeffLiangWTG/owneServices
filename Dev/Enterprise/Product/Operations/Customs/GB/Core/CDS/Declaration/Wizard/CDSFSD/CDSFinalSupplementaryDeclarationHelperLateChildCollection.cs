using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSFinalSupplementaryDeclarationHelperLateChildCollection : NonPersistentBusinessObjectCollection<CDSFinalSupplementaryDeclarationHelperLateChild>
	{
		public CDSFinalSupplementaryDeclarationHelperLateChildCollection(CDSFinalSupplementaryDeclarationHelper master)
			: base(master.Factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
	}
}
