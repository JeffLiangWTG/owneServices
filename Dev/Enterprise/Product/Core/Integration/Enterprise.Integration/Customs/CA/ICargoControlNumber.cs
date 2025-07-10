using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICargoControlNumber
			{
				ZGuid B7_ParentID { get; set; }
				ZString B7_ParentTableCode { get; set; }
				ZString B7_AddInfoData { get; set; }
				ZString B7_Type { get; set; }
			}
		}
	}
}
