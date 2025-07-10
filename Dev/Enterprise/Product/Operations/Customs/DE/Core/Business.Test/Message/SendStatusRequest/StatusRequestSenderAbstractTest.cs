using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(StatusRequestSender), ExcludePrivate = true)]
	public abstract class StatusRequestSenderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : StatusRequestSender
	{
		public virtual void TestSend()
		{
			Sender.Send();
			CombineAssertions(() =>
			{
				AssertEquals("EM_MessageText", true, !statusRequest.EM_MessageText.IsEmpty);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, statusRequest.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", ExpectedApplicationReference, statusRequest.EM_ApplicationReference);
				AssertEquals("EM_MessageType", ExpectedMessageType, statusRequest.EM_MessageType);
				AssertEquals("EM_MessageSubType", ExpectedMessageSubType, statusRequest.EM_MessageSubType);
				AssertEquals("EM_ApplicationCode", ExpectedApplicationCode, statusRequest.EM_ApplicationCode);

				AssertEquals("LogbookEORIBranchSuffix exists", "1234", statusRequest.GetLogbookEORIBranchSuffix());
				AssertEquals("LogbookRegistrationNumber exists", "MRN4TEST", statusRequest.GetLogbookRegistrationNumber());
			});
		}

		public void TestNew()
		{
			AssertType<T>(StatusRequestSender.New(statusRequest));
		}

		protected abstract StatusRequestSender GetSender();

		protected abstract ZString ExpectedMessageType { get; }

		protected abstract ZString ExpectedMessageSubType { get; }

		protected abstract ZString ExpectedApplicationReference { get; }

		protected abstract ZString ExpectedApplicationCode { get; }

		protected abstract ZString Module { get; }

		protected StatusRequestSender Sender => sender ?? (sender = GetSender());
		StatusRequestSender sender;

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
			statusRequest.Module = Module;
			statusRequest.MovementReferenceNumber = "MRN4TEST";
		}
		protected StatusRequest statusRequest;

		protected override T GetProvider() => (T)Sender;
	}
}
