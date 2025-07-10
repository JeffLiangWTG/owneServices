using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly TypeDecider TypeDecider = new OrgSupplierPartTypeDecider();

		[ChildEditable(true)]
		public new ICusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (ICusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		[ChildEditable(true)]
		public new ClassificationCollection<CusClassification> ClassificationsForBinding => (ClassificationCollection<CusClassification>)base.ClassificationsForBinding;

		protected override bool DoesPartMatchPivotForInactiveCheckCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = true;
			var pivotQuery = new ZQuery(CusClassPartPivotSchema.CI_OP, PK);
			var partPivots = Factory.Load<CusClassPartPivot>(pivotQuery);
			if (partPivots.Length > 1)
			{
				result = false;
			}
			else if (partPivots.Length == 1)
			{
				var pivot = partPivots[0];
				result = pivot.CI_RN_NKCountry == invoiceLine.CustomsCountryCode;
			}
			return result;
		}

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, CurrentCompanyCustomsCountryCode);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, CurrentCompanyCustomsCountryCode);
	}
}
