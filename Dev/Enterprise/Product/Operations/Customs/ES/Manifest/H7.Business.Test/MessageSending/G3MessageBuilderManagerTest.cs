using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3MessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestNewMessageBuilder_Present()
		{
			var header = InitializeManifestHeader();

			for (int i = 0; i < 3; i++)
			{
				header.Bills.AddNew();
			}

			certificate = new CertificateObject(header.CustomsAgent, header.AMA_CustomsProfile, ZString.Empty);
			var sendingObjectParent = new G3MessageSendingObjectParent(header);

			SetSendingObjectsSendFlags(sendingObjectParent);

			var messageBuilderManager = new G3MessageBuilderManager(sendingObjectParent.SelectedSendingObjects, certificate, "LRN");

			CombineAssertions(() =>
			{
				var messageBuilder = messageBuilderManager.NewMessageBuilder() as G3PresentGoodMessageBuilder;
				AssertNotNull("Message Builder should be G3PresentGoodMessageBuilder", messageBuilder);

				var messageWrapper = messageBuilder.Provider as G3PresentGoodMessageWrapper;
				AssertNotNull("Message Wrapper should be G3PresentGoodMessageWrapper", messageWrapper);

				AssertNotNull("Header should be populated", messageWrapper.Header);
				AssertEquals("LRN should be populated", "LRN", messageWrapper.Header.LRN);
				AssertEquals("House Consignment should be populated", 2, messageWrapper.Header.MasterConsignment.First().HouseConsignment.Count);
			});
		}

		public void TestNewMessageBuilder_Revoke()
		{
			var header = InitializeManifestHeader();
			header.G3MRNToRevoke = "MRN000123";

			for (int i = 0; i < 3; i++)
			{
				AddBillWithCustomsEntryNumber(header);
			}

			certificate = new CertificateObject(header.CustomsAgent, header.AMA_CustomsProfile, ZString.Empty);
			var sendingObjectParent = new G3MessageSendingObjectParent(header, isRevoke: true);

			sendingObjectParent.SendingObjectsCollection[0].RevokeReason = "G3001";
			sendingObjectParent.SendingObjectsCollection[0].RevokeReasonDescription = "Reason 1";
			sendingObjectParent.SendingObjectsCollection[2].RevokeReason = "G3003";
			sendingObjectParent.SendingObjectsCollection[2].RevokeReasonDescription = "Reason 3";

			SetSendingObjectsSendFlags(sendingObjectParent);

			var messageBuilderManager = new G3MessageBuilderManager(sendingObjectParent.SelectedSendingObjects, certificate, "LRN");

			CombineAssertions(() =>
			{
				var messageBuilder = messageBuilderManager.NewMessageBuilder() as G3RevokeGoodMessageBuilder;
				AssertNotNull("Message Builder should be G3RevokeGoodMessageBuilder", messageBuilder);

				var messageWrapper = messageBuilder.Provider as G3RevokeGoodMessageWrapper;
				AssertNotNull("Message Wrapper is G3RevokeGoodMessageWrapper", messageWrapper);

				AssertNotNull("Header should be populated", messageWrapper.Header);
				AssertEquals("LRN should be populated", "LRN", messageWrapper.Header.LRN);
				AssertEquals("House Consignment should be populated", 2, messageWrapper.Header.MasterConsignment.First().HouseConsignment.Count);

				AssertEquals("Revoke reason code of bill 1 should be populated", "G3001", messageWrapper.Header.MasterConsignment.First().HouseConsignment.First().AdditionalInformation.Number);
				AssertEquals("Revoke reason description of bill 1 should be populated", "Reason 1", messageWrapper.Header.MasterConsignment.First().HouseConsignment.First().AdditionalInformation.Name);
				AssertEquals("Revoke reason code of bill 3 should be populated", "G3003", messageWrapper.Header.MasterConsignment.FirstOrDefault().HouseConsignment.Last().AdditionalInformation.Number);
				AssertEquals("Revoke reason description of bill 3 should be populated", "Reason 3", messageWrapper.Header.MasterConsignment.First().HouseConsignment.Last().AdditionalInformation.Name);
			});
		}

		AsycudaManifestHeader InitializeManifestHeader()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			return header;
		}

		void AddBillWithCustomsEntryNumber(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = header.G3MRNToRevoke;
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum.CE_EntryLineReference = G3EntryLineReference;
		}

		void SetSendingObjectsSendFlags(G3MessageSendingObjectParent sendingObjectParent)
		{
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = false;
			sendingObjectParent.SendingObjectsCollection[2].ShouldSend = true;
		}

		CertificateObject certificate;
		const string G3EntryLineReference = "G3";
	}
}
