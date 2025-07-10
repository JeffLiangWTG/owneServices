using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	class AutoEDIMessageForTest : AutoEDIMessage
	{
		public AutoEDIMessageForTest(BusinessObjectFactory factory, DataRow row)
			 : base(factory, row)
		{
		}
	}
}
