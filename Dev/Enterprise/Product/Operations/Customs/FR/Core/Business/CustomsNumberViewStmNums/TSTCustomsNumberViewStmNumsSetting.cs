using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class TSTCustomsNumberViewStmNumsSetting : CustomsNumberViewStmNumsSetting
	{
		public TSTCustomsNumberViewStmNumsSetting(ICustomsNumberViewStmNumsParent parent, ZString rangeType) : base(parent, rangeType)
		{
		}

		protected override ZLong? DefaultTypeRangeMaxCore()
		{
			return 999999L;
		}
	}
}
