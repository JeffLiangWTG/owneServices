namespace Enterprise.Customs.CN.Business;

public class GTINElementStrategy : CommonAdditionalElementStrategy
{
	public static string AdditionalElementCode => "00009"; // GTIN

	public override bool IsMandatory => false;

	public override bool IsMergeKey => true;
}
