using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MX;

namespace Enterprise.Customs.MX.Business
{
	public class IdentifierCollection : Customs.Business.CusSupportingInfoCollection<Identifier>
	{
		public IdentifierCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.Identifier)
		{
		}
	}
}
