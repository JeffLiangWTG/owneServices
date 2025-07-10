using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDrawbackEntryLine : IBaseDrawbackEntryLine
	{
		ZDateTime DeclarationDate { get; }

		ZDecimal CL_DutyPercent { get; }

		ZDecimal Quantity { get; }

		ZString EntryNumber { get; }

		ZShort EffectiveLineNumber { get; }
	}
}
