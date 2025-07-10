
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCustomsImportUQList : CodeDescriptionPairList
	{
		public AUCustomsImportUQList()
		{
			AddPair("G", "Grams");
			AddPair("KG", "Kilograms");
			AddPair("T", "Tonne");
			AddPair("CM", "Centimetres");
			AddPair("M", "Metres");
			AddPair("M2", "Square Metres");
			AddPair("M3", "Cubic Metres");
			AddPair("L", "Litres");
			AddPair("LA", "Litres of Alcohol");
			AddPair("NO", "Number");
			AddPair("PR", "Pair");
			AddPair("TH", "Thousand");
			AddPair("SR", "Number of Sets");
			AddPair("BC", "Basic Carton");
			AddPair("MC", "Metric Carat");
			AddPair("IU", "International Unit (Number of International Units)");
			AddPair("  ", "Not Required");
		}
	}
}
