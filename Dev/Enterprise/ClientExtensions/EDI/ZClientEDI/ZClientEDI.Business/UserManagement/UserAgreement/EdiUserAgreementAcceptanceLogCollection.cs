using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAcceptanceLogCollection : ActiveBusinessObjectCollection<EdiUserAgreementAcceptanceLog>
	{
		public EdiUserAgreementAcceptanceLogCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public EdiUserAgreementAcceptanceLogCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
