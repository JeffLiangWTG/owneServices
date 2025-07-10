using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsGuarantee : EU.NCTS.Business.NctsGuarantee
	{
		public FRNctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGuaranteeHeader CusGuarantee => (CusGuaranteeHeader)base.CusGuarantee;

		public ZString GuaranteeReferenceNumber => CusGuarantee?.GetApplicationSpecificReference(OrgCusAccountDeltaTTypeList.Codes.TR) ?? PW_BondNumber;

		public override ZString PW_SuretyCode
		{
			get => base.PW_SuretyCode;
			set
			{
				var oldValue = base.PW_SuretyCode;
				base.PW_SuretyCode = value;
				if (!IsCopying && oldValue != value)
				{
					NctsHeader?.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
		}

		protected override EU.NCTS.Business.NctsGuaranteeValidation GetNewPhase4Validation() => new FRNctsGuaranteeValidation(this);

		protected override EU.NCTS.Business.NctsGuaranteePhase5Validation GetNewPhase5Validation() => new FRNctsGuaranteePhase5Validation(this);
	}
}
