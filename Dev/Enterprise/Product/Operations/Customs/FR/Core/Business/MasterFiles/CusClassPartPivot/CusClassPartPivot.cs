using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.FR.ICusClassPartPivot
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection() => new TaxForPivotCollection(this);

		public new TaxForPivotCollection Taxes => (TaxForPivotCollection)base.Taxes;

		protected override IDictionary<ZString, Type> GetCusAddInfoTypes()
		{
			var result = base.GetCusAddInfoTypes();
			result[CusAddInfoTypeAttribute.Codes.GBTax] = typeof(FRTaxOnlyForPivot);
			return result;
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		#region AdditionalInfos

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		#endregion

		#region SupportingDocuments

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#endregion

		#region PreviousDocuments

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		#endregion
	}
}

