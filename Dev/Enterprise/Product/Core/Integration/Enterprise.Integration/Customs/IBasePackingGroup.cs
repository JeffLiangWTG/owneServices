using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBasePackingGroup
		{
			ZGuid PK { get; }
			ZGuid CR_CO_Container { get; set; }
			ZGuid CR_CU_HouseBill { get; set; }
		}
	}
}
