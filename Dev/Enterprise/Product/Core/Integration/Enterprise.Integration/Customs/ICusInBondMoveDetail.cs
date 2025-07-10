using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondMoveDetail
		{
			ZGuid PK { get; }
			ZGuid B9_B0 { get; set; }
			ZGuid B9_BM { get; set; }
			ZString B9_CustomsStatus { get; set; }
			ZString B9_SeqNo { get; set; }
		}
	}
}
