using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class ProductPendingUpdateDataObjectValidation : AutoProductPendingUpdateDataObjectValidation
	{
		public ProductPendingUpdateDataObjectValidation(AutoProductPendingUpdateDataObject parent) : base(parent)
		{
		}

		#region ValidateProductPk

		protected override void CheckProductPk()
		{
			MandatoryValidation.CheckEntered(Parent.ProductPkInfo);
		}

		#endregion

		#region ValidateCustomsType

		protected override void CheckCustomsType()
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.CustomsTypeInfo, Parent.Lookups.CustomsTypes);
		}

		#endregion

		#region Parent

		public new ProductPendingUpdateDataObject Parent => (ProductPendingUpdateDataObject)base.Parent;

		#endregion
	}
}
