namespace Enterprise.Customs.IT.Business;

public interface IResponseMessage
{
	string ResponseStatus { get; }
}

public interface IResponseMessage<T> : IResponseMessage
	where T : class
{
	T ResponseBody { get; }
}
