using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
{
	public SupportingDocumentCollection(BusinessObject parent)
		: base(parent)
	{ }

	public new SupportingDocument this[int i] => (SupportingDocument)base[i];

	public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();

	public SupportingDocument GetFirstSupportingDocumentOrAddNewIfNotExists(ZString code)
	{
		var supportingDocument = this.Cast<SupportingDocument>().Where(x => x.CSI_Code == code).OrderBy(x => x.CSI_ReferenceNumber).FirstOrDefault();
		if (supportingDocument == null)
		{
			supportingDocument = AddNew();
			supportingDocument.CSI_Code = code;
		}
		return supportingDocument;
	}

	public IEnumerable<SupportingDocument> GetDeclarationOfIntentSupportingDocuments()
	{
		return Elements.Cast<SupportingDocument>().GetDeclarationOfIntentSupportingDocuments();
	}
}
