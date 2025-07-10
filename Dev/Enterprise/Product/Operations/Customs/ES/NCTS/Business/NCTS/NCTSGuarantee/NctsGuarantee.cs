using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
	{
		public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new NctsGuaranteeLookups(this);

		public new NctsGuaranteeLookups Lookups => (NctsGuaranteeLookups)base.Lookups;

		protected override EU.NCTS.Business.NctsGuaranteeValidation GetNewPhase4Validation() => new NctsGuaranteeValidation(this);

		protected override EU.NCTS.Business.NctsGuaranteePhase5Validation GetNewPhase5Validation() => new NctsGuaranteePhase5Validation(this);

		protected override GuaranteeApportionmentType GetApportionmentTypeCore()
		{
			switch (PW_BondType)
			{
				case ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver:
				case ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee:
				case ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived:
				case ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies:
					return GuaranteeApportionmentType.EqualShare;
				case ESNCTS5GuaranteeTypeList.Codes.FlatRateVoucher:
					return GuaranteeApportionmentType.Voucher;
				default:
					return GuaranteeApportionmentType.None;
			}
		}

		protected override ZBool PW_BondNumber2ReadOnly => IsPhase5;

		protected override ZBool PW_SuretyCodeReadOnly => BondTypeIs5Or6Or8ForPhase5 || base.PW_SuretyCodeReadOnly;

		protected override ZBool PW_BondAmountReadOnly => (NctsHeader.IsPhase5Arrival && !PW_Override) || (!NctsHeader.IsPhase5Arrival && (BondTypeIs5Or6Or8ForPhase5 || base.PW_BondAmountReadOnly));

		[ReadOnlyMember(nameof(PW_BondNumberReadOnly))]
		public override ZString PW_BondNumber { get => base.PW_BondNumber; set => base.PW_BondNumber = value; }

		protected override ZBool PW_BondNumberReadOnly => !NctsHeader.IsPhase5Arrival && IsPhase5 && (BondTypeIs5Or6Or8 || PW_BondType == ESNCTS5GuaranteeTypeList.Codes.GuaranteeForGoodsDispatchedUnderTirProcedure);

		protected override ZBool PW_RX_NKCurrencyReadOnly => NctsHeader.IsPhase5Arrival || base.PW_RX_NKCurrencyReadOnly;

		protected override ZBool PW_OverrideReadOnly => !NctsHeader.IsPhase5Arrival && base.PW_OverrideReadOnly;

		ZBool BondTypeIs5Or6Or8ForPhase5 => IsPhase5 && BondTypeIs5Or6Or8;
		ZBool BondTypeIs5Or6Or8 => PW_BondType == ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived || PW_BondType == ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies || PW_BondType == ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;

		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set
			{
				var oldValue = base.PW_BondType;
				base.PW_BondType = value;
				if (!IsCopying && oldValue != value)
				{
					SetPW_SuretyCodeIfNeeded();
					SetPW_BondAmountIfNeeded();
					SetPW_BondNumberIfNeeded();
				}
			}
		}

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

		void SetPW_SuretyCodeIfNeeded()
		{
			if (PW_SuretyCodeReadOnly)
			{
				PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.ZER;
			}
		}

		void SetPW_BondAmountIfNeeded()
		{
			if (PW_BondAmountReadOnly)
			{
				PW_BondAmount = ZDecimal.Zero;
			}
		}

		void SetPW_BondNumberIfNeeded()
		{
			if (PW_BondNumberReadOnly)
			{
				PW_BondNumber = ZString.Empty;
			}
		}
	}
}
