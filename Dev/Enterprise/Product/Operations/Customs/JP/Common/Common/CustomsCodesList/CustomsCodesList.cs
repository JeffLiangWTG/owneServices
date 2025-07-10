using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common;

public static class CustomsCodesList
{
	public static CodeDescriptionPairList GetJPCustomsCodesList(BusinessObjectFactory factory) => factory.GetCachedValue("JP|CustomsCodesList", () => new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Japan));
}
