using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new ClassificationCollection<CusClassification> ClassificationsForBinding => (ClassificationCollection<CusClassification>)base.ClassificationsForBinding;

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.Japan);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.Japan);
	}
}
