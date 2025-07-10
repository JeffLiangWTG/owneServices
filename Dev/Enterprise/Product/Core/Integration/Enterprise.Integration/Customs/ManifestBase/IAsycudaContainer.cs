using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IAsycudaContainer
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZString ACN_StowageLocation { get; set; }
				ZString ACN_SealingPartyType { get; set; }
				ZString ACN_SealingPartyName { get; set; }
				ZString ACN_Seal3 { get; set; }
				ZString ACN_Seal2 { get; set; }
				ZString ACN_Seal1 { get; set; }
				ZGuid ACN_RC_ContainerType { get; set; }
				ZInt ACN_NumberOfPackages { get; set; }
				ZString ACN_GoodsWeightUQ { get; set; }
				ZDecimal ACN_GoodsWeight { get; set; }
				ZString ACN_EmptyFullIndicator { get; set; }
				ZString ACN_ContainerNumber { get; set; }
				ZString ACN_CommodityCode { get; set; }
				ZGuid ACN_AMA_Manifest { get; set; }
			}
		}
	}
}
