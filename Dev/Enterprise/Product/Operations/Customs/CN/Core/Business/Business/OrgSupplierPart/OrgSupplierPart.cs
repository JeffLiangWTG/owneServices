using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart, Integration.Customs.CN.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new ClassificationCollection<CusClassification> ClassificationsForBinding => (ClassificationCollection<CusClassification>)base.ClassificationsForBinding;

		[ChildEditable(true)]
		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.China);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.China);
	}
}
