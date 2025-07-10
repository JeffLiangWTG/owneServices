using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.JP;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common;

public class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
{
	public override CodeDescriptionPairList GetCustomsPackList(BusinessObjectFactory factory, ZString type, ZString country)
	{
		if (type == MasterFiles.Business.Customs.RPTypeList.Codes.GlobalManifestBill)
		{
			return JPRefCusCodeListTypes.GetCachedList(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes);
		}
		else
		{
			return base.GetCustomsPackList(factory, type, country);
		}
	}
}
