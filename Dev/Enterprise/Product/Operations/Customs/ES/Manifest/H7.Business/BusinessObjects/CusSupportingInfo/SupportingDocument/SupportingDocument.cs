using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class SupportingDocument : EU.H7.Business.SupportingDocument
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);
}
