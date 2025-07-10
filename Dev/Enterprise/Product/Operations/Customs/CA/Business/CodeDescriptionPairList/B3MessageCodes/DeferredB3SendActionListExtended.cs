namespace Enterprise.Customs.CA.Business
{
	public partial class DeferredB3SendActionList
	{
		internal static bool IsValidCode(string code)
		{
			return code == Codes.Defer || code == Codes.Now;
		}
	}
}
