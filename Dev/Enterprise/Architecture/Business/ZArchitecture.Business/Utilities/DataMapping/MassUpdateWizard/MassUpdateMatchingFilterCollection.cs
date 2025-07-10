using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class MassUpdateMatchingFilterCollection : NonPersistentBusinessObjectCollection<MassUpdateMatchingFilter>
	{
		public MassUpdateMatchingFilterCollection(MassUpdateWizard wizard)
			: base(wizard.Factory)
		{
			this.wizard = wizard;
		}
		readonly MassUpdateWizard wizard;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MassUpdateMatchingFilter(wizard);
		}
	}
}
