using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class OrgSupplierPart : Customs.Business.OrgSupplierPart, Integration.Customs.AE.IOrgSupplierPart
{
	public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new static OrgSupplierPart New(BusinessObjectFactory factory) => factory.New<OrgSupplierPart>();

	[ChildEditable(true)]
	public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

	protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.UnitedArabEmirates);

	protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.UnitedArabEmirates);
}
