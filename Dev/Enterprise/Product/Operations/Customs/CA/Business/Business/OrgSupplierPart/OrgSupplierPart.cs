using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : MasterFiles.Business.OrgSupplierPart.Schema
		{
			public const string OP_AddInfoLine = "OP_AddInfoLine";
		}
		#endregion

		[ChildEditable(true)]
		public new ClassificationCollection<CusClassification> ClassificationsForBinding => (ClassificationCollection<CusClassification>)base.ClassificationsForBinding;

		[ChildEditable(true)]
		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.Canada);

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.Canada);

		#region Implementation
		protected TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;
		#endregion
	}
}
