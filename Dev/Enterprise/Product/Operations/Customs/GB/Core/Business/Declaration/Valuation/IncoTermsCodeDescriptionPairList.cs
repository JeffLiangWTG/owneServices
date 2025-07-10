using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class IncoTermsCodeDescriptionPairList : UntranslatableCodeDescriptionPairList
	{
		public IncoTermsCodeDescriptionPairList(ZString transportMode)
			: base("Incoterms should always be displayed in English")
		{
			DefaultIncoTerms();
			switch (transportMode)
			{
				case TransportTypeList.Codes.Sea:
				case TransportTypeList.Codes.InlandWaterwayTransport:
					AddPair(Constants.IncoTerms.FreeOnBoard, "Free on board");
					AddPair(Constants.IncoTerms.CostAndFreight, "Cost and freight");
					AddPair(Constants.IncoTerms.CostInsuranceAndFreight, "Cost, insurance and freight");
					AddPair(Constants.IncoTerms.FreeAlongsideShip, "Free alongside ship");
					break;
			}
			Sort();
		}

		void DefaultIncoTerms()
		{
			AddPair(Constants.IncoTerms.ExWorks, "Ex works");
			AddPair(Constants.IncoTerms.FreeCarrier, "Free carrier");
			AddPair(Constants.IncoTerms.CarriagePaidTo, "Carriage paid to");
			AddPair(Constants.IncoTerms.CarriageAndInsurancePaidTo, "Carriage and insurance paid to");
			AddPair(Constants.IncoTerms.DeliveredAtPlace, "Delivered at place");
			AddPair(Constants.IncoTerms.DeliveredAtPlaceUnloaded, "Delivered at place unloaded");
			AddPair(Constants.IncoTerms.DeliveredDutyPaid, "Delivered duty paid");
			AddPair(Constants.IncoTerms.Other, "Delivery terms other than those listed above");
		}
	}
}
