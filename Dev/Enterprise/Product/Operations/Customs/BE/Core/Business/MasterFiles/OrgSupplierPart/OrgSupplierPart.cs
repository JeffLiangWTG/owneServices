using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using EUMasterFiles = Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.BE.Business.MasterFiles;

public class OrgSupplierPart : EUMasterFiles.OrgSupplierPart, Integration.Customs.BE.IOrgSupplierPart
{
	public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[ChildEditable(true)]
	public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)(ICusClassPartPivotCollection<BaseCusClassPartPivot>)base.PivotsForBinding;

	protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.Belgium);

	protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new EUMasterFiles.ClassificationCollection<EUMasterFiles.CusClassification>(this, Core.Constants.CountryCodes.Belgium);
}
