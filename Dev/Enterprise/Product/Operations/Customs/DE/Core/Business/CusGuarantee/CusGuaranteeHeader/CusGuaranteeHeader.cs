using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.Business
{
	public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.DE.ICusGuaranteeHeader
	{
		public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGuaranteeHeaderValidation Validation => (CusGuaranteeHeaderValidation)base.Validation;

		protected override CusPermitHeaderValidation GetNewValidation() => new CusGuaranteeHeaderValidation(this);

		public override ZString CPH_Type
		{
			get => base.CPH_Type;
			set
			{
				var originalValue = CPH_Type;
				base.CPH_Type = value;
				if (originalValue != CPH_Type && IsTRAGuaranteeType)
				{
					CPH_UnitOfMeasure = Core.Constants.CurrencyCodes.EuropeanUnion;
				}
			}
		}

		public ZBool IsTRAGuaranteeType => CPH_Type == EUGuaranteeTypeList.Codes.TRA;

		protected override CusGuaranteeRuleCollection CreateNewCusGuaranteeRulesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this);

		protected override CusGuaranteeRuleCollection CreateNewAdditionalAccessCodesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this, PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber);
	}
}
