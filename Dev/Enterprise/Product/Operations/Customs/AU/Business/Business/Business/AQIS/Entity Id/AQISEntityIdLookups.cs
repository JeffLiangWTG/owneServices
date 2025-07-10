using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISEntityIdLookups : ZLookups
	{
		public AQISEntityIdLookups(AQISEntityId parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AQISEntityIdList
		{
			get
			{
				return Factory.GetCachedValue("AQISEntityIdLookups.AQISEntityIDList", () =>
				{
					var fAQISEntityIdList = CMRReferenceDataHelper.SetupAQISEntityIdList(Factory);
					fAQISEntityIdList.SortByDescription();
					return fAQISEntityIdList;
				});
			}
		}
	}
}
