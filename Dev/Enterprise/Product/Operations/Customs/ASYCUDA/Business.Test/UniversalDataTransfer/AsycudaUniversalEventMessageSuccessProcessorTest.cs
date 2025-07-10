using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(AsycudaUniversalEventMessageSuccessProcessor))]
	sealed class AsycudaUniversalEventMessageSuccessProcessorTest : AsycudaUniversalEventMessageProcessorTest
	{
		public void TestGetGroupToSendCore()
		{
			using (ManifestCustomsDataRegistry.Instance.GroupToSendSucceedNotification.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, newGroup.PK.ToGuid()))
			{
				AssertEquals(newGroup, Processor.GetGroupToSend());
			}
		}

		public void TestSendToGroup()
		{
			ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(Processor.SendToGroupForTest());

			ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NominatedGroup);
			Assert(Processor.SendToGroupForTest());

			ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NoEmails);
			Assert(!Processor.SendToGroupForTest());
		}

		public void TestSendToStaff()
		{
			ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(Processor.SendToStaffForTest());

			ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NominatedGroup);
			Assert(!Processor.SendToStaffForTest());

			ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(new Guid(), new Guid(), new Guid(), Core.Constants.EmailTo.NoEmails);
			Assert(!Processor.SendToStaffForTest());
		}

		AsycudaUniversalEventMessageSuccessProcessorForTest processor;
		AsycudaUniversalEventMessageSuccessProcessorForTest Processor => processor ??= new AsycudaUniversalEventMessageSuccessProcessorForTest(logger, universalEvent, message, header);

		sealed class AsycudaUniversalEventMessageSuccessProcessorForTest : AsycudaUniversalEventMessageSuccessProcessor
		{
			public AsycudaUniversalEventMessageSuccessProcessorForTest(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage message, AsycudaManifestHeader manifestHeader)
				: base(logger, universalEvent, message, manifestHeader)
			{
			}

			public bool SendToGroupForTest() => SendToGroup();

			public bool SendToStaffForTest() => SendToStaff();
		}
	}
}
