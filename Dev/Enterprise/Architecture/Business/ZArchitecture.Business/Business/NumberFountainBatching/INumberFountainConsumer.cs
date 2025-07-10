using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public interface INumberFountainConsumer : INumberFountainEntityWithID
	{
		INumberFountainProxy Fountain { get; }
	}
}
