using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsInspectionAtLocationCusCodeData : CusCodeData
	{
		public GvmsInspectionAtLocationCusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.GVI;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaManifestHeader));

		#endregion

		#region Lookups

		protected override CusCodeDataLookups GetNewLookups() => new GvmsInspectionAtLocationCusCodeDataLookups(this);

		public new GvmsInspectionAtLocationCusCodeDataLookups Lookups => (GvmsInspectionAtLocationCusCodeDataLookups)base.Lookups;

		#endregion

		#region Validation

		protected override CusCodeDataValidation GetNewValidation() => new GvmsInspectionAtLocationCusCodeDataValidation(this);

		#endregion

		#region New Properties

		[ResourceStringData("Enterprise.Customs.GB.GVMS.InspectionAtLocationCusCodeData.InspectionType", Caption = "Inspection Type")]
		public ZString InspectionType => Lookups.TypeList.GetWithDescription(CY_Code);

		[ResourceStringData("Enterprise.Customs.GB.GVMS.InspectionAtLocationCusCodeData.InspectionLocation", Caption = "Inspection Location")]
		public ZString InspectionLocation => Lookups.InspectionLocationList.GetWithDescription(CY_Data);

		#endregion
	}
}
