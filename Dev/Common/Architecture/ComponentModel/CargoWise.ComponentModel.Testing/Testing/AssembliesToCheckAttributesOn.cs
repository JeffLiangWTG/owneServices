#if DEBUG
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.ComponentModel.Testing
{
	public static class AssembliesToCheckAttributesOn
	{
		public static IEnumerable<Assembly> GetAssemblies()
		{
			yield return Assembly.Load("CargoWise.ComponentModel");
			yield return Assembly.Load("CargoWise.Windows.UI");
			//yield return Assembly.Load("CargoWise.Design");
			yield return Assembly.Load("Enterprise.ZArchitecture.Business");
		}
	}
}
#endif
