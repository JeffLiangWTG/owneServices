using System.Drawing;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Testing
{
	static class UserConfigurableDocumentTestMethodExtensions
	{
		internal static T[] LoadAll<T>(this BusinessObjectFactory factory) where T : class
		{
			var query = new ZQuery();
			return factory.Load<T>(query);
		}

		internal static bool EqualsArgb(this Color color1, Color color2)
		{
			return color1.ToArgb().Equals(color2.ToArgb());
		}
	}
}
