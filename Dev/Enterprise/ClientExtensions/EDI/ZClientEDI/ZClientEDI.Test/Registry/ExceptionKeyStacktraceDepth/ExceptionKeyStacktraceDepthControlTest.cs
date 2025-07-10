using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ExceptionKeyStacktraceDepthControl))]
	class ExceptionKeyStacktraceDepthControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ExceptionKeyStacktraceDepthCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ExceptionKeyStacktraceDepthControl)control).ExceptionKeyStacktraceDepthGrid.ReadOnly;
		}
	}
}
