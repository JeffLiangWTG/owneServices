using System.Diagnostics.CodeAnalysis;

namespace AnalyzersRunner.FunctionalTestingTarget
{
	class IDE0100
	{
		[SuppressMessage("Style", "WTG1007:Do not compare bool to a constant value", Justification = "This test triggers 2 analyzers, but only one is tested here")]
		public void Method(bool flag)
		{
			//IDE0100:Remove unnecessary equality operator
			_ = flag == true;
		}
	}
}
