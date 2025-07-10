using System.Threading;

namespace CargoWise.Async
{
	public interface IValidatedAsyncOperator
	{
		object GetSnapshot(object source);

		object TransformSnapshot(object initialSnapshot, CancellationTokenSource cancellationTokenSource = null);

		bool AssumptionsOfInitialSnapShotValid(object real, object initialSnapshot);

		void MapSnapshot(object newReal, object newSnapShot);
	}
}