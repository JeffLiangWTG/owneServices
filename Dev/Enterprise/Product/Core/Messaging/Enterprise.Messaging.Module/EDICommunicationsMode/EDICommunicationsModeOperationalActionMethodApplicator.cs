using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public EDICommunicationsModeOperationalActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("13be0cef-97cf-46a6-b332-507bfbef98e2", "Update EDI Client Details operational action"), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);
		}
	}
}
