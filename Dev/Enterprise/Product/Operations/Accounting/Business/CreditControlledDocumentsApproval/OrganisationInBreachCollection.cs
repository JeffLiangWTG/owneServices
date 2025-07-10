using CargoWise.EntityFramework;
using Enterprise.MasterFiles.CreditControl.Business;

namespace Enterprise.Accounting.Business
{
	public class OrganisationInBreachCollection : NonPersistentBusinessObjectCollection<OrganisationInBreach>
	{
		public OrganisationInBreachCollection() : base()
		{
		}

		internal void PopulateOrganisations(BusinessObject parentBusinessObject)
		{
			var documentDeliveryManager = new DocumentDeliveryCreditControlManager();
			var organisationsAndBreachReasons = documentDeliveryManager.GetOrganisationsInBreachAndTheirBreachReasons(parentBusinessObject);
			if (organisationsAndBreachReasons.Count > 0)
			{
				foreach (var orgReasonPair in organisationsAndBreachReasons)
				{
					var orgInBreach = new OrganisationInBreach();
					orgInBreach.SetValues(orgReasonPair.Key, orgReasonPair.Value, parentBusinessObject);
					Add(orgInBreach);
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new OrganisationInBreach();
	}
}