using System;
using System.ServiceModel.Activation;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DataTransfer.Native.Business;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Service
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public partial class EnterpriseNativeDataService : IEnterpriseNativeDataService
	{
		public string RetrieveData(string request)
		{
			var xmlDoc = XElement.Parse(request);
			var response = Retrieve(xmlDoc);
			return ToString(response);
		}

		public XElement Retrieve(XElement retrieveDataRequest)
		{
			return Service(retrieveDataRequest, ServiceAction.Retrieve);
		}

		public string UpdateData(string request)
		{
			var xmlDoc = XElement.Parse(request);
			var response = Update(xmlDoc);
			return ToString(response);
		}

		public XElement Update(XElement updateDataRequest)
		{
			return Service(updateDataRequest, ServiceAction.Update);
		}

		public XElement Service(XElement requestElement, ServiceAction action)
		{
			try
			{
				requestElement = requestElement ?? throw new NativeXMLUserVisibleException("Could not find any root element to process.");
				using (Db.DisposableActionForDbConnection())
				{
					GetDbConnectionFromAnotherThread_ForTest();

					var requestProcessor = RequestDeserializerBuilder.GetDeserializer(requestElement);
					var request = requestProcessor.Deserialize(requestElement);

					var handler = NativeHandler.GetHandler(action);
					var response = handler.Execute(request, requestProcessor);

					var responseSerializer = requestProcessor.GetResponseSerializer();
					return responseSerializer.Serialize(response);
				}
			}
			catch (NativeXMLUserVisibleException userVisibleException)
			{
				return CreateErrorElement(userVisibleException.Message);
			}
			catch (Exception exception)
			{
				ErrorReporter.ReportOnce(exception.Message, exception);
				throw;
			}
		}

		static XElement CreateErrorElement(string error)
		{
			return XElement.Parse(FormattableString.Invariant(
			$@"<Response xmlns=""http://www.cargowise.com/Schemas/Native"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <EntityInfo>
    <PrimaryKey xsi:nil=""true""/>
  </EntityInfo>
  <Status>Rejected</Status>
  <Information>
    <Item>Error - {error}</Item>
  </Information>
  <Data/>
</Response>"));
		}

		static string ToString(XElement element)
		{
			var stringBuilder = new StringBuilder();
			using (var xmlWriter = NativeXmlWriter.Create(stringBuilder))
			using (var cleanWriter = new CleanXmlWriter(xmlWriter))
			{
				element.WriteTo(cleanWriter);
			}
			return stringBuilder.ToString();
		}

		partial void GetDbConnectionFromAnotherThread_ForTest();
	}
}

#if DEBUG
namespace Enterprise.DataTransfer.Native.Service
{
	public partial class EnterpriseNativeDataService
	{
		partial void GetDbConnectionFromAnotherThread_ForTest()
		{
			if (ExceptionToThrow != null)
			{
				throw ExceptionToThrow;
			}
			if (ShouldGetDbConnectionToFromAnotherThread)
			{
				var testConnection = Db.Connection;
				testConnection.Dispose();
			}
		}
		public bool ShouldGetDbConnectionToFromAnotherThread;

		public Exception ExceptionToThrow;
	}
}
#endif
