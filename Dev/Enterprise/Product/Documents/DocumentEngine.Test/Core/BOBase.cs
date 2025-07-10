using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	class BOBase : NonPersistentBusinessObject
	{
		public ZInt IntField { get; set; }
	}
}
