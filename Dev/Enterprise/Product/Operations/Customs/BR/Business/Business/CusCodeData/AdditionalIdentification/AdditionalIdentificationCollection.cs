using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalIdentificationCollection : CusCodeDataCollection<AdditionalIdentification>
	{
		public AdditionalIdentificationCollection(OrgHeader parent)
			: base(parent, CusCodeDataTypeList.Codes.AdditionalIdentification)
		{
		}
	}
}
