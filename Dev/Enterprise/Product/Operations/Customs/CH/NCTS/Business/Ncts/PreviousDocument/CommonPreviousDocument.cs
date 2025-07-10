using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CommonPreviousDocument : EU.NCTS.Business.CommonPreviousDocument
{
	public CommonPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusSupportingInfoValidation GetNewValidation() => new CommonPreviousDocumentValidation(this);
}
