using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMHeaderLocationOfGoods
{
	ZString CodeAndCinPlaceOfExamination { get; }
	ZString CodeAndCinPlaceOfUnloading { get; }
}
