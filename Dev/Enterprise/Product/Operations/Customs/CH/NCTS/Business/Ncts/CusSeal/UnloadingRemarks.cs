using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class UnloadingRemarks : SingleCusCodeData
{
	public UnloadingRemarks(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusSeal Parent => (CusSeal)base.Parent;

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusSeal));

	protected override CusCodeDataValidation GetNewValidation() => new UnloadingRemarksValidation(this);

	public new UnloadingRemarksValidation Validation => (UnloadingRemarksValidation)base.Validation;

	protected override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
	{
		yield return CY_DataInfo;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_ParentTableCode = CusSealSchema.Constants.Prefix;
		CY_Type = CusCodeDataTypeList.Codes.UnloadingRemarks;
	}
}
