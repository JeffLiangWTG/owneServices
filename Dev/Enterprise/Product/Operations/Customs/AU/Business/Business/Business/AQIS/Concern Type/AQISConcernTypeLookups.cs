using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISConcernTypeLookups : ZLookups
	{
		public AQISConcernTypeLookups(AQISConcernType parent)
			: base(parent)
		{
		}

		#region AQISConcernTypeList
		public CodeDescriptionPairList AQISConcernTypeList
		{
			get
			{
				return Factory.GetCachedValue("AQISConcernTypeLookups.AQISConcernCodeList", () =>
				{
					var fAQISConcernTypeList = CMRReferenceDataHelper.SetupAQISConcernCodeList(Factory);
					fAQISConcernTypeList.SortByDescription();
					return fAQISConcernTypeList;
				});
			}
		}
		#endregion
	}
}
