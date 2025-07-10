using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Customs.EU.Business
{
	public interface ICusAuthorizationUsageMaster
	{
		SchemaGuidColumn FKSchemaColumnInDependent { get; }
	}

	public interface ICusAuthorizationUsageCollection<out TCusAuthorizationUsage, out MasterT> : IBusinessObjectCollection<TCusAuthorizationUsage>
		where TCusAuthorizationUsage : CusAuthorizationUsage
		where MasterT : BusinessObject, ICusAuthorizationUsageMaster, ILinkable
	{
		new TCusAuthorizationUsage this[int index] { get; }
		bool IsManagedForDataRefresh { get; set; }
		MasterT Master { get; }
		int MaxCount { get; }
	}

	public class CusAuthorizationUsageCollection<TCusAuthorizationUsage, MasterT> : DependentBusinessObjectCollection<TCusAuthorizationUsage, MasterT>, ICusAuthorizationUsageCollection<TCusAuthorizationUsage, MasterT>
		where TCusAuthorizationUsage : CusAuthorizationUsage
		where MasterT : BusinessObject, ICusAuthorizationUsageMaster, ILinkable
	{
		public CusAuthorizationUsageCollection(MasterT master, BusinessObjectFactory factory)
			: base(master, factory)
		{
			EnableMaxCountValidation();
		}

		protected virtual int MaxCountForValidation
		{
			get => maxCountForValidation;
		}
		const int maxCountForValidation = 9;

		protected virtual void EnableMaxCountValidation()
		{
			MaxCountValidationEnable(MaxCountForValidation, Res.GetString("193963BF-BBCB-4A51-BE1D-B06D0ED275BB", "Only {0} authorizations are allowed.", MaxCountForValidation), false);
		}

		public IEnumerator<TCusAuthorizationUsage> GetEnumerator() => Elements.Cast<TCusAuthorizationUsage>().GetEnumerator();

		protected override SchemaGuidColumn FKSchemaColumnInDependent => Master.FKSchemaColumnInDependent;
	}
}
