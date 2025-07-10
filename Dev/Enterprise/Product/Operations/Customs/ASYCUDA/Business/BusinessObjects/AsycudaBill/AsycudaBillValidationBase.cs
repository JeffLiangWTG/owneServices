using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillValidation : ManifestBase.AsycudaBillValidation
	{
		public AsycudaBillValidation(AsycudaBill parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCustomsEntryNumberType();
			ValidateCustomsEntryNumber();
			ValidateRegistrationDate();
		}

		public void ValidateCustomsEntryNumberType()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberTypeInfo);
		}

		protected virtual void CheckCustomsEntryNumberType()
		{
		}

		public void ValidateCustomsEntryNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberInfo);
		}

		protected virtual void CheckCustomsEntryNumber()
		{
		}

		public void ValidateRegistrationDate()
		{
			ValidateCalculatedProperty(Parent.RegistrationDateInfo);
		}

		protected virtual void CheckRegistrationDate()
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected ZZDatabaseValidationHelper ZZValidationHeaderHelper => zzValidationHeaderHelper ??= Parent.Header?.ZZValidationHelper ?? ZZDatabaseValidationHelper.GetDefaultValidationHelper(Parent.Factory);
		ZZDatabaseValidationHelper zzValidationHeaderHelper;

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			var header = Parent.Header;

			if (header != null && header.Sailing != null && ((ISailingSynchronisationTarget<BillOfLading>)Parent).Source == null)
			{
				Parent.ABL_BillNumberInfo.AddWarning(ResString.GetMultilingualString("AF69BFDB-668B-4BF6-850A-F9D5E9AC8984", "Bill is not linked to the same Sailing. It may have been incorrectly added."));
			}
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();
			if (NeedsToCheckABL_GrossWeight)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GrossWeightInfo);
				MandatoryValidation.CheckNotNegative(Parent.ABL_GrossWeightInfo);
			}
		}

		protected virtual ZBool NeedsToCheckABL_GrossWeight => false;

		protected override void CheckABL_GrossWeightUQ()
		{
			base.CheckABL_GrossWeightUQ();
			if (NeedsToCheckABL_GrossWeightUQ)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_GrossWeightUQInfo);
			}
		}

		protected virtual ZBool NeedsToCheckABL_GrossWeightUQ => false;

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();
			if (NeedsToCheckABL_ManifestQty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ManifestQtyInfo);
				MandatoryValidation.CheckNotNegative(Parent.ABL_ManifestQtyInfo);
			}
		}

		protected virtual ZBool NeedsToCheckABL_ManifestQty => false;

		protected override void CheckABL_ManifestUQ()
		{
			base.CheckABL_ManifestUQ();
			if (NeedsToCheckABL_ManifestUQ && !Parent.ABL_ManifestUQ.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_ManifestUQInfo);
			}
		}

		protected virtual ZBool NeedsToCheckABL_ManifestUQ => false;
	}
}
