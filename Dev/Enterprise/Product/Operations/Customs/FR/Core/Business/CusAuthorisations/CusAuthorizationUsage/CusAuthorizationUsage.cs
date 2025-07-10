using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using JobDeclaration = Enterprise.Customs.FR.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.FR.Business
{
	public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage, Integration.Customs.FR.ICusAuthorizationUsage
	{
		public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		JobDeclaration declaration;

		public JobDeclaration JobDeclaration => declaration ?? (declaration = (Instruction?.JobDeclaration ?? InvoiceLine?.Declaration) as JobDeclaration);

		protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => JobDeclaration?.ApplicationExtender.GetCusAuthorizationUsageLookups(this) ?? new DeltaGCusAuthorizationUsageLookups(this);

		protected override IValueSetStrategy GetValueSetStrategy() => JobDeclaration?.ApplicationExtender.GetCusAuthorizationUsageValueSetStrategy(this) ?? new CusAuthorizationUsageValueSetStrategy(this);

		public new class Schema : AutoCusAuthorizationUsage.Schema
		{
			public const string AGC_AuthorizationShortCode = nameof(CusAuthorizationUsage.AGC_AuthorizationShortCode);
		}

		public override ZString AGC_Code
		{
			get => base.AGC_Code;
			set
			{
				if (value != AGC_Code)
				{
					base.AGC_Code = value;
					Instruction?.Validation.ValidateCEI_Style();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Business.CusAuthorizationUsage|AGC_AuthorizationShortCode", Caption = "Short Code")]
		public ZString AGC_AuthorizationShortCode => RelatedAuthorisationHeader?.CusAuthorisationRules?.FirstOrDefault(r => r.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.AUT)?.CPR_ValueFrom ?? ZString.Empty;

		protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation() => new CusAuthorizationUsageValidation(this);

		protected override bool UseEffectiveReferenceNumberCore => true;
	}
}
