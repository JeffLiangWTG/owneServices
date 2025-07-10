using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(AFRReporterIDControl))]
	sealed class AFRReporterIDControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AFRReporterID
			{
				ReporterID = "12345",
				Password = "123"
			};
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((AFRReporterIDControl)control).CurrentDataItem?.ReadOnly ?? false;
		}
	}
}
