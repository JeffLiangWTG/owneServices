using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISDocumentLookups : ZLookups
	{
		public AQISDocumentLookups(AQISDocument parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AQISDocumentTypeList
		{
			get
			{
				return Factory.GetCachedValue("AQISDocumentLookups|AQISDocumentTypeList", () =>
				{
					var typeList = CMRReferenceDataHelper.SetupAQISDocumentTypeList(Factory);

					typeList.SortByDescription();
					return typeList;
				});
			}
		}
	}
}
