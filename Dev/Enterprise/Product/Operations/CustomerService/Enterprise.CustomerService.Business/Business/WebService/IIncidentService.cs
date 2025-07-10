using System;
using System.ServiceModel;

namespace Enterprise.CustomerService.Business.WebService
{
	[ServiceContract(Namespace = "http://schemas.cargowise.com/")]
	public interface IIncidentService
	{
		[OperationContract]
		IncidentResponse GetIncidentInfo(string request);

		[OperationContract]
		IncidentResponse UpdateIncidentInfo(string request);
	}

	[ServiceContract(Namespace = "http://schemas.cargowise.com/")]
	public interface IIncidentServiceAsync
	{
		[OperationContract(Action = "http://schemas.cargowise.com/IIncidentService/GetIncidentInfo", ReplyAction = "http://schemas.cargowise.com/IIncidentService/GetIncidentInfoResponse")]
		IncidentResponse GetIncidentInfo(string request);

		[OperationContract(Action = "http://schemas.cargowise.com/IIncidentService/UpdateIncidentInfo", ReplyAction = "http://schemas.cargowise.com/IIncidentService/UpdateIncidentInfoResponse")]
		IncidentResponse UpdateIncidentInfo(string request);

		[OperationContract(AsyncPattern = true, Action = "http://schemas.cargowise.com/IIncidentService/GetIncidentInfo", ReplyAction = "http://schemas.cargowise.com/IIncidentService/GetIncidentInfoResponse")]
		IAsyncResult BeginGetIncidentInfo(string request, AsyncCallback callback, object asyncState);

		IncidentResponse EndGetIncidentInfo(IAsyncResult result);

		[OperationContract(AsyncPattern = true, Action = "http://schemas.cargowise.com/IIncidentService/UpdateIncidentInfo", ReplyAction = "http://schemas.cargowise.com/IIncidentService/UpdateIncidentInfoResponse")]
		IAsyncResult BeginUpdateIncidentInfo(string request, AsyncCallback callback, object asyncState);

		IncidentResponse EndUpdateIncidentInfo(IAsyncResult result);
	}
}
