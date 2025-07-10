using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientBranchCollection : ActiveBusinessObjectCollection<ClientBranch>
	{
		public ClientBranchCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ClientBranchCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

