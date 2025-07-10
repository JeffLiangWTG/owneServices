using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
{
	public class CusClassPartPivotConfiguration : EU.Business.CusClassPartPivotConfiguration
	{
		protected override ZBool UCCAdditionalInfosSupportCore(EU.Business.MasterFiles.CusClassPartPivot pivot) => pivot.IsExport();
	}
}
