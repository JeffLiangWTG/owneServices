using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	[ModuleID("ApplicationActiveLogger")]
	public class ApplicationActiveLoggerCollection : ActiveBusinessObjectCollection<ApplicationActiveLogger>
	{
		public ApplicationActiveLoggerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
