using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EmptyEntryStyleCalculationStrategy : IEntryStyleCalculationStrategy
	{
		ZString IEntryStyleCalculationStrategy.Calculate() => ZString.Empty;
	}
}
