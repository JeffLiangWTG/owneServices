using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	[ModuleID("ApplicationLogger")]
	public class ApplicationLoggerCollection : ActiveBusinessObjectCollection<ApplicationLogger>
	{
		public ApplicationLoggerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
