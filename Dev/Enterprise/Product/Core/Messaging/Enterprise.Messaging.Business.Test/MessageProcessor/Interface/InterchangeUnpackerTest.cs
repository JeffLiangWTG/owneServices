using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	[TestsSubclassesOf(typeof(IUniversalCustomsInterchangeUnpacker))]
	public abstract class InterchangeUnpackerTest<T> : TestCaseWithFactory
		where T : class, IUniversalCustomsInterchangeUnpacker
	{
		public virtual void TestCorrectSubscribeToUCUSubscribers()
		{
			var provider = ObjectFactory.Get<IUCUSubscribersProvider>();

			foreach (var applicationCode in ApplicationCodes)
			{
				Assertion.AssertType<T>(provider.GetInterchangeUnpacker(applicationCode));
			}
		}

		protected abstract string[] ApplicationCodes { get; }
	}
}
