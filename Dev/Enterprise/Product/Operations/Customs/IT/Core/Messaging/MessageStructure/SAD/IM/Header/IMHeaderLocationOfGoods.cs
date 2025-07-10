using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderLocationOfGoods
{
	public IMHeaderLocationOfGoods(IIMHeaderLocationOfGoods locationOfGoods)
	{
		this.iMHeaderLocationOfGoods = Argument.NotNull(locationOfGoods, nameof(iMHeaderLocationOfGoods));
	}

	readonly IIMHeaderLocationOfGoods iMHeaderLocationOfGoods;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldImportRules("R", "CN95")]
	[MessageFieldDepositoRules("R", "CN95")]
	public ZString CodeAndCinPlaceOfExamination => iMHeaderLocationOfGoods.CodeAndCinPlaceOfExamination;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldImportRules("D", "CN96", "RN96")]
	[MessageFieldDepositoRules("D", "CN96", "RN96")]
	public ZString CodeAndCinPlaceOfUnloading => iMHeaderLocationOfGoods.CodeAndCinPlaceOfUnloading;
}
