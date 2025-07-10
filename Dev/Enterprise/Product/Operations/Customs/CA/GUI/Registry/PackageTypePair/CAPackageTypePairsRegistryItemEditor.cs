using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI
{
	public class CAPackageTypePairsRegistryItemEditor : PackageTypePairsRegistryItemEditor
	{
		public CAPackageTypePairsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override void SetDataSourceType(PackageTypePairsControl packageTypesControl)
		{
			packageTypesControl.BindingSource.DataSourceType = typeof(CAPackageTypePairCollection);
		}
	}
}
