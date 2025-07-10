namespace Enterprise.Customs.CN.Business
{
	public class NameOfGoodsElementStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "00000"; // 品名

		public override int MaxLength => 50;
	}
}
