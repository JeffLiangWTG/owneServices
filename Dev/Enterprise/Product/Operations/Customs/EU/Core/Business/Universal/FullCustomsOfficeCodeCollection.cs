using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class FullCustomsOfficeCodeCollection : ZZRefCusCodeListCombinedCollection
	{
		public FullCustomsOfficeCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var zQuery = base.CreateRelationshipFilter();
			zQuery.AddToFilter(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
			return zQuery;
		}
	}
}
