using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class CusPerson : ASYCUDA.Business.CusPerson, Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new Customs.Business.ICusPersonCountryCollection<CusPersonCountry> Countries => (CusPersonCountryCollection<CusPersonCountry>)base.Countries;

		protected override Customs.Business.ICusPersonCountryCollection<Customs.Business.CusPersonCountry> CreateNewCusPersonCountryCollection() => new CusPersonCountryCollection<CusPersonCountry>(this);

		protected override Type GetPersonCountryTypeCore() => typeof(CusPersonCountry);
	}
}
