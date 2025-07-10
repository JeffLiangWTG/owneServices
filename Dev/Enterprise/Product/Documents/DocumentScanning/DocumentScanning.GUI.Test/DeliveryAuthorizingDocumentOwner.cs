using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class DeliveryAuthorizingDocumentOwner : DummyBusinessObject, IEDocDeliveryAuthorization
	{
		public DeliveryAuthorizingDocumentOwner(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string DocManagerCode = "TST";

		string IEDocDeliveryAuthorization.DeliveryDisclaimerMessage => "To be able to print export documents";

		public bool DocumentDeliveryDisclaimerRequired;

		bool IEDocDeliveryAuthorization.IsDocumentDeliveryDisclaimerRequired(IeDocBase doc)
		{
			return DocumentDeliveryDisclaimerRequired;
		}

		public bool DocumentViewEnabled;

		bool IEDocDeliveryAuthorization.IsDocumentViewEnabled(IeDocBase doc)
		{
			return DocumentViewEnabled;
		}
	}
}
