using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class USPackageTypePairsRegistryItemEditor : PackageTypePairsRegistryItemEditor
	{
		public USPackageTypePairsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override void SetDataSourceType(PackageTypePairsControl packageTypesControl)
		{
			packageTypesControl.BindingSource.DataSourceType = typeof(USPackageTypePairCollection);
		}
	}
}
