using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.IT.ICusClassPartPivot
{
	public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{ }

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection()
	{
		return new SupportingDocumentCollection(this);
	}

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection()
	{
		return new PreviousDocumentCollection(this);
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	protected override CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection() => new TaxForPivotCollection(this);

	public new TaxForPivotCollection Taxes => (TaxForPivotCollection)base.Taxes;

	protected override IDictionary<ZString, Type> GetCusAddInfoTypes()
	{
		var result = base.GetCusAddInfoTypes();
		result[CusAddInfoTypeAttribute.Codes.GBTax] = typeof(ITTaxOnlyForPivot);
		return result;
	}
}
