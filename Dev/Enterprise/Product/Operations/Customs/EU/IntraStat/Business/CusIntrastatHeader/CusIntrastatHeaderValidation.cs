using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatHeaderValidation : AutoCusIntrastatHeaderValidation
	{
		public CusIntrastatHeaderValidation(AutoCusIntrastatHeader parent) : base(parent)
		{
		}

		protected override void CheckCIH_CountryOfReceipt()
		{
			var propertyInfo = Parent.CIH_CountryOfReceiptInfo;
			MandatoryValidation.CheckEntered(propertyInfo);
			ListValidation.ErrorIfInvalidCode(propertyInfo);
		}

		protected override void CheckCIH_CountryOfSupply()
		{
			var propertyInfo = Parent.CIH_CountryOfSupplyInfo;
			MandatoryValidation.CheckEntered(propertyInfo);
			ListValidation.ErrorIfInvalidCode(propertyInfo);
		}

		protected override void CheckCIH_TradersReference()
		{
			var propertyInfo = Parent.CIH_TradersReferenceInfo;
			MandatoryValidation.CheckEntered(propertyInfo);
		}

		protected override void CheckCIH_ConsigneeName()
		{
			var parent = Parent;
			var propertyInfo = parent.CIH_ConsigneeNameInfo;

			CheckExactlyOneOfIsEntered(propertyInfo, parent.CIH_OH_ConsigneeInfo);
		}

		protected override void CheckCIH_OH_Consignee()
		{
			var parent = Parent;
			var propertyInfo = parent.CIH_OH_ConsigneeInfo;

			CheckExactlyOneOfIsEntered(propertyInfo, parent.CIH_ConsigneeNameInfo);
		}

		protected override void CheckCIH_OH_Supplier()
		{
			var parent = Parent;
			var propertyInfo = parent.CIH_OH_SupplierInfo;

			CheckExactlyOneOfIsEntered(propertyInfo, parent.CIH_SupplierNameInfo);
		}

		protected override void CheckCIH_SupplierName()
		{
			var parent = Parent;
			var propertyInfo = parent.CIH_SupplierNameInfo;

			CheckExactlyOneOfIsEntered(propertyInfo, parent.CIH_OH_SupplierInfo);
		}

		static void CheckExactlyOneOfIsEntered(ZPropertyInfo target, ZPropertyInfo other)
		{
			if (target.Value.IsDefault == other.Value.IsDefault)
			{
				target.AddError(Res.GetString("67af2c09-560d-4ee3-b9c5-f6f09a60f733", "Either {0} or {1} should be entered", target.HumanReadableName, other.HumanReadableName));
			}
		}
	}
}
