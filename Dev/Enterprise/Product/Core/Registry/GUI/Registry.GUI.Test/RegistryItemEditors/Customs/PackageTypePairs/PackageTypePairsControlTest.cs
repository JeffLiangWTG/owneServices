using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Customs.US;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PackageTypePairsControl))]
	sealed class PackageTypePairsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new USPackageTypePairCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			PackageTypePairsControl packageTypePairsControl = (PackageTypePairsControl)control;
			return packageTypePairsControl.PackageTypesGrid.ReadOnly;
		}
	}
}
