using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<BaseCusClassification>(this, Core.Constants.CountryCodes.KoreaSouth);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.KoreaSouth);

		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		[ResourceStringData("46F20823-992C-4AAA-B1E2-A09A07AB8F02", Caption = "Model/Trade Name")]
		public override ZString OP_Model { get => base.OP_Model; set => base.OP_Model = value; }
		[ResourceStringData("E927ABFD-B19B-4DBA-8AA9-762DD3971DAD", Caption = "Brand Name")]
		public override ZString OP_Brand { get => base.OP_Brand; set => base.OP_Brand = value; }
	}
}
