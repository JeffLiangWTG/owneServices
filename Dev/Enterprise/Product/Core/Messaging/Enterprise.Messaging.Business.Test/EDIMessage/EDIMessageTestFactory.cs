using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business.Testing
{
	public static class EDIMessageTestFactory
	{
		public static EDIMessage New(BusinessObjectFactory factory)
		{
			return factory.New<EDIMessage>();
		}
	}
}
