using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ZA
		{
			public interface IZACustomsRegistry
			{
				ICodeDescriptionPairList FANList { get; }
				(ZGuid CompanyPK, ZGuid OrgPK, ZString FAN)[] GetFANsForOrgs(ZGuid[] orgPKs);

				bool IsWarehouseOperatorTransactionsModuleEnabled { get; }
				IRegistryItem WarehouseOperatorTransactionsModuleEnabled { get; }
			}
		}
	}
}
