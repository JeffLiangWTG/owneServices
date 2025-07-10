namespace Enterprise.ZArchitecture.Modules.Testing
{
	class DummyBusinessObjectWithInterface : IDummyInterface
	{
		public void DoNothing()
		{
		}
	}
	public interface IDummyInterface
	{
		void DoNothing(); // code analysis hates empty interfaces
	}
}
