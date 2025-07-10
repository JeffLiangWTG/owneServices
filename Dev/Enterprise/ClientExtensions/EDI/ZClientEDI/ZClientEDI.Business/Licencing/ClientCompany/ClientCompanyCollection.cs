using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientCompanyCollection : ActiveBusinessObjectCollection<ClientCompany>
	{
		public ClientCompanyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ClientCompanyCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

