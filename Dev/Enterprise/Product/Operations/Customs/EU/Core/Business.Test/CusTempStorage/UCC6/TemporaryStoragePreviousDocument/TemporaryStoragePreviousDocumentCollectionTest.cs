using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>))]
	class TemporaryStoragePreviousDocumentCollectionTest : CusSupportingInfoCollectionTest<TemporaryStoragePreviousDocument>
	{
		protected override CusSupportingInfoCollection<TemporaryStoragePreviousDocument> GetCusSupportingInfoCollection()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(header);
		}

		public void TestAllowNew()
		{
			var maxCount = 1;
			var previousDocumentCollection = GetCusSupportingInfoCollection();
			for (var i = 0; i < maxCount; i++)
			{
				AssertEquals("It is allowed to add more supporting documents", true, previousDocumentCollection.AllowNew);
				previousDocumentCollection.AddNew();
			}
			AssertEquals("No more supporting documents allowed", false, previousDocumentCollection.AllowNew);
		}

		public void TestAllowNew_FromTemporaryStorageBill()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_AMA = header.PK;
			var previousDocumentCollection = new TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(bill);

			var maxCount = 1;
			for (var i = 0; i < maxCount; i++)
			{
				AssertEquals("It is allowed to add more supporting documents", true, previousDocumentCollection.AllowNew);
				previousDocumentCollection.AddNew();
			}
			AssertEquals("No more supporting documents allowed", false, previousDocumentCollection.AllowNew);
		}

		public void TestAllowNew_MessageInfo()
		{
			var maxCount = 1;
			var document = Factory.New<TemporaryStoragePreviousDocument>();
			var previousDocumentCollection = GetCusSupportingInfoCollection();
			for (var i = 0; i < maxCount; i++)
			{
				AssertEquals("It is allowed to add more supporting documents", true, previousDocumentCollection.AllowNew);
				previousDocumentCollection.AddNew();
			}
			previousDocumentCollection.AddNew();
			AssertEquals("No more supporting documents allowed", "Error - Previous Document: You are only allowed a maximum of 1 Previous Document.", previousDocumentCollection.GetErrors().GetFirstMessage());
		}

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			var previousDocument = header.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Message Mode <> 'TF', no default", string.Empty, previousDocument.CSI_Code);
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				previousDocument = header.PreviousDocuments.AddNew();
				AssertEquals("Message Mode = 'TF', default 'NMRN'", "NMRN", previousDocument.CSI_Code);
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				previousDocument = header.PreviousDocuments.AddNew();
				AssertEquals("Message Mode = 'DC', default 'NMRN'", "NMRN", previousDocument.CSI_Code);
			});
		}
	}
}
