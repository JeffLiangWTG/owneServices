
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CodeDescriptionPairListDumpingExemptionType : CodeDescriptionPairList
	{
		public CodeDescriptionPairListDumpingExemptionType()
		{
			AddPair("C", "Country/Region - Dumping not applicable to Country/Region");
			AddPair("S", "Supplier - Dumping not applicable to Supplier");
			AddPair("G", "Goods - Dumping not applicable to type of Goods");
		}
	}
}
