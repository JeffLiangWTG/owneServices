namespace Enterprise.Customs.IT.Business;

public static class HeaderOrLineValueMapResolverFluent
{
	public static HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> Configure<THeader, TLine, TOut>()
	{
		return new HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut>();
	}
}
