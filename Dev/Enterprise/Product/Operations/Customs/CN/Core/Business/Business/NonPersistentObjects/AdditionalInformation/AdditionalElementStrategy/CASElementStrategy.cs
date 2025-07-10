namespace Enterprise.Customs.CN.Business;

public class CASElementStrategy : CommonAdditionalElementStrategy
{
	public static string AdditionalElementCode => "00005"; // CAS

	public override bool IsMandatory => false;

	public override bool IsMergeKey => true;
}
