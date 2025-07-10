using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class ContentInformationTypeCollection : CusCodeDataCollection<ContentInformationType>
	{
		public ContentInformationTypeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.ContentInformationType)
		{
		}
	}
}
