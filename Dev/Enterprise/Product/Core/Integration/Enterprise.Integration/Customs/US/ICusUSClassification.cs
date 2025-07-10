using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface ICusUSClassification
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZGuid CD_ParentID { get; set; }
				ZString CD_ParentTableCode { get; set; }
				ZString CD_ProductClaim { get; set; }
				ZString CD_SPI { get; set; }
			}
		}
	}
}
