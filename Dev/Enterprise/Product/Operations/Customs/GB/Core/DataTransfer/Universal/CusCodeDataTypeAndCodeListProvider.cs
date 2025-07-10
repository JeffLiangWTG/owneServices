using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal;

public class CusCodeDataTypeAndCodeListProvider : EU.DataTransfer.Universal.CusCodeDataTypeAndCodeListProvider
{
	protected override ICodeDescriptionPairList TableSpecificCusCodeDataTypeListCore(ZString tableCode, string dataContext)
	{
		switch (tableCode)
		{
			case AsycudaManifestHeaderSchema.Constants.Prefix:
				return GetListForAsycudaManifestHeader();
		}
		return base.TableSpecificCusCodeDataTypeListCore(tableCode, dataContext);
	}

	protected virtual CodeDescriptionPairList GetListForAsycudaManifestHeader()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(CusCodeDataTypeList.Codes.OfficeCode, CusCodeDataTypeList.Descriptions.OfficeCode);
		return result;
	}
}
