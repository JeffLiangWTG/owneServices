namespace Enterprise.ServiceManager.Host.Http
{
	interface IHttpListenerFactory
	{
		IHttpListener Create(string uriPrefix);
	}
}
