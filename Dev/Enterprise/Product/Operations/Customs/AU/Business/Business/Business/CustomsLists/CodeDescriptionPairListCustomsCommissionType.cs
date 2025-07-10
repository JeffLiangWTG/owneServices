
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CodeDescriptionPairListCustomsCommissionType : CodeDescriptionPairList
	{
		public CodeDescriptionPairListCustomsCommissionType()
		{
			AddPair("1", "Buying");
			AddPair("2", "Selling");
			AddPair("3", "Agency");
			AddPair("4", "Confirming");
			AddPair("9", "Other");
		}
	}
}
