using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusBondDetailTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = (string)row[CusBondDetailSchema.PW_ApplicationCode.Name];
			return applicationCode == SecondCusBondDetail.ApplicationCode ? typeof(SecondCusBondDetail) : typeof(CusBondDetail);
		}
	}
}
