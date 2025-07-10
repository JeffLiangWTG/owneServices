using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class StatusRequestSenderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new StatusRequestSenderForTest(null));
		}

		public void TestNew_NotNull()
		{
			AssertNotNull(StatusRequestSender.New(statusRequest));
		}

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
		}
		StatusRequest statusRequest;
	}

	class StatusRequestSenderForTest : StatusRequestSender
	{
		public StatusRequestSenderForTest(StatusRequest statusRequest) : base(statusRequest)
		{
		}

		protected override IStatusRequestHeader Provider => null;

		protected override Func<IStatusRequestHeader, IProduceMessageXml> MessageBuilder => s => null;
	}
}
