using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	[TestsSubclassesOf(typeof(IUniversalCustomsEDIMessagePacker))]
	public abstract class UniversalCustomsEDIMessagePackerTest<T> : TestCaseWithFactory
		where T : class, IUniversalCustomsEDIMessagePacker
	{
		public virtual void TestCorrectSubscribeToUCPSubscribers()
		{
			AssertType<T>("Make sure expected type is accompanied with an UniversalCustomsEDIMessagePacker attribute. DEVELOPER NOTE: Make sure to execute AssemblyMetaDataExtractor.exe on your local to reflect newly added processors.", ObjectFactory.Get<IUCPSubscribersProvider>().GetMessagePacker(ApplicationCode));
		}

		protected abstract string ApplicationCode { get; }
	}
}
