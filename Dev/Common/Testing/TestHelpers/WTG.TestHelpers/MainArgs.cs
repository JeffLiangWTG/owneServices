using System.Collections.Generic;

namespace WTG.TestHelpers
{
	public static class MainArgs
	{
		public static IEnumerable<string> Args
		{
			get
			{
				return NUnit.Framework.MainArgs.Args;
			}
			set
			{
				NUnit.Framework.MainArgs.Args = value;
			}
		}
	}
}
