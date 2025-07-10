using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsSupportingDocument : EU.NCTS.Business.NctsSupportingDocument
{
	public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsSupportingDocumentLookups(this);
}
