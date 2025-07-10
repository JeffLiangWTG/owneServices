namespace Enterprise.Customs.CN.Business
{
	public class OthersElementStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "99999"; // 其他

		public override bool IsMandatory => false;
	}
}
