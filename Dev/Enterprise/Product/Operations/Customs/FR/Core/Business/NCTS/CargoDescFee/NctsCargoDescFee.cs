using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.FR.Business.NCTS
{
	[SystemDefinedValues]
	public class NctsCargoDescFee : EU.NCTS.Business.NctsCargoDescFee, Integration.Customs.FR.INctsCargoDescFee
	{
		public NctsCargoDescFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsCargoDescFeeValidation Validation => (NctsCargoDescFeeValidation)base.Validation;

		protected override CusInBondFeeValidation GetNewValidation() => new NctsCargoDescFeeValidation(this);

		public new NctsCargoDescFeeLookups Lookups => (NctsCargoDescFeeLookups)base.Lookups;

		protected override CusInBondFeeLookups GetNewLookups() => new NctsCargoDescFeeLookups(this);

		#region Properties

		[ResourceStringData("FR.Business.NctsCargoDescFee|BFE_ChargeType", Caption = "Fee Code")]
		[List(nameof(Lookups) + "." + nameof(NctsCargoDescFeeLookups.ChargeTypeList))]
		public override ZString BFE_ChargeType { get => base.BFE_ChargeType; set => base.BFE_ChargeType = value; }

		[ReadOnlyMember(nameof(BFE_ChargeAmountReadOnly))]
		[ResourceStringData("FR.Business.NctsCargoDescFee|BFE_ChargeAmount", Caption = "Amount")]
		public override ZDecimal BFE_ChargeAmount { get => base.BFE_ChargeAmount; set => base.BFE_ChargeAmount = value; }

		ZBool BFE_ChargeAmountReadOnly => BFE_RateOverrideReasonCode.IsEmpty;

		[List(nameof(Lookups) + "." + nameof(NctsCargoDescFeeLookups.RateOverrideReasonCodeList))]
		[ResourceStringData("FR.Business.NctsCargoDescFee|BFE_RateOverrideReasonCode", Caption = "Action")]
		public override ZString BFE_RateOverrideReasonCode { get => base.BFE_RateOverrideReasonCode; set => base.BFE_RateOverrideReasonCode = value; }

		#endregion

		protected override IFeeRounder GetNewChargeAmountRounder() => new IntegerFeeRounder();
	}
}
