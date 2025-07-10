using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusCodeData
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }
			ZString CY_Code { get; set; }
			ZString CY_Data { get; set; }
			ZBool CY_IsOverridden { get; set; }
			ZShort CY_Order { get; set; }
			ZGuid CY_ParentID { get; set; }
			ZString CY_ParentTableCode { get; set; }
			ZString CY_Type { get; set; }
		}
	}
}
