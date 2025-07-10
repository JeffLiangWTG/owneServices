using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MasterFiles
{
	public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.ES.ICusClassPartPivot
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection() => new TaxForPivotCollection(this);

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection()
		{
			return new SupportingDocumentCollection(this);
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		public new TaxForPivotCollection Taxes => (TaxForPivotCollection)base.Taxes;

		protected override IDictionary<ZString, Type> GetCusAddInfoTypes()
		{
			var result = base.GetCusAddInfoTypes();
			result[CusAddInfoTypeAttribute.Codes.GBTax] = typeof(ESTaxOnlyForPivot);
			return result;
		}
	}
}

