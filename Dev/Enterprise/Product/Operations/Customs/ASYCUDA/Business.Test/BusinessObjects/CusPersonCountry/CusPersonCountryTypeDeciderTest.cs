using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class CusPersonCountryTypeDeciderTest : TestCaseWithFactory
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
			var header1PersonCountry = header1Person.Countries.AddNew();
			header1PersonCountry.CPC_RN_NKCountry = header1.AMA_RN_NKCountry;
			header1PersonCountry.CPC_Type = "123";
			header1PersonCountry.CPC_Value = "A";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Person = header2.Persons.AddNew();
			header2Person.CPN_PER_Person = glbPerson.PK;
			var header2PersonCountry = header2Person.Countries.AddNew();
			header2PersonCountry.CPC_RN_NKCountry = header2.AMA_RN_NKCountry;
			header2PersonCountry.CPC_Type = "123";
			header2PersonCountry.CPC_Value = "A";
			var header3 = Factory.New<AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Person = header3.Persons.AddNew();
			header3Person.CPN_PER_Person = glbPerson.PK;
			var header3PersonCountry = header3Person.Countries.AddNew();
			header3PersonCountry.CPC_RN_NKCountry = header3.AMA_RN_NKCountry;
			header3PersonCountry.CPC_Type = "123";
			header3PersonCountry.CPC_Value = "A";
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
			var header4PersonCountry = header4Person.Countries.AddNew();
			header4PersonCountry.CPC_RN_NKCountry = header4.AMA_RN_NKCountry;
			header4PersonCountry.CPC_Type = "123";
			header4PersonCountry.CPC_Value = "A";
			var header5 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header5.FillWithValidTestData();
			header5.AMA_JobReference = "964";
			header5.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var header5Person = header5.Persons.AddNew();
			header5Person.CPN_PER_Person = glbPerson.PK;
			var header5PersonCountry = header5Person.Countries.AddNew();
			header5PersonCountry.CPC_RN_NKCountry = header4.AMA_RN_NKCountry;
			header5PersonCountry.CPC_Type = "123";
			header5PersonCountry.CPC_Value = "A";
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(CusPersonCountry), header1PersonCountry.PK) is Integration.Customs.ASYCUDA.SGAccess.ICusPersonCountry);
			Assert(new BusinessObjectFactory().Load(typeof(CusPersonCountry), header2PersonCountry.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPersonCountry);
			Assert(new BusinessObjectFactory().Load(typeof(CusPersonCountry), header3PersonCountry.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPersonCountry);
			Assert(new BusinessObjectFactory().Load(typeof(CusPersonCountry), header4PersonCountry.PK) is Integration.Customs.ASYCUDA.ZAManifest.ICusPersonCountry);
			Assert(new BusinessObjectFactory().Load(typeof(CusPersonCountry), header5PersonCountry.PK) is Integration.Customs.ASYCUDA.ACEManifest.ICusPersonCountry);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPersonCountry>(), new CusPersonCountryTypeDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPersonCountry>(), new BusinessObjectFactory().New<CusPersonCountry>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(CusPersonCountry), new CusPersonCountryTypeDecider().GetTypeForBinding());
		}
	}
}
