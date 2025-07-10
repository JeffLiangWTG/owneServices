using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument
{
	public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentValidation(this);

	protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentValidation(this);
}
