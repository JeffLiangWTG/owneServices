using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementCollection : ActiveBusinessObjectCollection<EdiUserAgreement>
	{
		public EdiUserAgreementCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public EdiUserAgreementCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}