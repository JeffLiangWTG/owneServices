using System;
using System.Runtime.InteropServices;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1031
	{
		public void Method()
		{
			//CW1031:Do not use GetVersionEx or Environment.OSVersion to check feature availability
			_ = Environment.OSVersion.Version;
		}

		//CW1031:Do not use GetVersionEx or Environment.OSVersion to check feature availability
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool GetVersionEx(IntPtr ver);
	}
}
