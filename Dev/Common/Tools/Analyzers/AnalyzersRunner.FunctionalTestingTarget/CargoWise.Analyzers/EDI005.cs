using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI005
	{
		readonly WeakReference weakReference = new WeakReference(new object());

		public void Method()
		{
			if (weakReference.IsAlive)
			{
				//EDI005:Weak Reference Target Race Condition Rule
				_ = weakReference.Target.ToString();
			}
		}
	}
}
