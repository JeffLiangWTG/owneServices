using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business;

public class EdificeIncoTermList : CodeDescriptionPairList
{
	public EdificeIncoTermList()
	{
		AddPair(Core.Constants.IncoTerms.CostInsuranceAndFreight, "Cost, insurance and non-dutiable freight");
		AddPair(Core.Constants.IncoTerms.CostFreightWithAmpersand, "Cost and non-dutiable freight");
		AddPair(Core.Constants.IncoTerms.CostAndInsurance, "Cost and insurance");
		AddPair(Core.Constants.IncoTerms.FreeOnBoard, "Packed free on board at port, airport or container yard");
		AddPair(Core.Constants.IncoTerms.LandedIntoStore, "Landed into store excluding duty");
		AddPair(Core.Constants.IncoTerms.PackedAtFactory, "Packed at factory");
		AddPair(Core.Constants.IncoTerms.UnpackedAtFactory, "Unpacked at factory");
		AddPair(Core.Constants.IncoTerms.UnpackedCostAndFreight, "Unpacked cost and non-dutiable freight");
		AddPair(Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight, "Unpacked cost, insurance and non-dutiable freight");
		AddPair(Core.Constants.IncoTerms.UnpackedFreeOnBoard, "Unpacked free on board at port, airport or container yard");
	}
}
