using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeOperationalActionMethod : OperationalActionMethod
	{
		public EDICommunicationsModeOperationalActionMethod()
			: base(new ZGuid("C8BD6C01-0DB4-4822-A318-37F62D81CD29"))
		{
		}

		public override string Name => Res.GetString("798EF119-9528-4CBD-AAE7-473CE25DB88F", "Update EDI Client Details");

		public override string Description => Res.GetString("49A07FFE-E08E-4148-B952-8D4CD497B927", "Update EDI Client Details for EDI Communication Modes");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new EDICommunicationsModeOperationalActionMethodApplicator(factory);
	}
}
