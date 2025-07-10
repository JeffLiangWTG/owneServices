using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentSynchroniser))]
	sealed class AsycudaTransportDocumentSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniseIL2CodeLine()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryNum = "12345";
			number1.CE_EntryType = "IMP";
			
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "IL";

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			AssertEquals("Precondition", 0, manifestBill.TransportDocuments.Count);
			var number2 = Factory.New<CusEntryNumber>();
			number2.CE_EntryNum = "123456";
			number2.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			consol.Numbers.Add(number2);

			var transportDocumentIL2 = manifestBill.TransportDocuments?.Where(td => td.CSI_Code == Constants.IsraeliCustoms.ManifestTransportContractDocumentId).FirstOrDefault();
			AssertNotNull("Transport Document with 'IL2' code should be created", transportDocumentIL2);
			AssertEquals("Reference Number from IL2 Code line should be as Entry Number from Consol number line with 'PDN' Entry Type", "123456", transportDocumentIL2.CSI_ReferenceNumber);
		}
	}
}
