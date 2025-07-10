using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentCollectionSynchroniser))]
	sealed class AsycudaTransportDocumentCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestAsycudaTransportDocumentCollectionSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "IL";

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			var transportDocument1 = manifestBill.TransportDocuments.AddNew();
			transportDocument1.CSI_Code = Constants.IsraeliCustoms.ManifestTransportContractDocumentId;
			transportDocument1.CSI_ReferenceNumber = "123";
			var transportDocument2 = manifestBill.TransportDocuments.AddNew();
			transportDocument2.CSI_Code = Constants.IsraeliCustoms.ManifestTransportContractDocumentId;
			transportDocument2.CSI_ReferenceNumber = "456";
			
			var number1 = Factory.New<CusEntryNumber>();
			number1.CE_EntryNum = "12345";
			number1.CE_EntryType = "IMP";
			consol.Numbers.Add(number1);

			AssertEquals("Transport Documents with 'IL2' code should be deleted", 0, manifestBill.TransportDocuments.Count);

			transportDocument1 = manifestBill.TransportDocuments.AddNew();
			transportDocument1.CSI_Code = Constants.IsraeliCustoms.ManifestTransportContractDocumentId;
			transportDocument1.CSI_ReferenceNumber = "123";
			transportDocument2 = manifestBill.TransportDocuments.AddNew();
			transportDocument2.CSI_Code = Constants.IsraeliCustoms.ManifestTransportContractDocumentId;
			transportDocument2.CSI_ReferenceNumber = "456";

			var transportDocumentIL2 = manifestBill.TransportDocuments?.Where(td => td.CSI_Code == Constants.IsraeliCustoms.ManifestTransportContractDocumentId);
			AssertEquals("Transport Documents with 'IL2' code should be 2", 2, transportDocumentIL2.Count());

			var number2 = Factory.New<CusEntryNumber>();
			number2.CE_EntryNum = "123456";
			number2.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			consol.Numbers.Add(number2);

			AssertEquals("Precondition", 1, manifestBill.TransportDocuments.Count);
			transportDocumentIL2 = manifestBill.TransportDocuments?.Where(td => td.CSI_Code == Constants.IsraeliCustoms.ManifestTransportContractDocumentId);
			AssertEquals("Transport Document with 'IL2' code should be created", 1, transportDocumentIL2.Count());
			AssertEquals("Reference Number from IL2 Code line should be as Entry Number from Consol number line with 'PDN' Entry Type", "123456", transportDocumentIL2.FirstOrDefault().CSI_ReferenceNumber);
		}
	}
}
