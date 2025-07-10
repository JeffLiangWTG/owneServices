using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business
{
	[DescriptionProperty("CustomsGuaranteeFriendlyName")]
	public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.FR.ICusGuaranteeHeader
	{
		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGuaranteeHeaderValidation Validation => (CusGuaranteeHeaderValidation)base.Validation;

		public new CusGuaranteeHeaderLookups Lookups => (CusGuaranteeHeaderLookups)GetNewLookups();

		public new GuaranteeCountrySpecificInstruction CountrySpecificInstruction => (GuaranteeCountrySpecificInstruction)base.CountrySpecificInstruction;

		protected override Customs.Business.CusPermitHeaderLookups GetNewLookups()
		{
			return new CusGuaranteeHeaderLookups(this);
		}

		protected override Customs.Business.CusPermitHeaderValidation GetNewValidation()
		{
			return new CusGuaranteeHeaderValidation(this);
		}

		public ZString GuaranteeModeCode
		{
			get
			{
				var rule = CusGuaranteeRules.FirstOrDefault(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.MOD);
				return rule != null ? rule.CPR_ValueFrom : ZString.Empty;
			}
		}

		public ZString GuaranteeModeDescription
		{
			get
			{
				return new GuaranteeModeCodeList().GetDescriptionFromCode(GuaranteeModeCode);
			}
		}

		public ZString GetCustomsGuaranteeFriendlyName(string applicationType)
		{
			return ZString.Join("/", new[]
			{
				CPH_Number,
				GetApplicationSpecificReferenceWithoutFallbackToPermitNumber(applicationType),
				PermitHolder?.OH_Code ?? ZString.Empty,
				GuaranteeModeDescription,
			}.Where(x => !x.IsEmpty).ToArray());
		}

		public ZString CustomsGuaranteeFriendlyName => GetCustomsGuaranteeFriendlyName(ZString.Empty);

		public ZString CustomsGuaranteeFriendlyNameForDeltaT => GetCustomsGuaranteeFriendlyName(OrgCusAccountDeltaTTypeList.Codes.TR);

		public override ZString CPH_Type
		{
			get => base.CPH_Type;
			set
			{
				base.CPH_Type = value;
				AdditionalGuaranteeReferences.MarkAsNeedingValidation();
			}
		}

		public new CusGuaranteeReferenceNumberCollection AdditionalGuaranteeReferences => (CusGuaranteeReferenceNumberCollection)base.AdditionalGuaranteeReferences;

		protected override Customs.Business.CusGuaranteeReferenceNumberCollection GetNewAdditionalGuaranteeReferences()
		{
			return new CusGuaranteeReferenceNumberCollection(this);
		}

		protected override Dictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ Customs.Business.GuaranteeCusCodeDataTypeList.Codes.GRN, typeof(CusGuaranteeReferenceNumber) }
			};
			return result;
		}

		public ZString[] AddressCodes => CusGuaranteeRules.Where(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.ADD).Select(x => x.CPR_ValueFrom).ToArray();
	}
}
