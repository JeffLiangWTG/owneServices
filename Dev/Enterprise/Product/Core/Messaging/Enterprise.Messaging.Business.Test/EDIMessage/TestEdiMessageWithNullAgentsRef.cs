using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Testing
{
	public class TestEdiMessageWithNullAgentsRef : TestEdiMessage
	{
		public TestEdiMessageWithNullAgentsRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetAgentReference()
		{
			return null;
		}
	}
}
