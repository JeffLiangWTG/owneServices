using CargoWise.EntityFramework;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class SignalRClientCollection : NonPersistentBusinessObjectCollection<SignalRClientBusinessObject>
	{
		public SignalRClientCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SignalRClientBusinessObject();
		}
	}
}
