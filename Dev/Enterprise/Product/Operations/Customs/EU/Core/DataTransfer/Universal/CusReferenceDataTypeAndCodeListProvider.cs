using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CusReferenceDataTypeAndCodeListProvider
	{
		public ICodeDescriptionPairList TableSpecificCusReferenceDataTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusReferenceDataTypeListCore(tableCode, dataContext);
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusReferenceDataTypeListCore(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case CusInBondCargoDescSchema.Constants.Prefix:
					result = GetListForCusInBondCargoDesc();
					break;
			}
			return result;
		}

		protected virtual CodeDescriptionPairList GetListForCusInBondCargoDesc()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor);
			return result;
		}
	}
}
