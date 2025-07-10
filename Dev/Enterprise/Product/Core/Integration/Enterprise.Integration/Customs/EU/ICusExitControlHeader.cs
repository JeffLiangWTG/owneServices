using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusExitControlHeader
			{
				ZGuid PK { get; }
				ZGuid CEH_ParentID { get; set; }
				ZString CEH_ParentTableCode { get; set; }
				ZString CEH_ReferenceNumber { get; set; }
			}
		}
	}
}
