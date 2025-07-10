namespace GlowIndexQueryService.Business;

public class EmptyQuery : IGlowQuery
{
	public string ToUrlComponent()
	{
		return string.Empty;
	}
}
