using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class FindTriageFilterHelperValidation : AutoFindTriageFilterHelperValidation
	{
		public FindTriageFilterHelperValidation(AutoFindTriageFilterHelper parent) : base(parent)
		{
		}

		protected override void CheckNodeType()
		{
			base.CheckNodeType();
			ListValidation.ErrorIfInvalidCode(Parent.NodeTypeInfo);
		}

		protected override void CheckProduct()
		{
			base.CheckProduct();
			ListValidation.ErrorIfInvalidCode(Parent.ProductInfo);
		}

		protected override void CheckProductArea()
		{
			base.CheckProductArea();
			ListValidation.ErrorIfInvalidCode(Parent.ProductAreaInfo);
		}
	}
}
