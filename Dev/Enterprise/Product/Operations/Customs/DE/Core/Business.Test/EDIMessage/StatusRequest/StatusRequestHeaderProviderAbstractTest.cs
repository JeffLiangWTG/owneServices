using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(StatusRequestHeaderProvider))]
	public abstract class StatusRequestHeaderProviderAbstractTest<T> : TestCaseWithFactory
		where T : StatusRequestHeaderProvider
	{
		public abstract void TestInterchangeRecipientID();

		public abstract void TestPartyType();

		protected abstract IStatusRequestHeader GetProvider();

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
			provider = GetProvider();
		}
		protected StatusRequest statusRequest;
		protected IStatusRequestHeader provider;
	}
}
