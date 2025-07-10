using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class GroupInvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public GroupInvoiceChargeLookups(Customs.Business.BaseJobComInvHeaderCharge charge)
			: base(charge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var declaration = Parent.Parent.JobDeclaration;
				if (declaration?.IsImport ?? false)
				{
					var codeLists = declaration.Invoices.DistinctBy(x => x.JZ_ValuationCode).Select(x => ImportChargeMethodCodeList.GetChargeMethodCodeListByValuationCode(x.JZ_ValuationCode)).Distinct().Take(2).ToList();
					return codeLists.Count == 1 ? codeLists.Single() : [];
				}
				else
				{
					return base.ChargeTypeList;
				}
			}
		}

		public new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;
	}
}
