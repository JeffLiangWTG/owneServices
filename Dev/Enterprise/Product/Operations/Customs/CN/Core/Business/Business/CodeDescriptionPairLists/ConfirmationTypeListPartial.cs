using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	partial class ConfirmationTypeList
	{
		public static string GetDescriptionFromValue(bool value)
		{
			return value ? Descriptions.Yes : Descriptions.No;
		}

		public static CodeDescriptionPairList GetYesAndNoList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Customs.CN.Business.YesAndNoList", () =>
			 {
				 var list = new CodeDescriptionPairList();
				 list.AddPair(Codes.Yes, Descriptions.Yes);
				 list.AddPair(Codes.No, Descriptions.No);
				 return list;
			 });
		}
	}
}
