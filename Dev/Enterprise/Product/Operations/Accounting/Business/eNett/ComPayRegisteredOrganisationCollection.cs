using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.eNett
{
	public class ComPayRegisteredOrganisationCollection : NonPersistentBusinessObjectCollection<ComPayRegisteredOrganisation>
	{
		public ComPayRegisteredOrganisationCollection(BusinessObjectFactory factory)
			: base(factory) { }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComPayRegisteredOrganisation(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}