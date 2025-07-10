using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMHeaderLocationOfGoodWrapper : IIMHeaderLocationOfGoods
{
	public IMHeaderLocationOfGoodWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		jobDeclaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
	}
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration jobDeclaration;

	public ZString CodeAndCinPlaceOfExamination => entryHeader.LocationOfGoods;
	public ZString CodeAndCinPlaceOfUnloading => jobDeclaration.JE_SubLocationOfGoods;
}
