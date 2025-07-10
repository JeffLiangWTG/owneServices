using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCargoDescFee : CusInBondFee, Integration.Customs.EU.NCTS.INctsCargoDescFee
	{
		public NctsCargoDescFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly NctsCargoDescFeeTypeDecider TypeDecider = new NctsCargoDescFeeTypeDecider();

		public new NctsCargoDescFeeValidation Validation => (NctsCargoDescFeeValidation)base.Validation;

		protected override CusInBondFeeValidation GetNewValidation() => new NctsCargoDescFeeValidation(this);

		public new NctsCargoDescFeeLookups Lookups => (NctsCargoDescFeeLookups)base.Lookups;

		protected override CusInBondFeeLookups GetNewLookups() => new NctsCargoDescFeeLookups(this);

		public NctsCommonCargoDesc Parent => Factory.Load<NctsCommonCargoDesc>(BFE_BY);

		#region properties
		[List(nameof(Lookups) + "." + nameof(NctsCargoDescFeeLookups.ChargeTypeList))]
		public override ZString BFE_ChargeType { get => base.BFE_ChargeType; set => base.BFE_ChargeType = value; }

		public override ZDecimal BFE_ChargeAmount { get => base.BFE_ChargeAmount; set => base.BFE_ChargeAmount = ChargeAmountRounder.Round(value); }
		#endregion

		protected override bool SupportsCloneCore() => true;

		public IFeeRounder ChargeAmountRounder => chargeAmountRounder ?? (chargeAmountRounder = GetNewChargeAmountRounder());
		IFeeRounder chargeAmountRounder;

		protected virtual IFeeRounder GetNewChargeAmountRounder() => new FeeNoRounder();
	}
}
