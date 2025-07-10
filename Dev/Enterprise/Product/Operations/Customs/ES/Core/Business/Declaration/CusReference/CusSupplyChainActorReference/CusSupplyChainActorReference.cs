using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusSupplyChainActorReference : EU.Business.Declaration.CusSupplyChainActorReference
	{
		public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString DataGroupingCode => Declaration?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
