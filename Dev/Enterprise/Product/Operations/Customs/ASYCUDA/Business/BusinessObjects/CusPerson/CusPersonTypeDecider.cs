using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var headerPK = new ZGuid(row[CusPerson.Schema.CPN_ParentID]);
				var header = headerPK.IsValid ? factory.Load<AsycudaManifestHeader>(headerPK) : null;
				if (header != null)
				{
					result = header.GetPersonType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(CusPerson);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.ICusPerson>();
		}
	}
}
