using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2AsycudaBillDataObjectWriterTest : AsycudaWriterTestHelper
	{
		public void TestExportTransportDocumentType()
		{
			var header = ICS2DataObjectWriterTestHelper.SetupManifestHeader(Factory);
			var bill = header.Bills.AddNew();
			bill.TransportDocumentType = TransportDocumentTypes.Codes.CL754_N714;
			Factory.SaveForTesting();

			var manifestHeaderData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);

			var billData = manifestHeaderData.SubShipmentCollection.Single();
			Assert("An AddInfo with the key 'TransportDocumentType' and the value 'N704' should be present under SubShipment.",
				billData.AddInfoCollection.Any(x => x.Key.Value == AsycudaBill.Schema.TransportDocumentType
				&& x.Value.Value == TransportDocumentTypes.Codes.CL754_N714));
		}
	}
}
