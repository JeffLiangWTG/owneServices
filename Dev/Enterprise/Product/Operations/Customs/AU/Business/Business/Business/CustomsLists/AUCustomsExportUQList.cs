using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUCustomsExportUQList : CodeDescriptionPairList
{
	public AUCustomsExportUQList()
	{
		AddPair("G", "Grams");
		AddPair("KG", "Kilograms");
		AddPair("T", "Tonne");
		AddPair("M", "Metres");
		AddPair("SM", "Square Metres");
		AddPair("CU", "Cubic Metres");
		AddPair("L", "Litres");
		AddPair("LA", "Litres of Alcohol");
		AddPair("NO", "Number");
		AddPair("PR", "Pair");
		AddPair("TH", "Thousand");
		AddPair("BC", "Basic Carton");
		AddPair("CT", "Carton");
		AddPair("MC", "Metric Carat");
		AddPair("NR", "Not Recorded - For Export");
	}
}
