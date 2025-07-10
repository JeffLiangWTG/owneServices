using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalProcedureCode : EU.Business.AdditionalProcedureCode
	{
		public AdditionalProcedureCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaBill), typeof(CusClassPartPivot));
	}
}
