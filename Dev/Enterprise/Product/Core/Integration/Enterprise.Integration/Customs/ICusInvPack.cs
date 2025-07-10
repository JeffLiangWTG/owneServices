using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInvPack
		{
			ZGuid PK { get; }
			ZString B5_ParentTableCode { get; set; }
			ZGuid B5_ParentID { get; set; }
		}
	}
}
