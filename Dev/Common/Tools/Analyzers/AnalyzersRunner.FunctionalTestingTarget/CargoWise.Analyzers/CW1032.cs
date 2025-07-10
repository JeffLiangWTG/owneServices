namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1032
	{
		public void Method()
		{
			//CW1032:Use Moq for Mocking
			_ = new Rhino.Mocks.Mock();
			_ = new DotNetMock.Mock();
		}
	}
}

namespace Rhino.Mocks
{
	class Mock
	{ }
}

namespace DotNetMock
{
	class Mock
	{ }
}
