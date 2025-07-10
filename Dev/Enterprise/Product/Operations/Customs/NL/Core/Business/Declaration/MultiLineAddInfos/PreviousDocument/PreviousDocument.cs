using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

	public bool IsExport => ImportExportParent?.IsExport ?? false;

	internal ZBool IsInExportInvoiceLinePreviousDocuments => Factory.GetValue(ref isInExportInvoiceLinePreviousDocuments, () => IsExport && Parent is JobComInvoiceLine);
	CachedProperty<ZBool> isInExportInvoiceLinePreviousDocuments;
}
