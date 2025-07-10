using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class PersonForTest : NonPersistentBusinessObject
	{
		public ZString FirstName { get; set; }
		public ZString LastName { get; set; }
	}
}
