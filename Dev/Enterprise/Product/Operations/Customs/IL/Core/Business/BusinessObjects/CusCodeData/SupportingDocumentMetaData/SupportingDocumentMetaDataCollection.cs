using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentMetaDataCollection : CusCodeDataCollection<SupportingDocumentMetaData>
	{
		public SupportingDocumentMetaDataCollection(BusinessObject parent)
			: base(parent, CusCodeDataTypeList.Codes.SupportingDocument)
		{
		}
	}
}
