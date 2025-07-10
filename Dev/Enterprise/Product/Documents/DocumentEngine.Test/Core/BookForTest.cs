using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class BookForTest : NonPersistentBusinessObject
	{
		public PersonForTest Author { get; set; }
		public ZString Title { get; set; }
		public ZString ISBN { get; set; }
	}
}
