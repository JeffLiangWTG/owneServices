using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills;
		}

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			AssertEquals("New bill must be created as HWB type", "HWB", bill.ABL_BolType);
		}

		public void TestSequenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			short expectedValue = 1;
			var bill = header.Bills.AddNew();
			AssertEquals("Expected ABL_SequenceNumber: 1, automatically generated", expectedValue, bill.ABL_SequenceNumber);

			expectedValue = 2;
			bill = header.Bills.AddNew();
			AssertEquals("Expected ABL_SequenceNumber: 2, automatically generated", expectedValue, bill.ABL_SequenceNumber);

			expectedValue = 1;
			header.Bills.RemoveAndDeleteAll();
			bill = header.Bills.AddNew();
			AssertEquals("Expected ABL_SequenceNumber to be 1, after Delete All bills and a new one is added", expectedValue, bill.ABL_SequenceNumber);
		}

		public void TestAllowRemove()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OverrideFreightDefaults = true;
			var bills = header.Bills;
			AssertEquals("When the Manifest is standalone.", true, bills.AllowRemove);

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			AssertEquals("When the Manifest is created from consol, and OverrideFreightDefaults == true", true, bills.AllowRemove);

			header.AMA_OverrideFreightDefaults = false;
			AssertEquals("When the Manifest is created from consol, and OverrideFreightDefaults == false", false, bills.AllowRemove);
		}

		public void TestAllowNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OverrideFreightDefaults = true;
			var bills = header.Bills;
			AssertEquals("When the Manifest is standalone.", true, bills.AllowNew);

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			AssertEquals("When the Manifest is created from consol, and OverrideFreightDefaults == true", true, bills.AllowNew);

			header.AMA_OverrideFreightDefaults = false;
			AssertEquals("When the Manifest is created from consol, and OverrideFreightDefaults == false", false, bills.AllowNew);
		}

		public void TestAddNew_EnsureTransportDocumentType704()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			manifestHeader.AMA_MasterBill = "BOL123";

			var bill = manifestHeader.Bills.AddNew();
			AssertNull("Transport document 704 should not be created for road transport", bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704"));

			manifestHeader.Bills.RemoveAndDeleteAll();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			bill = manifestHeader.Bills.AddNew();
			var transportDocument = bill.TransportDocuments.FirstOrDefault(doc => doc.CSI_Code == "704" && doc.CSI_ReferenceNumber == "BOL123");

			AssertNotNull("Transport document 704 should be created if not road transport", transportDocument);
		}

		public void TestAddNew_DefaultDischargePort()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "ILTLV";
			var bill1 = header.Bills.AddNew();
			AssertEquals("Bill1 DischargePort", "ILTLV", bill1.ABL_RL_NKPortOfDischarge);
		}

		public void TestDefaultTransportDocument_WhenRoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "ROA";
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			CombineAssertions("When Import", () =>
			{
				var bill1 = header.Bills.AddNew();
				var transportDocument = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL3");
				AssertNull("There should no IL3 transport document in the new bill", transportDocument);

				header.AMA_ManifestNumber = "123456";
				var bill2 = header.Bills.AddNew();
				transportDocument = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "IL3");
				AssertEquals("There should an IL3 transport document in the new bill", "123456", transportDocument.CSI_ReferenceNumber);
			});
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "ROA";
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;

			CombineAssertions("When Export", () =>
			{
				var bill1 = header.Bills.AddNew();
				var transportDocument = bill1.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "ILF");
				AssertNull("There should no ILF transport document in the new bill", transportDocument);

				header.AMA_ManifestNumber = "123457";
				var bill2 = header.Bills.AddNew();
				transportDocument = bill2.TransportDocuments.FirstOrDefault(s => s.CSI_Code == "ILF");
				AssertEquals("There should an ILF transport document in the new bill", "123457", transportDocument.CSI_ReferenceNumber);
			});
		}
	}
}
