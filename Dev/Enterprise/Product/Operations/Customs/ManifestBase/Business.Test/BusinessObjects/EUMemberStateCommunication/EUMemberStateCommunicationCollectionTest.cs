using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public abstract class EUMemberStateCommunicationCollectionTest<T> : BusinessObjectCollectionTestCase
		where T : EUMemberStateCommunication
	{
		protected abstract EUMemberStateCommunicationCollection<T> GetEUMemberStateCommunicationCollection();

		protected override BusinessObjectCollection GetCollectionToTest() => GetEUMemberStateCommunicationCollection();
	}

	[TestedType(typeof(EUMemberStateCommunicationCollection<EUMemberStateCommunication>))]
	sealed class EUMemberStateCommunicationCollectionBaseOnlyTest : EUMemberStateCommunicationCollectionTest<EUMemberStateCommunication>
	{
		protected override EUMemberStateCommunicationCollection<EUMemberStateCommunication> GetEUMemberStateCommunicationCollection() => new (Factory.New<AsycudaManifestHeader>());
	}
}
