using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	abstract class CusPersonCollectionAbstractTest<T> : BusinessObjectCollectionTestCase
			where T : CusPersonCollection
	{
		protected override Type GetExpectedCollectionType() => typeof(T);

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.Persons.AddNew();
			return GetNewCollection(header);
		}

		protected abstract T GetNewCollection(AsycudaManifestHeader header);
	}
}
