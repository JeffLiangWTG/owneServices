using System.ServiceModel;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business;

namespace Enterprise.DataTransfer.Native.Service
{
	[ServiceContract(Name = "EnterpriseNativeDataService", Namespace = "http://www.cargowise.com/NativeDataService")]
	public interface IEnterpriseNativeDataService
	{
		[OperationContract]
		string RetrieveData(string retrieveDataRequest);

		[OperationContract]
		XElement Retrieve(XElement retrieveDataRequest);

		[OperationContract]
		string UpdateData(string updateDataRequest);

		[OperationContract]
		XElement Update(XElement updateDataRequest);

		[OperationContract]
		XElement Service(XElement request, ServiceAction action);
	}
}
