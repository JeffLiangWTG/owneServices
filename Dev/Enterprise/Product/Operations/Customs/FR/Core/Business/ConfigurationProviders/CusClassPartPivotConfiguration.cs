using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
namespace Enterprise.Customs.FR.Business
{
	public class CusClassPartPivotConfiguration : EU.Business.CusClassPartPivotConfiguration
	{
		protected override ZBool UCCAdditionalInfosSupportCore(EU.Business.MasterFiles.CusClassPartPivot pivot) => pivot.IsImportClassification && FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.Value || pivot.IsExportClassification && FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.Value;
	}
}
