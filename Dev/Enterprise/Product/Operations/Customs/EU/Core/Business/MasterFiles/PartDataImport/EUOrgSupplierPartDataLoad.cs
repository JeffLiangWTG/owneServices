using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class EUOrgSupplierPartDataLoad : Customs.Business.GlobalOrgSupplierPartDataLoad, Integration.Customs.EU.IEUOrgSupplierPartDataLoad
	{
		protected override IEnumerable<string> GetFieldNames()
		{
			return new List<string>() { EUPartsDataToLoad.Schema.ECSUPPLEMENT };
		}

		protected override PartsDataToLoad GetPartsDataToLoad()
		{
			return new EUPartsDataToLoad();
		}

		protected override void SetPivotSpecificFields(Customs.Business.BaseCusClassPartPivot pivot, PartsDataToLoad dataToLoad)
		{
			PartDataLoadHelper.SetSupplementCodes((CusClassPartPivot)pivot, ((EUPartsDataToLoad)dataToLoad).ECSUPPLEMENT.Split(';'));
		}
	}
}
