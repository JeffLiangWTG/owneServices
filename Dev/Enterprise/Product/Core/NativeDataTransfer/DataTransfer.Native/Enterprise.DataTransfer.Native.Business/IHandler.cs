using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;

namespace Enterprise.DataTransfer.Native.Business
{
	public interface IHandler
	{
		Response Execute(Request request, IResponseFactory responseFactory);
	}
}
