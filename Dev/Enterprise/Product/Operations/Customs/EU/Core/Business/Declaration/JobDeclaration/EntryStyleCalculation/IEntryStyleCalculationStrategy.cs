using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IEntryStyleCalculationStrategy
	{
		ZString Calculate();
	}
}
