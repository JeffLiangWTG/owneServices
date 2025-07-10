using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class CusSupplyChainActorReference : EU.NCTS.Business.CusSupplyChainActorReference
	{
		public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString DataGroupingCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
