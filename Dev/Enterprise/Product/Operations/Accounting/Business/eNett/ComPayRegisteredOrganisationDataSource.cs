using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.eNett
{
	public class ComPayRegisteredOrganisationDataSource : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ComPayRegisteredOrganisationDataSource(BusinessObjectFactory factory)
			: base(factory) { }

		public bool OnlyShowOrgsWithTerminalCode
		{
			get;
			set;
		}

		public ZString PopulateCollection()
		{
			IeNettWebServiceWrapper eNettWebServiceWrapper = ObjectFactory.Get<IeNettWebServiceWrapper>();
			return eNettWebServiceWrapper.DisplayClientList(ComPayRegisteredOrganisations);
		}

		public ComPayRegisteredOrganisationCollection fComPayRegisteredOrganisation;
		public ComPayRegisteredOrganisationCollection ComPayRegisteredOrganisations
		{
			get
			{
				if (fComPayRegisteredOrganisation == null)
				{
					fComPayRegisteredOrganisation = new ComPayRegisteredOrganisationCollection(Factory);
				}
				return fComPayRegisteredOrganisation;
			}
		}

		public ComPayRegisteredOrganisation SelectedOrganisation
		{
			get { return fSelectedOrganisation; }
			set { fSelectedOrganisation = value; }
		}

		ComPayRegisteredOrganisation fSelectedOrganisation;
	}
}