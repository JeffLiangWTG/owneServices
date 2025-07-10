using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Business
{
	public class NextRunTimeRecord : NonPersistentBusinessObject
	{
		public NextRunTimeRecord() { }

		public ZDateTime NextRunTime { get; set; }

		public ZDateTime NextRunTimeLocal { get; set; }
	}
}

