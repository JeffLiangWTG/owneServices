using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsInspectionAtLocationCusCodeDataCollection : CusCodeDataCollection<GvmsInspectionAtLocationCusCodeData>
	{
		public GvmsInspectionAtLocationCusCodeDataCollection(AsycudaManifestHeader header)
			: base(header, CusCodeDataTypeList.Codes.GVI)
		{
			this.header = header;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(CusCodeDataSchema.CY_ParentID, header.PK);
			filter.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.GVI);
			return filter;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var location = (GvmsInspectionAtLocationCusCodeData)child;
			location.Parent = header;
			location.CY_Type = CusCodeDataTypeList.Codes.GVI;
		}

		public override bool ReadOnly => true;

		readonly AsycudaManifestHeader header;
	}
}
