using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonCountryTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var personPK = new ZGuid(row[CusPersonCountry.Schema.CPC_CPN_Person]);
				var person = personPK.IsValid ? factory.Load<CusPerson>(personPK) : null;
				if (person != null)
				{
					result = person.GetPersonCountryType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding() => typeof(CusPersonCountry);

		public override Type GetTypeForNew() => ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPersonCountry>();
	}
}
