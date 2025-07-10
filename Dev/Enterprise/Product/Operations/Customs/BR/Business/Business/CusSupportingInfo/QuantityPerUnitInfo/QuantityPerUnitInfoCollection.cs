using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class QuantityPerUnitInfoCollection : Customs.Business.CusSupportingInfoCollection<QuantityPerUnitInfo>
	{
		public QuantityPerUnitInfoCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.QuantityPerUnit)
		{
		}

		public QuantityPerUnitInfo AddNew(ZString rateCode)
		{
			var quantityPerUnit = AddNew();
			using (quantityPerUnit.SuspendSettingHasChanges())
			{
				quantityPerUnit.CSI_SubType = rateCode;
			}
			return quantityPerUnit;
		}

		public QuantityPerUnitInfo FindByRateCode(ZString rateCode)
		{
			return this.Cast<QuantityPerUnitInfo>().FirstOrDefault(x => x.CSI_SubType == rateCode);
		}
	}
}
