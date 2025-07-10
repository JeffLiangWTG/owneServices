using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public class PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
{
	public PreviousDocumentCollection(BusinessObject parent) : base(parent)
	{
	}

	public new PreviousDocument this[int index] => (PreviousDocument)base[index];

	public new PreviousDocument AddNew() => (PreviousDocument)base.AddNew();
}
