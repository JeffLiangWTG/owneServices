using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class CusPersonTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_JobReference = "123";
			header1.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var header1Person = header1.Persons.AddNew();
			header1Person.CPN_PER_Person = glbPerson.PK;
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Person = header2.Persons.AddNew();
			header2Person.CPN_PER_Person = glbPerson.PK;
			var header3 = Factory.New<AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Person = header3.Persons.AddNew();
			header3Person.CPN_PER_Person = glbPerson.PK;
			var header4 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header4.FillWithValidTestData();
			header4.AMA_JobReference = "012";
			header4.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var header4Person = header4.Persons.AddNew();
			header4Person.CPN_PER_Person = glbPerson.PK;
			foreach (var personCountry in header4Person.LoadChildren<CusPersonCountry>(CusPersonCountrySchema.CPC_CPN_Person))
			{
				personCountry.CPC_Value = "A";
			}
			var header5 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header5.FillWithValidTestData();
			header5.AMA_JobReference = "964";
			header5.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var header5Person = header5.Persons.AddNew();
			header5Person.CPN_PER_Person = glbPerson.PK;
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(CusPerson), header1Person.PK) is Integration.Customs.ASYCUDA.SGAccess.ICusPerson);
			Assert(new BusinessObjectFactory().Load(typeof(CusPerson), header2Person.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson);
			Assert(new BusinessObjectFactory().Load(typeof(CusPerson), header3Person.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson);
			Assert(new BusinessObjectFactory().Load(typeof(CusPerson), header4Person.PK) is Integration.Customs.ASYCUDA.ZAManifest.ICusPerson);
			Assert(new BusinessObjectFactory().Load(typeof(CusPerson), header5Person.PK) is Integration.Customs.ASYCUDA.ACEManifest.ICusPerson);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson>(), new CusPersonTypeDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson>(), new BusinessObjectFactory().New<CusPerson>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(CusPerson), new CusPersonTypeDecider().GetTypeForBinding());
		}
	}
}
