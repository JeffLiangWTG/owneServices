using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusBondDetailValidation : MasterFiles.Business.CusBondDetailValidation
	{
		public CusBondDetailValidation(CusBondDetail bondData)
			: base(bondData)
		{
		}

		new CusBondDetail Parent
		{
			get { return (CusBondDetail)base.Parent; }
		}

		#region Overrides
		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();

			MandatoryValidation.CheckNotNegative(Parent.PW_BondAmountInfo);
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			ListValidation.ErrorIfInvalidCode(Parent.PW_BondTypeInfo, Parent.Lookups.BondTypeList);
		}

		protected override void CheckPW_BondExpiryDate()
		{
			base.CheckPW_BondExpiryDate();

			if (Parent.PW_BondExpiryDate < Parent.PW_BondEffectiveDate)
			{
				Parent.PW_BondExpiryDateInfo.AddError(ExpiryDateLessThenEffectiveDate);
			}
		}
		internal const string ExpiryDateLessThenEffectiveDate = "Bond Expiry Date should be greater or equal to Effective Date.";

		protected override void CheckPW_BondEffectiveDate()
		{
			base.CheckPW_BondEffectiveDate();

			var bondType = Parent.PW_BondType;
			if (bondType != BondTypeList.Codes.SingleTransactionBond && bondType != BondTypeList.Codes.OnPortal)
			{
				MandatoryValidation.CheckEntered(Parent.PW_BondEffectiveDateInfo, "Bond Effective Date");
			}
			ValidatePW_BondExpiryDate();
		}

		#endregion
	}
}
