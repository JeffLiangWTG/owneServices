namespace Enterprise.GraphEngine.ServiceTasks
{
	public abstract class ShouldEnqueueStrategy
	{
		public abstract bool ShouldEnqueue();
	}

	public class NeverEnqueueStrategy : ShouldEnqueueStrategy
	{
		public override bool ShouldEnqueue() => false;
	}
}
