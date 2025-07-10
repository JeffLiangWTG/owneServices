using Enterprise.Customs.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvHeaderChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public JobComInvHeaderChargeLookups(BaseJobComInvHeaderCharge charge)
			: base(charge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (ShouldUseEdificeChargeList)
				{
					result = Factory.GetCachedValue<AUChargeCodeList>();
				}
				else
				{
					result = new CodeDescriptionPairList(base.ChargeTypeList);

					if (ShouldUseCMRChargeList)
					{
						result.RemoveCode(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ExWorks);
					}
				}
				return result;
			}
		}

		public new BaseJobComInvHeaderCharge Parent
		{
			get { return base.Parent as BaseJobComInvHeaderCharge; }
		}

		protected JobDeclaration JobDeclaration
		{
			get
			{
				ICommonInvoice parentParent = Parent.Parent;
				return parentParent != null ? parentParent.JobDeclaration as JobDeclaration : null;
			}
		}

		protected bool ShouldUseEdificeChargeList
		{
			get
			{
				JobDeclaration jobDeclaration = JobDeclaration;
				return jobDeclaration != null && jobDeclaration.IsImport && !jobDeclaration.IsImportCMR;
			}
		}

		protected bool ShouldUseCMRChargeList
		{
			get
			{
				JobDeclaration jobDeclaration = JobDeclaration;
				return jobDeclaration != null && jobDeclaration.IsImportCMR;
			}
		}
	}
}
