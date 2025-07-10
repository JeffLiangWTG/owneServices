namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IUnderbondMovementRequestHeaderProvider
	{
		IUnderbondMovementRequestHeader GetHeader(CusUnderbond underbond);
	}
}
