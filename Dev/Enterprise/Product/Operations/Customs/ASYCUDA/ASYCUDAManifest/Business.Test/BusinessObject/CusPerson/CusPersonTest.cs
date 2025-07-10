using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	sealed class CusPersonTest : ASYCUDA.Business.Testing.CusPersonAbstractTest<CusPersonCountry>
	{
		public void TestICusPerson()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.CusPerson>(bizObj.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			return person;
		}
	}
}
