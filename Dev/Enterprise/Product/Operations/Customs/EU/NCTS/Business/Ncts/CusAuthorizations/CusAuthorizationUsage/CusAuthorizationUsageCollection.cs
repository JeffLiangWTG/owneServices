using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusAuthorizationUsageCollection<TCusAuthorizationUsage, TMaster> : EU.Business.CusAuthorizationUsageCollection<TCusAuthorizationUsage, TMaster>
		where TCusAuthorizationUsage : CusAuthorizationUsage
		where TMaster : BusinessObject, ILinkable, INCTSCusAuthorizationUsageMaster
	{
		public CusAuthorizationUsageCollection(TMaster master)
			: base(master, master.Factory)
		{
		}

		protected override void EnableMaxCountValidation()
		{
			if (Master.IsPhase5 && Master.Configuration.ValidationRuleConfiguration.IsRuleTR0004Active)
			{
				const int maxCount = 9;
				this.EnableMaxCountValidationWithMessageError(maxCount, warnAtHalfway: false, Res.GetString("8C65E410-7951-4E41-AEB8-A48EB5417B8B", "[TR0004] The maximum number of {0} Authorizations has exceeded.", maxCount));
			}
			else
			{
				base.EnableMaxCountValidation();
			}
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);

			var authUsage = (CusAuthorizationUsage)dependent;
			if (Master.IsPhase5Departure && Master.Configuration.ValidationRuleConfiguration.IsRuleNR0002Active)
			{
				authUsage.AGC_CodeInfo.AdditionalValidation += GetCheckRuleNR0002(authUsage);
			}
		}

		RunValidationInvoker GetCheckRuleNR0002(CusAuthorizationUsage authUsage) => () =>
		{
			if (Where(e => e.AGC_Code == authUsage.AGC_Code).IsCountMoreThan(1))
			{
				var error = Res.GetString("e8f26d7c-b0c2-4f09-81d0-e029bcec97e5",
					"[NR0002] Authorization Type needs to be unique.");
				authUsage.AGC_CodeInfo.AddMessageError(error);
			}
		};
	}

	public interface INCTSCusAuthorizationUsageMaster : ICusAuthorizationUsageMaster
	{
		bool IsPhase5 { get; }

		NctsConfiguration Configuration { get; }

		bool IsPhase5Departure { get; }
	}
}
