using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackImportEntryDocumentCollection : Customs.Business.CusSupportingInfoCollection<SuspensionDrawbackImportEntryDocument>
	{
		public SuspensionDrawbackImportEntryDocumentCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.SuspensionDrawbackImportEntryDocument)
		{
		}
	}
}
