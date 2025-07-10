using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyDataSource : DocumentWrapper
	{
		public DummyDataSource(string name = "Earl")
		{
			Name = name;
		}

		internal string Name { get; }
	}
}
