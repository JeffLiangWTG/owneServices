using System;
using System.Globalization;

namespace Enterprise.Build.Database.Script.Testing
{
	static class CultureUtils
	{
		public static IDisposable WithCulture(CultureInfo newCulture)
		{
			var result = new CultureReverter(CultureInfo.CurrentCulture);
			CultureInfo.CurrentCulture = newCulture;
			return result;
		}

		sealed class CultureReverter : IDisposable
		{
			public CultureReverter(CultureInfo oldCulture)
			{
				this.oldCulture = oldCulture;
			}

			public void Dispose()
			{
				CultureInfo.CurrentCulture = oldCulture;
			}

			readonly CultureInfo oldCulture;
		}
	}
}
