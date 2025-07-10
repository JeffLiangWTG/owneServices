using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class CustomsOfficeCollection : CusCodeDataCollection<CustomsOffice>
	{
		public CustomsOfficeCollection(JobDeclaration parent)
			: base(parent, CusCodeDataTypeList.Codes.CustomsOffice)
		{
		}
	}
}
