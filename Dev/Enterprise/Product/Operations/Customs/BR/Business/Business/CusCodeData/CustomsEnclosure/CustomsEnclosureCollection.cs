using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class CustomsEnclosureCollection : CusCodeDataCollection<CustomsEnclosure>
	{
		public CustomsEnclosureCollection(JobDeclaration parent)
			: base(parent, CusCodeDataTypeList.Codes.CustomsEnclosure)
		{
		}
	}
}
